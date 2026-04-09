using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.Buffs.Debuffs;

namespace Ben10Mod.Common.Absorption;

public class VanillaMaterialAbsorptionSystem : ModSystem {
    public override void Load() {
        RegisterVanillaBars();
    }

    public override void Unload() {
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
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.DemoniteBar, ItemID.LightsBane, ItemID.ShadowHelmet, ItemID.ShadowScalemail, ItemID.ShadowGreaves)
                .AddHitBuff(BuffID.CursedInferno, 180));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.CrimtaneBar, ItemID.BloodButcherer, ItemID.CrimsonHelmet, ItemID.CrimsonScalemail, ItemID.CrimsonGreaves)
                .AddHitBuff(BuffID.Bleeding, 300));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.MeteoriteBar, ItemID.SpaceGun, ItemID.MeteorHelmet, ItemID.MeteorSuit, ItemID.MeteorLeggings)
                .AddHitBuff(BuffID.OnFire, 180));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.HellstoneBar, ItemID.FieryGreatsword, ItemID.MoltenHelmet, ItemID.MoltenBreastplate, ItemID.MoltenGreaves)
                .AddHitBuff(BuffID.OnFire, 300));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.CobaltBar, ItemID.CobaltSword, ItemID.CobaltHelmet, ItemID.CobaltBreastplate, ItemID.CobaltLeggings)
                .AddHitBuff(ModContent.BuffType<EnemyElectrocuted>(), 120));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.PalladiumBar, ItemID.PalladiumSword, ItemID.PalladiumHelmet, ItemID.PalladiumBreastplate, ItemID.PalladiumLeggings)
                .AddHitBuff(BuffID.Weak, 240));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.MythrilBar, ItemID.MythrilSword, ItemID.MythrilHelmet, ItemID.MythrilChainmail, ItemID.MythrilGreaves)
                .AddHitBuff(BuffID.Confused, 90));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.OrichalcumBar, ItemID.OrichalcumSword, ItemID.OrichalcumHelmet, ItemID.OrichalcumBreastplate, ItemID.OrichalcumLeggings)
                .AddHitBuff(BuffID.Venom, 120));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.AdamantiteBar, ItemID.AdamantiteSword, ItemID.AdamantiteHelmet, ItemID.AdamantiteBreastplate, ItemID.AdamantiteLeggings)
                .AddHitBuff(BuffID.BrokenArmor, 180));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.TitaniumBar, ItemID.TitaniumSword, ItemID.TitaniumHelmet, ItemID.TitaniumBreastplate, ItemID.TitaniumLeggings)
                .AddHitBuff(BuffID.Frostburn2, 180));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.HallowedBar, ItemID.Excalibur, ItemID.HallowedHelmet, ItemID.HallowedPlateMail, ItemID.HallowedGreaves)
                .AddHitBuff(BuffID.Ichor, 120));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.ChlorophyteBar, ItemID.ChlorophyteClaymore, ItemID.ChlorophyteHelmet, ItemID.ChlorophytePlateMail, ItemID.ChlorophyteGreaves)
                .AddHitBuff(BuffID.Poisoned, 180));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.ShroomiteBar, ItemID.Shroomerang, ItemID.ShroomiteHelmet, ItemID.ShroomiteBreastplate, ItemID.ShroomiteLeggings)
                .AddHitBuff(BuffID.Slow, 180));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.SpectreBar, ItemID.SpectreStaff, ItemID.SpectreMask, ItemID.SpectreRobe, ItemID.SpectrePants)
                .AddHitBuff(BuffID.ShadowFlame, 180));
        MaterialAbsorptionRegistry.Register(
            MaterialAbsorptionRegistry.CreateRegistration(ItemID.LunarBar, ItemID.SolarEruption, ItemID.SolarFlareHelmet, ItemID.SolarFlareBreastplate, ItemID.SolarFlareLeggings)
                .AddHitBuff(BuffID.Daybreak, 240));
    }
}
