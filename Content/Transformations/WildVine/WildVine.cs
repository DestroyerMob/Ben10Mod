using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WildVine;

public class WildVine : ModItem {
    public static string TransformationDescription =>
        "A flexible plant-form control fighter. Wildvine mixes thorn lashes, toxic blooms, a mobility vine grapple, and a briar snare for battlefield control.";

    public static IReadOnlyList<string> TransformationAbilities => new[] {
        "Main attack: thorn whip that poisons on hit.",
        "Alt attack: lob a gas seed that blooms into a lingering toxic cloud.",
        "Primary ability: Vine Grapple for quick repositioning.",
        "Secondary ability: Briar Snare that hooks smaller enemies and drags them toward you.",
        "Ultimate attack: Verdant Bloom, a barrage of bloom seeds that saturates an area with larger toxic clouds."
    };

    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new XLR8Head());
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
    }

    public override void SetStaticDefaults() {
        SetupDrawing();
    }

    public override void SetDefaults() {
        Item.width = 40;
        Item.height = 80;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.HiddenAnimation;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player) {
        return !TransformationHandler.HasTransformation(player, "Ben10Mod:WildVine");
    }

    public override bool? UseItem(Player player) {
        TransformationHandler.AddTransformation(player, "Ben10Mod:WildVine");
        return true;
    }

    private void SetupDrawing() {
        if (Main.netMode == NetmodeID.Server)
            return;

        int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
        int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
        int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

        ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
        ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
        ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
        ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
    }
}

public class XLR8Head : EquipTexture {
    public override bool IsVanitySet(int head, int body, int legs) => true;
}
