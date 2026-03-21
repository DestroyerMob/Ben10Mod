using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Armour;

public class HeroVanillaArmorPlayer : ModPlayer {
    private static DamageClass HeroClass => ModContent.GetInstance<HeroDamage>();

    public bool orichalcumSet;
    public bool spectreSet;

    public int orichalcumPetalCooldown;
    public int spectreEchoCooldown;

    public override void ResetEffects() {
        orichalcumSet = false;
        spectreSet = false;
    }

    public override void PostUpdate() {
        if (orichalcumPetalCooldown > 0)
            orichalcumPetalCooldown--;
        if (spectreEchoCooldown > 0)
            spectreEchoCooldown--;
    }

    public override void PostHurt(Player.HurtInfo info) {
        base.PostHurt(info);
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
        if (item.DamageType == HeroClass)
            ApplyHeroHitEffects(target, damageDone);

        base.OnHitNPCWithItem(item, target, hit, damageDone);
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
        if (proj.DamageType == HeroClass)
            ApplyHeroHitEffects(target, damageDone);

        base.OnHitNPCWithProj(proj, target, hit, damageDone);
    }

    private void ApplyHeroHitEffects(NPC target, int damageDone) {
        if (orichalcumSet && orichalcumPetalCooldown <= 0 && Player.whoAmI == Main.myPlayer) {
            SpawnOrichalcumPetals(target, Math.Max(1, damageDone / 4));
            orichalcumPetalCooldown = 24;
        }

        if (spectreSet) {
            if (spectreEchoCooldown <= 0) {
                TriggerSpectreEcho(target, Math.Max(1, damageDone / 3));
                spectreEchoCooldown = 45;
            }
        }
    }

    private void SpawnOrichalcumPetals(NPC target, int damage) {
        for (int i = 0; i < 3; i++) {
            Vector2 spawnPosition = target.Center + new Vector2(Main.rand.NextFloat(-120f, 120f), -Main.rand.NextFloat(120f, 180f));
            Vector2 velocity = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(5f, 7f);
            int projectileIndex = Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPosition, velocity,
                ProjectileID.FlowerPetal, damage, 0f, Player.whoAmI);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.friendly = true;
                projectile.hostile = false;
                projectile.DamageType = HeroClass;
            }
        }
    }

    private void TriggerSpectreEcho(NPC primaryTarget, int damage) {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        NPC echoTarget = FindNearestTarget(320f, primaryTarget.whoAmI);
        if (echoTarget == null)
            return;

        echoTarget.SimpleStrikeNPC(damage, Player.direction, false, 0f, HeroClass);

        for (int i = 0; i < 12; i++) {
            Vector2 position = Vector2.Lerp(primaryTarget.Center, echoTarget.Center, i / 11f);
            Dust dust = Dust.NewDustPerfect(position, DustID.SpectreStaff, Vector2.Zero, 100, default, 1.05f);
            dust.noGravity = true;
        }
    }

    private NPC FindNearestTarget(float maxDistance, int excludedNpc = -1) {
        NPC chosenTarget = null;
        float bestDistance = maxDistance;

        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.whoAmI == excludedNpc || !npc.CanBeChasedBy())
                continue;

            float distance = Vector2.Distance(Player.Center, npc.Center);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            chosenTarget = npc;
        }

        return chosenTarget;
    }
}

public abstract class VanillaHeroHelmet : PlumberArmorPiece {
    protected sealed override string ArmorTexture => PlumberArmorTextures.Helmet;

    protected abstract string SetBonusText { get; }

    protected static bool MatchesAny(int type, params int[] candidates) {
        for (int i = 0; i < candidates.Length; i++) {
            if (type == candidates[i])
                return true;
        }

        return false;
    }

    protected abstract bool MatchesBody(Item body);
    protected abstract bool MatchesLegs(Item legs);
    protected abstract void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap);

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return MatchesBody(body) && MatchesLegs(legs);
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = SetBonusText;
        ApplySetBonus(player, player.GetModPlayer<OmnitrixPlayer>(), player.GetModPlayer<HeroVanillaArmorPlayer>());
    }
}

