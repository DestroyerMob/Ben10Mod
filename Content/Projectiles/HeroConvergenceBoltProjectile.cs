using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class HeroConvergenceBoltProjectile : ModProjectile {
    private const float HomingDistance = 640f;
    private const float HomingSpeed = 18f;
    private const float HomingInertia = 12f;

    private int TargetIndex => (int)Projectile.ai[0] - 1;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.HallowStar}";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 7;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        NPC target = FindTarget();
        if (target != null) {
            Vector2 desiredVelocity = Projectile.Center.DirectionTo(target.Center) * HomingSpeed;
            Projectile.velocity = (Projectile.velocity * (HomingInertia - 1f) + desiredVelocity) / HomingInertia;
        }
        else {
            Projectile.velocity *= 0.995f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.44f, 0.18f));

        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.GoldFlame : DustID.Enchanted_Gold,
            -Projectile.velocity * 0.08f, 95, new Color(255, 240, 155), Main.rand.NextFloat(0.9f, 1.15f));
        dust.noGravity = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            float alpha = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
            Color trailColor = new Color(255, 235, 150, 0) * (0.42f * alpha);
            Vector2 oldDrawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Main.EntitySpriteDraw(texture, oldDrawPosition, frame, trailColor, Projectile.rotation, origin,
                Projectile.scale * (0.85f + alpha * 0.2f), SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(texture, drawPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale,
            SpriteEffects.None, 0);

        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Main.EntitySpriteDraw(pixel, drawPosition, null, new Color(255, 235, 150, 0) * 0.85f, Projectile.rotation,
            new Vector2(0.5f, 0.5f), new Vector2(22f, 4.2f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, drawPosition, null, new Color(255, 250, 210, 0) * 0.65f, Projectile.rotation + MathHelper.PiOver2,
            new Vector2(0.5f, 0.5f), new Vector2(10f, 2.8f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        SpawnImpactDust();
    }

    public override void OnKill(int timeLeft) {
        SpawnImpactDust();
    }

    private NPC FindTarget() {
        if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs) {
            NPC lockedTarget = Main.npc[TargetIndex];
            if (lockedTarget.CanBeChasedBy(Projectile) &&
                Vector2.Distance(Projectile.Center, lockedTarget.Center) <= HomingDistance)
                return lockedTarget;
        }

        NPC bestTarget = null;
        float bestDistance = HomingDistance;

        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestTarget = npc;
        }

        return bestTarget;
    }

    private void SpawnImpactDust() {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GoldFlame : DustID.Enchanted_Gold,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 90, new Color(255, 238, 165), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }
}
