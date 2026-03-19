using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

public class MaterialAbsorptionLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.dead || player.invis)
            return false;

        return player.GetModPlayer<OmnitrixPlayer>().TryGetActiveAbsorptionProfile(out _);
    }

    public override Position GetDefaultPosition() {
        return new AfterParent(PlayerDrawLayers.ArmOverItem);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.TryGetActiveAbsorptionProfile(out var profile))
            return;

        int originalCount = drawInfo.DrawDataCache.Count;
        if (originalCount == 0)
            return;

        float pulse = 0.88f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.12f;
        Color glowColor = Color.Lerp(profile.TintColor, Color.White, 0.12f);
        Color shellColor = Color.Lerp(profile.TintColor, Color.White, 0.28f);
        Vector2[] glowOffsets = {
            new(-2f, 0f),
            new(2f, 0f),
            new(0f, -2f),
            new(0f, 2f),
            new(-1.5f, -1.5f),
            new(1.5f, -1.5f),
            new(-1.5f, 1.5f),
            new(1.5f, 1.5f)
        };

        for (int i = 0; i < originalCount; i++) {
            DrawData data = drawInfo.DrawDataCache[i];
            if (data.texture == null || data.color.A == 0)
                continue;

            float alphaScale = data.color.A / 255f;

            for (int j = 0; j < glowOffsets.Length; j++) {
                DrawData glowCopy = data;
                glowCopy.position += glowOffsets[j];
                glowCopy.scale *= new Vector2(1.03f, 1.03f);
                glowCopy.color = new Color(
                    (byte)MathHelper.Clamp(glowColor.R * pulse, 0f, 255f),
                    (byte)MathHelper.Clamp(glowColor.G * pulse, 0f, 255f),
                    (byte)MathHelper.Clamp(glowColor.B * pulse, 0f, 255f),
                    (byte)MathHelper.Clamp(110f * alphaScale, 0f, 255f)
                );
                drawInfo.DrawDataCache.Add(glowCopy);
            }

            DrawData shellCopy = data;
            shellCopy.scale *= new Vector2(1.015f, 1.015f);
            shellCopy.color = new Color(
                (byte)MathHelper.Clamp(shellColor.R * pulse, 0f, 255f),
                (byte)MathHelper.Clamp(shellColor.G * pulse, 0f, 255f),
                (byte)MathHelper.Clamp(shellColor.B * pulse, 0f, 255f),
                (byte)MathHelper.Clamp(185f * alphaScale, 0f, 255f)
            );
            drawInfo.DrawDataCache.Add(shellCopy);
        }
    }
}
