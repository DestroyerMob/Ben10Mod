using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.ChromaStone;

public class ChromaStoneTransformation : Transformation {
    private const float CrystalVolleyDamageMultiplier = 0.76f;
    private const float SpectrumBeamDamageMultiplier = 0.42f;
    private const float PrismaticLanceDamageMultiplier = 1.2f;
    private const float FullSpectrumDischargeDamageMultiplier = 0.64f;
    private const float PrismBoltSideShardMultiplier = 0.38f;
    private const int SpectrumBeamActivationCost = 6;
    private const int SpectrumBeamSustainCost = 3;
    private const int SpectrumBeamSustainInterval = 12;
    private const int PrismaticLanceEnergyCost = 24;
    private const int FullSpectrumDischargeSelectionCost = 24;
    private const int FullSpectrumDischargeActivationCost = 14;
    private const int FullSpectrumDischargeSustainCost = 5;
    private const int FullSpectrumDischargeSustainInterval = 12;

    public override string FullID => AlienIdentityPlayer.ChromaStoneTransformationId;
    public override string TransformationName => "Chromastone";
    public override string IconPath => "Ben10Mod/Content/Interface/ChromaStoneSelect";
    public override int TransformationBuffId => ModContent.BuffType<ChromaStone_Buff>();
    public override string Description => ChromaStone.TransformationDescription;
    public override List<string> Abilities => new(ChromaStone.TransformationAbilities);

    public override string PrimaryAttackName => "Crystal Volley";
    public override string SecondaryAttackName => "Spectrum Beam";
    public override string PrimaryAbilityName => "Absorption Guard";
    public override string SecondaryAbilityName => "Prismatic Lance";
    public override string SecondaryAbilityAttackName => "Prismatic Lance";
    public override string TertiaryAbilityName => "Resonance Facets";
    public override string UltimateAbilityName => "Full Spectrum Discharge";
    public override string UltimateAttackName => "Full Spectrum Discharge";