[AutoloadEquip(EquipType.Head)]
public class CobaltHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 1, silver: 20);
    protected override int ArmorRarity => ItemRarityID.LightRed;
    protected override int ArmorDefense => 4;
    protected override Color ArmorTint => new(92, 152, 238);
    protected override string EquipBonusText => "+8% hero damage and +4 hero crit";
    protected override string SetBonusText => "+15% hero attack speed";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetCritChance<HeroDamage>() += 4f;
    }

    protected override bool MatchesBody(Item body) =>
        MatchesAny(body.type, ItemID.CobaltBreastplate, ItemID.AncientCobaltBreastplate);

    protected override bool MatchesLegs(Item legs) =>
        MatchesAny(legs.type, ItemID.CobaltLeggings, ItemID.AncientCobaltLeggings);

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        player.GetAttackSpeed<HeroDamage>() += 0.15f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.CobaltBar, 10)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PalladiumHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 1, silver: 30);
    protected override int ArmorRarity => ItemRarityID.LightRed;
    protected override int ArmorDefense => 5;
    protected override Color ArmorTint => new(232, 170, 110);
    protected override string EquipBonusText => "+8% hero damage and +6 hero crit";
    protected override string SetBonusText => "Greatly increases life regeneration after striking an enemy";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetCritChance<HeroDamage>() += 6f;
    }

    protected override bool MatchesBody(Item body) => body.type == ItemID.PalladiumBreastplate;
    protected override bool MatchesLegs(Item legs) => legs.type == ItemID.PalladiumLeggings;

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        player.palladiumRegen = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.PalladiumBar, 10)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class MythrilHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 1, silver: 80);
    protected override int ArmorRarity => ItemRarityID.LightRed;
    protected override int ArmorDefense => 7;
    protected override Color ArmorTint => new(86, 212, 220);
    protected override string EquipBonusText => "+10% hero damage and +6 hero crit";
    protected override string SetBonusText => "+10 hero crit";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.10f;
        player.GetCritChance<HeroDamage>() += 6f;
    }

    protected override bool MatchesBody(Item body) => body.type == ItemID.MythrilChainmail;
    protected override bool MatchesLegs(Item legs) => legs.type == ItemID.MythrilGreaves;

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        player.GetCritChance<HeroDamage>() += 10f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.MythrilBar, 12)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class OrichalcumHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 1, silver: 80);
    protected override int ArmorRarity => ItemRarityID.LightRed;
    protected override int ArmorDefense => 7;
    protected override Color ArmorTint => new(241, 122, 182);
    protected override string EquipBonusText => "+8% hero damage and +10 hero crit";
    protected override string SetBonusText => "Flower petals will fall on your target for extra damage";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetCritChance<HeroDamage>() += 10f;
    }

    protected override bool MatchesBody(Item body) => body.type == ItemID.OrichalcumBreastplate;
    protected override bool MatchesLegs(Item legs) => legs.type == ItemID.OrichalcumLeggings;

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        hvap.orichalcumSet = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.OrichalcumBar, 12)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class AdamantiteHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 2, silver: 30);
    protected override int ArmorRarity => ItemRarityID.Pink;
    protected override int ArmorDefense => 10;
    protected override Color ArmorTint => new(228, 101, 92);
    protected override string EquipBonusText => "+12% hero damage and +5 hero crit";
    protected override string SetBonusText => "+20% hero attack speed and movement speed";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 5f;
    }

    protected override bool MatchesBody(Item body) => body.type == ItemID.AdamantiteBreastplate;
    protected override bool MatchesLegs(Item legs) => legs.type == ItemID.AdamantiteLeggings;

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        player.GetAttackSpeed<HeroDamage>() += 0.20f;
        player.moveSpeed += 0.20f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.AdamantiteBar, 12)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class TitaniumHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 2, silver: 40);
    protected override int ArmorRarity => ItemRarityID.Pink;
    protected override int ArmorDefense => 10;
    protected override Color ArmorTint => new(181, 197, 221);
    protected override string EquipBonusText => "+10% hero damage and +6 hero crit";
    protected override string SetBonusText => "Attacking generates a defensive barrier of titanium shards";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.10f;
        player.GetCritChance<HeroDamage>() += 6f;
    }

    protected override bool MatchesBody(Item body) => body.type == ItemID.TitaniumBreastplate;
    protected override bool MatchesLegs(Item legs) => legs.type == ItemID.TitaniumLeggings;

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        player.onHitTitaniumStorm = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.TitaniumBar, 12)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class HallowedHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 4, silver: 20);
    protected override int ArmorRarity => ItemRarityID.Pink;
    protected override int ArmorDefense => 13;
    protected override Color ArmorTint => new(251, 240, 172);
    protected override string EquipBonusText => "+12% hero damage and +8 hero crit";
    protected override string SetBonusText => "Become immune after striking an enemy";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 8f;
    }

    protected override bool MatchesBody(Item body) =>
        MatchesAny(body.type, ItemID.HallowedPlateMail, ItemID.AncientHallowedPlateMail);

    protected override bool MatchesLegs(Item legs) =>
        MatchesAny(legs.type, ItemID.HallowedGreaves, ItemID.AncientHallowedGreaves);

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        player.onHitDodge = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class ChlorophyteHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 6, silver: 20);
    protected override int ArmorRarity => ItemRarityID.Lime;
    protected override int ArmorDefense => 15;
    protected override Color ArmorTint => new(116, 205, 102);
    protected override string EquipBonusText => "+12% hero damage and +8 hero armor penetration";
    protected override string SetBonusText =>
        "Summons a powerful leaf crystal to shoot at nearby enemies\nReduces damage taken by 5%";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetArmorPenetration<HeroDamage>() += 8;
    }

    protected override bool MatchesBody(Item body) => body.type == ItemID.ChlorophytePlateMail;
    protected override bool MatchesLegs(Item legs) => legs.type == ItemID.ChlorophyteGreaves;

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        player.AddBuff(BuffID.LeafCrystal, 2);
        player.endurance += 0.05f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ChlorophyteBar, 12)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class ShroomiteHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 7, silver: 50);
    protected override int ArmorRarity => ItemRarityID.Yellow;
    protected override int ArmorDefense => 15;
    protected override Color ArmorTint => new(108, 122, 229);
    protected override string EquipBonusText => "+12% hero damage and +10 hero crit";
    protected override string SetBonusText =>
        "Not moving puts you in stealth,\nincreasing hero damage and reducing chance for enemies to target you";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 10f;
    }

    protected override bool MatchesBody(Item body) => body.type == ItemID.ShroomiteBreastplate;
    protected override bool MatchesLegs(Item legs) => legs.type == ItemID.ShroomiteLeggings;

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        player.shroomiteStealth = true;

        bool stationary = Math.Abs(player.velocity.X) <= 1f && Math.Abs(player.velocity.Y) <= 0.1f;
        if (!stationary)
            return;

        player.GetDamage<HeroDamage>() += 0.15f;
        player.GetCritChance<HeroDamage>() += 5f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ShroomiteBar, 12)
            .AddTile(TileID.Autohammer)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class SpectreHeroHelmet : VanillaHeroHelmet {
    protected override int ArmorValue => Item.buyPrice(gold: 7, silver: 50);
    protected override int ArmorRarity => ItemRarityID.Yellow;
    protected override int ArmorDefense => 11;
    protected override Color ArmorTint => new(150, 236, 255);
    protected override string EquipBonusText => "+10% hero damage and +6 hero crit";
    protected override string SetBonusText =>
        "Hero damage done will hurt extra nearby enemies";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.10f;
        player.GetCritChance<HeroDamage>() += 6f;
    }

    protected override bool MatchesBody(Item body) => body.type == ItemID.SpectreRobe;
    protected override bool MatchesLegs(Item legs) => legs.type == ItemID.SpectrePants;

    protected override void ApplySetBonus(Player player, OmnitrixPlayer omp, HeroVanillaArmorPlayer hvap) {
        hvap.spectreSet = true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.SpectreBar, 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
