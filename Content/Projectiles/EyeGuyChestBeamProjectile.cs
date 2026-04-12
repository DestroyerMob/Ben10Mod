using System;
using System.IO;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.EyeGuy;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EyeGuyChestBeamProjectile : ModProjectile {
    public const int VariantPrimary = 0;
    public const int VariantWatcher = 1;

    private SlotId _loopSlot;
    private bool _loopStarted;
    private int _sustainTimer;
    private Vector2 _syncedAimDirection = Vector2.UnitX;
    private bool _hasSyncedAimDirection;
    private int _aimSyncTimer;

    private int Variant => Utils.Clamp((int)Math.Round(Projectile.ai[0]), VariantPrimary, VariantWatcher);
    private int WatcherEyeIndex => Utils.Clamp((int)Math.Round(Projectile.ai[1]), 0, 3);
    private bool WatcherRelay => Variant == VariantWatcher;

    private float BeamHitLength {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float BeamDrawLength {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;

    private EyeGuyStatePlayer State => Main.player[Projectile.owner].GetModPlayer<EyeGuyStatePlayer>();
    private bool AllEyesOpen => State.AllEyesOpenActive;
    private float MaxLength => WatcherRelay ? (AllEyesOpen ? 1260f : 980f) : (AllEyesOpen ? 2100f : 1720f);
    private float BeamThickness => WatcherRelay ? (AllEyesOpen ? 18f : 14f) : (AllEyesOpen ? 34f : 26f);
    private Vector2 StartScale => WatcherRelay
        ? (AllEyesOpen ? new Vector2(1.25f, 0.92f) : new Vector2(1.05f, 0.9f))
        : (AllEyesOpen ? new Vector2(1.82f, 1.05f) : new Vector2(1.55f, 1f));
    private Vector2 OuterScale => WatcherRelay
        ? (AllEyesOpen ? new Vector2(1.95f, 0.94f) : new Vector2(1.6f, 0.9f))
        : (AllEyesOpen ? new Vector2(3f, 1.08f) : new Vector2(2.45f, 1f));
    private Vector2 MidScale => WatcherRelay
        ? (AllEyesOpen ? new Vector2(1.46f, 0.92f) : new Vector2(1.18f, 0.88f))
        : (AllEyesOpen ? new Vector2(2.12f, 1.02f) : new Vector2(1.72f, 1f));
    private Vector2 InnerScale => WatcherRelay
        ? (AllEyesOpen ? new Vector2(1.02f, 0.88f) : new Vector2(0.82f, 0.84f))
        : (AllEyesOpen ? new Vector2(1.42f, 0.98f) : new Vector2(1.16f, 0.96f));
    private Color BeamColor => WatcherRelay
        ? (AllEyesOpen ? new Color(180, 240, 255) : new Color(138, 220, 255))
        : (AllEyesOpen ? new Color(255, 240, 170) : new Color(255, 214, 125));
    private Color BeamHighlightColor => WatcherRelay
        ? new Color(245, 255, 255)
        : new Color(255, 250, 230);
    private int EndDustType => WatcherRelay ? DustID.GemDiamond : DustID.GoldFlame;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = VanillaBeamDrawHelper.LastPrismFrameCount;
    }

    public override void SetDefaults() {
        Projectile.width = 34;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = false;
        Projectile.alpha = 255;
        Projectile.timeLeft = 2;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (!ShouldStayAlive(owner, omp)) {
            Projectile.Kill();
            return;
        }

        if (!WatcherRelay && !TryConsumeSustainCost(owner, omp)) {
            owner.channel = false;
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        Projectile.localNPCHitCooldown = WatcherRelay
            ? (AllEyesOpen ? 10 : 12)
            : (AllEyesOpen ? 8 : 10);

        Vector2 direction = GetAimDirection(owner);
        Projectile.velocity = direction;
        Projectile.rotation = direction.ToRotation();

        if (!WatcherRelay) {
            owner.ChangeDir(direction.X >= 0f ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.itemRotation = (float)Math.Atan2(direction.Y * owner.direction, direction.X * owner.direction);
        }

        Vector2 start = GetBeamStart(owner, direction);
        Projectile.Center = start;
        BeamHitLength = GetBeamLength(start, direction);
        BeamDrawLength = MathHelper.Clamp(BeamHitLength - 6f, 14f, BeamHitLength);

        UpdateLoopSound();
        Lighting.AddLight(Projectile.Center,
            WatcherRelay ? new Vector3(0.18f, 0.42f, 0.55f) : new Vector3(0.45f, 0.35f, 0.12f));

        if (!Main.dedServ && Main.rand.NextBool(3)) {
            Vector2 end = start + direction * BeamHitLength;
            Dust dust = Dust.NewDustPerfect(end + Main.rand.NextVector2Circular(18f, 18f), EndDustType,
                Main.rand.NextVector2Circular(1.2f, 1.2f), 100, BeamColor,
                Main.rand.NextFloat(0.9f, AllEyesOpen ? 1.35f : 1.12f));
            dust.noGravity = true;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_syncedAimDirection.X);
        writer.Write(_syncedAimDirection.Y);
        writer.Write(_hasSyncedAimDirection);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _syncedAimDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        _hasSyncedAimDirection = reader.ReadBoolean();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = Projectile.velocity;
        if (direction.LengthSquared() < 0.0001f)
            return false;

        Vector2 start = GetBeamStart(owner, direction);
        Vector2 end = start + direction * BeamHitLength;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, BeamThickness,
            ref collisionPoint);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (AllEyesOpen)
            modifiers.SourceDamage *= WatcherRelay ? 1.06f : 1.14f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        EyeGuyTransformation.ResolveChestBeamHit(Projectile, target, damageDone, WatcherRelay);
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer &&
            SoundEngine.TryGetActiveSound(_loopSlot, out ActiveSound active)) {
            active.Stop();
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = Projectile.velocity;
        if (direction.LengthSquared() < 0.0001f)
            return false;

        direction.Normalize();
        Vector2 start = GetBeamStart(owner, direction);
        float length = BeamDrawLength;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(
            Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred,
            Microsoft.Xna.Framework.Graphics.BlendState.Additive,
            Main.DefaultSamplerState,
            Microsoft.Xna.Framework.Graphics.DepthStencilState.None,
            Microsoft.Xna.Framework.Graphics.RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );

        VanillaBeamDrawHelper.DrawLastPrismBeam(start, direction, length, BeamColor, BeamHighlightColor,
            StartScale, OuterScale, MidScale, InnerScale,
            outerOpacity: WatcherRelay ? 0.14f : 0.18f,
            midOpacity: WatcherRelay ? 0.28f : 0.32f,
            innerOpacity: WatcherRelay ? 0.5f : 0.58f,
            beamColorIntensity: AllEyesOpen ? 1.4f : 1.18f);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(
            Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred,
            Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend,
            Main.DefaultSamplerState,
            Microsoft.Xna.Framework.Graphics.DepthStencilState.None,
            Microsoft.Xna.Framework.Graphics.RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );

        return false;
    }

    private bool ShouldStayAlive(Player owner, OmnitrixPlayer omp) {
        if (!owner.active || owner.dead || owner.noItems || owner.CCed ||
            omp.currentTransformationId != EyeGuyStatePlayer.TransformationId) {
            return false;
        }

        if (!omp.altAttack || !owner.channel)
            return false;

        return !WatcherRelay || State.HasWatcherEyes;
    }

    private bool TryConsumeSustainCost(Player owner, OmnitrixPlayer omp) {
        var transformation = omp.CurrentTransformation;
        if (transformation == null || owner.whoAmI != Main.myPlayer)
            return true;

        int sustainInterval = transformation.GetAttackSustainInterval(OmnitrixPlayer.AttackSelection.Secondary, omp);
        int sustainCost = transformation.GetAttackSustainEnergyCost(OmnitrixPlayer.AttackSelection.Secondary, omp);
        if (sustainInterval <= 0 || sustainCost <= 0)
            return true;

        _sustainTimer++;
        if (_sustainTimer < sustainInterval)
            return true;

        _sustainTimer = 0;
        return transformation.TryConsumeAttackSustainCost(OmnitrixPlayer.AttackSelection.Secondary, omp);
    }

    private Vector2 GetLocalAimDirection(Player owner) {
        Vector2 direction = Main.MouseWorld - owner.Center;
        if (direction.LengthSquared() < 0.0001f)
            direction = new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f);

        direction.Normalize();
        return direction;
    }

    private Vector2 GetAimDirection(Player owner) {
        if (Main.netMode == NetmodeID.SinglePlayer || Projectile.owner == Main.myPlayer) {
            Vector2 localDirection = GetLocalAimDirection(owner);
            SyncAimDirection(localDirection);
            return localDirection;
        }

        return GetSyncedAimDirection(owner);
    }

    private Vector2 GetBeamStart(Player owner, Vector2 direction) {
        if (WatcherRelay)
            return EyeGuyTransformation.GetWatcherOrigin(owner, direction, WatcherEyeIndex) + direction * 8f;

        return owner.MountedCenter + new Vector2(owner.direction * 10f, -10f) + direction * 14f;
    }

    private float GetBeamLength(Vector2 start, Vector2 direction) {
        float[] samples = new float[3];
        Collision.LaserScan(start, direction, BeamThickness, MaxLength, samples);

        float tileLength = 0f;
        for (int i = 0; i < samples.Length; i++)
            tileLength += samples[i];
        tileLength /= samples.Length;

        tileLength = MathHelper.Clamp(tileLength, 16f, MaxLength);

        float best = tileLength;
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                continue;

            float collisionPoint = 0f;
            bool hit = Collision.CheckAABBvLineCollision(npc.Hitbox.TopLeft(), npc.Hitbox.Size(), start,
                start + direction * tileLength, BeamThickness, ref collisionPoint);
            if (hit && collisionPoint > 0f && collisionPoint < best)
                best = collisionPoint;
        }

        return best;
    }

    private void UpdateLoopSound() {
        if (Projectile.owner != Main.myPlayer)
            return;

        SoundStyle loopSound = WatcherRelay
            ? SoundID.Item15 with { Pitch = 0.42f, Volume = 0.12f, IsLooped = true, MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }
            : SoundID.Item15 with { Pitch = 0.14f, Volume = 0.22f, IsLooped = true, MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew };

        if (!_loopStarted) {
            _loopSlot = SoundEngine.PlaySound(loopSound, Projectile.Center);
            _loopStarted = true;
        }

        if (SoundEngine.TryGetActiveSound(_loopSlot, out ActiveSound active))
            active.Position = Projectile.Center;
    }

    private void SyncAimDirection(Vector2 direction) {
        bool changed = !_hasSyncedAimDirection || Vector2.DistanceSquared(direction, _syncedAimDirection) > 0.0004f;
        _aimSyncTimer++;
        if (!changed && _aimSyncTimer < 6)
            return;

        _syncedAimDirection = direction;
        _hasSyncedAimDirection = true;
        _aimSyncTimer = 0;
        if (Main.netMode != NetmodeID.SinglePlayer)
            Projectile.netUpdate = true;
    }

    private Vector2 GetSyncedAimDirection(Player owner) {
        if (_hasSyncedAimDirection && _syncedAimDirection.LengthSquared() > 0.0001f)
            return _syncedAimDirection;

        Vector2 fallback = Projectile.velocity.LengthSquared() > 0.0001f
            ? Projectile.velocity
            : new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f);
        return fallback.SafeNormalize(new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f));
    }
}
