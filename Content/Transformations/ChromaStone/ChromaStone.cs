using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.ChromaStone;

public class ChromaStone : ModItem {
    public static string TransformationDescription =>
        "A durable counter-caster who absorbs punishment into Radiance, stores extra energy in Prism Facets, and spends Omnitrix Energy to blast it back harder.";

    public static IReadOnlyList<string> TransformationAbilities => new[] {
        "Crystal Volley is the reliable left-click stream: fast crystal bolts pierce once, split after impact, and every fourth shot upgrades into a heavier Prism Bolt.",
        "Spectrum Beam is the Omnitrix Energy spender: a sustained piercing beam that stays clean and focused while scaling up as your Radiance climbs.",
        "Absorption Guard braces Chromastone behind a crystal shield, absorbs weaker hostile projectiles outright, softens stronger ones, refunds a little OE, and feeds both Facets and Radiance.",
        "Prismatic Lance cashes out your stored Facets in one piercing shot, scaling from a clean punish tool into a shard-bursting rainbow spear with a delayed echo hit at full power.",
        "Resonance Facets orbit visibly around Chromastone, feed bonus side-shards into Prism Bolts, strengthen the beam and lance, and can intercept one very weak projectile each.",
        "Radiance climbs when Chromastone takes hits, boosts all of his damage, and makes the prismatic disco aura on his body flare brighter and wilder at higher percentages.",
        "Full Spectrum Discharge is a true ultimate attack: once Radiance reaches 90%, swap to the ultimate slot and fire a massive channeled beam that drains Omnitrix Energy and burns down its stored discharge power while you sustain it."
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
