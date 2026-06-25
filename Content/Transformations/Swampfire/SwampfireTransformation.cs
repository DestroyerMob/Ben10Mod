using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Swampfire;

public class SwampfireTransformation : Transformation {
    private const int FallbackBaseDamage = 42;
    private const int RootingCost = 12;
    private const int WildfireBloomCost = 18;
    private const int WildfireBloomCooldown = 18 * 60;
    private const int SwampInfernoCost = 55;
    private const int SwampInfernoCooldown = 62 * 60;
    private const float WildfireBloomRadius = 720f;
    private const float SwampInfernoRadius = 1120f;
    private const float RootingPodGrowthPerTick = 0.018f;

    public override string FullID => "Ben10Mod:Swampfire";
    public override string TransformationName => "Swampfire";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Swampfire_Buff>();

    public override string Description =>
        "A regenerative fire-plant bruiser that seeds the arena with gas pods, then ignites the whole garden.";

    public override List<string> Abilities => new() {
        "Methane Bolt applies Fuel Vapour, marking enemies so later fire effects can ignite harder.",
        "Seed Pod grows Bloom Pods that release gas and turn the arena into fuel.",
        "Regenerative Rooting slows Swampfire but grows pods faster and improves self-healing.",
        "Wildfire Bloom ignites pods and fuelled enemies across the field.",
        "Swamp Inferno triggers a large chain reaction and heals Swampfire from the burning field."
    };

    public override string PrimaryAttackName => "Methane Bolt";
    public override string SecondaryAttackName => "Seed Pod";
    public override string PrimaryAbilityName => "Regenerative Rooting";
    public override string SecondaryAbilityName => "Wildfire Bloom";
    public override string UltimateAbilityName => "Swamp Inferno";
    public override int PrimaryAttack => ModContent.ProjectileType<SwampfireBoltProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 13;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<SwampfireSeedProjectile>();
    public override int SecondaryAttackSpeed => 32;
    public override int SecondaryShootSpeed => 10;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.25f;
    public override int PrimaryAbilityCost => RootingCost;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 10 * 60;
    public override int PrimaryAbilityCooldown => 36 * 60;
    public override bool HasSecondaryAbility => true;
    public override int SecondaryAbilityCost => WildfireBloomCost;
    public override int SecondaryAbilityCooldown => WildfireBloomCooldown;
    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityCost => SwampInfernoCost;
    public override int UltimateAbilityCooldown => SwampInfernoCooldown;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.14f;
        player.lifeRegen += 2;
        player.fireWalk = true;
        player.buffImmune[BuffID.OnFire] = true;
        player.buffImmune[BuffID.OnFire3] = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.lifeRegen += 6;
        player.statDefense += 10;
        player.endurance += 0.04f;
        player.moveSpeed *= 0.8f;
        player.maxRunSpeed *= 0.76f;
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        int grownPods = SwampfireVineProjectile.GrowOwnedPods(player.whoAmI, RootingPodGrowthPerTick, 5 * 60);
        if (Main.dedServ || grownPods <= 0 || !Main.rand.NextBool(3))
            return;

