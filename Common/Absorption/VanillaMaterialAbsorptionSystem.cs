using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Absorption;

public class VanillaMaterialAbsorptionSystem : ModSystem {
    public override void Load() {
        RegisterVanillaBars();
    }

    public override void Unload() {
        MaterialAbsorptionRegistry.Clear();
    }

    private static void RegisterVanillaBars() {
        MaterialAbsorptionRegistry.Register(ItemID.CopperBar, ItemID.CopperBroadsword, ItemID.CopperHelmet, ItemID.CopperChainmail, ItemID.CopperGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.TinBar, ItemID.TinBroadsword, ItemID.TinHelmet, ItemID.TinChainmail, ItemID.TinGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.IronBar, ItemID.IronBroadsword, ItemID.IronHelmet, ItemID.IronChainmail, ItemID.IronGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.LeadBar, ItemID.LeadBroadsword, ItemID.LeadHelmet, ItemID.LeadChainmail, ItemID.LeadGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.SilverBar, ItemID.SilverBroadsword, ItemID.SilverHelmet, ItemID.SilverChainmail, ItemID.SilverGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.TungstenBar, ItemID.TungstenBroadsword, ItemID.TungstenHelmet, ItemID.TungstenChainmail, ItemID.TungstenGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.GoldBar, ItemID.GoldBroadsword, ItemID.GoldHelmet, ItemID.GoldChainmail, ItemID.GoldGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.PlatinumBar, ItemID.PlatinumBroadsword, ItemID.PlatinumHelmet, ItemID.PlatinumChainmail, ItemID.PlatinumGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.DemoniteBar, ItemID.LightsBane, ItemID.ShadowHelmet, ItemID.ShadowScalemail, ItemID.ShadowGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.CrimtaneBar, ItemID.BloodButcherer, ItemID.CrimsonHelmet, ItemID.CrimsonScalemail, ItemID.CrimsonGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.HellstoneBar, ItemID.FieryGreatsword, ItemID.MoltenHelmet, ItemID.MoltenBreastplate, ItemID.MoltenGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.CobaltBar, ItemID.CobaltSword, ItemID.CobaltHelmet, ItemID.CobaltBreastplate, ItemID.CobaltLeggings);
        MaterialAbsorptionRegistry.Register(ItemID.PalladiumBar, ItemID.PalladiumSword, ItemID.PalladiumHelmet, ItemID.PalladiumBreastplate, ItemID.PalladiumLeggings);
        MaterialAbsorptionRegistry.Register(ItemID.MythrilBar, ItemID.MythrilSword, ItemID.MythrilHelmet, ItemID.MythrilChainmail, ItemID.MythrilGreaves);
        MaterialAbsorptionRegistry.Register(ItemID.OrichalcumBar, ItemID.OrichalcumSword, ItemID.OrichalcumHelmet, ItemID.OrichalcumBreastplate, ItemID.OrichalcumLeggings);
        MaterialAbsorptionRegistry.Register(ItemID.AdamantiteBar, ItemID.AdamantiteSword, ItemID.AdamantiteHelmet, ItemID.AdamantiteBreastplate, ItemID.AdamantiteLeggings);
        MaterialAbsorptionRegistry.Register(ItemID.TitaniumBar, ItemID.TitaniumSword, ItemID.TitaniumHelmet, ItemID.TitaniumBreastplate, ItemID.TitaniumLeggings);
        MaterialAbsorptionRegistry.Register(ItemID.ChlorophyteBar, ItemID.ChlorophyteClaymore, ItemID.ChlorophyteHelmet, ItemID.ChlorophytePlateMail, ItemID.ChlorophyteGreaves);
    }
}
