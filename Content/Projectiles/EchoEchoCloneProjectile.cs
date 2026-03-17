using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoCloneProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != "Ben10Mod:EchoEcho" || !omp.PrimaryAbilityEnabled) {
            Projectile.Kill();
            return;
        }

        int totalClones = System.Math.Max(2, (int)Projectile.ai[1]);
        int cloneIndex = (int)Projectile.ai[0];
        if (cloneIndex < 0 || cloneIndex >= totalClones) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        if (Projectile.localAI[1] == 0f && Projectile.ai[1] > 0f)
            Projectile.localAI[1] = Projectile.ai[1];

        float slot = GetCloneSlot(cloneIndex);
        float bob = (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 5.2f + cloneIndex * 0.75f) * 8f;
        float horizontalOffset = slot * 38f;
        float rowOffset = System.Math.Abs(slot) > 2f ? -14f : 0f;
        Vector2 targetCenter = owner.Center + new Vector2(horizontalOffset, rowOffset + bob);
        Projectile.Center = Vector2.Lerp(Projectile.Center, targetCenter, 0.22f);
        Projectile.rotation = MathHelper.Clamp(slot * 0.04f, -0.16f, 0.16f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Firework_Red,
                Main.rand.NextVector2Circular(1f, 1f), 120, new Color(255, 140, 140), 0.9f);
            dust.noGravity = true;
        }

        if ((int)Projectile.localAI[1] != omp.transformationAttackSerial && Main.myPlayer == Projectile.owner) {
            Projectile.localAI[1] = omp.transformationAttackSerial;
            NPC target = FindClosestNPC(420f);
            if (target != null) {
                Vector2 velocity = Projectile.Center.DirectionTo(target.Center) * 11f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                    ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(), Projectile.damage, 0f, Projectile.owner);
            }
        }
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

    private static float GetCloneSlot(int cloneIndex) {
        int ring = cloneIndex / 2 + 1;
        bool leftSide = cloneIndex % 2 == 0;
        return leftSide ? -ring : ring;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float slot = GetCloneSlot((int)Projectile.ai[0]);
        SpriteEffects effects = slot < 0f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Main.EntitySpriteDraw(pixel, center + new Vector2(0f, -10f), null, new Color(255, 120, 120, 110), 0f,
            Vector2.One * 0.5f, new Vector2(7f, 7f), effects, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(0f, 3f), null, new Color(255, 90, 90, 95), Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(11f, 14f), effects, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(-4f, 15f), null, new Color(255, 90, 90, 90), 0f,
            Vector2.One * 0.5f, new Vector2(3f, 10f), effects, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(4f, 15f), null, new Color(255, 90, 90, 90), 0f,
            Vector2.One * 0.5f, new Vector2(3f, 10f), effects, 0);
        return false;
    }
}
