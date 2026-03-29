using Ben10Mod.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastPotisCoronaProjectile : ModProjectile {
    private const int OrbCount = 6;
    private const int BaseFireInterval = 7;
    private const float OrbitRadiusX = 22f;
    private const float OrbitRadiusY = 48f;
    private const float OrbitRotationSpeed = 0.085f;
    private const float CenterYOffset = -8f;
    private const float BackOffset = 8f;
    private const float LanceSpeed = 17.8f;

    private int _sustainTimer;

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

    public override string Texture => "Terraria/Images/Projectile_0";

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
                float scale = MathHelper.Lerp(0.62f, 0.96f, depth) + (activeOrb ? 0.2f : 0f);
                float glowScale = scale * (activeOrb ? 1.72f : 1.34f);
                Color coreColor = GetOrbColor(omp, depth, activeOrb);
                Color glowColor = Color.Lerp(coreColor, Color.White, activeOrb ? 0.52f : 0.28f) * 0.42f;
                Vector2 drawPosition = Projectile.Center + offset - Main.screenPosition;
                float rotation = Main.GlobalTimeWrappedHourly * 5.2f + i * 0.64f;
                Rectangle frame = texture.Frame(1, frameCount, 0, (int)(Main.GameUpdateCount / 4 + i) % frameCount);
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

        OmnitrixPlayer omp = Main.player[Projectile.owner].GetModPlayer<OmnitrixPlayer>();
        for (int i = 0; i < OrbCount; i++) {
            Vector2 position = Projectile.Center + GetOrbitOffset(i);
            SpawnShotBurst(position, omp, 5);
        }
    }

    private bool ShouldStayAlive(Player owner, OmnitrixPlayer omp) {
        return owner.active &&
               !owner.dead &&
               omp.currentTransformationId == "Ben10Mod:HeatBlast" &&
               owner.GetModPlayer<PotisAltiarePlayer>().potisAltiareEquipped &&
               omp.IsSecondaryAbilityAttackLoaded &&
               owner.channel &&
               !owner.noItems &&
               !owner.CCed;
    }

    private void TryFire(Player owner, OmnitrixPlayer omp, Vector2 aimDirection) {
        FireTimer++;
        if (FireTimer < GetFireInterval(omp))
            return;

        FireTimer = 0f;
        int orbIndex = ActiveOrbIndex;
        ActiveOrbIndex = (ActiveOrbIndex + 1) % OrbCount;
        if (Main.netMode != NetmodeID.SinglePlayer)
            Projectile.netUpdate = true;

        Vector2 spawnPosition = Projectile.Center + GetOrbitOffset(orbIndex);
        Vector2 shotDirection = (Main.MouseWorld - spawnPosition).SafeNormalize(aimDirection);
        shotDirection = shotDirection.RotatedBy(Main.rand.NextFloat(-0.05f, 0.05f));

        int projectileIndex = Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPosition,
            shotDirection * LanceSpeed, ModContent.ProjectileType<HeatBlastPotisLanceProjectile>(),
            Projectile.damage, Projectile.knockBack + 0.5f, owner.whoAmI, omp.IsTertiaryAbilityActive ? 1f : 0f,
            omp.snowflake ? 1f : 0f);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;

        SpawnShotBurst(spawnPosition, omp, 8);
    }

    private void SpawnOrbitDust(OmnitrixPlayer omp) {
        if (Main.dedServ || !Main.rand.NextBool(3))
            return;

        int orbIndex = Main.rand.Next(OrbCount);
        Vector2 dustPosition = Projectile.Center + GetOrbitOffset(orbIndex) + Main.rand.NextVector2Circular(5f, 5f);
        Vector2 dustVelocity = Main.rand.NextVector2Circular(0.75f, 0.75f);
        int dustType = omp.snowflake
            ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce)
            : (Main.rand.NextBool(3) ? DustID.InfernoFork : DustID.Flare);
        Color dustColor = omp.snowflake
            ? Color.Lerp(new Color(165, 228, 255), new Color(240, 250, 255), Main.rand.NextFloat())
            : Color.Lerp(new Color(255, 152, 72), new Color(255, 232, 150), Main.rand.NextFloat());

        Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 96, dustColor, Main.rand.NextFloat(0.92f, 1.3f));
        dust.noGravity = true;
    }

    private void AddOrbitLighting(OmnitrixPlayer omp) {
        Vector3 lightColor = omp.snowflake ? new Vector3(0.16f, 0.48f, 0.7f) : new Vector3(0.66f, 0.24f, 0.04f);
        for (int i = 0; i < OrbCount; i++)
            Lighting.AddLight(Projectile.Center + GetOrbitOffset(i), lightColor);
    }

    private void SpawnShotBurst(Vector2 position, OmnitrixPlayer omp, int dustCount) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < dustCount; i++) {
            int dustType = omp.snowflake
                ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce)
                : (Main.rand.NextBool(4) ? DustID.InfernoFork : DustID.Flare);
            Color dustColor = omp.snowflake
                ? Color.Lerp(new Color(168, 230, 255), new Color(240, 250, 255), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 165, 72), new Color(255, 232, 152), Main.rand.NextFloat());

            Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(5f, 5f), dustType,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 96, dustColor, Main.rand.NextFloat(0.95f, 1.45f));
            dust.noGravity = true;
        }
    }

    private Color GetOrbColor(OmnitrixPlayer omp, float depth, bool activeOrb) {
        Color baseColor = omp.snowflake
            ? Color.Lerp(new Color(128, 210, 255), new Color(238, 250, 255), depth)
            : Color.Lerp(new Color(255, 128, 42), new Color(255, 238, 165), depth);

        return activeOrb ? Color.Lerp(baseColor, Color.White, 0.36f) : baseColor * 0.92f;
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
        return omp.IsTertiaryAbilityActive ? BaseFireInterval - 2 : BaseFireInterval;
    }

    private Vector2 GetAimDirection(Player owner) {
        if (Main.netMode == NetmodeID.SinglePlayer || Projectile.owner == Main.myPlayer)
            return (Main.MouseWorld - owner.MountedCenter).SafeNormalize(new Vector2(owner.direction, 0f));

        if (Projectile.velocity.LengthSquared() > 0.0001f)
            return Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));

        return new Vector2(owner.direction, 0f);
    }
}
