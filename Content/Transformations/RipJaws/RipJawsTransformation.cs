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
    private const float RainDamageMultiplier = 1.2f;
    private const float WaterMoveSpeedMultiplier = 2.25f;
    private const float RainMoveSpeedMultiplier = 1.35f;
    private const float WaterFallSpeedMultiplier = 1.5f;
    private const float RainFallSpeedMultiplier = 1.15f;
    private const int LandBreathDrainInterval = 18;
    private const int UnderworldBreathDrainInterval = 8;
    private const int LandBreathDrainAmount = 1;
    private const int UnderworldBreathDrainAmount = 2;

    public override string FullID => "Ben10Mod:RipJaws";
    public override string TransformationName => "Ripjaws";
    public override string IconPath => "Ben10Mod/Content/Interface/RipJawsSelect";
    public override int TransformationBuffId => ModContent.BuffType<RipJaws_Buff>();

    public override string Description =>
        "An aquatic predator that dominates in water, suffers on land, and lunges with devastating bites.";

    public override List<string> Abilities => new() {
        "Aquatic rush",
        "Heavy bite lunge",
        "Water mobility",
        "Amphibious survival pressure"
    };

    public override string PrimaryAttackName => "Razor Bite";
    public override string SecondaryAttackName => "Bite Dash";
    public override int PrimaryAttack => ModContent.ProjectileType<RipJawsProjectile>();
    public override int PrimaryAttackSpeed => 28;
    public override int PrimaryShootSpeed => 6;

    public override int SecondaryAttack => ModContent.ProjectileType<RipJawsBiteProjectile>();
    public override int SecondaryAttackSpeed => 75;
    public override int SecondaryShootSpeed => 6;
    public override int SecondaryUseStyle => ItemUseStyleID.HiddenAnimation;
    public override float SecondaryAttackModifier => 3f;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        bool inWater = player.wet;
        bool inRain = Main.raining && player.ZoneRain && !inWater;
        bool inUnderworld = player.ZoneUnderworldHeight;

        player.GetDamage<HeroDamage>() += 0.08f;
        player.statDefense += 6;
        player.GetArmorPenetration<HeroDamage>() += 8;

        if (inWater) {
            player.merman = true;
            player.breathCD = 0;
            player.breath = player.breathMax;
            player.GetDamage<HeroDamage>() *= WaterDamageMultiplier;
            player.GetAttackSpeed<HeroDamage>() += 0.16f;
            Lighting.AddLight(player.Center, Vector3.One);
            player.maxFallSpeed *= WaterFallSpeedMultiplier;
            player.moveSpeed *= WaterMoveSpeedMultiplier;
        }
        else if (inRain) {
            player.GetDamage<HeroDamage>() *= RainDamageMultiplier;
            player.GetAttackSpeed<HeroDamage>() += 0.08f;
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
                player.lifeRegen -= 60;
        }

        player.accFlipper = true;
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