        Vector2 dustPosition = player.Bottom + Main.rand.NextVector2Circular(player.width * 0.5f, 8f);
        Dust dust = Dust.NewDustPerfect(dustPosition, Main.rand.NextBool() ? DustID.Grass : DustID.Torch,
            new Vector2(Main.rand.NextFloat(-0.45f, 0.45f), Main.rand.NextFloat(-1.35f, -0.35f)),
            105, new Color(180, 235, 95), Main.rand.NextFloat(0.9f, 1.2f));
        dust.noGravity = true;
    }

    public override bool TryActivateSecondaryAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<SecondaryAbilityCooldown>() || player.dead || player.CCed)
            return true;

        int cost = GetSecondaryAbilityCost(omp);
        if (!omp.CanSpendOmnitrixEnergy(cost)) {
            omp.ShowTransformFailureFeedback($"Need {cost} OE for {SecondaryAbilityName}.");
            return true;
        }

        IEntitySource source = player.GetSource_FromThis();
        int podDamage = ResolveHeroDamage(player, 1.08f);
        int fuelDamage = ResolveHeroDamage(player, 0.62f);
        int ignitedPods = SwampfireVineProjectile.IgniteOwnedPods(player.whoAmI, source, podDamage, ultimate: false);
        int ignitedTargets = IgniteFuelVapourTargets(player, source, fuelDamage, WildfireBloomRadius, ultimate: false);

        if (ignitedPods + ignitedTargets <= 0) {
            omp.ShowTransformFailureFeedback("No Bloom Pods or Fuel Vapour to ignite.");
            SpawnFailedIgnitionDust(player);
            return true;
        }

        ConsumeEnergyAndCooldown(player, omp, cost, GetSecondaryAbilityCooldown(omp),
            ModContent.BuffType<SecondaryAbilityCooldown>());
        HealFromIgnition(player, Math.Min(18, ignitedPods + ignitedTargets * 2));
        PlayIgnitionSound(player, ultimate: false);
        return true;
    }

    public override bool TryActivateUltimateAbility(Player player, OmnitrixPlayer omp) {
        if (player.HasBuff<UltimateAbilityCooldown>() || player.dead || player.CCed)
            return true;

        int cost = GetUltimateAbilityCost(omp);
        if (!omp.CanSpendOmnitrixEnergy(cost)) {
            omp.ShowTransformFailureFeedback($"Need {cost} OE for {UltimateAbilityName}.");
            return true;
        }

        IEntitySource source = player.GetSource_FromThis();
        int podDamage = ResolveHeroDamage(player, 1.62f);
        int fuelDamage = ResolveHeroDamage(player, 0.92f);
        int ignitedPods = SwampfireVineProjectile.IgniteOwnedPods(player.whoAmI, source, podDamage, ultimate: true);
        int ignitedTargets = IgniteFuelVapourTargets(player, source, fuelDamage, SwampInfernoRadius, ultimate: true);
        int setupCount = ignitedPods + ignitedTargets;

        if (setupCount <= 0) {
            omp.ShowTransformFailureFeedback("Swamp Inferno needs Bloom Pods or Fuel Vapour.");
            SpawnFailedIgnitionDust(player);
            return true;
        }

        float centerBurstScale = MathHelper.Clamp(1.55f + setupCount * 0.11f, 1.55f, 3f);
        int centerDamage = ResolveHeroDamage(player, 0.74f + Math.Min(setupCount, 10) * 0.035f);
        int burstIndex = Projectile.NewProjectile(source, player.Center, Vector2.Zero,
            ModContent.ProjectileType<SwampfireIgnitionBurstProjectile>(), centerDamage, 4.2f, player.whoAmI,
            centerBurstScale, 1f);

        if (burstIndex >= 0 && burstIndex < Main.maxProjectiles)
            Main.projectile[burstIndex].netUpdate = true;

        ConsumeEnergyAndCooldown(player, omp, cost, GetUltimateAbilityCooldown(omp),
            ModContent.BuffType<UltimateAbilityCooldown>());

        int healAmount = Math.Min(115, 12 + ignitedPods * 6 + ignitedTargets * 8);
        if (omp.PrimaryAbilityEnabled)
            healAmount += Math.Min(18, setupCount * 2);

        HealFromIgnition(player, healAmount);
        PlayIgnitionSound(player, ultimate: true);
        return true;
    }

    private static int IgniteFuelVapourTargets(Player player, IEntitySource source, int damage, float radius, bool ultimate) {
        int fuelType = ModContent.BuffType<FuelVapour>();
        int burstType = ModContent.ProjectileType<SwampfireIgnitionBurstProjectile>();
        int maxTargets = ultimate ? 18 : 10;
        int ignited = 0;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.friendly || npc.dontTakeDamage || !npc.HasBuff(fuelType))
                continue;

            float allowedDistance = radius + Math.Max(npc.width, npc.height) * 0.5f;
            if (Vector2.DistanceSquared(player.Center, npc.Center) > allowedDistance * allowedDistance)
                continue;

            if (player.whoAmI == Main.myPlayer || Main.netMode != NetmodeID.MultiplayerClient) {
                float targetScale = ultimate
                    ? MathHelper.Lerp(0.95f, 1.45f, ignited / (float)Math.Max(1, maxTargets - 1))
                    : 0.72f;
                int burstIndex = Projectile.NewProjectile(source, npc.Center, Vector2.Zero, burstType, damage,
                    ultimate ? 2.8f : 1.7f, player.whoAmI, targetScale, ultimate ? 1f : 0f);

                if (burstIndex >= 0 && burstIndex < Main.maxProjectiles)
                    Main.projectile[burstIndex].netUpdate = true;
            }

            ignited++;
            if (ignited >= maxTargets)
                break;
        }

        return ignited;
    }

    private static void ConsumeEnergyAndCooldown(Player player, OmnitrixPlayer omp, int cost, int cooldown, int cooldownBuffType) {
        omp.TrySpendOmnitrixEnergy(cost);

        if (cooldown > 0)
            player.AddBuff(cooldownBuffType, cooldown);
    }

    private static void HealFromIgnition(Player player, int amount) {
        if (amount <= 0 || player.dead)
            return;

        int heal = Math.Min(amount, player.statLifeMax2 - player.statLife);
        if (heal <= 0)
            return;

        player.statLife += heal;
        player.HealEffect(heal, true);

        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
    }

    private static void PlayIgnitionSound(Player player, bool ultimate) {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item74 with {
            Pitch = ultimate ? -0.28f : 0.04f,
            Volume = ultimate ? 0.72f : 0.48f
        }, player.Center);
    }

    private static void SpawnFailedIgnitionDust(Player player) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust smoke = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(18f, 24f),
                DustID.Smoke, Main.rand.NextVector2Circular(0.7f, 0.7f), 120,
                new Color(120, 110, 80), Main.rand.NextFloat(0.75f, 1f));
            smoke.noGravity = true;
        }
    }

    private static int ResolveHeroDamage(Player player, float multiplier) {
        float baseDamage = ResolveBaseDamage(player) * multiplier;
        return Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(baseDamage)));
    }

    private static int ResolveBaseDamage(Player player) {
        Item heldItem = player.HeldItem;
        if (heldItem != null && !heldItem.IsAir && heldItem.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return Math.Max(1, heldItem.damage);

        return FallbackBaseDamage;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<Swampfire>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }
}
