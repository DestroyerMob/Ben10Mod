using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GoopPuddleDetonationProjectile : ModProjectile {
    private const float BaseRadiusX = 74f;
    private const float BaseRadiusY = 30f;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    public override void SetDefaults() {
        Projectile.width = 150;
        Projectile.height = 70;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 12;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        float scale = MathHelper.Clamp(Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0], 1f, GoopPuddleProjectile.MaxPuddleScale);
        Projectile.scale = MathHelper.Lerp(0.85f, 1.25f, 1f - Projectile.timeLeft / 12f) * scale;
        Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.32f, 0.08f));

        if (Main.dedServ)
            return;

        int dustCount = Projectile.timeLeft > 9 ? 22 : 5;
        for (int i = 0; i < dustCount; i++) {
            Vector2 offset = Main.rand.NextVector2Circular(BaseRadiusX * scale, BaseRadiusY * scale);
            Vector2 velocity = new(offset.X * 0.035f, Main.rand.NextFloat(-2.4f, -0.2f));
            Dust spray = Dust.NewDustPerfect(Projectile.Center + offset, i % 3 == 0 ? DustID.GreenMoss : DustID.GreenTorch,
                velocity, 85, new Color(130, 245, 135), Main.rand.NextFloat(1f, 1.45f));
            spray.noGravity = false;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float scale = MathHelper.Clamp(Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0], 1f, GoopPuddleProjectile.MaxPuddleScale);
        Vector2 targetCenter = targetHitbox.Center.ToVector2();
        Vector2 offset = targetCenter - Projectile.Center;
        float normalized = offset.X * offset.X / (BaseRadiusX * BaseRadiusX * scale * scale) +
                           offset.Y * offset.Y / (BaseRadiusY * BaseRadiusY * scale * scale);
        return normalized <= 1.15f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<GoopDissolved>(), 5 * 60);
        target.AddBuff(BuffID.Venom, 3 * 60);
        target.velocity.X *= 0.72f;
    }
}
