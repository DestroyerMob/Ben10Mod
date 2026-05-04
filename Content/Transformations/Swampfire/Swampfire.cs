using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Swampfire;

public class Swampfire : ModItem {
    private const string HeadTexturePath = "Ben10Mod/Content/Transformations/Swampfire/Swampfire_head";
    private const string BodyTexturePath = "Ben10Mod/Content/Transformations/Swampfire/Swampfire_body";
    private const string LegsTexturePath = "Ben10Mod/Content/Transformations/Swampfire/Swampfire_legs";

    public override string Texture => "Ben10Mod/Content/Interface/EmptyAlien";

    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        EquipLoader.AddEquipTexture(Mod, HeadTexturePath, EquipType.Head, this, equipTexture: new SwampfireHead());
        EquipLoader.AddEquipTexture(Mod, BodyTexturePath, EquipType.Body, this);
        EquipLoader.AddEquipTexture(Mod, LegsTexturePath, EquipType.Legs, this);
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
        return !TransformationHandler.HasTransformation(player, "Ben10Mod:Swampfire");
    }

    public override bool? UseItem(Player player) {
        TransformationHandler.AddTransformation(player, "Ben10Mod:Swampfire");
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

public class SwampfireHead : EquipTexture {
    public override bool IsVanitySet(int head, int body, int legs) => true;
}
