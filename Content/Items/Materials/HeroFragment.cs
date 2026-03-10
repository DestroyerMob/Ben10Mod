using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Materials;

public class HeroFragment : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ItemNoGravity[Type] = true;
        ItemID.Sets.ItemIconPulse[Type] = true;
        Item.ResearchUnlockCount        = 25;
    }

    public override void SetDefaults() {
        Item.width    = 24;
        Item.height   = 24;
        Item.maxStack = Item.CommonMaxStack;
        Item.value    = Item.buyPrice(gold: 1);
        Item.rare = ItemRarityID.Blue;
    }
    
    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, Color.LimeGreen.ToVector3() * 0.6f * Main.essScale);
    }
}