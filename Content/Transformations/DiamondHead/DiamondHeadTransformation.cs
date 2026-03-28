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
    private const int SecondaryGroundSearchAttempts = 10;
    private const float SecondaryBloomSpread = 176f;
    private const float SecondaryGroundSearchAbove = 6f * 16f;
    private const float SecondaryGroundSearchBelow = 28f * 16f;
    private const int SecondarySpireClearanceWidth = 34;
    private const int SecondarySpireClearanceHeight = 96;
    private const float SecondaryMinimumUpwardAim = 20f;
    private const float SecondaryMaxLeanRadians = 1.08f;
    private const float BodyBloomOriginOffset = 6f;
    private const int BulwarkRetaliationBaseDamage = 28;
    private const int BulwarkRetaliationShardCount = 8;
    private const int BulwarkRetaliationCooldown = 24;

    public override string FullID => "Ben10Mod:DiamondHead";
    public override string TransformationName => "Diamondhead";
    public override string IconPath => "Ben10Mod/Content/Interface/DiamondHeadSelect";
    public override int TransformationBuffId => ModContent.BuffType<DiamondHead_Buff>();

    public override string Description =>
        "A durable Petrosapien that controls the lane with piercing shard fire, ground-burst crystal blooms, fortified crystal plating, and crushing prism strikes from any angle.";

    public override List<string> Abilities => new() {
        "Piercing crystal shards for precise ranged pressure",
        "Crystal blooms that burst from the ground",
        "Crystalline Bulwark for heavier armor and retaliation",
        "Prism Pincer that crushes a point from both sides",
        "Diamond Drop that calls a massive crystal down from above"
    };

    public override string PrimaryAttackName => "Crystal Shard";
    public override string SecondaryAttackName => "Crystal Bloom";
    public override string PrimaryAbilityName => "Crystalline Bulwark";
    public override string SecondaryAbilityAttackName => "Prism Pincer";
    public override string UltimateAttackName => "Diamond Drop";

    public override int PrimaryAttack => ModContent.ProjectileType<DiamondHeadProjectile>();
    public override float PrimaryAttackModifier => 0.66f;
    public override int PrimaryAttackSpeed => 10;
    public override int PrimaryShootSpeed => 22;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int PrimaryArmorPenetration => 18;

    public override int SecondaryAttack => ModContent.ProjectileType<DiamondHeadSpireProjectile>();
    public override float SecondaryAttackModifier => 0.3f;
    public override int SecondaryAttackSpeed => 24;
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
            Vector2 target = Main.MouseWorld;
            if (!TrySpawnGroundBloom(source, player, target, finalDamage, knockback))
                SpawnBodyFallback(source, player, target, direction, projectileSpeed, finalDamage, knockback);

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
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHeadDiamondMask_Body"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHead_Back",
                "Ben10Mod/Content/Transformations/DiamondHead/DiamondHeadDiamondMask_Back")
        ),
    };

    private static bool TrySpawnGroundBloom(IEntitySource source, Player player, Vector2 target, int damage,
        float knockback) {
        if (!TryFindGroundBloomSpawn(target, out Vector2 spawnCenter, out float groundY))
            return false;

        float rotation = ComputeBloomRotation(new Vector2(spawnCenter.X, groundY), target);
        Projectile.NewProjectile(source, spawnCenter, Vector2.Zero,
            ModContent.ProjectileType<DiamondHeadSpireProjectile>(), damage, knockback, player.whoAmI, rotation, groundY);
        return true;
    }

    private static void SpawnBodyFallback(IEntitySource source, Player player, Vector2 target, Vector2 directionHint,
        float projectileSpeed, int damage, float knockback) {
        if (CanBodyShotReachTarget(player, target)) {
            SpawnFallbackShard(source, player, target, directionHint, projectileSpeed, damage, knockback);
            return;
        }

        SpawnBodyBloom(source, player, damage, knockback);
    }

    private static bool CanBodyShotReachTarget(Player player, Vector2 target) {
        Vector2 origin = player.MountedCenter;
        return Collision.CanHitLine(origin, 2, 2, target, 2, 2);
    }

    private static void SpawnFallbackShard(IEntitySource source, Player player, Vector2 target, Vector2 directionHint,
        float projectileSpeed, int damage, float knockback) {
        Vector2 spawnPosition = player.MountedCenter;
        Vector2 shardDirection = (target - spawnPosition).SafeNormalize(directionHint);
        Projectile.NewProjectile(source, spawnPosition + shardDirection * 16f, shardDirection * projectileSpeed,
            ModContent.ProjectileType<DiamondHeadProjectile>(), damage, knockback, player.whoAmI);
    }

    private static void SpawnBodyBloom(IEntitySource source, Player player, int damage, float knockback) {
        Vector2 bodyOrigin = player.MountedCenter + new Vector2(
            Main.rand.NextFloat(-player.width * 0.22f, player.width * 0.22f),
            Main.rand.NextFloat(-player.height * 0.2f, player.height * 0.16f));
        float rotation = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
        Vector2 growthDirection = (rotation - MathHelper.PiOver2).ToRotationVector2();
        bodyOrigin += growthDirection * BodyBloomOriginOffset;

        Projectile.NewProjectile(source, bodyOrigin, Vector2.Zero,
            ModContent.ProjectileType<DiamondHeadSpireProjectile>(), damage, knockback, player.whoAmI, rotation, bodyOrigin.Y);
    }

    private static bool TryFindGroundBloomSpawn(Vector2 target, out Vector2 spawnCenter, out float groundY) {
        for (int attempt = 0; attempt < SecondaryGroundSearchAttempts; attempt++) {
            float candidateX = target.X + Main.rand.NextFloat(-SecondaryBloomSpread * 0.5f, SecondaryBloomSpread * 0.5f);
            if (TryFindGroundBloomSpawnAt(new Vector2(candidateX, target.Y), out spawnCenter, out groundY))
                return true;
        }

        return TryFindGroundBloomSpawnAt(target, out spawnCenter, out groundY);
    }

    private static bool TryFindGroundBloomSpawnAt(Vector2 target, out Vector2 spawnCenter, out float groundY) {
        spawnCenter = default;
        groundY = 0f;

        int tileX = (int)(target.X / 16f);
        int startTileY = (int)Math.Floor((target.Y - SecondaryGroundSearchAbove) / 16f);
        int endTileY = (int)Math.Ceiling((target.Y + SecondaryGroundSearchBelow) / 16f);

        for (int tileY = startTileY; tileY <= endTileY; tileY++) {
            if (!WorldGen.InWorld(tileX, tileY, 10))
                continue;

            Tile tile = Framing.GetTileSafely(tileX, tileY);
            if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                continue;

            Tile tileAbove = Framing.GetTileSafely(tileX, tileY - 1);
            if (tileAbove.HasTile && Main.tileSolid[tileAbove.TileType] && !Main.tileSolidTop[tileAbove.TileType])
                continue;

            float surfaceY = tileY * 16f;
            if (tile.IsHalfBlock)
                surfaceY += 8f;

            Vector2 clearanceTopLeft = new(target.X - SecondarySpireClearanceWidth * 0.5f,
                surfaceY - SecondarySpireClearanceHeight);
            if (Collision.SolidCollision(clearanceTopLeft, SecondarySpireClearanceWidth, SecondarySpireClearanceHeight))
                continue;

            groundY = surfaceY;
            spawnCenter = new Vector2(target.X, surfaceY - DiamondHeadSpireProjectile.BaseHeight * 0.5f);
            return true;
        }

        return false;
    }

    private static float ComputeBloomRotation(Vector2 groundPoint, Vector2 target) {
        Vector2 aim = target - groundPoint;
        if (aim.LengthSquared() <= 1f)
            aim = -Vector2.UnitY;

        if (aim.Y > -SecondaryMinimumUpwardAim)
            aim.Y = -SecondaryMinimumUpwardAim;

        aim.Normalize();
        float rotation = aim.ToRotation() + MathHelper.PiOver2;
        return MathHelper.Clamp(MathHelper.WrapAngle(rotation), -SecondaryMaxLeanRadians, SecondaryMaxLeanRadians);
    }
}

public class DiamondHeadPlayer : ModPlayer {
    public int BulwarkRetaliationCooldown;

    public override void PostUpdate() {
        if (BulwarkRetaliationCooldown > 0)
            BulwarkRetaliationCooldown--;
    }
}
