using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

public class GoopSquishLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.dead || player.invis)
            return false;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        return omp.currentTransformationId == "Ben10Mod:Goop" &&
               Vector2.DistanceSquared(omp.GoopVisualScale, Vector2.One) > 0.0001f;
    }

    public override Position GetDefaultPosition() {
        return new AfterParent(PlayerDrawLayers.ArmOverItem);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        Vector2 scale = omp.GoopVisualScale;
        if (Vector2.DistanceSquared(scale, Vector2.One) <= 0.0001f)
            return;

        Vector2 pivot = player.Bottom - Main.screenPosition;
        var heldItemTexture = !player.HeldItem.IsAir ? TextureAssets.Item[player.HeldItem.type].Value : null;

        for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
            DrawData data = drawInfo.DrawDataCache[i];
            if (data.texture == null)
                continue;

            if (heldItemTexture != null && data.texture == heldItemTexture)
                continue;

            Vector2 originalScale = data.scale;
            Vector2 scaledScale = new(originalScale.X * scale.X, originalScale.Y * scale.Y);
            Vector2 renderedTopLeft = data.position - data.origin * originalScale;
            Vector2 offsetFromPivot = renderedTopLeft - pivot;
            Vector2 scaledTopLeft = pivot + new Vector2(offsetFromPivot.X * scale.X, offsetFromPivot.Y * scale.Y);

            data.position = scaledTopLeft + data.origin * scaledScale;
            data.scale = scaledScale;
            drawInfo.DrawDataCache[i] = data;
        }
    }
}