    public override int PrimaryAttack => ModContent.ProjectileType<ChromaStoneProjectile>();
    public override int PrimaryAttackSpeed => 8;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => CrystalVolleyDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<ChromaStoneBeamProjectile>();
    public override int SecondaryAttackSpeed => 14;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override bool SecondaryChannel => true;
    public override float SecondaryAttackModifier => SpectrumBeamDamageMultiplier;
    public override int SecondaryEnergyCost => SpectrumBeamActivationCost;
    public override int SecondaryAttackSustainEnergyCost => SpectrumBeamSustainCost;
    public override int SecondaryAttackSustainInterval => SpectrumBeamSustainInterval;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 1;
    public override int PrimaryAbilityCooldown => ChromaStoneStatePlayer.AbsorptionGuardCooldownTicks;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<ChromaStoneLanceProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 22;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => PrismaticLanceDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => PrismaticLanceEnergyCost;
    public override int SecondaryAbilityCooldown => ChromaStoneStatePlayer.PrismaticLanceCooldownTicks;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<ChromaStoneSupernovaProjectile>();
    public override int UltimateAttackSpeed => 18;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override bool UltimateChannel => true;
    public override float UltimateAttackModifier => FullSpectrumDischargeDamageMultiplier;
    public override int UltimateEnergyCost => FullSpectrumDischargeActivationCost;
    public override int UltimateAttackSustainEnergyCost => FullSpectrumDischargeSustainCost;
    public override int UltimateAttackSustainInterval => FullSpectrumDischargeSustainInterval;
    public override int UltimateAbilityCost => FullSpectrumDischargeSelectionCost;
    public override int UltimateAbilityCooldown => ChromaStoneStatePlayer.FullSpectrumDischargeCooldownTicks;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<ChromaStoneBeamProjectile>(),
            ModContent.ProjectileType<ChromaStoneGuardProjectile>(),
            ModContent.ProjectileType<ChromaStoneFacetProjectile>(),
            ModContent.ProjectileType<ChromaStoneLanceProjectile>(),
            ModContent.ProjectileType<ChromaStoneLanceEchoProjectile>(),
            ModContent.ProjectileType<ChromaStoneSupernovaProjectile>(),
            ModContent.ProjectileType<ChromaStoneRadianceBurstProjectile>());
        player.GetModPlayer<ChromaStoneStatePlayer>().ResetTransientState();
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        float facetPower = state.FacetPowerRatio;
        float radianceRatio = state.VisualRadianceRatio;
        float cycleRate = MathHelper.Lerp(150f, 48f, radianceRatio);
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(radianceRatio * 3.1f + player.miscCounter / cycleRate,
            1.02f + radianceRatio * 0.34f + (state.DischargeActive ? 0.08f : 0f));

        player.GetDamage<HeroDamage>() += 0.08f + radianceRatio * 0.24f;
        player.statDefense += 12;
        player.noFallDmg = true;
        player.runAcceleration *= 0.82f;
        player.maxRunSpeed *= 0.95f;
        Lighting.AddLight(player.Center, prismColor.ToVector3() * (0.24f + radianceRatio * 0.42f + facetPower * 0.08f));
        if (radianceRatio >= 0.35f || state.DischargeActive)
            player.armorEffectDrawShadow = true;

        if (state.Guarding) {
            player.statDefense += 8;
            player.endurance += 0.08f;
            player.noKnockback = true;
            player.moveSpeed *= 0.28f;
            player.runAcceleration *= 0.18f;
            player.maxRunSpeed *= 0.45f;
        }

        if (!state.DischargeActive)
            return;

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.statDefense += 4;
        player.endurance += 0.05f;
        player.noKnockback = true;
        player.armorEffectDrawShadow = true;
    }

    public override bool? CanUseItem(Player player, OmnitrixPlayer omp, Item item) {
        bool? baseResult = base.CanUseItem(player, omp, item);
        if (baseResult.HasValue && !baseResult.Value)
            return baseResult;

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        if (state.Guarding || state.DischargeActive)
            return false;

        return baseResult;
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        if (!base.CanStartCurrentAttack(player, omp))
            return false;

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        return !state.Guarding && !state.DischargeActive;
    }

    public override bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<ChromaStoneStatePlayer>().TryStartAbsorptionGuard();
        return true;
    }

    public override bool TryActivateUltimateAbility(Player player, OmnitrixPlayer omp) {
        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        if (omp.ultimateAttack)
            return false;

        if (!state.HasDischargeThreshold) {
            omp.ShowTransformFailureFeedback("Need 90% Radiance for Full Spectrum Discharge.");
            return true;
        }

        int readyCost = GetUltimateAbilityCost(omp);
        if (omp.omnitrixEnergy < readyCost) {
            omp.ShowTransformFailureFeedback($"Need {readyCost} OE to ready {UltimateAbilityName}.");
            return true;
        }

        return false;
    }

    public override bool TryGetTransformationTint(Player player, OmnitrixPlayer omp, out Color tint,
        out float blendStrength, out bool forceFullBright) {
        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        float radianceRatio = state.VisualRadianceRatio;
        float cycleRate = MathHelper.Lerp(132f, 38f, radianceRatio);
        tint = ChromaStonePrismHelper.GetSpectrumColor(radianceRatio * 3.2f + player.miscCounter / cycleRate,
            1.04f + radianceRatio * 0.36f + (state.DischargeActive ? 0.1f : 0f));
        blendStrength = 0.08f + radianceRatio * 0.26f + (state.Guarding ? 0.04f : 0f) + (state.DischargeActive ? 0.08f : 0f);
        forceFullBright = state.DischargeActive || radianceRatio >= 0.82f;
        return blendStrength > 0f;
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<ChromaStoneStatePlayer>().ApplyHoverControl();
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (Main.dedServ)
            return;

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        float radianceRatio = state.VisualRadianceRatio;
        int spawnRate = state.DischargeActive ? 1 : radianceRatio >= 0.75f ? 1 : radianceRatio >= 0.38f ? 2 : state.VisibleFacetCount > 0 ? 3 : 5;
        if (!Main.rand.NextBool(spawnRate))
            return;

        Vector2 offset = Main.rand.NextVector2Circular(player.width * 0.55f, player.height * 0.65f);
        Dust dust = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
            Main.rand.NextVector2Circular(0.45f, 0.45f), 100,
            ChromaStonePrismHelper.GetSpectrumColor(offset.Length() * 0.012f + radianceRatio * 2.6f),
            Main.rand.NextFloat(0.95f, 1.12f + radianceRatio * 0.34f));
        dust.noGravity = true;
    }

    public override void ModifyHurt(Player player, OmnitrixPlayer omp, ref Player.HurtModifiers modifiers) {
        modifiers.Knockback *= 0.85f;
    }

    public override void OnHurt(Player player, OmnitrixPlayer omp, Player.HurtInfo info) {
        player.GetModPlayer<ChromaStoneStatePlayer>().AddRadianceFromDamage(info.Damage);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);
        float radianceRatio = state.RadianceRatio;
        float powerRatio = Math.Max(radianceRatio, state.FacetPowerRatio * 0.4f);

        if (omp.ultimateAttack) {
            if (HasActiveOwnedProjectile(player, UltimateAttack))
                return false;

            int consumedFacets = state.ConsumeAllFacetsForDischarge();
            float dischargePowerRatio = radianceRatio;
            state.ConsumeRadiance(ChromaStoneStatePlayer.FullSpectrumDischargeRadianceCost);
            state.StartFullSpectrumDischarge(consumedFacets, dischargePowerRatio);
            KillOwnedProjectiles(player,
                SecondaryAttack,
                ModContent.ProjectileType<ChromaStoneGuardProjectile>());

            int dischargeDamage = ScaleDamage(damage,
                UltimateAttackModifier * (1f + dischargePowerRatio * 0.32f + consumedFacets * 0.08f));
            Projectile.NewProjectile(source, player.Center, direction, UltimateAttack, dischargeDamage, knockback + 2.2f,
                player.whoAmI, consumedFacets, dischargePowerRatio);

            if (!Main.dedServ) {
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.22f, Volume = 0.84f }, player.Center);
                for (int i = 0; i < 30; i++) {
                    Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(22f, 28f), DustID.WhiteTorch,
                        Main.rand.NextVector2Circular(4.8f, 4.8f), 95,
                        ChromaStonePrismHelper.GetSpectrumColor(i * 0.16f + dischargePowerRatio), Main.rand.NextFloat(1.05f, 1.55f));
                    dust.noGravity = true;
                }
            }

            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            int consumedFacets = state.ConsumeAllFacets(clearPartialCharge: false);
            int lanceDamage = ScaleDamage(damage,
                SecondaryAbilityAttackModifier * (1f + consumedFacets * 0.16f + radianceRatio * 0.22f));
            Projectile.NewProjectile(source, player.MountedCenter + direction * 16f, direction * SecondaryAbilityAttackShootSpeed,
                SecondaryAbilityAttack, lanceDamage, knockback + 2.2f, player.whoAmI, consumedFacets, powerRatio);
            return false;
        }

        if (omp.altAttack) {
            if (HasActiveOwnedProjectile(player, SecondaryAttack))
                return false;

            int beamDamage = ScaleDamage(damage,
                SecondaryAttackModifier * (1f + state.VisibleFacetCount * 0.05f + radianceRatio * 0.18f));
            Projectile.NewProjectile(source, player.Center, direction, SecondaryAttack, beamDamage, knockback + 0.4f,
                player.whoAmI, powerRatio, 0f);
            return false;
        }

        bool prismBolt = state.TryAdvanceVolleyToPrismBolt();
        int volleyMode = prismBolt ? ChromaStoneProjectile.ModePrismBolt : ChromaStoneProjectile.ModeVolleyBolt;
        float volleyMultiplier = prismBolt ? PrimaryAttackModifier * 1.24f : PrimaryAttackModifier;
        int volleyDamage = ScaleDamage(damage, volleyMultiplier);
        float shotSpeed = prismBolt ? PrimaryShootSpeed + 1.8f : PrimaryShootSpeed;
        Projectile.NewProjectile(source, player.MountedCenter + direction * 16f, direction * shotSpeed, PrimaryAttack,
            volleyDamage, knockback + (prismBolt ? 1.1f : 0.4f), player.whoAmI, volleyMode, powerRatio);

        if (prismBolt && state.VisibleFacetCount > 0) {
            int shardDamage = ScaleDamage(damage, PrismBoltSideShardMultiplier);
            for (int i = 0; i < state.VisibleFacetCount; i++) {
                float spread = state.VisibleFacetCount switch {
                    1 => 0f,
                    2 => i == 0 ? -0.2f : 0.2f,
                    _ => MathHelper.Lerp(-0.34f, 0.34f, i / 2f)
                };
                Vector2 shardVelocity = direction.RotatedBy(spread) * Main.rand.NextFloat(shotSpeed - 2.8f, shotSpeed - 0.6f);
                Projectile.NewProjectile(source, player.MountedCenter + direction * 16f, shardVelocity, PrimaryAttack,
                    shardDamage, knockback * 0.6f, player.whoAmI, ChromaStoneProjectile.ModeVolleyShard, powerRatio);
            }
        }

        return false;
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (projectile.type != PrimaryAttack)
            return;

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        int mode = (int)Math.Round(projectile.ai[0]);
        float gain = mode switch {
            ChromaStoneProjectile.ModePrismBolt => 22f,
            ChromaStoneProjectile.ModeVolleyBolt => 12f,
            ChromaStoneProjectile.ModeVolleyShard => 5f,
            _ => 0f
        };

        if (gain <= 0f)
            return;

        gain += Math.Min(6f, damageDone * 0.025f);
        state.AddPrimaryAttackCharge(gain);
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        ChromaStoneStatePlayer state = omp.Player.GetModPlayer<ChromaStoneStatePlayer>();
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        string radianceText = $"Radiance {(int)Math.Round(state.RadianceRatio * 100f)}%";

        return resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"Facets {state.VisibleFacetCount}/3 • {radianceText}"
                : $"Volley + shards • Facets {state.VisibleFacetCount}/3 • {radianceText}",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{state.VisibleFacetCount}/3 Facets • {radianceText}"
                : $"Beam refractions {state.VisibleFacetCount}/3 • {radianceText}",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? $"Guard • {radianceText}"
                : $"Absorb projectiles • {radianceText}",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"Spend Facets • {radianceText}"
                : $"Consumes stored Facets • {radianceText}",
            OmnitrixPlayer.AttackSelection.Ultimate => state.DischargeActive
                ? compact ? "Discharge active" : "Full Spectrum Discharge active"
                : state.HasDischargeThreshold
                    ? compact ? "90% Radiance ready" : $"Ready • {radianceText}"
                    : compact ? $"Need 90% • {radianceText}" : $"Needs 90% Radiance • {radianceText}",
            _ => radianceText
        };
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<ChromaStone>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction;
    }

    private static bool HasActiveOwnedProjectile(Player player, int projectileType) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
                return true;
        }

        return false;
    }

    private static void KillOwnedProjectiles(Player player, params int[] projectileTypes) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active || projectile.owner != player.whoAmI)
                continue;

            for (int typeIndex = 0; typeIndex < projectileTypes.Length; typeIndex++) {
                if (projectile.type != projectileTypes[typeIndex])
                    continue;

                projectile.Kill();
                break;
            }
        }
    }

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return Math.Max(1, (int)Math.Round(baseDamage * multiplier));
    }
}
