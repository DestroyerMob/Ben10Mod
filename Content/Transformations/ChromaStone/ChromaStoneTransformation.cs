using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.ChromaStone;

public class ChromaStoneTransformation : Transformation {
    private const float SecondaryLanceRadianceCost = 12f;
    private const float RadianceBurstRadianceCost = 24f;
    private const float SpectrumOverloadRadianceCost = 60f;
    private const int RadianceBurstEnergyCost = 28;
    private const int RadianceBurstCooldown = 16 * 60;
    private const int CrystalGuardRetaliationBaseDamage = 28;
    private const int CrystalGuardRetaliationShardCount = 6;

    public override string FullID => AlienIdentityPlayer.ChromaStoneTransformationId;
    public override string TransformationName => "Chromastone";
    public override string IconPath => "Ben10Mod/Content/Interface/ChromaStoneSelect";
    public override int TransformationBuffId => ModContent.BuffType<ChromaStone_Buff>();
    public override string Description => ChromaStone.TransformationDescription;
    public override List<string> Abilities => new(ChromaStone.TransformationAbilities);

    public override string PrimaryAttackName => "Prism Bolt";
    public override string SecondaryAttackName => "Prism Lance";
    public override string PrimaryAbilityName => "Crystal Guard";
    public override string SecondaryAbilityAttackName => "Radiance Burst";
    public override string UltimateAttackName => "Spectrum Overload";

    public override int PrimaryAttack => ModContent.ProjectileType<ChromaStoneProjectile>();
    public override int PrimaryAttackSpeed => 13;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.84f;

    public override int SecondaryAttack => ModContent.ProjectileType<ChromaStoneLanceProjectile>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 18;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.18f;
    public override int SecondaryArmorPenetration => 14;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 10 * 60;
    public override int PrimaryAbilityCooldown => 30 * 60;
    public override int PrimaryAbilityCost => 20;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<ChromaStoneRadianceBurstProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 1.08f;
    public override int SecondaryAbilityAttackEnergyCost => RadianceBurstEnergyCost;
    public override int SecondaryAbilityCooldown => RadianceBurstCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<ChromaStoneSupernovaProjectile>();
    public override int UltimateAttackSpeed => 30;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => 1.95f;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 52 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        float radianceRatio = identity.ChromaStoneRadianceRatio;
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(radianceRatio * 2.2f, omp.PrimaryAbilityEnabled ? 1.1f : 0.95f);

