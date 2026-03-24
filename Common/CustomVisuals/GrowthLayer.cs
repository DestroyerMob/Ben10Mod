using Ben10Mod.Content.Transformations.Humungousaur;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

public class GrowthLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        var omp = player.GetModPlayer<OmnitrixPlayer>();

        return !player.dead &&
               !player.invis &&
               omp.CurrentTransformationScale > 1f;
    }

    public override Position GetDefaultPosition() {
        return new AfterParent(PlayerDrawLayers.ArmOverItem);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        int originalCount = drawInfo.DrawDataCache.Count;
        if (originalCount == 0)
            return;

        Player player = drawInfo.drawPlayer;
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        Vector2 pivot = player.Bottom - Main.screenPosition;
        Vector2 scale = new(omp.CurrentTransformationScale, omp.CurrentTransformationScale);

        for (int i = 0; i < originalCount; i++) {
            DrawData data = drawInfo.DrawDataCache[i];
            if (data.texture == null)
                continue;

            Vector2 originalScale = data.scale;
            Vector2 scaledSize = originalScale * scale;

            // DrawData.position is not the sprite's top-left once origin and scale are applied.
            // Scale the rendered sprite position around the feet pivot, then rebuild position
            // from the original origin so the player grows in place instead of drifting sideways.
            Vector2 renderedTopLeft = data.position - data.origin * originalScale;
            Vector2 offsetFromPivot = renderedTopLeft - pivot;
            Vector2 scaledTopLeft = pivot + offsetFromPivot * scale;

            data.position = scaledTopLeft + data.origin * scaledSize;
            data.scale = scaledSize;
            drawInfo.DrawDataCache[i] = data;
        }
    }
}
