using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Rath;

public class RathTransformation : Transformation {
    public const int RendMaxStacks = 5;
    public const int RendDurationTicks = 9 * 60;

    private const float PrimaryDamageMultiplier = 0.96f;
    private const float SecondClawDamageMultiplier = 1.06f;
    private const float FinisherDamageMultiplier = 1.28f;
    private const float RageFinisherDamageMultiplier = 1.42f;
    private const float PounceDamageMultiplier = 1.32f;
    private const float BaseClawSpawnOffset = 92f;
    private const float RageClawSpawnOffset = 116f;
    private const float BasePounceRange = 300f;
    private const float RagePounceRange = 390f;
    private const int BaseRendDurationTicks = 7 * 60;
    private const int RageRendDurationTicks = RendDurationTicks;
    private const float PreyPounceRangeBonus = 110f;

    public override string FullID => RathStatePlayer.TransformationId;
    public override string TransformationName => "Rath";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Rath_Buff>();

    public override string Description =>
        "An Appoplexian duelist predator who marks one prey target, builds Rend stacks with claw strings, then pounces in to cash them out.";

    public override List<string> Abilities => new() {
        "Savage 3-hit claw combo marks one prey target and builds Rend",
        "Pounce chases marked prey, grants brief guard, and cashes out Rend",
        "Battle Rage widens claws, speeds attacks, and sharpens single-target pressure"
    };

    public override string PrimaryAttackName => "Savage Combo";
    public override string SecondaryAttackName => "Pounce";
    public override string PrimaryAbilityName => "Battle Rage";

    public override int PrimaryAttack => ModContent.ProjectileType<RathClawProjectile>();
    public override int PrimaryAttackSpeed => 16;
    public override int PrimaryShootSpeed => 10;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<RathPounceProjectile>();
    public override int SecondaryAttackSpeed => 38;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => PounceDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 8 * 60;
    public override int PrimaryAbilityCooldown => 32 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        RathStatePlayer state = player.GetModPlayer<RathStatePlayer>();

        player.GetDamage<HeroDamage>() += 0.14f;
        player.GetAttackSpeed<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 8f;
        player.GetKnockback<HeroDamage>() += 0.45f;
        player.GetArmorPenetration<HeroDamage>() += 6;
        player.statDefense += 8;
        player.moveSpeed += 0.1f;
        player.runAcceleration += 0.06f;
        player.jumpSpeedBoost += 1f;
        player.noFallDmg = true;

        if (state.GuardActive) {
            float guardStrength = state.GuardStrength;
            player.statDefense += 4 + (int)Math.Round(guardStrength * 40f);
            player.endurance += guardStrength;
            player.noKnockback = true;
        }

        if (!state.RageActive)
            return;

        player.GetDamage<HeroDamage>() += 0.14f;
        player.GetAttackSpeed<HeroDamage>() += 0.16f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetArmorPenetration<HeroDamage>() += 6;
        player.moveSpeed += 0.18f;
        player.runAcceleration *= 1.18f;
        player.noKnockback = true;
        player.blackBelt = true;
        player.armorEffectDrawShadow = true;
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        RathStatePlayer state = omp.Player.GetModPlayer<RathStatePlayer>();
        if (omp.setAttack == OmnitrixPlayer.AttackSelection.Primary) {
            float useMultiplier = state.RageActive ? 0.76f : 0.88f;
            item.useTime = item.useAnimation = Math.Max(9, (int)Math.Round(item.useTime * useMultiplier));
        }
        else if (omp.setAttack == OmnitrixPlayer.AttackSelection.Secondary) {
            float useMultiplier = state.RageActive ? 0.82f : 0.92f;
            item.useTime = item.useAnimation = Math.Max(24, (int)Math.Round(item.useTime * useMultiplier));
        }
        else {
            return;
        }

        NPC prey = FindMarkedPrey(omp.Player, (state.RageActive ? RagePounceRange : BasePounceRange) + PreyPounceRangeBonus);
        if (prey == null)
            return;

