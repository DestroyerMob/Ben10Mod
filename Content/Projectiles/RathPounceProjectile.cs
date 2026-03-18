using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class RathPounceProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 60;
        Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.velocity = direction;
        Projectile.rotation = direction.ToRotation();
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = 6;
        owner.noKnockback = true;
        owner.velocity = direction * 13f + new Vector2(0f, -1.2f);

        Vector2 desiredCenter = owner.Center + direction * 34f;
        Projectile.Center = desiredCenter;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, -Projectile.velocity * 0.2f,
                120, new Color(255, 170, 100), 1.1f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(210, 120, 70, 220),
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(24f, 18f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), new Color(255, 220, 165, 180),
            Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(10f, 8f), SpriteEffects.None, 0f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Bleeding, 180);

        for (int i = 0; i < 18; i++) {
            Vector2 burstVelocity = Projectile.rotation.ToRotationVector2().RotatedByRandom(0.55f) * Main.rand.NextFloat(1.2f, 4f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.Smoke : DustID.Blood, burstVelocity, 90,
                new Color(255, 185, 120), 1.25f);
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        owner.velocity *= 0.18f;
        owner.noKnockback = false;
    }
}
