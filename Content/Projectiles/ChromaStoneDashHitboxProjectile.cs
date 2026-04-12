using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.ChromaStone;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneDashHitboxProjectile : ModProjectile {
    private bool Overloaded => Projectile.ai[0] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.timeLeft = 2;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 16;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        ChromaStoneStatePlayer state = owner.GetModPlayer<ChromaStoneStatePlayer>();
        if (!owner.active || owner.dead || !state.DashActive) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        Projectile.Center = owner.Center;
        Projectile.velocity = owner.velocity;
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (!Main.dedServ && Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.GemDiamond,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 95,
                ChromaStonePrismHelper.GetSpectrumColor(Projectile.identity * 0.11f), Main.rand.NextFloat(0.95f, 1.3f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 currentCenter = Projectile.Center;
        Vector2 previousCenter = Projectile.oldPos[0] == Vector2.Zero
            ? currentCenter - Projectile.velocity
            : Projectile.oldPos[0] + Projectile.Size * 0.5f;
        float collisionPoint = 0f;
        float width = Overloaded ? 40f : 32f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), previousCenter, currentCenter,
            width, ref collisionPoint);
    }
}
