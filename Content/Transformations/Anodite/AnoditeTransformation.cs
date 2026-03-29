using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles.Gwen;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Anodite;

public class AnoditeTransformation : Transformation {
    private const int HexCircleCooldown = 16 * 60;
    private const int BarrierDuration = 8 * 60;
    private const int BarrierCooldown = 22 * 60;
    private const int HaloDuration = 12 * 60;
    private const int HaloCooldown = 60 * 60;

    public override string FullID => "Ben10Mod:Anodite";
    public override string TransformationName => "Anodite";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Anodite_Buff>();
    public override bool IsAccessoryTransformation(OmnitrixPlayer omp) => true;

    public override string Description =>
        "A living mana-form that rides the air, threads targets with raw energy, and shapes shields and wards out of pure anodite power.";

    public override List<string> Abilities => new() {
        "Mana Thread that lashes through targets",
        "Anodite Orb that blooms with arcane energy",
        "Mana Barrier for protection and control",
        "Hex Circle that snares enemies in place",
        "Ascendant Halo that surrounds you with living magic"
    };

    public override string PrimaryAttackName => "Mana Thread";
    public override string SecondaryAttackName => "Anodite Orb";
    public override string PrimaryAbilityName => "Mana Barrier";
    public override string SecondaryAbilityAttackName => "Hex Circle";
    public override string UltimateAbilityName => "Ascendant Halo";
    public override Color TransformTextColor => new(255, 145, 225);

    public override int PrimaryAttack => ModContent.ProjectileType<ManaThreadProjectile>();
    public override int PrimaryAttackSpeed => 16;
    public override int PrimaryShootSpeed => 14;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.92f;

    public override int SecondaryAttack => ModContent.ProjectileType<AnoditeOrbProjectile>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 9;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.18f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => BarrierDuration;
    public override int PrimaryAbilityCooldown => BarrierCooldown;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<HexCircleProjectile>();
    public override int SecondaryAbilityAttackSpeed => 20;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override int SecondaryAbilityCooldown => HexCircleCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;
    public override float SecondaryAbilityAttackModifier => 1.1f;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => HaloDuration;
    public override int UltimateAbilityCooldown => HaloCooldown;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 8f;
        player.GetAttackSpeed<HeroDamage>() += 0.06f;
        player.moveSpeed += 0.14f;
        player.maxRunSpeed += 0.8f;
        player.accRunSpeed += 0.12f;
        player.jumpSpeedBoost += 1.8f;
        player.noFallDmg = true;
        player.ignoreWater = true;
        player.maxFallSpeed *= omp.IsUltimateAbilityActive ? 0.45f : 0.6f;
        player.armorEffectDrawShadow = true;

        if (omp.IsPrimaryAbilityActive) {
            player.statDefense += 8;
            player.endurance += 0.06f;
            player.GetDamage<HeroDamage>() += 0.05f;
        }

        if (omp.IsUltimateAbilityActive) {
            player.GetDamage<HeroDamage>() += 0.12f;
            player.GetCritChance<HeroDamage>() += 6f;
            player.GetAttackSpeed<HeroDamage>() += 0.08f;
            player.moveSpeed += 0.08f;
            player.maxRunSpeed += 0.6f;
        }

