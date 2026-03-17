using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.Gwen;

public class ManaThreadProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 180;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Projectile.rotation += 0.35f;
        Lighting.AddLight(Projectile.Center, new Vector3(1.2f, 0.4f, 0.85f));

        for (int i = 0; i < 2; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.22f), 70, new Color(255, 120, 200), 1.45f);
            dust.noGravity = true;
        }

        NPC target = FindClosestNPC(360f);
        if (target == null)
            return;

        Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * 13f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.045f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.ai[1] >= 1f)
            return;

        NPC nextTarget = FindClosestNPC(320f, target.whoAmI);
        if (nextTarget == null)
            return;

        Vector2 velocity = Projectile.Center.DirectionTo(nextTarget.Center) * 13f;
        Projectile chained = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
            Type, (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner, 0f, 1f);
        chained.timeLeft = Projectile.timeLeft;
    }

    private NPC FindClosestNPC(float maxDistance, int ignoreWhoAmI = -1) {
        NPC closestTarget = null;
        float closestDistance = maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc.whoAmI == ignoreWhoAmI || !npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Projectile.Center.Distance(npc.Center);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = npc;
        }

        return closestTarget;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float rotation = direction.ToRotation() + MathHelper.PiOver2;

        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 90, 185, 130), rotation,
            Vector2.One * 0.5f, new Vector2(12f, 30f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 170, 230, 230), rotation,
            Vector2.One * 0.5f, new Vector2(6f, 20f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center - direction * 9f, null, new Color(255, 250, 255, 235), rotation,
            Vector2.One * 0.5f, new Vector2(4f, 12f), SpriteEffects.None, 0);
        return false;
    }
}
