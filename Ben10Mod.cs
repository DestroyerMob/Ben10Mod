using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Ben10Mod
{
	public class Ben10Mod : Mod
	{
        internal const string HeatDistortFilterKey = "Ben10Mod:HeatDistort";

        public override void Load() {
            if (Main.dedServ) {
                return;
            }

            Asset<Effect> heatDistortEffect = ModContent.Request<Effect>("Ben10Mod/Assets/Effects/HeatDistort", AssetRequestMode.ImmediateLoad);

            Filters.Scene[HeatDistortFilterKey] = new Filter(
                new ScreenShaderData(heatDistortEffect, "HeatDistortPass"),
                EffectPriority.VeryHigh
            );
        }

        public override void Unload() {
            if (!Main.dedServ && Filters.Scene[HeatDistortFilterKey] != null) {
                Filters.Scene[HeatDistortFilterKey].Deactivate();
            }
        }
    }
}
