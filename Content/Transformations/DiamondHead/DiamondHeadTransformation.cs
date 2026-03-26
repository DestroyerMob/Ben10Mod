using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.DiamondHead;

public class DiamondHeadTransformation : Transformation {
    private const int BarrageShardCount = 5;
    private const float BarrageSpread = 0.3f;
    private const int BulwarkRetaliationBaseDamage = 28;
    private const int BulwarkRetaliationShardCount = 8;
    private const int BulwarkRetaliationCooldown = 24;

    public override string FullID => "Ben10Mod:DiamondHead";
    public override string TransformationName => "Diamondhead";
    public override string IconPath => "Ben10Mod/Content/Interface/DiamondHeadSelect";
    public override int TransformationBuffId => ModContent.BuffType<DiamondHead_Buff>();

    public override string Description =>
        "A durable Petrosapien that controls the lane with piercing shard fire, fortified crystal plating, and crushing prism strikes from any angle.";

    public override List<string> Abilities => new() {
        "Piercing crystal shard primary",
        "Wide shard barrage secondary",
        "Crystalline bulwark stance",
        "Prism pincer crush attack",
        "Falling giant diamond ultimate"
    };

    public override string PrimaryAttackName => "Crystal Shard";
    public override string SecondaryAttackName => "Shard Barrage";
    public override string SecondaryAbilityAttackName => "Prism Pincer";
    public override string UltimateAttackName => "Diamond Drop";

    public override int PrimaryAttack => ModContent.ProjectileType<DiamondHeadProjectile>();
    public override float PrimaryAttackModifier => 0.66f;
    public override int PrimaryAttackSpeed => 10;
    public override int PrimaryShootSpeed => 22;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int PrimaryArmorPenetration => 18;

    public override int SecondaryAttack => ModContent.ProjectileType<DiamondHeadProjectile>();
    public override float SecondaryAttackModifier => 0.3f;
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 18;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryArmorPenetration => 10;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 12 * 60;
    public override int PrimaryAbilityCooldown => 36 * 60;
    public override int PrimaryAbilityCost => 20;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<DiamondHeadPrismPincerProjectile>();
    public override float SecondaryAbilityAttackModifier => 0.7f;
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override int SecondaryAbilityAttackEnergyCost => 25;
    public override int SecondaryAbilityCooldown => 20 * 60;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<GiantDiamondProjectile>();
    public override float UltimateAttackModifier => 2.4f;
    public override int UltimateAttackSpeed => 26;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override int UltimateArmorPenetration => 24;
    public override int UltimateEnergyCost => 55;
    public override int UltimateAbilityCooldown => 42 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.12f;
        player.statDefense += 14;
        player.endurance += 0.05f;
        player.GetArmorPenetration<HeroDamage>() += 12;
        player.GetKnockback<HeroDamage>() += 0.35f;
        player.noKnockback = true;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.statDefense += 24;
        player.endurance += 0.12f;
        player.lifeRegen += 8;
        player.moveSpeed *= 0.38f;
        player.runAcceleration *= 0.45f;
        player.maxRunSpeed *= 0.6f;
        player.jumpSpeedBoost -= 1.4f;
        player.gravity *= 1.15f;
        player.wingTime = 0;
        player.wingTimeMax = 0;
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        player.velocity = new Vector2(
            MathHelper.Clamp(player.velocity.X, -1.2f, 1.2f),
            player.velocity.Y
        );

