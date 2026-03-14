using System;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

public class HeatShimmerLayer : PlayerDrawLayer {

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        var    omp    = player.GetModPlayer<OmnitrixPlayer>();

        return omp.currentTransformationId == "Ben10Mod:HeatBlast";
    }

    // Position after armor/body
        public override Position GetDefaultPosition()
        {
            // Draw after armor layer so we overlay everything the player is wearing
            return new AfterParent(PlayerDrawLayers.ArmOverItem);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            Player player        = drawInfo.drawPlayer;
            int    originalCount = drawInfo.DrawDataCache.Count;
            if (originalCount <= 0)
                return;

            float t = (float)Main.GameUpdateCount;

            // Keep it subtle
            float baseAlpha = 0.10f; // lower = subtler
            float wobble    = 2.5f;     // pixel offset magnitude

            // Warm colors cycling
            Color c1 = new Color(255, 140, 40, 0);
            Color c2 = new Color(255, 90, 20, 0);

            // Multiple tiny offset passes for refractive feel
            Vector2[] offsets = new Vector2[]
            {
                new Vector2((float)System.Math.Sin(t * 0.18f), (float)System.Math.Cos(t * 0.15f)) * wobble,
                new Vector2((float)System.Math.Cos(t * 0.21f), (float)System.Math.Sin(t * 0.19f)) * (wobble * 0.8f),
                new Vector2((float)System.Math.Sin(t * 0.25f + 1.3f), (float)System.Math.Cos(t * 0.22f + 0.7f)) * (wobble * 0.6f),
            };

            for (int i = 0; i < originalCount; i++)
            {
                var src = drawInfo.DrawDataCache[i];

                for (int p = 0; p < offsets.Length; p++)
                {
                    var copy = src;
                    copy.position += offsets[p];

                    Color tint = Color.Lerp(c1, c2, (float)p / (offsets.Length - 1));
                    copy.color =  tint * baseAlpha;
                    copy.scale *= new Vector2(1.2f, 1.2f);

                    drawInfo.DrawDataCache.Add(copy);
                }
            }
    }
}