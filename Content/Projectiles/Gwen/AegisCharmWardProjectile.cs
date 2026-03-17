using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.Gwen;

public class AegisCharmWardProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 360;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        if (Projectile.ai[2] >= 1f) {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, new Vector3(1.2f, 0.48f, 0.95f) * 0.8f);

            for (int i = 0; i < 2; i++) {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                    -Projectile.velocity * Main.rand.NextFloat(0.08f, 0.15f), 90, new Color(255, 170, 230), 1.05f);
                dust.noGravity = true;
            }

            return;
        }

        int total = 0;
        int slot = 0;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile other = Main.projectile[i];
            if (!other.active || other.owner != Projectile.owner || other.type != Type)
                continue;

            if (other.whoAmI < Projectile.whoAmI)
                slot++;

            total++;
        }

        if (total == 0)
            total = 1;

        float angle = Main.GlobalTimeWrappedHourly * 2.4f + Projectile.ai[0] + MathHelper.TwoPi * slot / total;
        float radius = 56f + total * 6f;
        Vector2 targetCenter = owner.Center + angle.ToRotationVector2() * radius;

        Projectile.Center = Vector2.Lerp(Projectile.Center, targetCenter, 0.25f);
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation += 0.2f;

        Lighting.AddLight(Projectile.Center, new Vector3(1.15f, 0.45f, 0.9f) * 0.7f);
        for (int i = 0; i < 2; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                Main.rand.NextVector2Circular(1.4f, 1.4f), 100, new Color(255, 155, 225), 1.1f);
            dust.noGravity = true;
        }

        Projectile.ai[1]++;
        if (Projectile.ai[1] < 55f)
            return;

        NPC target = FindClosestNPC(420f);
        if (target == null)
            return;

        Projectile.ai[2] = 1f;
        Projectile.velocity = Projectile.Center.DirectionTo(target.Center) * 13f;
        SpawnShootFlash();
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.Knockback += 1.5f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Confused, 90);
        Projectile.Kill();
    }

    private NPC FindClosestNPC(float maxDistance) {
        NPC closestTarget = null;
        float closestDistance = maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Projectile.Center.Distance(npc.Center);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = npc;
        }

        return closestTarget;
    }

    private void SpawnShootFlash() {
        const int dustCount = 16;
        for (int i = 0; i < dustCount; i++) {
            float angle = MathHelper.TwoPi * i / dustCount;
            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2.2f, 4.8f);

            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.PinkTorch : DustID.GemRuby,
                velocity, 70, new Color(255, 185, 235), i % 2 == 0 ? 1.45f : 1.15f);
            dust.noGravity = true;
        }

        for (int i = 0; i < 6; i++) {
            Dust burst = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                Main.rand.NextVector2Circular(1.2f, 1.2f), 40, new Color(255, 235, 250), 1.85f);
            burst.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float spin = Projectile.rotation;

        DrawDiamond(pixel, center, 15f, 24f, new Color(255, 95, 190, 135), spin);
        DrawDiamond(pixel, center, 9f, 16f, new Color(255, 185, 235, 215), -spin * 1.2f);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 245, 255, 245), 0f, Vector2.One * 0.5f,
            new Vector2(5.5f, 5.5f), SpriteEffects.None, 0);
        return false;
    }

    private static void DrawDiamond(Texture2D pixel, Vector2 center, float shortAxis, float longAxis, Color color,
        float rotationOffset) {
        for (int i = 0; i < 4; i++) {
            float angle = rotationOffset + MathHelper.PiOver4 + MathHelper.PiOver2 * i;
            Main.EntitySpriteDraw(pixel, center + angle.ToRotationVector2() * (shortAxis * 0.45f), null, color, angle,
                Vector2.One * 0.5f, new Vector2(shortAxis * 0.22f, longAxis), SpriteEffects.None, 0);
        }
    }
}