        float preyPressureMultiplier = omp.setAttack == OmnitrixPlayer.AttackSelection.Secondary ? 0.9f : 0.94f;
        item.useTime = item.useAnimation = Math.Max(8, (int)Math.Round(item.useTime * preyPressureMultiplier));
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        return base.CanStartCurrentAttack(player, omp) &&
               !HasActiveOwnedProjectile(player, SecondaryAttack);
    }

    public override void ModifyHurt(Player player, OmnitrixPlayer omp, ref Player.HurtModifiers modifiers) {
        RathStatePlayer state = player.GetModPlayer<RathStatePlayer>();
        modifiers.Knockback *= 0.72f;

        if (!state.GuardActive)
            return;

        modifiers.FinalDamage *= 1f - state.GuardStrength;
        modifiers.Knockback *= 0.35f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        RathStatePlayer state = player.GetModPlayer<RathStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);
        bool rage = state.RageActive;

        if (omp.altAttack) {
            float maxPounceRange = rage ? RagePounceRange : BasePounceRange;
            NPC preyTarget = FindMarkedPrey(player, maxPounceRange + PreyPounceRangeBonus);
            if (preyTarget != null)
                maxPounceRange += PreyPounceRangeBonus;

            Vector2 pounceOffset = ResolvePounceOffset(player, direction, maxPounceRange, preyTarget);
            float requestedDistance = pounceOffset.Length();
            Vector2 pounceDirection = pounceOffset.SafeNormalize(new Vector2(player.direction, 0f));
            float pounceSpeed = RathPounceProjectile.GetDashSpeed(rage);
            int pounceFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / pounceSpeed),
                RathPounceProjectile.MinPounceFrames, RathPounceProjectile.MaxPounceFrames);
            int pounceDamage = ScaleDamage(damage, PounceDamageMultiplier * (rage ? 1.08f : 1f));

            state.RegisterRathGuard(22, rage ? 0.17f : 0.13f);
            int projectileIndex = Projectile.NewProjectile(source, player.Center + pounceDirection * 18f,
                pounceDirection * pounceSpeed, SecondaryAttack, pounceDamage, knockback + 1.8f, player.whoAmI,
                rage ? 1f : 0f, preyTarget == null ? 0f : preyTarget.whoAmI + 1f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile projectile = Main.projectile[projectileIndex];
                projectile.timeLeft = pounceFrames;
                projectile.netUpdate = true;
            }

            return false;
        }

        int comboStep = state.ConsumeComboStep();
        bool finisher = comboStep >= 2;
        float clawScale = finisher
            ? rage ? 1.46f : 1.28f
            : rage ? 1.16f : 1f;
        float clawRange = (rage ? RageClawSpawnOffset : BaseClawSpawnOffset) + (finisher ? 12f : 0f);
        float damageMultiplier = finisher
            ? rage ? RageFinisherDamageMultiplier : FinisherDamageMultiplier
            : comboStep == 1 ? SecondClawDamageMultiplier : PrimaryDamageMultiplier;
        int clawDamage = ScaleDamage(damage, damageMultiplier);
        Vector2 clawSpawnPosition = ResolveClawSpawnPosition(player, direction, clawRange);

        state.RegisterRathGuard(finisher ? 18 : 12, (finisher ? 0.12f : 0.08f) + (rage ? 0.04f : 0f));
        Projectile.NewProjectile(source, clawSpawnPosition, direction * PrimaryShootSpeed, PrimaryAttack, clawDamage,
            knockback + (finisher ? 0.8f : 0.25f), player.whoAmI, comboStep, clawScale, direction.ToRotation());
        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (!IsRathProjectile(projectile.type))
            return;

        RathStatePlayer state = player.GetModPlayer<RathStatePlayer>();
        bool bleeding = target.HasBuff(BuffID.Bleeding);
        bool pounce = projectile.type == SecondaryAttack;
        bool finisher = projectile.type == PrimaryAttack && projectile.ai[0] >= 2f;
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        bool markedPrey = identity.IsRathPreyFor(player.whoAmI);
        int rendStacks = identity.GetRathRendStacks(player.whoAmI);

        if (markedPrey) {
            modifiers.ArmorPenetration += pounce ? 6 + rendStacks * 2 : 3 + rendStacks;
            float preyMultiplier = pounce
                ? 1.04f + rendStacks * 0.035f
                : 1.03f + rendStacks * 0.018f;
            if (finisher)
                preyMultiplier += 0.03f;

            modifiers.FinalDamage *= preyMultiplier;
            if (pounce)
                projectile.localAI[1] = 1f;
        }

        if (bleeding) {
            modifiers.ArmorPenetration += pounce ? 12 : 8;
            modifiers.FinalDamage *= pounce ? 1.22f : finisher ? 1.18f : 1.1f;
            if (pounce)
                projectile.localAI[1] = 1f;
        }

        if (!state.RageActive)
            return;

        modifiers.ArmorPenetration += 6;
        if (pounce && (bleeding || markedPrey))
            modifiers.FinalDamage *= markedPrey ? 1.08f : 1.1f;
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsRathProjectile(projectile.type))
            return;

        RathStatePlayer state = player.GetModPlayer<RathStatePlayer>();
        bool pounce = projectile.type == SecondaryAttack;
        bool finisher = projectile.type == PrimaryAttack && projectile.ai[0] >= 2f;
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        bool markedPrey = identity.IsRathPreyFor(player.whoAmI);
        int rendStacks = identity.GetRathRendStacks(player.whoAmI);
        state.RegisterRathImpact(target, pounce, finisher);

        if (!pounce) {
            MarkRathPrey(player, target, 1 + (finisher ? 1 : 0) + (state.RageActive ? 1 : 0), state.RageActive);
            return;
        }

        if (markedPrey)
            rendStacks = identity.ConsumeRathRend(player.whoAmI);

        if (markedPrey || projectile.localAI[1] > 0f)
            SpawnRendingCashout(player, target, projectile, state.RageActive, rendStacks, markedPrey);
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        if (resolvedSelection != OmnitrixPlayer.AttackSelection.Primary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Secondary)
            return base.GetAttackResourceSummary(selection, omp, compact);

        NPC prey = FindMarkedPrey(omp.Player);
        int rendStacks = prey?.GetGlobalNPC<AlienIdentityGlobalNPC>().GetRathRendStacks(omp.Player.whoAmI) ?? 0;
        string preyText = prey == null
            ? compact ? "No prey" : "No prey marked"
            : compact ? $"Rend {rendStacks}/{RendMaxStacks}" : $"Prey Rend {rendStacks}/{RendMaxStacks}";

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"Build Rend • {preyText}"
                : $"Mark prey and build Rend • {preyText}",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"Cashout • {preyText}"
                : $"Pounce to prey and cash out Rend • {preyText}",
            _ => preyText
        };
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<Rath>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction;
    }

    private static Vector2 ResolveClawSpawnPosition(Player player, Vector2 fallbackDirection, float maxRange) {
        Vector2 origin = player.MountedCenter;
        if (Main.netMode != NetmodeID.SinglePlayer && player.whoAmI != Main.myPlayer)
            return origin + fallbackDirection * maxRange;

        Vector2 mouseOffset = Main.MouseWorld - origin;
        if (mouseOffset == Vector2.Zero)
            return origin + fallbackDirection * maxRange;

        float distanceToMouse = mouseOffset.Length();
        Vector2 direction = mouseOffset / distanceToMouse;
        return origin + direction * Math.Min(distanceToMouse, maxRange);
    }

    private static Vector2 ResolvePounceOffset(Player player, Vector2 fallbackDirection, float maxRange, NPC preyTarget = null) {
        Vector2 offset = fallbackDirection * maxRange;
        if (preyTarget != null && preyTarget.active) {
            offset = preyTarget.Center - player.Center;
        }
        else if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseOffset = Main.MouseWorld - player.Center;
            if (mouseOffset != Vector2.Zero)
                offset = mouseOffset;
        }

        float distance = offset.Length();
        if (distance <= maxRange)
            return offset;

        return offset / distance * maxRange;
    }

    private static void MarkRathPrey(Player player, NPC target, int stacks, bool rage) {
        ClearOtherRathPrey(player, target);

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        identity.ApplyRathPrey(player.whoAmI, stacks, rage ? RageRendDurationTicks : BaseRendDurationTicks);

        if (Main.dedServ)
            return;

        int dustCount = 6 + stacks * 3;
        for (int i = 0; i < dustCount; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(12f, 12f),
                i % 3 == 0 ? DustID.Smoke : DustID.Blood,
                Main.rand.NextVector2Circular(2.6f, 2.6f), 95, new Color(255, 118, 86),
                Main.rand.NextFloat(0.9f, rage ? 1.28f : 1.12f));
            dust.noGravity = true;
        }
    }

    private static void ClearOtherRathPrey(Player player, NPC target) {
        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.whoAmI == target.whoAmI)
                continue;

            npc.GetGlobalNPC<AlienIdentityGlobalNPC>().ClearRathPrey(player.whoAmI);
        }
    }

    private static NPC FindMarkedPrey(Player player, float maxRange = float.MaxValue) {
        NPC bestTarget = null;
        float maxRangeSq = maxRange * maxRange;
        float bestDistanceSq = float.MaxValue;

        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy())
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            if (!identity.IsRathPreyFor(player.whoAmI))
                continue;

            float distanceSq = Vector2.DistanceSquared(player.Center, npc.Center);
            if (distanceSq > maxRangeSq || distanceSq >= bestDistanceSq)
                continue;

            bestDistanceSq = distanceSq;
            bestTarget = npc;
        }

        return bestTarget;
    }

    private static void SpawnRendingCashout(Player player, NPC target, Projectile projectile, bool rage, int rendStacks,
        bool markedPrey) {
        int consumedStacks = Utils.Clamp(rendStacks, 0, RendMaxStacks);
        if (Main.netMode != NetmodeID.MultiplayerClient && target.active) {
            float bonusMultiplier = rage ? 0.34f : 0.26f;
            bonusMultiplier += markedPrey ? 0.08f + consumedStacks * 0.045f : 0.06f;
            int bonusDamage = ScaleDamage(projectile.damage, bonusMultiplier);
            int hitDirection = target.Center.X >= player.Center.X ? 1 : -1;
            target.SimpleStrikeNPC(bonusDamage, hitDirection, false, projectile.knockBack * 0.45f,
                ModContent.GetInstance<HeroDamage>());
        }

        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item71 with {
            Pitch = (rage ? -0.12f : 0f) - consumedStacks * 0.015f,
            Volume = 0.56f
        }, target.Center);
        int dustCount = (rage ? 22 : 15) + consumedStacks * 3 + (markedPrey ? 4 : 0);
        for (int i = 0; i < dustCount; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(14f, 14f),
                i % 3 == 0 ? DustID.Smoke : DustID.Blood,
                Main.rand.NextVector2Circular(rage ? 4.4f : 3.2f, rage ? 4.4f : 3.2f), 90,
                new Color(255, 164, 120), Main.rand.NextFloat(1f, rage ? 1.45f : 1.24f));
            dust.noGravity = true;
        }
    }

    private static bool IsRathProjectile(int projectileType) {
        return projectileType == ModContent.ProjectileType<RathClawProjectile>() ||
               projectileType == ModContent.ProjectileType<RathPounceProjectile>();
    }

    private static bool HasActiveOwnedProjectile(Player player, int projectileType) {
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.owner == player.whoAmI && projectile.type == projectileType)
                return true;
        }

        return false;
    }

    private static int ScaleDamage(int damage, float multiplier) {
        return Math.Max(1, (int)Math.Round(damage * multiplier));
    }
}