        Lighting.AddLight(player.Center, omp.IsUltimateAbilityActive
            ? new Vector3(1.1f, 0.45f, 0.95f)
            : new Vector3(0.78f, 0.32f, 0.72f));
    }

    public override void ModifyDrawInfo(Player player, OmnitrixPlayer omp, ref PlayerDrawSet drawInfo) {
        Color glowColor = omp.IsUltimateAbilityActive
            ? new Color(255, 150, 235)
            : new Color(255, 190, 240);
        byte targetAlpha = omp.IsUltimateAbilityActive ? (byte)145 : (byte)175;
        float tintStrength = omp.IsUltimateAbilityActive ? 0.58f : 0.42f;

        drawInfo.colorArmorHead = TintDrawColor(drawInfo.colorArmorHead, glowColor, tintStrength, targetAlpha);
        drawInfo.colorArmorBody = TintDrawColor(drawInfo.colorArmorBody, glowColor, tintStrength, targetAlpha);
        drawInfo.colorArmorLegs = TintDrawColor(drawInfo.colorArmorLegs, glowColor, tintStrength, targetAlpha);
        drawInfo.colorEyeWhites = TintDrawColor(drawInfo.colorEyeWhites, Color.White, 0.38f, targetAlpha);
        drawInfo.colorEyes = TintDrawColor(drawInfo.colorEyes, new Color(255, 220, 250), 0.48f, targetAlpha);
    }

    public override bool TryGetTransformationTint(Player player, OmnitrixPlayer omp, out Color tint,
        out float blendStrength, out bool forceFullBright) {
        tint = omp.IsUltimateAbilityActive
            ? new Color(255, 105, 225)
            : new Color(255, 135, 220);
        blendStrength = omp.IsUltimateAbilityActive ? 0.94f : 0.86f;
        forceFullBright = true;
        return true;
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();

        if (!Main.rand.NextBool(2)) {
            Vector2 offset = Main.rand.NextVector2Circular(player.width * 0.46f, player.height * 0.55f);
            Color dustColor = omp.IsUltimateAbilityActive
                ? new Color(255, 190, 245)
                : new Color(255, 145, 220);
            Dust dust = Dust.NewDustPerfect(player.Center + offset,
                Main.rand.NextBool(3) ? DustID.GemRuby : DustID.PinkTorch,
                player.velocity * 0.08f + Main.rand.NextVector2Circular(0.45f, 0.45f), 90, dustColor,
                Main.rand.NextFloat(1f, 1.28f));
            dust.noGravity = true;
        }
    }

    public override void SpawnTransformParticles(Player player, OmnitrixPlayer omp) {
        SpawnDustBurst(player, DustID.PinkTorch, Color.White);
    }

    public override void SpawnDetransformParticles(Player player, OmnitrixPlayer omp) {
        SpawnDustBurst(player, DustID.PinkTorch, Color.White);
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        if (player.mount.Active)
            return;

        player.gravity *= omp.IsUltimateAbilityActive ? 0.3f : 0.45f;

        if (player.controlJump)
            player.velocity.Y = Math.Max(player.velocity.Y - (omp.IsUltimateAbilityActive ? 0.24f : 0.16f),
                omp.IsUltimateAbilityActive ? -6.4f : -4.8f);

        if (player.controlDown)
            player.velocity.Y = Math.Min(player.velocity.Y + 0.08f, 5.5f);
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (Main.rand.NextBool(4)) {
            Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(18f, 28f), DustID.PinkTorch,
                Main.rand.NextVector2Circular(0.8f, 0.8f), 80, new Color(255, 155, 225), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer)
            return;

        UpdateBarrier(player, omp);
        UpdateHaloWards(player, omp);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.IsSecondaryAbilityAttackLoaded) {
            Vector2 hexCenter = player.Center + direction * 96f;
            if (Main.netMode != NetmodeID.Server && player.whoAmI == Main.myPlayer)
                hexCenter = Main.MouseWorld;

            int hexDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAbilityAttackModifier));
            SpawnHeroProjectile(source, hexCenter, Vector2.Zero, ModContent.ProjectileType<HexCircleProjectile>(),
                hexDamage, knockback, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            int orbDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            SpawnHeroProjectile(source, player.MountedCenter + direction * 14f, direction * SecondaryShootSpeed,
                ModContent.ProjectileType<AnoditeOrbProjectile>(), orbDamage, knockback + 0.6f, player.whoAmI);
            return false;
        }

        SpawnHeroProjectile(source, player.MountedCenter + direction * 10f, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<ManaThreadProjectile>(), damage, knockback, player.whoAmI);
        return false;
    }

    private static void UpdateBarrier(Player player, OmnitrixPlayer omp) {
        int barrierType = ModContent.ProjectileType<ManaBarrierProjectile>();
        int existingBarrier = FindOwnedProjectile(player.whoAmI, barrierType);

        if (!omp.IsPrimaryAbilityActive) {
            if (existingBarrier >= 0)
                Main.projectile[existingBarrier].Kill();

            return;
        }

        int barrierDamage = Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(28)));
        if (existingBarrier >= 0) {
            Projectile barrier = Main.projectile[existingBarrier];
            barrier.damage = barrierDamage;
            barrier.originalDamage = barrierDamage;
            barrier.timeLeft = 2;
            barrier.DamageType = ModContent.GetInstance<HeroDamage>();
            barrier.netUpdate = true;
            return;
        }

        int projectileIndex = Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
            barrierType, barrierDamage, 5f, player.whoAmI);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Projectile barrier = Main.projectile[projectileIndex];
            barrier.DamageType = ModContent.GetInstance<HeroDamage>();
            barrier.timeLeft = 2;
        }
    }

    private static void UpdateHaloWards(Player player, OmnitrixPlayer omp) {
        int wardType = ModContent.ProjectileType<AegisCharmWardProjectile>();
        if (!omp.IsUltimateAbilityActive)
            return;

        int activeWards = CountOwnedProjectiles(player.whoAmI, wardType);
        int wardDamage = Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(24)));

        for (int i = activeWards; i < 3; i++) {
            float angleOffset = MathHelper.TwoPi * i / 3f;
            int projectileIndex = Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
                wardType, wardDamage, 5f, player.whoAmI, angleOffset);

            if (projectileIndex < 0 || projectileIndex >= Main.maxProjectiles)
                continue;

            Projectile ward = Main.projectile[projectileIndex];
            ward.DamageType = ModContent.GetInstance<HeroDamage>();
            ward.originalDamage = wardDamage;
        }
    }

    private static int SpawnHeroProjectile(IEntitySource source, Vector2 position, Vector2 velocity, int projectileType,
        int damage, float knockback, int owner) {
        int projectileIndex = Projectile.NewProjectile(source, position, velocity, projectileType, damage, knockback, owner);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].DamageType = ModContent.GetInstance<HeroDamage>();

        return projectileIndex;
    }

    private static int FindOwnedProjectile(int owner, int projectileType) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == owner && projectile.type == projectileType)
                return i;
        }

        return -1;
    }

    private static int CountOwnedProjectiles(int owner, int projectileType) {
        int count = 0;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == owner && projectile.type == projectileType)
                count++;
        }

        return count;
    }

    private static Color TintDrawColor(Color baseColor, Color tint, float tintStrength, byte maxAlpha) {
        return new Color(
            (byte)MathHelper.Lerp(baseColor.R, tint.R, tintStrength),
            (byte)MathHelper.Lerp(baseColor.G, tint.G, tintStrength),
            (byte)MathHelper.Lerp(baseColor.B, tint.B, tintStrength),
            (byte)Math.Min(baseColor.A, maxAlpha)
        );
    }
}
