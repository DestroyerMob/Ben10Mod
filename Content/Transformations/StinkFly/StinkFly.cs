using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.StinkFly;

public class StinkFly : ModItem {
    public static string TransformationDescription =>
        "A flying harassment alien that controls space with sticky slime, corrosive spit, and a bursting finisher from the air.";

    public static IReadOnlyList<string> TransformationAbilities => new[] {
        "Passive: built-in wings while transformed.",
        "Main attack: sticky slime glob that slows enemies.",
        "Alt attack: corrosive spit that poisons and pierces.",
        "Ultimate attack: corrosive barrage shot that bursts into status droplets.",
        "Role: aerial control and safe ranged pressure."
    };

    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new StinkFlyHead());
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Waist}", EquipType.Waist, this);
    }

    private void SetupDrawing() {
        if (Main.netMode == NetmodeID.Server)
            return;

        int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
        int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
        int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
        int equipSlotWaist = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Waist);

        ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
        ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
        ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
        ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
        ArmorIDs.Waist.Sets.UsesTorsoFraming[equipSlotWaist] = true;
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
        return !TransformationHandler.HasTransformation(player, "Ben10Mod:StinkFly");
    }

    public override bool? UseItem(Player player) {
        TransformationHandler.AddTransformation(player, "Ben10Mod:StinkFly");
        return true;
    }
}

public class StinkFlyHead : EquipTexture {
    public override bool IsVanitySet(int head, int body, int legs) => true;
}
