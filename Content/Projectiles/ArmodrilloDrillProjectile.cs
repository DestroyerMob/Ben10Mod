using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArmodrilloDrillProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Item_{ItemID.ChlorophyteJackhammer}";

    public override void SetDefaults() {
        Projectile.width = 36;
        Projectile.height = 36;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 18;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float progress = 1f - Projectile.timeLeft / 18f;
        float reach = MathHelper.Lerp(18f, 50f, Utils.GetLerpValue(0f, 0.55f, progress, true));
        Projectile.Center = owner.Center + direction * reach;
        Projectile.rotation += 0.55f * owner.direction;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.itemRotation = direction.ToRotation();

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke,
                direction.X * 1.4f, direction.Y * 1.4f, 110, new Color(170, 130, 70), 1.1f);
            dust.noGravity = true;
        }
    }
}
