using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class LodestarMagnetBoltProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 84;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        NPC target = FindTarget(240f);
        if (target != null) {
            float speed = Projectile.velocity.Length();
            Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity) * speed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.08f);
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.34f, 0.3f) * 0.38f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                Main.rand.NextBool() ? DustID.Firework_Red : DustID.Iron,
                -Projectile.velocity * Main.rand.NextFloat(0.03f, 0.1f), 110, new Color(235, 130, 115),
                Main.rand.NextFloat(0.9f, 1.08f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 lineStart = Projectile.Center - direction * 12f;
        Vector2 lineEnd = Projectile.Center + direction * 18f;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, 11f,
            ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = direction.ToRotation();

        Main.EntitySpriteDraw(pixel, center, null, new Color(225, 85, 72, 120), rotation, Vector2.One * 0.5f,
            new Vector2(34f, 7f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(235, 235, 240, 210), rotation, Vector2.One * 0.5f,
            new Vector2(22f, 3.2f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center + perpendicular * 6f, null, new Color(185, 195, 210, 120), rotation + 0.26f,
            Vector2.One * 0.5f, new Vector2(11f, 2.1f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center - perpendicular * 6f, null, new Color(185, 195, 210, 120), rotation - 0.26f,
            Vector2.One * 0.5f, new Vector2(11f, 2.1f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 pull = (Projectile.Center - target.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX)) * 5.5f;
        target.velocity = Vector2.Lerp(target.velocity, target.velocity + pull, 0.45f);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 75);
        target.AddBuff(BuffID.BrokenArmor, 90);
        target.netUpdate = true;
    }

    private NPC FindTarget(float maxDistance) {
        NPC bestTarget = null;
        float bestDistanceSquared = maxDistance * maxDistance;
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distanceSquared = Vector2.DistanceSquared(Projectile.Center, npc.Center);
            if (distanceSquared >= bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            bestTarget = npc;
        }

        return bestTarget;
    }
}
