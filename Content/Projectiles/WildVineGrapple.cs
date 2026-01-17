using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles
{
    public class WildVineGrapple : ModProjectile
    {
        public override string Texture => "Ben10Mod/Content/Projectiles/WildVineProjectile";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.SingleGrappleHook[Type] = true;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.AmberHook);
            Projectile.width = 18;
            Projectile.height = 18;
        }

        public override bool? CanHitNPC(NPC target) => false;

        public override bool? CanUseGrapple(Player player)
        {
            int hooksOut = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == Type)
                    hooksOut++;
            }
            return hooksOut < 1;
        }

        public override void NumGrappleHooks(Player player, ref int numHooks) => numHooks = 1;

        public override float GrappleRange() => 360f;

        public override void GrappleRetreatSpeed(Player player, ref float speed) => speed = 20f;

        public override void GrapplePullSpeed(Player player, ref float speed) => speed = 12.5f;

        public override bool PreDrawExtras()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
                return false;

            Texture2D chainTex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;

            Vector2 start = player.MountedCenter;
            Vector2 end = Projectile.Center;

            Vector2 dir = end - start;
            float length = dir.Length();
            if (length < 8f)
                return false;

            dir /= length;

            float segmentLen = chainTex.Height;
            float rotation = dir.ToRotation() - MathHelper.PiOver2;

            for (float i = 0; i <= length; i += segmentLen)
            {
                Vector2 pos = start + dir * i;
                Color c = Lighting.GetColor((int)(pos.X / 16f), (int)(pos.Y / 16f));

                Main.EntitySpriteDraw(
                    chainTex,
                    pos - Main.screenPosition,
                    null,
                    c,
                    rotation,
                    new Vector2(chainTex.Width / 2f, chainTex.Height / 2f),
                    1f,
                    SpriteEffects.None,
                    0
                );
            }

            return false;
        }
    }
}
