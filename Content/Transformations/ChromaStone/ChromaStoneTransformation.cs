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
    private const float PrismBoltSideShardMultiplier = 0.38f;
    private const int SpectrumBeamActivationCost = 6;
    private const int SpectrumBeamSustainCost = 3;
    private const int SpectrumBeamSustainInterval = 12;
    private const int PrismaticLanceEnergyCost = 24;
    private const int FullSpectrumDischargeEnergyCost = 72;

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

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => ChromaStoneStatePlayer.FullSpectrumDischargeDurationTicks;
    public override int UltimateAbilityCooldown => ChromaStoneStatePlayer.FullSpectrumDischargeCooldownTicks;
    public override int UltimateAbilityCost => FullSpectrumDischargeEnergyCost;

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
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(facetPower * 2.2f + player.miscCounter / 110f,
            state.DischargeActive ? 1.14f : 1f);

        player.GetDamage<HeroDamage>() += 0.08f;
        player.statDefense += 12;
        player.noFallDmg = true;
        player.runAcceleration *= 0.82f;
        player.maxRunSpeed *= 0.95f;
        Lighting.AddLight(player.Center, prismColor.ToVector3() * (0.24f + facetPower * 0.2f));

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

        player.GetDamage<HeroDamage>() += 0.14f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.statDefense += 4;
        player.endurance += 0.05f;
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
        if (player.HasBuff<UltimateAbility>() ||
            player.HasBuff<UltimateAbilityCooldown>() ||
            omp.ultimateAttack ||
            omp.HasLoadedAbilityAttack) {
            return true;
        }

        int energyCost = GetUltimateAbilityCost(omp);
        if (omp.omnitrixEnergy < energyCost) {
            omp.ShowTransformFailureFeedback($"Need {energyCost} OE for {UltimateAbilityName}.");
            return true;
        }

        omp.omnitrixEnergy -= energyCost;
        int consumedFacets = state.ConsumeAllFacetsForDischarge();
        state.StartFullSpectrumDischarge(consumedFacets);
        KillOwnedProjectiles(player,
            SecondaryAttack,
            ModContent.ProjectileType<ChromaStoneGuardProjectile>());

        player.AddBuff(ModContent.BuffType<UltimateAbility>(), ChromaStoneStatePlayer.FullSpectrumDischargeDurationTicks);
        omp.ultimateAbilityTransformationId = FullID;

        if (player.whoAmI == Main.myPlayer &&
            !HasActiveOwnedProjectile(player, ModContent.ProjectileType<ChromaStoneSupernovaProjectile>())) {
            int dischargeDamage = state.ResolveHeroDamage(0.56f + consumedFacets * 0.09f);
            Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.UnitX * player.direction,
                ModContent.ProjectileType<ChromaStoneSupernovaProjectile>(), dischargeDamage, 4.6f, player.whoAmI,
                consumedFacets, state.FacetPowerRatio);
        }

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.25f, Volume = 0.82f }, player.Center);
            for (int i = 0; i < 30; i++) {
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(22f, 28f), DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(4.8f, 4.8f), 95,
                    ChromaStonePrismHelper.GetSpectrumColor(i * 0.16f), Main.rand.NextFloat(1.05f, 1.55f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    public override bool TryGetTransformationTint(Player player, OmnitrixPlayer omp, out Color tint,
        out float blendStrength, out bool forceFullBright) {
        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        float facetPower = state.FacetPowerRatio;
        tint = ChromaStonePrismHelper.GetSpectrumColor(facetPower * 2f + player.miscCounter / 110f,
            state.DischargeActive ? 1.16f : 1.03f);
        blendStrength = 0.08f + facetPower * 0.16f + (state.DischargeActive ? 0.08f : 0f);
        forceFullBright = state.DischargeActive && facetPower >= 0.6f;
        return blendStrength > 0f;
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<ChromaStoneStatePlayer>().ApplyHoverControl();
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (Main.dedServ)
            return;

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        float facetPower = state.FacetPowerRatio;
        int spawnRate = state.DischargeActive ? 1 : state.VisibleFacetCount > 0 ? 3 : 5;
        if (!Main.rand.NextBool(spawnRate))
            return;

        Vector2 offset = Main.rand.NextVector2Circular(player.width * 0.55f, player.height * 0.65f);
        Dust dust = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
            Main.rand.NextVector2Circular(0.45f, 0.45f), 100,
            ChromaStonePrismHelper.GetSpectrumColor(offset.Length() * 0.012f + facetPower), Main.rand.NextFloat(0.9f, 1.22f));
        dust.noGravity = true;
    }

    public override void ModifyHurt(Player player, OmnitrixPlayer omp, ref Player.HurtModifiers modifiers) {
        modifiers.Knockback *= 0.85f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);
        float powerRatio = state.FacetPowerRatio;

        if (omp.IsSecondaryAbilityAttackLoaded) {
            int consumedFacets = state.ConsumeAllFacets(clearPartialCharge: false);
            int lanceDamage = ScaleDamage(damage, SecondaryAbilityAttackModifier * (1f + consumedFacets * 0.16f));
            Projectile.NewProjectile(source, player.MountedCenter + direction * 16f, direction * SecondaryAbilityAttackShootSpeed,
                SecondaryAbilityAttack, lanceDamage, knockback + 2.2f, player.whoAmI, consumedFacets, powerRatio);
            return false;
        }

        if (omp.altAttack) {
            if (HasActiveOwnedProjectile(player, SecondaryAttack))
                return false;

            int beamDamage = ScaleDamage(damage, SecondaryAttackModifier * (1f + state.VisibleFacetCount * 0.05f));
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
