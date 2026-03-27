using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class XLR8VectorDashProjectile : ModProjectile {
    public const int MinDashFrames = 5;
    public const int MaxDashFrames = 18;
    private const float BaseDashSpeed = 30f;
    private const float EmpoweredDashSpeed = 36f;

    public static float GetDashSpeed(bool empowered) => empowered ? EmpoweredDashSpeed : BaseDashSpeed;

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

        bool empowered = Projectile.ai[0] > 0f;
        float dashSpeed = GetDashSpeed(empowered);
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.velocity = direction * dashSpeed;
        Projectile.rotation = direction.ToRotation();

        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = System.Math.Max(owner.immuneTime, 10);
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.armorEffectDrawShadow = true;

        Projectile.Center = owner.Center + direction * (empowered ? 18f : 16f);

        if (Main.rand.NextBool(empowered ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                DustID.BlueCrystalShard, -Projectile.velocity * 0.1f, 115, new Color(120, 210, 255),
                empowered ? 1.15f : 1f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for (int i = 0; i < 14; i++) {
            Vector2 burstVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.65f) *
                Main.rand.NextFloat(1.4f, 4.6f);
            Dust dust = Dust.NewDustPerfect(target.Center, DustID.BlueCrystalShard, burstVelocity, 100,
                new Color(135, 220, 255), Main.rand.NextFloat(1f, 1.2f));
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        owner.noKnockback = false;
        owner.velocity *= 0.55f;

        if (Main.dedServ)
            return;

        for (int i = 0; i < 16; i++) {
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(12f, 16f), DustID.BlueCrystalShard,
                Main.rand.NextVector2Circular(2.2f, 2.2f), 105, new Color(120, 210, 255), 1.05f);
            dust.noGravity = true;
        }
    }
}
