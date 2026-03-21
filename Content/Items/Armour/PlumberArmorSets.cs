using System;
using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Materials;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Armour;

internal static class PlumberArmorTextures {
    public const string Helmet = "Ben10Mod/Content/Items/Armour/PlumbersHelmet";
    public const string GlassHelmet = "Ben10Mod/Content/Items/Armour/PlumbersGlassHelmet";
    public const string Shirt = "Ben10Mod/Content/Items/Armour/PlumbersShirt";
    public const string Pants = "Ben10Mod/Content/Items/Armour/PlumbersPants";
}

public abstract class PlumberArmorPiece : ModItem {
    protected abstract string ArmorTexture { get; }
    protected abstract int ArmorValue { get; }
    protected abstract int ArmorRarity { get; }
    protected abstract int ArmorDefense { get; }
    protected virtual int ArmorWidth => 18;
    protected virtual int ArmorHeight => 14;
    protected virtual string EquipBonusText => "";

    public override string Texture => ArmorTexture;

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.width = ArmorWidth;
        Item.height = ArmorHeight;
        Item.value = ArmorValue;
        Item.rare = ArmorRarity;
        Item.defense = ArmorDefense;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        if (!string.IsNullOrWhiteSpace(EquipBonusText)) {
            tooltips.Add(new TooltipLine(Mod, "EquipBonus", EquipBonusText));
        }
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberAssaultHelmet : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.GlassHelmet;
    protected override int ArmorValue => Item.buyPrice(gold: 1);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 6;
    protected override string EquipBonusText => "+5% hero damage and +6 hero crit";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.05f;
        player.GetCritChance<HeroDamage>() += 6f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberAssaultHarness>()
            && legs.type == ModContent.ItemType<PlumberAssaultGreaves>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +10% hero damage, +12 hero armor penetration, and +10% hero attack speed while moving quickly";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.isTransformed) {
            return;
        }

        player.GetDamage<HeroDamage>() += 0.10f;
        player.GetArmorPenetration<HeroDamage>() += 12;

        if (Math.Abs(player.velocity.X) >= 3f || Math.Abs(player.velocity.Y) > 0.1f) {
            player.GetAttackSpeed<HeroDamage>() += 0.10f;
        }
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.DemoniteBar, 12)
            .AddIngredient(ItemID.MeteoriteBar, 8)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.CrimtaneBar, 12)
            .AddIngredient(ItemID.MeteoriteBar, 8)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberAssaultHarness : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 1, silver: 20);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 7;
    protected override string EquipBonusText => "+6% hero damage";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.06f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.DemoniteBar, 20)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.CrimtaneBar, 20)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberAssaultGreaves : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 1, silver: 10);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 5;
    protected override string EquipBonusText => "+6% movement speed while transformed";

    public override void UpdateEquip(Player player) {
        player.GetModPlayer<OmnitrixPlayer>().transformedMoveSpeedBonus += 0.06f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.DemoniteBar, 16)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.CrimtaneBar, 16)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberOverclockHelm : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 2);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 7;
    protected override string EquipBonusText => "+5% hero attack speed and +15 Omnitrix energy";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        player.GetAttackSpeed<HeroDamage>() += 0.05f;
        omp.omnitrixEnergyMaxBonus += 15;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberOverclockPlate>()
            && legs.type == ModContent.ItemType<PlumberOverclockGreaves>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +35 Omnitrix energy, +1 energy regen, 12% shorter primary/secondary/tertiary cooldowns, and +8% hero attack speed";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.isTransformed) {
            return;
        }

        omp.omnitrixEnergyMaxBonus += 35;
        omp.omnitrixEnergyRegenBonus += 1;
        omp.primaryAbilityCooldownMultiplier *= 0.88f;
        omp.secondaryAbilityCooldownMultiplier *= 0.88f;
        omp.tertiaryAbilityCooldownMultiplier *= 0.88f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 16)
            .AddTile(TileID.Hellforge)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberOverclockPlate : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 2, silver: 40);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 8;
    protected override string EquipBonusText => "+6% hero damage";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.06f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 24)
            .AddTile(TileID.Hellforge)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberOverclockGreaves : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 2, silver: 20);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 6;
    protected override string EquipBonusText => "+6% movement speed and acceleration while transformed";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedMoveSpeedBonus += 0.06f;
        omp.transformedRunAccelerationBonus += 0.05f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 20)
            .AddTile(TileID.Hellforge)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberBulwarkHelm : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 4);
    protected override int ArmorRarity => ItemRarityID.Pink;
    protected override int ArmorDefense => 10;
    protected override string EquipBonusText => "+3 defense while transformed and +4 hero armor penetration";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedDefenseBonus += 3;
        player.GetArmorPenetration<HeroDamage>() += 4;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberBulwarkMail>()
            && legs.type == ModContent.ItemType<PlumberBulwarkGreaves>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +10 defense and +5% endurance. Below half life, gain +12% hero damage and +10% movement speed";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.isTransformed) {
            return;
        }

        omp.transformedDefenseBonus += 10;
        omp.transformedEnduranceBonus += 0.05f;

        if (player.statLife <= player.statLifeMax2 / 2) {
            player.GetDamage<HeroDamage>() += 0.12f;
            omp.transformedMoveSpeedBonus += 0.10f;
        }
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddIngredient(ItemID.SoulofFright, 5)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberBulwarkMail : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 4, silver: 60);
    protected override int ArmorRarity => ItemRarityID.Pink;
    protected override int ArmorDefense => 12;
    protected override string EquipBonusText => "+4% hero damage and +2 defense while transformed";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        player.GetDamage<HeroDamage>() += 0.04f;
        omp.transformedDefenseBonus += 2;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 20)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberBulwarkGreaves : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 4, silver: 20);
    protected override int ArmorRarity => ItemRarityID.Pink;
    protected override int ArmorDefense => 8;
    protected override string EquipBonusText => "+5% movement speed while transformed";

    public override void UpdateEquip(Player player) {
        player.GetModPlayer<OmnitrixPlayer>().transformedMoveSpeedBonus += 0.05f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 16)
            .AddIngredient(ItemID.SoulofMight, 6)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberRelayVisor : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.GlassHelmet;
    protected override int ArmorValue => Item.buyPrice(gold: 6);
    protected override int ArmorRarity => ItemRarityID.Lime;
    protected override int ArmorDefense => 11;
    protected override string EquipBonusText => "+5% hero damage and +15 Omnitrix energy";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        player.GetDamage<HeroDamage>() += 0.05f;
        omp.omnitrixEnergyMaxBonus += 15;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberRelayCoat>()
            && legs.type == ModContent.ItemType<PlumberRelayLeggings>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +45 Omnitrix energy, +1 energy regen, 20% longer transformations, 15% shorter ultimate cooldowns, and +6% hero attack speed";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.isTransformed) {
            return;
        }

        omp.omnitrixEnergyMaxBonus += 45;
        omp.omnitrixEnergyRegenBonus += 1;
        omp.transformationDurationMultiplier *= 1.20f;
        omp.ultimateAbilityCooldownMultiplier *= 0.85f;
        player.GetAttackSpeed<HeroDamage>() += 0.06f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ChlorophyteBar, 14)
            .AddIngredient(ItemID.Wire, 20)
            .AddIngredient(ItemID.SoulofSight, 4)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberRelayCoat : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 6, silver: 40);
    protected override int ArmorRarity => ItemRarityID.Lime;
    protected override int ArmorDefense => 13;
    protected override string EquipBonusText => "+6 hero crit and +20 Omnitrix energy";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        player.GetCritChance<HeroDamage>() += 6f;
        omp.omnitrixEnergyMaxBonus += 20;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ChlorophyteBar, 24)
            .AddIngredient(ItemID.Wire, 35)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberRelayLeggings : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 6);
    protected override int ArmorRarity => ItemRarityID.Lime;
    protected override int ArmorDefense => 9;
    protected override string EquipBonusText => "+7% movement speed and improved jump height while transformed";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedMoveSpeedBonus += 0.07f;
        omp.transformedJumpSpeedBonus += 1.2f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ChlorophyteBar, 18)
            .AddIngredient(ItemID.Wire, 25)
            .AddIngredient(ItemID.SoulofSight, 6)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberSiegeMask : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 8);
    protected override int ArmorRarity => ItemRarityID.Yellow;
    protected override int ArmorDefense => 13;
    protected override string EquipBonusText => "+5% hero damage and +8 hero armor penetration";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.05f;
        player.GetArmorPenetration<HeroDamage>() += 8;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberSiegeCuirass>()
            && legs.type == ModContent.ItemType<PlumberSiegeBoots>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +15 hero crit and +0.8 hero knockback. While grounded and nearly stationary, gain +12% hero damage and +12 hero armor penetration";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.isTransformed) {
            return;
        }

        player.GetCritChance<HeroDamage>() += 15f;
        player.GetKnockback<HeroDamage>() += 0.8f;

        bool grounded = Math.Abs(player.velocity.Y) <= 0.1f;
        bool braced = Math.Abs(player.velocity.X) <= 1.25f;
        if (grounded && braced) {
            player.GetDamage<HeroDamage>() += 0.12f;
            player.GetArmorPenetration<HeroDamage>() += 12;
        }
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ShroomiteBar, 14)
            .AddIngredient(ItemID.Ectoplasm, 5)
            .AddTile(TileID.Autohammer)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberSiegeCuirass : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 8, silver: 40);
    protected override int ArmorRarity => ItemRarityID.Yellow;
    protected override int ArmorDefense => 15;
    protected override string EquipBonusText => "+8% hero damage";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.08f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ShroomiteBar, 24)
            .AddIngredient(ItemID.Ectoplasm, 8)
            .AddTile(TileID.Autohammer)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberSiegeBoots : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 8);
    protected override int ArmorRarity => ItemRarityID.Yellow;
    protected override int ArmorDefense => 11;
    protected override string EquipBonusText => "+4% hero attack speed";

    public override void UpdateEquip(Player player) {
        player.GetAttackSpeed<HeroDamage>() += 0.04f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ShroomiteBar, 18)
            .AddIngredient(ItemID.Ectoplasm, 6)
            .AddTile(TileID.Autohammer)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberMagistrataHelm : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.GlassHelmet;
    protected override int ArmorValue => Item.buyPrice(gold: 12);
    protected override int ArmorRarity => ItemRarityID.Red;
    protected override int ArmorDefense => 16;
    protected override string EquipBonusText => "+7% hero damage and +8 hero crit";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.07f;
        player.GetCritChance<HeroDamage>() += 8f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberMagistrataCoat>()
            && legs.type == ModContent.ItemType<PlumberMagistrataGreaves>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +10% hero damage, +8 defense, +5% endurance, +60 Omnitrix energy, +1 energy regen, and 10% shorter primary and ultimate cooldowns";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.isTransformed) {
            return;
        }

        player.GetDamage<HeroDamage>() += 0.10f;
        omp.transformedDefenseBonus += 8;
        omp.transformedEnduranceBonus += 0.05f;
        omp.omnitrixEnergyMaxBonus += 60;
        omp.omnitrixEnergyRegenBonus += 1;
        omp.primaryAbilityCooldownMultiplier *= 0.90f;
        omp.ultimateAbilityCooldownMultiplier *= 0.90f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.LunarBar, 12)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 8)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberMagistrataCoat : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 12, silver: 60);
    protected override int ArmorRarity => ItemRarityID.Red;
    protected override int ArmorDefense => 18;
    protected override string EquipBonusText => "+6% hero attack speed and +10 hero armor penetration";

    public override void UpdateEquip(Player player) {
        player.GetAttackSpeed<HeroDamage>() += 0.06f;
        player.GetArmorPenetration<HeroDamage>() += 10;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.LunarBar, 18)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 12)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberMagistrataGreaves : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 12);
    protected override int ArmorRarity => ItemRarityID.Red;
    protected override int ArmorDefense => 14;
    protected override string EquipBonusText => "+8% movement speed and improved jump height while transformed";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedMoveSpeedBonus += 0.08f;
        omp.transformedJumpSpeedBonus += 1.8f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.LunarBar, 14)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 10)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}