        Lighting.AddLight(player.Center, new Vector3(0.22f, 0.34f, 0.48f));
    }

    public override void OnHurt(Player player, OmnitrixPlayer omp, Player.HurtInfo info) {
        if (!omp.PrimaryAbilityEnabled)
            return;

        DiamondHeadPlayer diamondHeadPlayer = player.GetModPlayer<DiamondHeadPlayer>();
        if (diamondHeadPlayer.BulwarkRetaliationCooldown > 0)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer)
            return;

        int retaliationDamage = Math.Max(1,
            (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(BulwarkRetaliationBaseDamage)));

        for (int i = 0; i < BulwarkRetaliationShardCount; i++) {
            float angle = MathHelper.TwoPi * i / BulwarkRetaliationShardCount;
            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(9f, 12.5f);
            Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, velocity,
                ModContent.ProjectileType<DiamondHeadProjectile>(), retaliationDamage, 2.5f, player.whoAmI);
        }

        diamondHeadPlayer.BulwarkRetaliationCooldown = BulwarkRetaliationCooldown;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        TransformationAttackProfile profile = GetSelectedAttackProfile(omp);
        if (profile == null || profile.ProjectileType <= 0)
            return false;

        int finalDamage = Math.Max(1, (int)Math.Round(damage * profile.DamageMultiplier));
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));
        float projectileSpeed = velocity.Length();
        if (projectileSpeed <= 0f)
            projectileSpeed = profile.ShootSpeed > 0f ? profile.ShootSpeed : 18f;

        if (omp.IsSecondaryAbilityAttackLoaded) {
            Vector2 target = Main.MouseWorld;
            Vector2[] approachDirections = {
                Vector2.UnitX,
                -Vector2.UnitX,
                Vector2.UnitY,
                -Vector2.UnitY
            };

            for (int i = 0; i < approachDirections.Length; i++) {
                Vector2 approachDirection = approachDirections[i];
                float spawnDistance = Math.Abs(approachDirection.Y) > 0f ? 148f : 176f;
                Vector2 spawnPosition = target + approachDirection * spawnDistance;
                Vector2 inwardVelocity = -approachDirection * (Math.Abs(approachDirection.Y) > 0f ? 15.5f : 17.5f);
                int prismDamage = Math.Max(1, (int)Math.Round(finalDamage * 0.48f));

                Projectile.NewProjectile(source, spawnPosition, inwardVelocity,
                    ModContent.ProjectileType<DiamondHeadPrismPincerProjectile>(), prismDamage, knockback + 1.5f,
                    player.whoAmI, target.X, target.Y);
            }

            return false;
        }

        if (omp.ultimateAttack) {
            Vector2 target = Main.MouseWorld;
            Vector2 spawnPosition = target - new Vector2(0f, 520f);
            Projectile.NewProjectile(source, spawnPosition, new Vector2(0f, 10f), UltimateAttack,
                finalDamage, knockback + 2f, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            for (int i = 0; i < BarrageShardCount; i++) {
                float spreadOffset = MathHelper.Lerp(-BarrageSpread, BarrageSpread,
                    BarrageShardCount == 1 ? 0.5f : i / (float)(BarrageShardCount - 1));
                Vector2 shardVelocity = direction.RotatedBy(spreadOffset) * projectileSpeed;
                Projectile.NewProjectile(source, player.MountedCenter + direction * 16f, shardVelocity,
                    ModContent.ProjectileType<DiamondHeadProjectile>(), finalDamage, knockback, player.whoAmI);
            }

            return false;
        }

        return ShootAttackProfile(player, source, profile, player.MountedCenter + direction * 14f,
            direction * projectileSpeed, damage, knockback);
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.PrimaryAbilityEnabled || Main.rand.NextBool(2))
            return;

        Vector2 offset = Main.rand.NextVector2Circular(player.width * 0.42f, player.height * 0.5f);
        Dust dust = Dust.NewDustPerfect(player.Center + offset, DustID.GemDiamond,
            Main.rand.NextVector2Circular(0.5f, 0.5f), 110, new Color(210, 255, 255), Main.rand.NextFloat(0.95f, 1.25f));
        dust.noGravity = true;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<DiamondHead>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        player.back = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Back);
    }

    public override IReadOnlyList<string> GetPalettePreviewBaseTexturePaths(OmnitrixPlayer omp) => new[] {
        "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Back",
        "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Legs",
        "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Body",
        "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Head"
    };

    public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => new[] {
        new TransformationPaletteChannel(
            "eyes",
            "Eyes",
            new Color(255, 255, 255),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Head",
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHeadEyesMask_Head")
        ),
        new TransformationPaletteChannel(
            "diamond",
            "Diamond",
            new Color(255, 255, 255),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Head",
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHeadDiamondMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Body",
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHeadDiamondMask_Body")
        ),
    };
}

public class DiamondHeadPlayer : ModPlayer {
    public int BulwarkRetaliationCooldown;

    public override void PostUpdate() {
        if (BulwarkRetaliationCooldown > 0)
            BulwarkRetaliationCooldown--;
    }
}
