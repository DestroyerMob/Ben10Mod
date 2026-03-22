using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

public class NRGHeatUpLayer : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.dead || player.invis)
            return false;

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        return omp.currentTransformationId == "Ben10Mod:NRG" &&
               (omp.IsPrimaryAbilityAttackLoaded || omp.IsUltimateAbilityActive);
    }

    public override Position GetDefaultPosition() {
        return new AfterParent(PlayerDrawLayers.ArmOverItem);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != "Ben10Mod:NRG")
            return;

        bool heatBurstLoaded = omp.IsPrimaryAbilityAttackLoaded;
        bool unboundCoreActive = omp.IsUltimateAbilityActive;
        if (!heatBurstLoaded && !unboundCoreActive)
            return;

        int originalCount = drawInfo.DrawDataCache.Count;
        if (originalCount == 0)
            return;

        float pulse = 0.94f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * (unboundCoreActive ? 11f : 9f)) * 0.16f;
        float surge = 1f + omp.AttackSelectionPulseProgress * 0.45f + (unboundCoreActive ? 0.18f : 0f);
        Color innerColor = unboundCoreActive ? new Color(255, 120, 70, 0) : new Color(255, 85, 45, 0);
        Color outerColor = unboundCoreActive ? new Color(255, 205, 120, 0) : new Color(255, 170, 85, 0);
        float innerAlpha = unboundCoreActive ? 132f : 95f;
        float outerAlpha = unboundCoreActive ? 96f : 62f;
        float shellScale = unboundCoreActive ? 1.13f : 1.08f;
        Vector2[] glowOffsets = {
            new(-2f, 0f),
            new(2f, 0f),
            new(0f, -2f),
            new(0f, 2f),
            new(-2f, -2f),
            new(2f, -2f),
            new(-2f, 2f),
            new(2f, 2f)
        };

        for (int i = 0; i < originalCount; i++) {
            DrawData data = drawInfo.DrawDataCache[i];
            if (data.texture == null || data.color.A == 0)
                continue;

            float alphaScale = data.color.A / 255f;

            for (int j = 0; j < glowOffsets.Length; j++) {
                DrawData glowCopy = data;
                glowCopy.position += glowOffsets[j] * surge;
                glowCopy.scale *= new Vector2(1.03f, 1.03f);
                glowCopy.color = new Color(
                    (byte)MathHelper.Clamp(innerColor.R * pulse, 0f, 255f),
                    (byte)MathHelper.Clamp(innerColor.G * pulse, 0f, 255f),
                    (byte)MathHelper.Clamp(innerColor.B * pulse, 0f, 255f),
                    (byte)MathHelper.Clamp(innerAlpha * alphaScale, 0f, 255f)
                );
                drawInfo.DrawDataCache.Add(glowCopy);
            }

            DrawData shellCopy = data;
            shellCopy.scale *= new Vector2(shellScale, shellScale);
            shellCopy.color = new Color(
                (byte)MathHelper.Clamp(outerColor.R * pulse, 0f, 255f),
                (byte)MathHelper.Clamp(outerColor.G * pulse, 0f, 255f),
                (byte)MathHelper.Clamp(outerColor.B * pulse, 0f, 255f),
                (byte)MathHelper.Clamp(outerAlpha * alphaScale, 0f, 255f)
            );
            drawInfo.DrawDataCache.Add(shellCopy);
        }
    }
}
