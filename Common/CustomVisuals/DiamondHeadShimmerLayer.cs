using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

public class DiamondHeadShimmerLayer : PlayerDrawLayer {

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        var    omp    = player.GetModPlayer<OmnitrixPlayer>();

        return omp.currentTransformationId == "Ben10Mod:DiamondHead" && omp.PrimaryAbilityEnabled;
    }

    // Position after armor/body
        public override Position GetDefaultPosition()
        {
            // Draw after armor layer so we overlay everything the player is wearing
            return new AfterParent(PlayerDrawLayers.ArmOverItem);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;

            // If our player is dead or invisible, skip
            if (player.dead || player.invis)
                return;

            // How many existing draw entries there are; we only clone the originals
            int originalCount = drawInfo.DrawDataCache.Count;
            if (originalCount == 0)
                return;

            // Pulsing alpha between 0.3 and 0.5
            float pulse = (float)((System.Math.Sin(Main.GameUpdateCount / 15f) + 1f) * 0.5f); // 0..1
            float alpha = MathHelper.Lerp(0.3f, 0.5f, pulse);

            // Rainbow shimmer color
            Color baseColor = Color.White;
            Color rainbow = Main.DiscoColor; // built-in cycling rainbow
            Color shimmerColor = Color.Lerp(baseColor, rainbow, 0.75f) * alpha;

            // Slight random offset (1–2 px in a random direction each frame)
            Vector2 jitter = new Vector2(
                Main.rand.NextFloat(-2f, 2f),
                Main.rand.NextFloat(-2f, 2f)
            );

            // Clone each original DrawData and add a tinted copy with small offset
            for (int i = 0; i < originalCount; i++)
            {
                var data = drawInfo.DrawDataCache[i];

                // Optional: skip shadows, or stuff not attached to the player
                // (Here we just clone everything; you can add filters if needed)

                var copy = data;
                copy.position += jitter;

                // Multiply color by our shimmer; we also respect original alpha
                // by combining them.
                Color originalColor = data.color;
                // Combine original color and shimmer; you can simplify this if you want
                Color combined = new Color(
                    (byte)(originalColor.R * alpha + shimmerColor.R * (1f - alpha)),
                    (byte)(originalColor.G * alpha + shimmerColor.G * (1f - alpha)),
                    (byte)(originalColor.B * alpha + shimmerColor.B * (1f - alpha)),
                    (byte)(originalColor.A * alpha)
                );

                copy.color = combined;

                // Add the draw to the cache so tML draws it after the original
                drawInfo.DrawDataCache.Add(copy);
            }
        
    }
}