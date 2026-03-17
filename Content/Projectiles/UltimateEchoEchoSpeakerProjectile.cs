using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UltimateEchoEchoSpeakerProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 26;
        Projectile.height = 26;
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
        if (omp.currentTransformationId != "Ben10Mod:UltimateEchoEcho" || !omp.PrimaryAbilityEnabled) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        Projectile.ai[1]++;

        float angle = Main.GlobalTimeWrappedHourly * 1.8f + Projectile.ai[0];
        Vector2 targetCenter = owner.Center + angle.ToRotationVector2() * 66f;
        Projectile.Center = Vector2.Lerp(Projectile.Center, targetCenter, 0.18f);
        Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation();

        if (Projectile.ai[1] % 42f == 0f && Main.myPlayer == Projectile.owner) {
            NPC target = FindClosestNPC(460f);
            if (target != null) {
                for (int i = -1; i <= 1; i++) {
                    Vector2 velocity = Projectile.DirectionTo(target.Center).RotatedBy(MathHelper.ToRadians(8f * i)) * 12f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                        ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(), Projectile.damage, 0f, Projectile.owner);
                }
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
        float rotation = Projectile.rotation + MathHelper.PiOver2;

        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 80, 80, 110), rotation, Vector2.One * 0.5f,
            new Vector2(18f, 18f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 220, 220, 170), rotation, Vector2.One * 0.5f,
            new Vector2(10f, 10f), SpriteEffects.None, 0);
        return false;
    }
}
