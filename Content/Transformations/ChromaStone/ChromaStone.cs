using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.ChromaStone;

public class ChromaStone : ModItem {
    public static string TransformationDescription =>
        "An absorption battery and beam artillery form. Chromastone turns damage, guarded hits, and absorbed projectiles into Radiance and Prism Facets for stronger beams and lances.";

    public static IReadOnlyList<string> TransformationAbilities => new[] {
        "Radiance rises when Chromastone takes damage, absorbs hostile projectiles, or guards through attacks, then boosts all Hero damage.",
        "Crystal Volley builds Facets through repeated hits. Every fourth volley shot becomes a Prism Bolt, and stored Facets add extra shards to those Prism Bolts.",
        "Spectrum Beam is an OE channel that scales with Radiance and Facets, spending stored Facets over time for a stronger beam.",
        "Absorption Guard roots Chromastone in place, absorbs weaker hostile projectiles outright, softens stronger ones, refunds a little OE, and rapidly builds Radiance and Facets.",
        "Prismatic Lance spends stored Facets on one piercing shot, growing from a clean punish into a shard-bursting spear with a delayed echo hit at full power.",
        "Resonance Facets orbit visibly around Chromastone, strengthen beam and lance output, add Prism Bolt side-shards, and can intercept one very weak projectile each.",
        "Full Spectrum Discharge becomes ready at 90% Radiance. Firing it spends Radiance and Facets into a massive channeled beam that drains OE while sustained."
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
