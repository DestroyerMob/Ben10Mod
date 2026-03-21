using System;
using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
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

internal static class PlumberArmorPalette {
    public static readonly Color Neutral = new(220, 225, 232);
    public static readonly Color Vanguard = new(120, 174, 224);
    public static readonly Color Scout = new(240, 198, 104);
    public static readonly Color Assault = new(215, 96, 96);
    public static readonly Color Overclock = new(255, 148, 72);
    public static readonly Color Bulwark = new(246, 229, 155);
    public static readonly Color Relay = new(118, 238, 163);
    public static readonly Color Siege = new(98, 132, 218);
    public static readonly Color Magistrata = new(247, 156, 230);

    public static Color ResolveSharedEarlySetColor(Player player) {
        if (player.armor[0].type == ModContent.ItemType<PlumbersGlassHelmet>()) {
            return Scout;
        }

        if (player.armor[0].type == ModContent.ItemType<PlumbersHelmet>()) {
            return Vanguard;
        }

        return Neutral;
    }

    public static Color Blend(Color baseColor, Color tint, float amount = 0.58f) {
        Color blended = Color.Lerp(baseColor, tint, amount);
        blended.A = baseColor.A;
        return blended;
    }

    public static bool DrawInventory(ModItem item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
        Vector2 origin, float scale, Color tint) {
        spriteBatch.Draw(TextureAssets.Item[item.Type].Value, position, frame, Blend(drawColor, tint), 0f, origin, scale,
            SpriteEffects.None, 0f);
        return false;
    }

    public static bool DrawWorld(ModItem item, SpriteBatch spriteBatch, Color alphaColor, ref float rotation, ref float scale,
        Color tint) {
        Main.GetItemDrawFrame(item.Item.type, out Texture2D itemTexture, out Rectangle itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = item.Item.Bottom - Main.screenPosition - new Vector2(0f, drawOrigin.Y);
        spriteBatch.Draw(itemTexture, drawPosition, itemFrame, Blend(alphaColor, tint), rotation, drawOrigin, scale,
            SpriteEffects.None, 0f);
        return false;
    }
}

public abstract class PlumberArmorPiece : ModItem {
    protected abstract string ArmorTexture { get; }
    protected abstract int ArmorValue { get; }
    protected abstract int ArmorRarity { get; }
    protected abstract int ArmorDefense { get; }
    protected abstract Color ArmorTint { get; }
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

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask,
        ref Color glowMaskColor) {
        color = PlumberArmorPalette.Blend(color, ArmorTint);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
        Color itemColor, Vector2 origin, float scale) {
        return PlumberArmorPalette.DrawInventory(this, spriteBatch, position, frame, drawColor, origin, scale, ArmorTint);
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation,
        ref float scale, int whoAmI) {
        return PlumberArmorPalette.DrawWorld(this, spriteBatch, alphaColor, ref rotation, ref scale, ArmorTint);
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumbersHelmet : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(silver: 90);
    protected override int ArmorRarity => ItemRarityID.White;
    protected override int ArmorDefense => 3;
    protected override Color ArmorTint => PlumberArmorPalette.Vanguard;
    protected override string EquipBonusText => "+2 defense while transformed and +4 hero armor penetration";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedDefenseBonus += 2;
        player.GetArmorPenetration<HeroDamage>() += 4;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumbersShirt>()
            && legs.type == ModContent.ItemType<PlumbersPants>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = "While transformed: +8 defense and +4% endurance. Also grants +0.6 hero knockback";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.isTransformed) {
            omp.transformedDefenseBonus += 8;
            omp.transformedEnduranceBonus += 0.04f;
        }

        player.GetKnockback<HeroDamage>() += 0.6f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 15)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 15)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumbersGlassHelmet : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.GlassHelmet;
    protected override int ArmorValue => Item.buyPrice(silver: 95);
    protected override int ArmorRarity => ItemRarityID.White;
    protected override int ArmorDefense => 2;
    protected override Color ArmorTint => PlumberArmorPalette.Scout;
    protected override string EquipBonusText => "+6 hero crit and +4% hero attack speed";

    public override void UpdateEquip(Player player) {
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetAttackSpeed<HeroDamage>() += 0.04f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumbersShirt>()
            && legs.type == ModContent.ItemType<PlumbersPants>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = "While transformed: +12% movement speed and improved jump height. Also grants +10 hero crit";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.isTransformed) {
            omp.transformedMoveSpeedBonus += 0.12f;
            omp.transformedJumpSpeedBonus += 1.6f;
        }

        player.GetCritChance<HeroDamage>() += 10f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 5)
            .AddIngredient(ItemID.Glass, 10)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 5)
            .AddIngredient(ItemID.Glass, 10)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumbersShirt : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(silver: 110);
    protected override int ArmorRarity => ItemRarityID.White;
    protected override int ArmorDefense => 4;
    protected override Color ArmorTint => PlumberArmorPalette.Neutral;
    protected override string EquipBonusText => "+4% hero damage";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.04f;
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask,
        ref Color glowMaskColor) {
        color = PlumberArmorPalette.Blend(color, PlumberArmorPalette.ResolveSharedEarlySetColor(drawPlayer));
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 25)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 25)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumbersPants : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(silver: 100);
    protected override int ArmorRarity => ItemRarityID.White;
    protected override int ArmorDefense => 3;
    protected override Color ArmorTint => PlumberArmorPalette.Neutral;
    protected override string EquipBonusText => "+5% movement speed while transformed";

    public override void UpdateEquip(Player player) {
        player.GetModPlayer<OmnitrixPlayer>().transformedMoveSpeedBonus += 0.05f;
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask,
        ref Color glowMaskColor) {
        color = PlumberArmorPalette.Blend(color, PlumberArmorPalette.ResolveSharedEarlySetColor(drawPlayer));
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 20)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 20)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberAssaultHelmet : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 1);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 6;
    protected override Color ArmorTint => PlumberArmorPalette.Assault;
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
    protected override Color ArmorTint => PlumberArmorPalette.Assault;
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
    protected override Color ArmorTint => PlumberArmorPalette.Assault;
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
    protected override Color ArmorTint => PlumberArmorPalette.Overclock;
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
    protected override Color ArmorTint => PlumberArmorPalette.Overclock;
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
    protected override Color ArmorTint => PlumberArmorPalette.Overclock;
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
    protected override Color ArmorTint => PlumberArmorPalette.Bulwark;
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
    protected override Color ArmorTint => PlumberArmorPalette.Bulwark;
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
    protected override Color ArmorTint => PlumberArmorPalette.Bulwark;
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
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 6);
    protected override int ArmorRarity => ItemRarityID.Lime;
    protected override int ArmorDefense => 11;
    protected override Color ArmorTint => PlumberArmorPalette.Relay;
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
    protected override Color ArmorTint => PlumberArmorPalette.Relay;
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
    protected override Color ArmorTint => PlumberArmorPalette.Relay;
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
    protected override Color ArmorTint => PlumberArmorPalette.Siege;
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
    protected override Color ArmorTint => PlumberArmorPalette.Siege;
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
    protected override Color ArmorTint => PlumberArmorPalette.Siege;
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
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 12);
    protected override int ArmorRarity => ItemRarityID.Red;
    protected override int ArmorDefense => 16;
    protected override Color ArmorTint => PlumberArmorPalette.Magistrata;
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
    protected override Color ArmorTint => PlumberArmorPalette.Magistrata;
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
    protected override Color ArmorTint => PlumberArmorPalette.Magistrata;
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