        player.GetDamage<HeroDamage>() += 0.1f + radianceRatio * 0.12f;
        player.GetCritChance<HeroDamage>() += 4f + radianceRatio * 5f;
        player.GetArmorPenetration<HeroDamage>() += 6 + (int)Math.Round(radianceRatio * 10f);
        player.GetKnockback<HeroDamage>() += 0.35f + radianceRatio * 0.35f;
        player.statDefense += 8 + (int)Math.Round(radianceRatio * 7f);
        player.endurance += 0.04f + radianceRatio * 0.04f;
        player.moveSpeed += 0.06f;
        player.jumpSpeedBoost += 0.7f;
        player.noFallDmg = true;
        player.buffImmune[BuffID.Obstructed] = true;
        Lighting.AddLight(player.Center, prismColor.ToVector3() * (0.25f + radianceRatio * 0.12f));

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.statDefense += 18;
        player.endurance += 0.08f;
        player.noKnockback = true;
        player.moveSpeed *= 0.82f;
        player.runAcceleration *= 0.84f;
        player.maxRunSpeed *= 0.86f;
        player.armorEffectDrawShadow = true;
    }

    public override bool TryGetTransformationTint(Player player, OmnitrixPlayer omp, out Color tint,
        out float blendStrength, out bool forceFullBright) {
        float radianceRatio = player.GetModPlayer<AlienIdentityPlayer>().ChromaStoneRadianceRatio;
        tint = ChromaStonePrismHelper.GetSpectrumColor(radianceRatio * 2f + player.miscCounter / 90f, 1.05f);
        blendStrength = 0.07f + radianceRatio * 0.12f + (omp.PrimaryAbilityEnabled ? 0.05f : 0f);
        forceFullBright = omp.PrimaryAbilityEnabled && radianceRatio >= 0.7f;
        return blendStrength > 0f;
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (Main.dedServ || !Main.rand.NextBool(omp.PrimaryAbilityEnabled ? 2 : 4))
            return;

        float radianceRatio = player.GetModPlayer<AlienIdentityPlayer>().ChromaStoneRadianceRatio;
        Vector2 offset = Main.rand.NextVector2Circular(player.width * 0.48f, player.height * 0.56f);
        Dust dust = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
            Main.rand.NextVector2Circular(0.3f, 0.3f), 100,
            ChromaStonePrismHelper.GetSpectrumColor(offset.Length() * 0.01f + radianceRatio), Main.rand.NextFloat(0.85f, 1.18f));
        dust.noGravity = true;
    }

    public override void OnHurt(Player player, OmnitrixPlayer omp, Player.HurtInfo info) {
        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        float radianceGain = 4f + info.Damage * (omp.PrimaryAbilityEnabled ? 0.95f : 0.38f);
        identity.AddChromaStoneRadiance(Math.Min(34f, radianceGain));

        if (!omp.PrimaryAbilityEnabled)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer)
            return;

        float radianceRatio = identity.ChromaStoneRadianceRatio;
        int retaliationDamage = Math.Max(1,
            (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(CrystalGuardRetaliationBaseDamage + radianceRatio * 18f)));

        for (int i = 0; i < CrystalGuardRetaliationShardCount; i++) {
            float angle = MathHelper.TwoPi * i / CrystalGuardRetaliationShardCount + Main.rand.NextFloat(-0.08f, 0.08f);
            Vector2 direction = angle.ToRotationVector2();
            int projectileIndex = Projectile.NewProjectile(player.GetSource_FromThis(), player.Center + direction * 16f,
                direction * Main.rand.NextFloat(13f, 16f), PrimaryAttack, retaliationDamage, 1.8f, player.whoAmI,
                MathHelper.Clamp(radianceRatio + 0.15f, 0f, 1f), 1f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                Main.projectile[projectileIndex].timeLeft = Math.Min(Main.projectile[projectileIndex].timeLeft, 54);
        }
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        float radianceRatio = identity.ChromaStoneRadianceRatio;
        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.ultimateAttack) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 targetPosition = Main.MouseWorld;
            int finalDamage = Math.Max(1,
                (int)Math.Round(damage * UltimateAttackModifier * (1f + radianceRatio * 0.34f)));
            Projectile.NewProjectile(source, targetPosition, Vector2.Zero, UltimateAttack, finalDamage, knockback + 2f,
                player.whoAmI, radianceRatio, omp.PrimaryAbilityEnabled ? 1f : 0f);
            identity.ConsumeChromaStoneRadiance(SpectrumOverloadRadianceCost);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 targetPosition = Main.MouseWorld;
            int finalDamage = Math.Max(1,
                (int)Math.Round(damage * SecondaryAbilityAttackModifier * (1f + radianceRatio * 0.26f)));
            Projectile.NewProjectile(source, targetPosition, Vector2.Zero, SecondaryAbilityAttack, finalDamage,
                knockback + 1.6f, player.whoAmI, radianceRatio, omp.PrimaryAbilityEnabled ? 1f : 0f);
            identity.ConsumeChromaStoneRadiance(RadianceBurstRadianceCost);
            return false;
        }

        if (omp.altAttack) {
            int finalDamage = Math.Max(1,
                (int)Math.Round(damage * SecondaryAttackModifier * (1f + radianceRatio * 0.22f)));
            Projectile.NewProjectile(source, player.Center + direction * 18f, direction * SecondaryShootSpeed, SecondaryAttack,
                finalDamage, knockback + 1.4f, player.whoAmI, radianceRatio, omp.PrimaryAbilityEnabled ? 1f : 0f);
            identity.ConsumeChromaStoneRadiance(SecondaryLanceRadianceCost);
            return false;
        }

        int boltCount = omp.PrimaryAbilityEnabled
            ? (radianceRatio >= 0.72f ? 3 : 2)
            : (radianceRatio >= 0.85f ? 2 : 1);
        float perBoltScale = boltCount switch {
            3 => 0.58f,
            2 => 0.76f,
            _ => 1f
        };
        int primaryDamage = Math.Max(1,
            (int)Math.Round(damage * PrimaryAttackModifier * (1f + radianceRatio * 0.16f) * perBoltScale));

        for (int i = 0; i < boltCount; i++) {
            float spread = boltCount switch {
                3 when i == 0 => -0.12f,
                3 when i == 2 => 0.12f,
                2 when i == 0 => -0.07f,
                2 => 0.07f,
                _ => 0f
            };
            Vector2 shotVelocity = direction.RotatedBy(spread) * PrimaryShootSpeed;
            Projectile.NewProjectile(source, player.Center + direction * 14f, shotVelocity, PrimaryAttack, primaryDamage,
                knockback, player.whoAmI, radianceRatio, omp.PrimaryAbilityEnabled ? 1f : 0f);
        }

        return false;
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsChromaStoneProjectile(projectile.type))
            return;

        AlienIdentityPlayer identity = player.GetModPlayer<AlienIdentityPlayer>();
        float gain = projectile.type switch {
            _ when projectile.type == PrimaryAttack => omp.PrimaryAbilityEnabled ? 6f : 4f,
            _ when projectile.type == SecondaryAttack => 9f,
            _ when projectile.type == SecondaryAbilityAttack => 7f,
            _ when projectile.type == UltimateAttack => 5f,
            _ => 0f
        };
        gain += Math.Min(10f, damageDone * 0.028f);
        identity.AddChromaStoneRadiance(gain);
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<ChromaStone>();
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

    private static bool IsChromaStoneProjectile(int projectileType) {
        return projectileType == ModContent.ProjectileType<ChromaStoneProjectile>() ||
               projectileType == ModContent.ProjectileType<ChromaStoneLanceProjectile>() ||
               projectileType == ModContent.ProjectileType<ChromaStoneRadianceBurstProjectile>() ||
               projectileType == ModContent.ProjectileType<ChromaStoneSupernovaProjectile>();
    }
}
