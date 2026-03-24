using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AlienXBlackHoleProjectile : ModProjectile {
    private const float PullRadius = 220f;
    private const float DamageRadius = 42f;
    private const float StrongPullRadius = 112f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 150;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI() {
        Projectile.rotation += 0.08f;
        Projectile.velocity *= 0.965f;
        if (Projectile.velocity.LengthSquared() < 0.16f)
            Projectile.velocity = Vector2.Zero;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            PullNPCs();

        Lighting.AddLight(Projectile.Center, new Vector3(0.06f, 0.06f, 0.12f));
        SpawnSingularityDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= DamageRadius;
    }

    private void PullNPCs() {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.active || !npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(npc.Center, Projectile.Center);
            if (distance > PullRadius || distance <= 4f)
                continue;

            Vector2 pullDirection = (Projectile.Center - npc.Center) / distance;
            float distanceFactor = 1f - distance / PullRadius;
            float pullStrength = MathHelper.Lerp(1.8f, 15.5f, distanceFactor);
            if (distance <= StrongPullRadius)
                pullStrength *= MathHelper.Lerp(1.25f, 1.8f, 1f - distance / StrongPullRadius);

            if (npc.boss)
                pullStrength *= 0.55f;
            else if (npc.knockBackResist > 0f)
                pullStrength *= MathHelper.Lerp(0.7f, 1.15f, npc.knockBackResist);

            Vector2 targetVelocity = pullDirection * pullStrength;
            npc.velocity = Vector2.Lerp(npc.velocity, targetVelocity, npc.boss ? 0.12f : 0.3f);
            npc.netUpdate = true;
        }
    }

    private void SpawnSingularityDust() {
        if (Main.dedServ)
            return;

        float orbitRotation = Main.GlobalTimeWrappedHourly * 2.8f;
        for (int i = 0; i < 3; i++) {
            float angle = orbitRotation + MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.18f, 0.18f);
            float radius = Main.rand.NextFloat(DamageRadius + 6f, PullRadius * 0.55f);
            Vector2 offset = angle.ToRotationVector2() * radius;
            Vector2 tangential = offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * 0.7f;

            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, i == 0 ? DustID.PurpleTorch : DustID.ShadowbeamStaff,
                tangential - offset * 0.009f, 120, new Color(95, 95, 130), Main.rand.NextFloat(0.95f, 1.25f));
            dust.noGravity = true;
        }

        if (Main.rand.NextBool(2)) {
            Dust core = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.GemAmethyst,
                Main.rand.NextVector2Circular(0.5f, 0.5f), 150, new Color(210, 210, 255), Main.rand.NextFloat(0.8f, 1.1f));
            core.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        DrawRing(pixel, center, PullRadius * 0.37f, new Color(32, 32, 56, 38), 3.2f, Projectile.rotation * 0.35f);
        DrawRing(pixel, center, PullRadius * 0.26f, new Color(82, 82, 128, 72), 4.2f, -Projectile.rotation * 0.65f);
        DrawRing(pixel, center, PullRadius * 0.16f, new Color(170, 170, 235, 105), 5f, Projectile.rotation);
        Main.EntitySpriteDraw(pixel, center, null, new Color(25, 25, 35, 230), 0f, Vector2.One * 0.5f,
            new Vector2(DamageRadius * 1.2f, DamageRadius * 1.2f), SpriteEffects.None);
        Main.EntitySpriteDraw(pixel, center, null, new Color(95, 95, 145, 95), 0f, Vector2.One * 0.5f,
            new Vector2(DamageRadius * 1.75f, DamageRadius * 1.75f), SpriteEffects.None);
        Main.EntitySpriteDraw(pixel, center, null, new Color(180, 180, 235, 110), 0f, Vector2.One * 0.5f,
            new Vector2(10f, 10f), SpriteEffects.None);
        return false;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, Color color, float thickness, float rotation) {
        for (int i = 0; i < 18; i++) {
            float angle = rotation + MathHelper.TwoPi * i / 18f;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 3.6f), SpriteEffects.None);
        }
    }
}
