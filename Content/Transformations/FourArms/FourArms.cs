using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.FourArms;

public class FourArms : ModItem {
    public static string TransformationDescription =>
        "A Tetramand melee bruiser built around gauntlet-range punches, shockwave claps, and slams. Four Arms snowballs through Rage, then cashes it out in Berserker mode.";

    public static IReadOnlyList<string> TransformationAbilities => new[] {
        "Main attack: Titan Combo chains fast fists and ends in a wide cleaving third hit.",
        "Alt attack: Shock Clap is a short-range crowd-control shockwave that hits through enemies.",
        "Primary ability: Ground Slam triggers from F or a double tap down, then crashes into the floor with a shockwave.",
        "Secondary ability: Haymaker is a hold-to-charge punch with super armor and heavy single-target damage.",
        "Passive: Rage builds from dealing or taking punishment and slightly boosts attack speed.",
        "Ultimate ability: Berserker activates once Rage reaches 90% for faster combos, larger fists, and fissure slams."
    };

    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new FourArmsHead());
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
    }

    private void SetupDrawing() {
        if (Main.netMode == NetmodeID.Server)
            return;

        int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
        int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
        int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

        ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
        ArmorIDs.Head.Sets.IsTallHat[equipSlotHead] = true;
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
        return !TransformationHandler.HasTransformation(player, "Ben10Mod:FourArms");
    }

    public override bool? UseItem(Player player) {
        TransformationHandler.AddTransformation(player, "Ben10Mod:FourArms");
        return true;
    }
}

public class FourArmsHead : EquipTexture {
    public override bool IsVanitySet(int head, int body, int legs) => true;
}
