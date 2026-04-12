using System;
using System.IO;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.Transformations.HeatBlast;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastSolarHaloProjectile : ModProjectile {
    private const int OrbCount = 5;
    private const int DefaultFireInterval = 9;
    private const float OrbitRadiusX = 12f;
    private const float OrbitRadiusY = 42f;
    private const float OrbitRotationSpeed = 0.065f;
    private const float CenterYOffset = -6f;
    private const float BackOffset = 14f;
    private const float FireballSpeed = 10.5f;

    private int _sustainTimer;
    private Vector2 _syncedAimDirection = Vector2.UnitX;
    private bool _hasSyncedAimDirection;
    private int _aimSyncTimer;

    private float OrbitRotation {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float FireTimer {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    private int ActiveOrbIndex {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.alpha = 255;
        Projectile.timeLeft = 2;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (!ShouldStayAlive(owner, omp)) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;

        Vector2 aimDirection = GetAimDirection(owner);
        Projectile.velocity = aimDirection;
        owner.ChangeDir(aimDirection.X >= 0f ? 1 : -1);
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;
        owner.itemRotation = (float)System.Math.Atan2(aimDirection.Y * owner.direction, aimDirection.X * owner.direction);

        Projectile.Center = owner.MountedCenter + new Vector2(-owner.direction * BackOffset, CenterYOffset);
        OrbitRotation = MathHelper.WrapAngle(OrbitRotation + OrbitRotationSpeed);

        if (Projectile.owner == Main.myPlayer) {
            if (!TryUpdateSustain(owner, omp)) {
                owner.channel = false;
                Projectile.Kill();
                return;
            }

            TryFire(owner, omp, aimDirection);
        }

        SpawnOrbitDust(omp);
        AddOrbitLighting(omp);
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

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[ProjectileID.ImpFireball].Value;
        int frameCount = Main.projFrames[ProjectileID.ImpFireball] > 0 ? Main.projFrames[ProjectileID.ImpFireball] : 1;
        OmnitrixPlayer omp = Main.player[Projectile.owner].GetModPlayer<OmnitrixPlayer>();

        for (int pass = 0; pass < 2; pass++) {
            for (int i = 0; i < OrbCount; i++) {
                Vector2 offset = GetOrbitOffset(i);
                bool isFront = offset.Y >= 0f;
                if ((pass == 0 && isFront) || (pass == 1 && !isFront))
                    continue;

                float depth = Utils.GetLerpValue(-OrbitRadiusY, OrbitRadiusY, offset.Y, true);
                bool activeOrb = i == ActiveOrbIndex;
                float scale = MathHelper.Lerp(0.48f, 0.78f, depth) + (activeOrb ? 0.16f : 0f);
                float glowScale = scale * (activeOrb ? 1.55f : 1.26f);
                Color coreColor = GetOrbColor(omp, depth, activeOrb);
                Color glowColor = Color.Lerp(coreColor, Color.White, activeOrb ? 0.5f : 0.28f) * 0.45f;
                Vector2 drawPosition = Projectile.Center + offset - Main.screenPosition;
                float rotation = Main.GlobalTimeWrappedHourly * 4.2f + i * 0.7f;
                Rectangle frame = texture.Frame(1, frameCount, 0, (int)(Main.GameUpdateCount / 5 + i) % frameCount);
                Vector2 origin = frame.Size() * 0.5f;

                Main.EntitySpriteDraw(texture, drawPosition, frame, glowColor, rotation, origin, glowScale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, drawPosition, frame, coreColor, rotation, origin, scale, SpriteEffects.None, 0);
            }
        }

        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        Player owner = Main.player[Projectile.owner];
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        for (int i = 0; i < OrbCount; i++) {
            Vector2 position = Projectile.Center + GetOrbitOffset(i);
            SpawnShotBurst(position, omp, 4);
        }
    }

    private bool ShouldStayAlive(Player owner, OmnitrixPlayer omp) {
        return owner.active &&
               !owner.dead &&
               omp.currentTransformationId == "Ben10Mod:HeatBlast" &&
               omp.IsSecondaryAbilityAttackLoaded &&
               owner.channel &&
               !owner.noItems &&
               !owner.CCed;
    }

    private void TryFire(Player owner, OmnitrixPlayer omp, Vector2 aimDirection) {
        HeatBlastStatePlayer state = owner.GetModPlayer<HeatBlastStatePlayer>();
        bool queuedShot = state.TryConsumeHaloQueuedShot();
        FireTimer++;
        if (!queuedShot && FireTimer < GetFireInterval(omp))
            return;

        FireTimer = 0f;

        int orbIndex = ActiveOrbIndex;
        ActiveOrbIndex = (ActiveOrbIndex + 1) % OrbCount;
        if (Main.netMode != NetmodeID.SinglePlayer)
            Projectile.netUpdate = true;

        Vector2 spawnPosition = Projectile.Center + GetOrbitOffset(orbIndex);
        Vector2 targetPosition = Main.MouseWorld;
        if (state.TryGetFocusedTarget(out NPC focusedTarget))
            targetPosition = focusedTarget.Center;

        Vector2 shotDirection = (targetPosition - spawnPosition).SafeNormalize(aimDirection);
        shotDirection = shotDirection.RotatedBy(Main.rand.NextFloat(queuedShot ? -0.025f : -0.07f, queuedShot ? 0.025f : 0.07f));
        float shotSpeed = FireballSpeed + (omp.IsTertiaryAbilityActive ? 1.25f : 0f) + (queuedShot ? 0.85f : 0f);

        int projectileIndex = Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPosition,
            shotDirection * shotSpeed, ModContent.ProjectileType<HeatBlastHaloFireballProjectile>(),
            Projectile.damage, Projectile.knockBack + 0.5f, owner.whoAmI, omp.snowflake ? 1f : 0f);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;

        SpawnShotBurst(spawnPosition, omp, 8);
        SoundEngine.PlaySound(SoundID.Item20 with { Pitch = -0.2f, Volume = 0.44f, MaxInstances = 12 }, spawnPosition);
    }

    private void SpawnOrbitDust(OmnitrixPlayer omp) {
        if (Main.dedServ || !Main.rand.NextBool(3))
            return;

        int orbIndex = Main.rand.Next(OrbCount);
        Vector2 dustPosition = Projectile.Center + GetOrbitOffset(orbIndex) + Main.rand.NextVector2Circular(5f, 5f);
        Vector2 dustVelocity = Main.rand.NextVector2Circular(0.6f, 0.6f);
        int dustType = omp.snowflake
            ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce)
            : (Main.rand.NextBool(3) ? DustID.Flare : DustID.Torch);
        Color dustColor = omp.snowflake
            ? Color.Lerp(new Color(155, 225, 255), new Color(225, 245, 255), Main.rand.NextFloat())
            : Color.Lerp(new Color(255, 145, 55), new Color(255, 218, 125), Main.rand.NextFloat());

        Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 96, dustColor, Main.rand.NextFloat(0.85f, 1.25f));
        dust.noGravity = true;
    }

    private void AddOrbitLighting(OmnitrixPlayer omp) {
        Vector3 lightColor = omp.snowflake
            ? new Vector3(0.12f, 0.4f, 0.56f)
            : new Vector3(0.58f, 0.24f, 0.04f);

        for (int i = 0; i < OrbCount; i++)
            Lighting.AddLight(Projectile.Center + GetOrbitOffset(i), lightColor);
    }

    private void SpawnShotBurst(Vector2 position, OmnitrixPlayer omp, int dustCount) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < dustCount; i++) {
            int dustType = omp.snowflake
                ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce)
                : (Main.rand.NextBool(4) ? DustID.Flare : DustID.Torch);
            Color dustColor = omp.snowflake
                ? Color.Lerp(new Color(165, 228, 255), new Color(240, 250, 255), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 162, 72), new Color(255, 228, 150), Main.rand.NextFloat());

            Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(4f, 4f), dustType,
                Main.rand.NextVector2Circular(2.6f, 2.6f), 96, dustColor, Main.rand.NextFloat(0.95f, 1.4f));
            dust.noGravity = true;
        }
    }

    private Color GetOrbColor(OmnitrixPlayer omp, float depth, bool activeOrb) {
        Color baseColor = omp.snowflake
            ? Color.Lerp(new Color(125, 205, 255), new Color(238, 250, 255), depth)
            : Color.Lerp(new Color(255, 130, 42), new Color(255, 235, 165), depth);

        return activeOrb ? Color.Lerp(baseColor, Color.White, 0.34f) : baseColor * 0.92f;
    }

    private Vector2 GetOrbitOffset(int index) {
        float angle = OrbitRotation + MathHelper.TwoPi * index / OrbCount;
        return new Vector2(System.MathF.Cos(angle) * OrbitRadiusX, System.MathF.Sin(angle) * OrbitRadiusY);
    }

    private bool TryUpdateSustain(Player owner, OmnitrixPlayer omp) {
        var transformation = omp.CurrentTransformation;
        if (transformation == null || owner.whoAmI != Main.myPlayer)
            return true;

        int sustainInterval = transformation.GetAttackSustainInterval(OmnitrixPlayer.AttackSelection.SecondaryAbility, omp);
        int sustainCost = transformation.GetAttackSustainEnergyCost(OmnitrixPlayer.AttackSelection.SecondaryAbility, omp);
        if (sustainInterval <= 0 || sustainCost <= 0)
            return true;

        _sustainTimer++;
        if (_sustainTimer < sustainInterval)
            return true;

        _sustainTimer = 0;
        return transformation.TryConsumeAttackSustainCost(OmnitrixPlayer.AttackSelection.SecondaryAbility, omp);
    }

    private int GetFireInterval(OmnitrixPlayer omp) {
        var transformation = omp.CurrentTransformation;
        if (transformation == null)
            return DefaultFireInterval;

        int sustainInterval = transformation.GetAttackSustainInterval(OmnitrixPlayer.AttackSelection.SecondaryAbility, omp);
        int interval = sustainInterval > 0 ? sustainInterval : DefaultFireInterval;
        if (omp.IsTertiaryAbilityActive)
            interval = Math.Max(5, interval - 2);

        return interval;
    }

    private Vector2 GetLocalAimDirection(Player owner) {
        Vector2 direction = Main.MouseWorld - owner.Center;
        if (direction.LengthSquared() < 0.0001f)
            direction = new Vector2(owner.direction, 0f);

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
}
