using System.IO;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.UltimateAttacks;

public abstract class ChannelBeamUltimateProjectile : ModProjectile
{
    private SlotId _loopSlot;
    private bool _loopStarted;
    private int _sustainTimer;
    private Vector2 _syncedAimDirection = Vector2.UnitX;
    private bool _hasSyncedAimDirection;
    private int _aimSyncTimer;

    protected float BeamHitLength {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    protected float BeamDrawLength {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    protected virtual float MaxLength => 2600f;
    protected virtual float BeamThickness => 28f;
    protected virtual float StartOffset => 52f;
    protected virtual float EndCapPadding => 6f;
    protected virtual int MinEnergyToSustain => 10;
    protected virtual int ProjectileAlpha => 255;
    protected virtual int BeamFrameCount => 3;
    protected virtual Vector2 StartScale => new(1.55f, 1f);
    protected virtual Vector2 OuterScale => new(2.55f, 1f);
    protected virtual Vector2 MidScale => new(1.85f, 1f);
    protected virtual Vector2 InnerScale => new(1.25f, 1f);
    protected virtual Color BeamColor => new(60, 255, 140);
    protected virtual Color BeamHighlightColor => Color.White;
    protected virtual int EndDustType => DustID.GreenTorch;
    protected virtual int EndDustCount => 5;
    protected virtual SoundStyle? LoopSound => SoundID.Item15;
    protected virtual float LightR => 0.2f;
    protected virtual float LightG => 1.6f;
    protected virtual float LightB => 0.6f;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = BeamFrameCount;
    }

    public override void SetDefaults() {
        Projectile.width = 34;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = false;
        Projectile.alpha = ProjectileAlpha;
        Projectile.timeLeft = 2;
        Projectile.DamageType = DamageClass.Magic;
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

        if (!TryConsumeSustainCost(owner, omp)) {
            owner.channel = false;
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;

        Vector2 dir = GetAimDirection(owner);
        Projectile.velocity = dir;
        Projectile.rotation = dir.ToRotation();
        Vector2 start = GetBeamStart(owner, dir);
        Projectile.Center = start;
        BeamHitLength = GetBeamLength(start, dir);
        BeamDrawLength = MathHelper.Clamp(BeamHitLength - EndCapPadding, 16f, BeamHitLength);

        UpdateLoopSound();
        Lighting.AddLight(Projectile.Center, LightR, LightG, LightB);
        OnBeamUpdated(owner, omp, start, dir);
    }

    protected virtual bool ShouldStayAlive(Player owner, OmnitrixPlayer omp) {
        return owner.active &&
               !owner.dead &&
               omp.ultimateAttack &&
               owner.channel &&
               !owner.noItems &&
               !owner.CCed &&
               omp.omnitrixEnergy >= MinEnergyToSustain;
    }

    protected virtual Vector2 GetLocalAimDirection(Player owner) {
        Vector2 dir = Main.MouseWorld - owner.Center;
        if (dir.LengthSquared() < 0.0001f)
            dir = new Vector2(owner.direction, 0f);

        dir.Normalize();
        return dir;
    }

    protected virtual Vector2 GetAimDirection(Player owner) {
        if (Main.netMode == NetmodeID.SinglePlayer || Projectile.owner == Main.myPlayer) {
            Vector2 localDirection = GetLocalAimDirection(owner);
            SyncAimDirection(localDirection);
            return localDirection;
        }

        return GetSyncedAimDirection(owner);
    }

    protected virtual Vector2 GetBeamStart(Player owner, Vector2 direction) {
        return owner.Center + direction * StartOffset;
    }

    protected virtual void OnBeamUpdated(Player owner, OmnitrixPlayer omp, Vector2 start, Vector2 direction) { }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_syncedAimDirection.X);
        writer.Write(_syncedAimDirection.Y);
        writer.Write(_hasSyncedAimDirection);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _syncedAimDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        _hasSyncedAimDirection = reader.ReadBoolean();
    }

    private bool TryConsumeSustainCost(Player owner, OmnitrixPlayer omp) {
        var trans = omp.CurrentTransformation;
        if (trans == null)
            return true;

        int sustainInterval = trans.GetAttackSustainInterval(OmnitrixPlayer.AttackSelection.Ultimate, omp);
        int sustainCost = trans.GetAttackSustainEnergyCost(OmnitrixPlayer.AttackSelection.Ultimate, omp);
        if (sustainInterval <= 0 || sustainCost <= 0 || owner.whoAmI != Main.myPlayer)
            return true;

        _sustainTimer++;
        if (_sustainTimer < sustainInterval)
            return true;

        _sustainTimer = 0;
        return trans.TryConsumeAttackSustainCost(OmnitrixPlayer.AttackSelection.Ultimate, omp);
    }

    private void UpdateLoopSound() {
        if (Projectile.owner != Main.myPlayer)
            return;

        SoundStyle? loopSound = LoopSound;
        if (loopSound is null)
            return;

        if (!_loopStarted) {
            SoundStyle style = loopSound.Value;
            style.IsLooped = true;
            style.MaxInstances = 1;
            style.SoundLimitBehavior = SoundLimitBehavior.IgnoreNew;

            _loopSlot = SoundEngine.PlaySound(style, Projectile.Center);
            _loopStarted = true;
        }

        if (SoundEngine.TryGetActiveSound(_loopSlot, out ActiveSound active))
            active.Position = Projectile.Center;
    }

    protected virtual float GetBeamLength(Vector2 start, Vector2 dir) {
        float[] samples = new float[3];
        Collision.LaserScan(start, dir, BeamThickness, MaxLength, samples);

        float tileLength = 0f;
        for (int i = 0; i < samples.Length; i++)
            tileLength += samples[i];
        tileLength /= samples.Length;

        tileLength = MathHelper.Clamp(tileLength, 16f, MaxLength);

        float best = tileLength;

        for (int n = 0; n < Main.maxNPCs; n++) {
            NPC npc = Main.npc[n];
            if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                continue;

            float collisionPoint = 0f;
            bool hit = Collision.CheckAABBvLineCollision(
                npc.Hitbox.TopLeft(),
                npc.Hitbox.Size(),
                start,
                start + dir * tileLength,
                BeamThickness,
                ref collisionPoint
            );

            if (hit && collisionPoint > 0f && collisionPoint < best)
                best = collisionPoint;
        }

        return best;
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
            : new Vector2(owner.direction, 0f);
        return fallback.SafeNormalize(new Vector2(owner.direction, 0f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];

        Vector2 dir = Projectile.velocity;
        if (dir.LengthSquared() < 0.0001f)
            return false;

        Vector2 start = GetBeamStart(owner, dir);
        Vector2 end = start + dir * BeamHitLength;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            start,
            end,
            BeamThickness,
            ref collisionPoint
        );
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer &&
            SoundEngine.TryGetActiveSound(_loopSlot, out ActiveSound active)) {
            active.Stop();
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];

        Vector2 dir = Projectile.velocity;
        if (dir.LengthSquared() < 0.0001f)
            return false;

        dir.Normalize();

        Vector2 start = GetBeamStart(owner, dir);
        float length = BeamDrawLength;
        Vector2 endPos = start + dir * length;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Additive,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );

        VanillaBeamDrawHelper.DrawLastPrismBeam(start, dir, length, BeamColor, BeamHighlightColor, StartScale, OuterScale, MidScale,
            InnerScale);

        for (int i = 0; i < EndDustCount; i++) {
            int dustNum = Dust.NewDust(endPos, 26, 26, EndDustType, 0, 0, 0, Color.White, 3f);
            Main.dust[dustNum].noGravity = true;
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );

        return false;
    }
}
