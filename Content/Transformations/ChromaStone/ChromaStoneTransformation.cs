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
    private const float PrismBarrageDamageMultiplier = 0.78f;
    private const float SpectrumBeamDamageMultiplier = 0.44f;
    private const float FacetBonusShotMultiplier = 0.32f;

    public override string FullID => AlienIdentityPlayer.ChromaStoneTransformationId;
    public override string TransformationName => "Chromastone";
    public override string IconPath => "Ben10Mod/Content/Interface/ChromaStoneSelect";
    public override int TransformationBuffId => ModContent.BuffType<ChromaStone_Buff>();
    public override string Description => ChromaStone.TransformationDescription;
    public override List<string> Abilities => new(ChromaStone.TransformationAbilities);

    public override string PrimaryAttackName => "Prism Barrage";
    public override string SecondaryAttackName => "Spectrum Beam";
    public override string PrimaryAbilityName => "Facet Dash";
    public override string SecondaryAbilityName => "Refraction Guard";
    public override string TertiaryAbilityName => "Resonant Facets";
    public override string UltimateAbilityName => "Full Spectrum Overload";

    public override int PrimaryAttack => ModContent.ProjectileType<ChromaStoneProjectile>();
    public override int PrimaryAttackSpeed => 8;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => PrismBarrageDamageMultiplier;

    public override int SecondaryAttack => ModContent.ProjectileType<ChromaStoneBeamProjectile>();
    public override int SecondaryAttackSpeed => 14;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override bool SecondaryChannel => true;
    public override float SecondaryAttackModifier => SpectrumBeamDamageMultiplier;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 1;
    public override int PrimaryAbilityCooldown => ChromaStoneStatePlayer.FacetDashCooldownTicks;

    public override bool HasSecondaryAbility => true;
    public override int SecondaryAbilityDuration => 1;
    public override int SecondaryAbilityCooldown => ChromaStoneStatePlayer.RefractionGuardCooldownTicks;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => ChromaStoneStatePlayer.FullSpectrumOverloadDurationTicks;
    public override int UltimateAbilityCooldown => ChromaStoneStatePlayer.FullSpectrumOverloadCooldownTicks;

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<ChromaStoneBeamProjectile>(),
            ModContent.ProjectileType<ChromaStoneDashHitboxProjectile>(),
            ModContent.ProjectileType<ChromaStoneGuardProjectile>(),
            ModContent.ProjectileType<ChromaStoneFacetProjectile>());
        player.GetModPlayer<ChromaStoneStatePlayer>().ResetTransientState();
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        float chargeRatio = identity.ChromaStonePrismChargeRatio;
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(chargeRatio * 2.1f + player.miscCounter / 100f,
            state.OverloadActive ? 1.12f : 1f);

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.03f;
        player.statDefense += 12;
        player.runAcceleration *= 0.86f;
        player.maxRunSpeed *= 0.94f;
        player.noFallDmg = true;
        Lighting.AddLight(player.Center, prismColor.ToVector3() * (0.26f + chargeRatio * 0.12f));

        if (state.Guarding) {
            player.statDefense += 10;
            player.endurance += 0.06f;
            player.noKnockback = true;
            player.moveSpeed *= 0.22f;
            player.runAcceleration *= 0.14f;
            player.maxRunSpeed *= 0.34f;
        }

        if (state.DashActive) {
            player.noKnockback = true;
            player.armorEffectDrawShadow = true;
        }

        if (!state.OverloadActive)
            return;

        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetAttackSpeed<HeroDamage>() += 0.18f;
        player.statDefense += 4;
        player.armorEffectDrawShadow = true;
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        ChromaStoneStatePlayer state = omp.Player.GetModPlayer<ChromaStoneStatePlayer>();
        if (!state.OverloadActive)
            return;

        if (omp.setAttack == OmnitrixPlayer.AttackSelection.Primary)
            item.useTime = item.useAnimation = Math.Max(6, item.useTime - 2);
    }

    public override bool? CanUseItem(Player player, OmnitrixPlayer omp, Item item) {
        bool? baseResult = base.CanUseItem(player, omp, item);
        if (baseResult.HasValue && !baseResult.Value)
            return baseResult;

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        if (state.Guarding || state.DashActive)
            return false;

        return baseResult;
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        if (!base.CanStartCurrentAttack(player, omp))
            return false;

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        return !state.Guarding && !state.DashActive;
    }

    public override bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) {
        Vector2 direction = ResolveAimDirection(player, new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
        player.GetModPlayer<ChromaStoneStatePlayer>().TryStartFacetDash(direction);
        return true;
    }

    public override bool TryActivateSecondaryAbility(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<ChromaStoneStatePlayer>().TryStartRefractionGuard();
        return true;
    }

    public override bool TryActivateUltimateAbility(Player player, OmnitrixPlayer omp) {
        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();

        if (player.HasBuff<UltimateAbility>() ||
            player.HasBuff<UltimateAbilityCooldown>() ||
            omp.ultimateAttack ||
            omp.HasLoadedAbilityAttack) {
            return true;
        }

        if (!state.HasFullCharge)
            return true;

        identity.ConsumeChromaStonePrismCharge(identity.ChromaStonePrismCharge);
        state.RestoreAllFacets();
        player.AddBuff(ModContent.BuffType<UltimateAbility>(), ChromaStoneStatePlayer.FullSpectrumOverloadDurationTicks);
        omp.ultimateAbilityTransformationId = FullID;

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.22f, Volume = 0.78f }, player.Center);
            for (int i = 0; i < 26; i++) {
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(18f, 24f), DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(4.2f, 4.2f), 95,
                    ChromaStonePrismHelper.GetSpectrumColor(i * 0.19f), Main.rand.NextFloat(1.05f, 1.55f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    public override bool TryGetTransformationTint(Player player, OmnitrixPlayer omp, out Color tint,
        out float blendStrength, out bool forceFullBright) {
        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        float chargeRatio = player.GetModPlayer<AlienIdentityPlayer>().ChromaStonePrismChargeRatio;
        tint = ChromaStonePrismHelper.GetSpectrumColor(chargeRatio * 2f + player.miscCounter / 90f,
            state.OverloadActive ? 1.12f : 1.02f);
        blendStrength = 0.08f + chargeRatio * 0.12f + (state.OverloadActive ? 0.08f : 0f);
        forceFullBright = state.OverloadActive && chargeRatio >= 0.6f;
        return blendStrength > 0f;
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<ChromaStoneStatePlayer>().UpdateDashMovement();
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (Main.dedServ)
            return;

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        float chargeRatio = player.GetModPlayer<AlienIdentityPlayer>().ChromaStonePrismChargeRatio;
        int spawnRate = state.OverloadActive ? 2 : state.VisibleFacetCount > 0 ? 3 : 5;
        if (!Main.rand.NextBool(spawnRate))
            return;

        Vector2 offset = Main.rand.NextVector2Circular(player.width * 0.5f, player.height * 0.6f);
        Dust dust = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
            Main.rand.NextVector2Circular(0.4f, 0.4f), 100,
            ChromaStonePrismHelper.GetSpectrumColor(offset.Length() * 0.01f + chargeRatio), Main.rand.NextFloat(0.88f, 1.2f));
        dust.noGravity = true;
    }

    public override void ModifyHurt(Player player, OmnitrixPlayer omp, ref Player.HurtModifiers modifiers) {
        modifiers.Knockback *= 0.85f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.altAttack) {
            if (HasActiveOwnedProjectile(player, SecondaryAttack))
                return false;

            int beamDamage = ScaleDamage(damage, SecondaryAttackModifier * (state.OverloadActive ? 1.12f : 1f));
            Projectile.NewProjectile(source, player.Center, direction, SecondaryAttack, beamDamage, knockback + 0.4f,
                player.whoAmI);
            return false;
        }

        int mode = state.OverloadActive ? 2 : 0;
        int primaryDamage = ScaleDamage(damage, PrimaryAttackModifier * (state.OverloadActive ? 1.1f : 1f));
        float shotSpeed = state.OverloadActive ? PrimaryShootSpeed + 3f : PrimaryShootSpeed;
        Projectile.NewProjectile(source, player.MountedCenter + direction * 16f, direction * shotSpeed, PrimaryAttack,
            primaryDamage, knockback, player.whoAmI, identity.ChromaStonePrismChargeRatio, mode);

        int facetShots = state.VisibleFacetCount;
        if (facetShots > 0) {
            int bonusDamage = ScaleDamage(damage, FacetBonusShotMultiplier * (state.OverloadActive ? 1.12f : 1f));
            for (int i = 0; i < facetShots; i++) {
                float spread = facetShots switch {
                    1 => 0f,
                    2 => i == 0 ? -0.14f : 0.14f,
                    _ => MathHelper.Lerp(-0.22f, 0.22f, i / 2f)
                };
                Vector2 bonusVelocity = direction.RotatedBy(spread) * Main.rand.NextFloat(shotSpeed - 2.2f, shotSpeed - 0.4f);
                Projectile.NewProjectile(source, player.Center + direction * 12f, bonusVelocity, PrimaryAttack, bonusDamage,
                    knockback * 0.7f, player.whoAmI, identity.ChromaStonePrismChargeRatio, 1f);
            }
        }

        return false;
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsChromaStoneProjectile(projectile.type))
            return;

        ChromaStoneStatePlayer state = player.GetModPlayer<ChromaStoneStatePlayer>();
        if (state.OverloadActive)
            return;

        float gain = projectile.type switch {
            _ when projectile.type == PrimaryAttack && projectile.ai[1] >= 0.5f => 1.2f,
            _ when projectile.type == PrimaryAttack => 3.2f,
            _ when projectile.type == SecondaryAttack => 1.6f,
            _ when projectile.type == ModContent.ProjectileType<ChromaStoneDashHitboxProjectile>() => 5.4f,
            _ => 0f
        };
        gain += Math.Min(projectile.type == SecondaryAttack ? 3.6f : 6f, damageDone * (projectile.type == SecondaryAttack ? 0.012f : 0.02f));
        player.GetModPlayer<AlienIdentityPlayer>().AddChromaStonePrismCharge(gain);
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

    private static bool IsChromaStoneProjectile(int projectileType) {
        return projectileType == ModContent.ProjectileType<ChromaStoneProjectile>() ||
               projectileType == ModContent.ProjectileType<ChromaStoneBeamProjectile>() ||
               projectileType == ModContent.ProjectileType<ChromaStoneDashHitboxProjectile>();
    }

    private static int ScaleDamage(int baseDamage, float multiplier) {
        return Math.Max(1, (int)Math.Round(baseDamage * multiplier));
    }
}
