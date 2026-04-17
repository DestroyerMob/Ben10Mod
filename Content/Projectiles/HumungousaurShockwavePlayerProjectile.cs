using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HumungousaurShockwavePlayerProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DD2OgreSmash}";

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.velocity.Y = 0f;
        Projectile.rotation = 0f;
        float ownerScale = Projectile.ai[0] == 0f ? 1f : System.Math.Abs(Projectile.ai[0]);
        float variant = Projectile.ai[1];
        Projectile.direction = Projectile.ai[0] < 0f ? -1 : 1;
        Projectile.spriteDirection = Projectile.direction;
        Projectile.scale = ownerScale * (1f + (60f - Projectile.timeLeft) * (variant >= 2f ? 0.019f : 0.015f));
        Projectile.GetGlobalProjectile<OmnitrixProjectile>().EnableScaleHitboxSync(Projectile);
        Color lightColor = variant >= 2f ? new Color(255, 185, 125) : new Color(255, 145, 105);
        Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.0042f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                variant >= 2f ? DustID.Torch : DustID.Smoke, Projectile.direction * Main.rand.NextFloat(0.6f, 1.8f), 0f,
                110, lightColor, Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }
}
