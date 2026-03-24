using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.DiamondHead;

public class DiamondHeadTransformation : Transformation {
    public override string FullID => "Ben10Mod:DiamondHead";
    public override string TransformationName => "Diamondhead";
    public override string IconPath => "Ben10Mod/Content/Interface/DiamondHeadSelect";
    public override int TransformationBuffId => ModContent.BuffType<DiamondHead_Buff>();

    public override string Description =>
        "A durable Petrosapien that fires crystal shards, hardens into a defensive stance, and slams giant diamonds from above.";

    public override List<string> Abilities => new() {
        "Crystal shard volley",
        "Crystalline fortify stance",
        "Heavy armor penetration",
        "Falling giant diamond ultimate"
    };

    public override string PrimaryAttackName => "Crystal Shard";
    public override string UltimateAttackName => "Giant Diamond";
    public override int PrimaryAttack => ModContent.ProjectileType<DiamondHeadProjectile>();
    public override float PrimaryAttackModifier => 0.5f;
    public override int PrimaryAttackSpeed => 8;
    public override int PrimaryShootSpeed => 35;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int PrimaryArmorPenetration => 25;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 30 * 60;
    public override int PrimaryAbilityCooldown => 60 * 60;

    public override int UltimateAttack => ModContent.ProjectileType<GiantDiamondProjectile>();
    public override float UltimateAttackModifier => 5f;
    public override int UltimateAttackSpeed => 8;
    public override int UltimateShootSpeed => 35;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override int UltimateArmorPenetration => 25;
    public override int UltimateEnergyCost => 25;
    public override int UltimateAbilityCooldown => 30 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.statDefense += 20;
        player.GetArmorPenetration<HeroDamage>() += 10;
        player.GetKnockback<HeroDamage>() += 0.35f;
        player.wingTimeMax = 0;
        player.wingTime = 0;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.moveSpeed /= 10f;
        player.lifeRegen += 15;
        player.statDefense *= 1.5f;
        player.releaseJump = false;
        player.gravity *= 2f;
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        player.velocity = new Vector2(float.Clamp(player.velocity.X, -0.5f, 0.5f), System.Math.Max(0f, player.velocity.Y));
        Lighting.AddLight(player.Center, new Vector3(0.4f, 0.3f, 0.8f));
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (!omp.ultimateAttack)
            return base.Shoot(player, omp, source, position, velocity, damage, knockback);

        if (Main.netMode == NetmodeID.Server ||
            (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
            return false;

        Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, UltimateAttack, (int)(damage * UltimateAttackModifier),
            knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<DiamondHead>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        player.back = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Back);
    }

    public override IReadOnlyList<string> GetPalettePreviewBaseTexturePaths(OmnitrixPlayer omp) => new[] {
        "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Back",
        "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Legs",
        "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Body",
        "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Head"
    };
    
    public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => new[] {
        new TransformationPaletteChannel(
            "eyes",
            "Eyes",
            new Color(255, 255, 255),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Head",
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHeadEyesMask_Head")
        ),
        new TransformationPaletteChannel(
            "diamond",
            "Diamond",
            new Color(255, 255, 255),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Head",
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHeadDiamondMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Body",
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHeadDiamondMask_Body")
        ),
    };
}
