using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class XLR8VectorDashProjectile : ModProjectile {
    public const int MinDashFrames = 5;
    public const int MaxDashFrames = 18;
    public const int PotisMaxDashFrames = 22;
    private const float BaseDashSpeed = 30f;
    private const float EmpoweredDashSpeed = 36f;
    private const float PotisBaseDashSpeed = 35f;
    private const float PotisEmpoweredDashSpeed = 42f;

    private bool Empowered => Projectile.ai[0] > 0f;
    private bool PotisInfused => Projectile.ai[1] >= 0.5f;

    public static float GetDashSpeed(bool empowered, bool potisInfused = false) {
        if (potisInfused)
            return empowered ? PotisEmpoweredDashSpeed : PotisBaseDashSpeed;

        return empowered ? EmpoweredDashSpeed : BaseDashSpeed;
    }

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 42;
        Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = MinDashFrames;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        owner.GetModPlayer<OmnitrixPlayer>().RegisterActiveLunge();

        bool empowered = Empowered;
        bool potisInfused = PotisInfused;
        float dashSpeed = GetDashSpeed(empowered, potisInfused);
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.velocity = direction * dashSpeed;
        Projectile.rotation = direction.ToRotation();

        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = System.Math.Max(owner.immuneTime, potisInfused ? 14 : 10);
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.armorEffectDrawShadow = true;

        Projectile.Center = owner.Center + direction * (potisInfused ? empowered ? 24f : 21f : empowered ? 18f : 16f);
        Lighting.AddLight(Projectile.Center, (potisInfused ? new Vector3(0.14f, 0.9f, 1f) : new Vector3(0.08f, 0.48f, 0.65f)) *
                                             (potisInfused ? 0.32f : 0.18f));

        if (Main.rand.NextBool(potisInfused || empowered ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                potisInfused && Main.rand.NextBool(4) ? DustID.WhiteTorch : DustID.BlueCrystalShard,
                -Projectile.velocity * (potisInfused ? 0.14f : 0.1f), 115,
                potisInfused ? new Color(170, 250, 255) : new Color(120, 210, 255),
                potisInfused ? Main.rand.NextFloat(1.15f, 1.45f) : empowered ? 1.15f : 1f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool potisInfused = PotisInfused;
        int dustCount = potisInfused ? 22 : 14;
        Color dustColor = potisInfused ? new Color(180, 255, 244) : new Color(135, 220, 255);
        for (int i = 0; i < dustCount; i++) {
            Vector2 burstVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.65f) *
                Main.rand.NextFloat(1.4f, potisInfused ? 6.2f : 4.6f);
            Dust dust = Dust.NewDustPerfect(target.Center,
                potisInfused && i % 4 == 0 ? DustID.WhiteTorch : DustID.BlueCrystalShard,
                burstVelocity, 100, dustColor, Main.rand.NextFloat(1f, potisInfused ? 1.48f : 1.2f));
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        owner.noKnockback = false;
        bool potisInfused = PotisInfused;
        owner.velocity *= potisInfused ? 0.62f : 0.55f;

        if (Main.dedServ)
            return;

        for (int i = 0; i < (potisInfused ? 24 : 16); i++) {
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(12f, 16f), DustID.BlueCrystalShard,
                Main.rand.NextVector2Circular(potisInfused ? 3.2f : 2.2f, potisInfused ? 3.2f : 2.2f), 105,
                potisInfused ? new Color(170, 250, 255) : new Color(120, 210, 255), potisInfused ? 1.22f : 1.05f);
            dust.noGravity = true;
        }
    }
}
