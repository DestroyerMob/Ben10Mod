using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.ChromaStone;

public class ChromaStone : ModItem {
    public static string TransformationDescription =>
        "A prismatic battlemage built around steady crystal fire, projectile counterplay, and a Prism Charge payoff window that supercharges the entire kit.";

    public static IReadOnlyList<string> TransformationAbilities => new[] {
        "Prism Barrage is the main attack: a rapid stream of crystal bolts that pierce once, split into shards, and keep pressure on mid-range targets.",
        "Spectrum Beam channels a focused prism ray, and active Facets refract it into extra angled beams for lined-up targets and safe boss damage.",
        "Facet Dash is a short omnidirectional burst that carries you through danger and explodes your active Facets into splinters.",
        "Refraction Guard braces in place, absorbs or softens hostile projectiles, converts that pressure into Prism Charge, and releases the stored energy forward.",
        "Resonant Facets appear at 33%, 66%, and 100% Prism Charge, adding bonus shots, blocking weak projectiles, and empowering the beam.",
        "Full Spectrum Overload spends full Prism Charge for eight seconds, restoring all Facets and supercharging Barrage, Beam, Dash, and Guard together."
    };

    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new ChromaStoneHead());
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
        return !TransformationHandler.HasTransformation(player, "Ben10Mod:ChromaStone");
    }

    public override bool? UseItem(Player player) {
        TransformationHandler.AddTransformation(player, "Ben10Mod:ChromaStone");
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

public class ChromaStoneHead : EquipTexture {
    public override bool IsVanitySet(int head, int body, int legs) => true;
}
