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

        Projectile.timeLeft = 2;
        Projectile.ai[1]++;

        float angle = Main.GlobalTimeWrappedHourly * 2.1f + Projectile.ai[0];
        Vector2 targetCenter = owner.Center + angle.ToRotationVector2() * 54f;
        Projectile.Center = Vector2.Lerp(Projectile.Center, targetCenter, 0.22f);
        Projectile.rotation += 0.15f;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Firework_Red,
                Main.rand.NextVector2Circular(1f, 1f), 120, new Color(255, 140, 140), 0.9f);
            dust.noGravity = true;
        }

        if (Projectile.ai[1] % 36f == 0f && Main.myPlayer == Projectile.owner) {
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

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 90, 90, 90), Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(10f, 18f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 220, 220, 150), -Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(6f, 10f), SpriteEffects.None, 0);
        return false;
    }
}
