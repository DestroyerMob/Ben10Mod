using Ben10Mod.Content.Transformations.FourArms;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsHaymakerChargeProjectile : ModProjectile {
    private const int MaxChargeFrames = 54;
    private const int PotisMaxChargeFrames = 46;
    private const float PunchSpeed = 12f;
    private const float PotisPunchSpeed = 14.5f;

    private bool Berserked => Projectile.ai[0] >= 0.5f;
    private bool PotisInfused => Projectile.ai[1] >= 0.5f;
    private int ChargeFrameTarget => PotisInfused ? PotisMaxChargeFrames : MaxChargeFrames;
    private ref float ChargeFrames => ref Projectile.localAI[0];
    private ref float ReleasedFlag => ref Projectile.localAI[1];

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.timeLeft = 2;
    }

    public override bool? CanDamage() => false;

    public override bool PreDraw(ref Color lightColor) => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (!owner.active || owner.dead || omp.currentTransformationId != FourArmsGroundSlamPlayer.TransformationId) {
            CancelCharge(owner, omp);
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;

        Vector2 direction = ResolveAimDirection(owner);
        Projectile.velocity = direction;
        Projectile.Center = owner.MountedCenter + direction * 10f;
        owner.ChangeDir(direction.X >= 0f ? 1 : -1);
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;
        owner.itemRotation = (float)System.Math.Atan2(direction.Y * owner.direction, direction.X * owner.direction);
        owner.noKnockback = true;
        owner.velocity.X *= 0.82f;

        float chargeRatio = MathHelper.Clamp(ChargeFrames / ChargeFrameTarget, 0f, 1f);
        owner.GetModPlayer<FourArmsGroundSlamPlayer>().RegisterHaymakerCharge(chargeRatio);

        if (!omp.IsSecondaryAbilityAttackLoaded) {
            CancelCharge(owner, omp);
            Projectile.Kill();
            return;
        }

        if (owner.channel && !owner.noItems && !owner.CCed) {
            if (ChargeFrames < ChargeFrameTarget)
                ChargeFrames++;

            SpawnChargeDust(owner, direction, chargeRatio, PotisInfused);
            return;
        }

        if (Projectile.owner == Main.myPlayer)
            ReleaseHaymaker(owner, omp, direction);

        Projectile.Kill();
    }

    private void ReleaseHaymaker(Player owner, OmnitrixPlayer omp, Vector2 direction) {
        if (ReleasedFlag > 0f)
            return;

        ReleasedFlag = 1f;
        float chargeRatio = MathHelper.Clamp(ChargeFrames / ChargeFrameTarget, 0f, 1f);
        float releaseMultiplier = MathHelper.Lerp(PotisInfused ? 1.02f : 0.92f, PotisInfused ? 1.88f : 1.72f, chargeRatio) *
                                  (Berserked ? 1.12f : 1f);
        int releaseDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * releaseMultiplier));
        float releaseScale = 1.38f + chargeRatio * 0.72f + (Berserked ? 0.14f : 0f) + (PotisInfused ? 0.28f : 0f);
        float releaseKnockback = Projectile.knockBack + 2.2f + chargeRatio * (PotisInfused ? 5.1f : 4.2f);
        Vector2 spawnPosition = owner.MountedCenter + direction * 22f;

        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, direction * (PotisInfused ? PotisPunchSpeed : PunchSpeed),
            ModContent.ProjectileType<FourArmsPunchProjectile>(), releaseDamage, releaseKnockback, owner.whoAmI,
            releaseScale, PotisInfused ? 13f : 3f);

        if (PotisInfused) {
            int fissureDamage = System.Math.Max(1, (int)System.Math.Round(releaseDamage * 0.46f));
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Bottom + new Vector2(direction.X * 14f, -10f), Vector2.Zero,
                ModContent.ProjectileType<FourArmsFissureProjectile>(), fissureDamage, releaseKnockback * 0.62f,
                owner.whoAmI, System.Math.Sign(direction.X == 0f ? owner.direction : direction.X), 1f);
        }

        omp.NotifyLoadedAbilityAttackFired();
        omp.ClearLoadedAbilityAttack(addCooldownIfUsed: true);
        owner.channel = false;

        SoundEngine.PlaySound(SoundID.Item1 with { Pitch = PotisInfused ? -0.36f : -0.28f, Volume = PotisInfused ? 0.94f : 0.82f },
            spawnPosition);
        for (int i = 0; i < (PotisInfused ? 28 : 18); i++) {
            Dust dust = Dust.NewDustPerfect(spawnPosition + Main.rand.NextVector2Circular(10f, 10f),
                PotisInfused && i % 5 == 0 ? DustID.WhiteTorch : i % 4 == 0 ? DustID.Torch : DustID.Smoke,
                direction.RotatedByRandom(PotisInfused ? 0.42f : 0.34f) * Main.rand.NextFloat(2.4f, PotisInfused ? 8f : 6.6f),
                90, PotisInfused ? new Color(255, 205, 125) : i % 4 == 0 ? new Color(255, 168, 105) : new Color(225, 205, 190),
                Main.rand.NextFloat(PotisInfused ? 1.08f : 0.95f, PotisInfused ? 1.75f : 1.45f));
            dust.noGravity = true;
        }
    }

    private static void CancelCharge(Player owner, OmnitrixPlayer omp) {
        if (owner.whoAmI != Main.myPlayer)
            return;

        omp.ClearLoadedAbilityAttack();
        owner.channel = false;
    }

    private static Vector2 ResolveAimDirection(Player owner) {
        Vector2 direction;
        if (Main.netMode == NetmodeID.SinglePlayer || owner.whoAmI == Main.myPlayer) {
            direction = Main.MouseWorld - owner.Center;
            if (direction.LengthSquared() < 0.0001f)
                direction = new Vector2(owner.direction, 0f);
        }
        else {
            direction = new Vector2(owner.direction, 0f);
        }

        return direction.SafeNormalize(new Vector2(owner.direction, 0f));
    }

    private static void SpawnChargeDust(Player owner, Vector2 direction, float chargeRatio, bool potisInfused) {
        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 dustPosition = owner.MountedCenter + direction * 18f + normal * Main.rand.NextFloat(-10f, 10f);
        Vector2 dustVelocity = direction * Main.rand.NextFloat(0.6f, 1.8f) + normal * Main.rand.NextFloat(-0.45f, 0.45f);
        int dustType = potisInfused && Main.rand.NextBool(3) ? DustID.WhiteTorch : Main.rand.NextBool(3) ? DustID.Torch : DustID.Smoke;
        Color startColor = potisInfused ? new Color(255, 205, 130) : new Color(225, 190, 170);
        Color endColor = potisInfused ? new Color(255, 238, 174) : new Color(255, 170, 110);
        Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, potisInfused ? 85 : 100,
            Color.Lerp(startColor, endColor, chargeRatio), (potisInfused ? 1.08f : 0.95f) + chargeRatio * (potisInfused ? 0.62f : 0.45f));
        dust.noGravity = true;
    }
}
