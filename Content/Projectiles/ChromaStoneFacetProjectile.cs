using Ben10Mod.Content.Transformations.ChromaStone;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneFacetProjectile : ModProjectile {
    private int FacetSlot => (int)Projectile.ai[0];

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.timeLeft = 2;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != ChromaStoneStatePlayer.TransformationId) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        ChromaStoneStatePlayer state = owner.GetModPlayer<ChromaStoneStatePlayer>();
        Projectile.Center = owner.Center + state.GetFacetWorldOffset(FacetSlot);
        Projectile.rotation += 0.08f + FacetSlot * 0.01f;
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        ChromaStoneStatePlayer state = owner.GetModPlayer<ChromaStoneStatePlayer>();
        if (!state.IsFacetVisible(FacetSlot))
            return false;

        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(FacetSlot * 0.5f + state.FacetPowerRatio * 1.8f, 1.02f) * 0.88f;
        Color inner = new Color(245, 250, 255, 225);

        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, Projectile.rotation, new Vector2(16f, 16f), outer * 0.52f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, Projectile.rotation + MathHelper.PiOver4, new Vector2(12f, 12f), outer * 0.72f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, Projectile.rotation, new Vector2(8f, 8f), inner);
        return false;
    }
}
