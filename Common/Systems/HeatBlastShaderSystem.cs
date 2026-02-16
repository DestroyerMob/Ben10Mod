using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Systems;

public class HeatBlastShaderSystem : ModSystem {
    public override void Load() {
        if (Main.dedServ)
            return;

        Ref<Effect> effect = new Ref<Effect>(ModContent.Request<Effect>("Ben10Mod/Assets/Effects/HeatDistort",
            ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);

        Filters.Scene["Ben10Mod:HeatDistort"] = new Filter(
            new ScreenShaderData(effect, "HeatDistortPass"),
            EffectPriority.VeryHigh);
    }

    public override void Unload() {
        if (!Main.dedServ) {
            Filters.Scene.Deactivate("Ben10Mod:HeatDistort");
            Filters.Scene["Ben10Mod:HeatDistort"] = null;
        }
    }
}