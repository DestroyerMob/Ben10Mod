using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.BigChill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BigChillFrostShardProjectile : ModProjectile {
    private bool AbsoluteZero => Projectile.ai[0] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 110;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        HomeTowardTarget(AbsoluteZero ? 0.18f : 0.12f, AbsoluteZero ? 640f : 520f);
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center,
            AbsoluteZero ? new Vector3(0.22f, 0.46f, 0.72f) : new Vector3(0.14f, 0.32f, 0.56f));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                Main.rand.NextBool() ? DustID.Frost : DustID.IceTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 105,
                AbsoluteZero ? new Color(205, 245, 255) : new Color(180, 225, 255),
                Main.rand.NextFloat(0.86f, AbsoluteZero ? 1.16f : 1.02f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float rotation = direction.ToRotation();

        Main.EntitySpriteDraw(pixel, center, null, new Color(118, 205, 255, 200), rotation, Vector2.One * 0.5f,
            new Vector2(18f, 8f) * Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center - direction * 1.5f, null, new Color(235, 250, 255, 225), rotation,
            Vector2.One * 0.5f, new Vector2(10f, 4f) * Projectile.scale, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        BigChillTransformation.ResolveShardHit(Projectile, target, damageDone);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Frost : DustID.IceTorch,
                Main.rand.NextVector2Circular(1.8f, 1.8f), 105, new Color(192, 240, 255),
                Main.rand.NextFloat(0.88f, 1.12f));
            dust.noGravity = true;
        }
    }

    private void HomeTowardTarget(float homingStrength, float maxDistance) {
        NPC target = FindTarget(maxDistance);
        if (target == null)
            return;

        float speed = Projectile.velocity.Length();
        if (speed <= 0.01f)
            speed = AbsoluteZero ? 12.5f : 11f;

        Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity) * speed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
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
