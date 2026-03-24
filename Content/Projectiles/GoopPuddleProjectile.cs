using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GoopPuddleProjectile : ModProjectile {
    private const float PuddleWidth = 54f;
    private const float PuddleHeight = 18f;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    public override void SetDefaults() {
        Projectile.width = (int)PuddleWidth;
        Projectile.height = (int)PuddleHeight;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 5 * 60;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.hide = true;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 24;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        Projectile.velocity = Vector2.Zero;

        float fade = Utils.GetLerpValue(0f, 24f, Projectile.timeLeft, true);
        if (Main.rand.NextBool(2)) {
            Vector2 position = Projectile.Bottom + new Vector2(Main.rand.NextFloat(-PuddleWidth * 0.45f, PuddleWidth * 0.45f), Main.rand.NextFloat(-PuddleHeight, 0f));
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.45f, 0.45f), Main.rand.NextFloat(-1.6f, -0.35f));
            Dust drip = Dust.NewDustPerfect(position, Main.rand.NextBool(3) ? DustID.GreenTorch : DustID.GreenMoss, velocity,
                95, new Color(125, 245, 145), Main.rand.NextFloat(0.95f, 1.2f) * fade);
            drip.noGravity = true;
        }

        if (Projectile.timeLeft == 5 * 60 - 1) {
            for (int i = 0; i < 14; i++) {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 offset = new Vector2((float)System.Math.Cos(angle) * PuddleWidth * 0.42f,
                    (float)System.Math.Sin(angle) * PuddleHeight * 0.32f);
                Dust ring = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GreenTorch, Vector2.Zero, 80,
                    new Color(115, 235, 130), 1.05f);
                ring.noGravity = true;
            }
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Rectangle puddleHitbox = new(
            (int)(Projectile.Center.X - PuddleWidth * 0.5f),
            (int)(Projectile.Bottom.Y - PuddleHeight),
            (int)PuddleWidth,
            (int)PuddleHeight
        );
        return puddleHitbox.Intersects(targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Venom, 3 * 60);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 10; i++) {
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-1.8f, 1.8f), Main.rand.NextFloat(-2.2f, -0.4f));
            Dust splash = Dust.NewDustPerfect(Projectile.Bottom + new Vector2(Main.rand.NextFloat(-PuddleWidth * 0.4f, PuddleWidth * 0.4f), -4f),
                i % 2 == 0 ? DustID.GreenMoss : DustID.GreenTorch, velocity, 95, new Color(115, 240, 135), Main.rand.NextFloat(0.9f, 1.15f));
            splash.noGravity = false;
        }
    }
}
