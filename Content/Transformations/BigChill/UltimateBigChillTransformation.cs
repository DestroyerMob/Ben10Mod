using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Accessories.Wings;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.BigChill;

public class UltimateBigChillTransformation : BigChillTransformation {
    private const int SpectralPhaseBaseCost = 18;
    private const int SpectralPhaseOverdriveCost = 10;
    private const int PermafrostWakeCost = 20;
    private const int PolarCataclysmCost = 60;

    public override string FullID => "Ben10Mod:UltimateBigChill";
    public override string TransformationName => "Ultimate Big Chill";
    public override int TransformationBuffId => ModContent.BuffType<UltimateBigChill_Buff>();
    public override Transformation ParentTransformation => ModContent.GetInstance<BigChillTransformation>();
    public override Transformation ChildTransformation => null;

    public override string Description =>
        "An evolved Necrofriggian air-superiority form that blankets the arena in coldfire, holds targets in Deep Freeze, and chains Shatters through the fight.";

    public override List<string> Abilities => new() {
        "Direct hits add Frostbite and light coldfire; Deep Frozen targets lose more defense and spread Frostbite when shattered.",
        "Coldfire Stream is the long stacking breath and refreshes Deep Freeze briefly while you stay on target.",
        "Absolute Lance is the piercing cash-out shot that shatters, splinters, and leaves a short coldfire patch.",
        "Spectral Phase dashes intangible, ends in a frost pulse, and supercharges your next lance window.",
        "Permafrost Wake keeps building Frostbite and slowing the arena while you stay airborne.",
        "Shatters refund OE, launch frost wisps, fracture bosses, and Polar Cataclysm keeps targets half-stacked for repeat freezes."
    };

    public override string PrimaryAttackName => "Coldfire Stream";
    public override string SecondaryAttackName => "Absolute Lance";
    public override string PrimaryAbilityName => "Spectral Phase";
    public override string SecondaryAbilityName => "Permafrost Wake";
    public override string UltimateAbilityName => "Polar Cataclysm";

    public override int PrimaryAttackSpeed => 6;
    public override int SecondaryAttackSpeed => 18;
    public override int SecondaryShootSpeed => 21;
    public override float PrimaryAttackModifier => 0.44f;
    public override float SecondaryAttackModifier => 1.36f;
    public override int PrimaryAbilityCost => SpectralPhaseBaseCost;
    public override int SecondaryAbilityCost => PermafrostWakeCost;
    public override int UltimateAbilityCost => PolarCataclysmCost;

    public override int GetPrimaryAbilityCost(OmnitrixPlayer omp) {
        return omp.Player.GetModPlayer<BigChillStatePlayer>().AbsoluteZeroActive
            ? SpectralPhaseOverdriveCost
            : SpectralPhaseBaseCost;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        BigChillStatePlayer state = omp.Player.GetModPlayer<BigChillStatePlayer>();
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => state.AbsoluteZeroActive
                ? compact ? "Wide refreeze" : "Wider coldfire stream that rebuilds Deep Freeze extremely quickly"
                : compact ? "Stack Frostbite" : "Long coldfire breath that stacks Frostbite and holds Deep Freeze open",
            OmnitrixPlayer.AttackSelection.Secondary => state.PhaseDriftEmpowered
                ? compact ? "Lance +" : "Faster empowered lance with stronger Shatter payoff"
                : compact ? "Shatter lance" : "Piercing payoff spear that cashes Deep Freeze out into Shatter",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => omp.IsPrimaryAbilityActive
                ? compact
                    ? $"Phase {OmnitrixPlayer.FormatCooldownTicks(state.PhaseDriftTicksRemaining)}"
                    : $"Spectral Phase active • {OmnitrixPlayer.FormatCooldownTicks(state.PhaseDriftTicksRemaining)} left"
                : compact
                    ? $"{GetPrimaryAbilityCost(omp)} OE"
                    : $"Dash intangible, pulse on exit, and empower your next lance • {GetPrimaryAbilityCost(omp)} OE",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"{GetSecondaryAbilityCost(omp)} OE"
                : $"Drifting frost storm that keeps stacking and slowing the arena • {GetSecondaryAbilityCost(omp)} OE",
            OmnitrixPlayer.AttackSelection.Ultimate => state.AbsoluteZeroActive
                ? compact
                    ? $"Polar {OmnitrixPlayer.FormatCooldownTicks(state.AbsoluteZeroTicksRemaining)}"
                    : $"Polar Cataclysm active • {OmnitrixPlayer.FormatCooldownTicks(state.AbsoluteZeroTicksRemaining)} left"
                : compact
                    ? $"{GetUltimateAbilityCost(omp)} OE"
                    : $"Air-superiority overdrive with side lances, stronger wake, and repeat freeze-shatters • {GetUltimateAbilityCost(omp)} OE",
            _ => base.GetAttackResourceSummary(selection, omp, compact)
        };
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.10f;
        player.moveSpeed += 0.02f;
        player.runAcceleration += 0.04f;
        player.maxRunSpeed += 0.25f;
        player.buffImmune[BuffID.OnFire] = true;
        player.buffImmune[BuffID.OnFire3] = true;
        player.buffImmune[BuffID.Burning] = true;
        player.buffImmune[BuffID.CursedInferno] = true;
        player.buffImmune[BuffID.ShadowFlame] = true;
        player.buffImmune[BuffID.Daybreak] = true;

        ModContent.GetInstance<AbilitySlot>().FunctionalItem = new Item(ModContent.ItemType<UltimateBigChillWings>());
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Legs);
        player.wings = EquipLoader.GetEquipSlot(Mod, nameof(UltimateBigChillWings), EquipType.Wings);
    }
}
