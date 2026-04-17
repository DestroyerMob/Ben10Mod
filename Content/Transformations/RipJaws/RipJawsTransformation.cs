using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.RipJaws;

public class RipJawsTransformation : Transformation {
    private const float WaterDamageMultiplier = 1.5f;
    private const float RainDamageMultiplier = 1.25f;
    private const float HydroRushDamageMultiplier = 1.38f;
    private const float WaterMoveSpeedMultiplier = 2.25f;
    private const float RainMoveSpeedMultiplier = 1.35f;
    private const float HydroRushMoveSpeedMultiplier = 1.8f;
    private const float WaterFallSpeedMultiplier = 1.5f;
    private const float RainFallSpeedMultiplier = 1.15f;
    private const float HydroRushFallSpeedMultiplier = 1.28f;
    private const int LandBreathDrainInterval = 40;
    private const int UnderworldBreathDrainInterval = 16;
    private const int LandBreathDrainAmount = 1;
    private const int UnderworldBreathDrainAmount = 1;
    private const int HydroRushDuration = 10 * 60;
    private const int HydroRushCooldown = 18 * 60;
    private const int HydroRushCost = 14;

    public override string FullID => "Ben10Mod:RipJaws";
    public override string TransformationName => "Ripjaws";
    public override string IconPath => "Ben10Mod/Content/Interface/RipJawsSelect";
    public override int TransformationBuffId => ModContent.BuffType<RipJaws_Buff>();

    public override string Description =>
        "An aquatic predator that dominates in water, suffers on land, and lunges with devastating bites.";

    public override List<string> Abilities => new() {
        "Aquatic rush",
        "Heavy bite lunge",
        "Hydro Rush for a land-combat power spike",
        "Water mobility",
        "Amphibious survival pressure"
    };

    public override string PrimaryAttackName => "Razor Bite";
    public override string SecondaryAttackName => "Bite Dash";
    public override string PrimaryAbilityName => "Hydro Rush";
    public override int PrimaryAttack => ModContent.ProjectileType<RipJawsProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 9;
    public override float PrimaryAttackModifier => 1.12f;

    public override int SecondaryAttack => ModContent.ProjectileType<RipJawsBiteProjectile>();
    public override int SecondaryAttackSpeed => 42;
    public override int SecondaryShootSpeed => 9;
    public override int SecondaryUseStyle => ItemUseStyleID.HiddenAnimation;
    public override float SecondaryAttackModifier => 3.35f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => HydroRushDuration;
    public override int PrimaryAbilityCooldown => HydroRushCooldown;
    public override int PrimaryAbilityCost => HydroRushCost;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        bool inWater = player.wet;
        bool inRain = Main.raining && player.ZoneRain && !inWater;
        bool hydroRush = omp.PrimaryAbilityEnabled;
        bool inUnderworld = player.ZoneUnderworldHeight;

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetAttackSpeed<HeroDamage>() += 0.1f;
        player.statDefense += 10;
        player.GetArmorPenetration<HeroDamage>() += 12;
        player.moveSpeed += 0.1f;
        player.runAcceleration += 0.1f;
        player.noFallDmg = true;

        if (inWater) {
            player.merman = true;
            player.breathCD = 0;
            player.breath = player.breathMax;
            player.GetDamage<HeroDamage>() *= WaterDamageMultiplier;
            player.GetAttackSpeed<HeroDamage>() += 0.18f;
            Lighting.AddLight(player.Center, Vector3.One);
            player.maxFallSpeed *= WaterFallSpeedMultiplier;
            player.moveSpeed *= WaterMoveSpeedMultiplier;
        }
        else if (hydroRush) {
            player.GetDamage<HeroDamage>() *= HydroRushDamageMultiplier;
            player.GetAttackSpeed<HeroDamage>() += 0.14f;
            player.maxFallSpeed *= HydroRushFallSpeedMultiplier;
            player.moveSpeed *= HydroRushMoveSpeedMultiplier;
            player.jumpSpeedBoost += 1.4f;
            player.ignoreWater = true;
            player.gills = true;
            player.armorEffectDrawShadow = true;
            Lighting.AddLight(player.Center, Vector3.One * 0.8f);
        }
        else if (inRain) {
            player.GetDamage<HeroDamage>() *= RainDamageMultiplier;
            player.GetAttackSpeed<HeroDamage>() += 0.1f;
            Lighting.AddLight(player.Center, Vector3.One * 0.65f);
            player.maxFallSpeed *= RainFallSpeedMultiplier;
            player.moveSpeed *= RainMoveSpeedMultiplier;
        }
        else {
            int drainInterval = inUnderworld ? UnderworldBreathDrainInterval : LandBreathDrainInterval;
            int drainAmount = inUnderworld ? UnderworldBreathDrainAmount : LandBreathDrainAmount;

            if (Main.GameUpdateCount % drainInterval == 0)
                player.breath = System.Math.Max(0, player.breath - drainAmount);

            if (player.breath <= 1)
                player.lifeRegen -= 50;
        }

        player.accFlipper = true;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (projectile.type != PrimaryAttack && projectile.type != SecondaryAttack)
            return;

        bool pressureWindow = player.wet || omp.PrimaryAbilityEnabled;
        if (pressureWindow)
            modifiers.ArmorPenetration += 8;

        float damageMultiplier = 1f;
        if (pressureWindow)
            damageMultiplier *= projectile.type == SecondaryAttack ? 1.24f : 1.14f;

        if (target.wet)
            damageMultiplier *= 1.12f;

        if (target.HasBuff(BuffID.Bleeding))
            damageMultiplier *= projectile.type == SecondaryAttack ? 1.18f : 1.1f;

        modifiers.FinalDamage *= damageMultiplier;
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (projectile.type != PrimaryAttack && projectile.type != SecondaryAttack)
            return;

        target.AddBuff(BuffID.Bleeding, projectile.type == SecondaryAttack ? 300 : 180);

        if (player.wet)
            return;

        player.breath = System.Math.Min(player.breathMax, player.breath + (projectile.type == SecondaryAttack ? 4 : 2));
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<RipJaws>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);

        if (player.wet) {
            player.legs = EquipLoader.GetEquipSlot(Mod, "RipJaws_alt", EquipType.Legs);
            return;
        }

        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        player.waist = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Waist);
    }
}
