using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Rath;

public class Rath : ModItem {
    private const string CostumeTexturePath = "Ben10Mod/Content/Transformations/Rath/Rath";

    public static string TransformationDescription =>
        "An Appoplexian duelist predator who marks one prey target, tears Rend into them with claw strings, and gets more dangerous while enraged.";

    public static IReadOnlyList<string> TransformationAbilities => new[] {
        "Main attack: Savage Combo marks prey and stacks Rend with a rending finisher.",
        "Alt attack: Pounce chases marked prey and hits harder against Rend or bleeding targets.",
        "Primary ability: Battle Rage widens claws, speeds attacks, and builds Rend faster."
    };

    public override string Texture => $"Terraria/Images/Item_{ItemID.FeralClaws}";

    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        EquipLoader.AddEquipTexture(Mod, $"{CostumeTexturePath}_{EquipType.Head}", EquipType.Head, this,
            equipTexture: new RathHead());
        EquipLoader.AddEquipTexture(Mod, $"{CostumeTexturePath}_{EquipType.Body}", EquipType.Body, this);
        EquipLoader.AddEquipTexture(Mod, $"{CostumeTexturePath}_{EquipType.Legs}", EquipType.Legs, this);
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
        return !TransformationHandler.HasTransformation(player, RathStatePlayer.TransformationId);
    }

    public override bool? UseItem(Player player) {
        TransformationHandler.AddTransformation(player, RathStatePlayer.TransformationId);
        return true;
    }
}

public class RathHead : EquipTexture {
    public override bool IsVanitySet(int head, int body, int legs) => true;
}
