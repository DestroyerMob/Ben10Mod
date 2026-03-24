using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Terraspin;

public class TerraspinTransformation : Transformation {
    private const int TempestPrisonEnergyCost = 25;
    private const int TempestPrisonCooldown = 16 * 60;
    private const float LiftOffHoverHeight = 32f;
    private const float SustainedHoverHeight = 40f;

    public override string FullID => "Ben10Mod:Terraspin";
    public override string TransformationName => "Terraspin";
    public override int TransformationBuffId => ModContent.BuffType<Terraspin_Buff>();

    public override string Description =>
        "A Geochelone Aerio who hovers on turbine winds, spins his shell into crushing gales, and controls fights with layered air pressure.";

    public override List<string> Abilities => new() {
        "Lift-off wind cannon",
        "Shell-spin gale",
        "Shell turbine hover",
        "Cyclone pocket",
        "Tempest spiral ultimate"
    };

    public override string PrimaryAttackName => "Wind Cannon";
    public override string SecondaryAttackName => "Gale Spin";
    public override string SecondaryAbilityAttackName => "Cyclone Pocket";
    public override string UltimateAttackName => "Tempest Spiral";

    public override int PrimaryAttack => ModContent.ProjectileType<TerraspinGustProjectile>();
    public override int PrimaryAttackSpeed => 20;
    public override int PrimaryShootSpeed => 14;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 1.05f;

    public override int SecondaryAttack => ModContent.ProjectileType<TerraspinBurstProjectile>();
    public override int SecondaryAttackSpeed => 24;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAttackModifier => 1.2f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 10 * 60;
    public override int PrimaryAbilityCooldown => 30 * 60;
    public override int PrimaryAbilityCost => 20;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<TerraspinVortexFieldProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 0.9f;
    public override int SecondaryAbilityAttackEnergyCost => TempestPrisonEnergyCost;
    public override int SecondaryAbilityCooldown => TempestPrisonCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<TerraspinUltimateProjectile>();
    public override int UltimateAttackSpeed => 26;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override float UltimateAttackModifier => 2.1f;
    public override int UltimateEnergyCost => 60;
    public override int UltimateAbilityCooldown => 50 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        TerraspinHoverPlayer hoverPlayer = player.GetModPlayer<TerraspinHoverPlayer>();
        bool hoverActive = omp.PrimaryAbilityEnabled || hoverPlayer.IsLiftOffHoverActive;

        player.GetDamage<HeroDamage>() += 0.1f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.statDefense += 8;
        player.endurance += 0.04f;
        player.moveSpeed += 0.1f;
        player.maxRunSpeed += 0.8f;
        player.jumpSpeedBoost += 1.4f;
        player.noFallDmg = true;

        if (!hoverActive)
            return;

        player.GetAttackSpeed<HeroDamage>() += 0.14f;
        player.moveSpeed += 0.18f;
        player.maxRunSpeed += 1.2f;
        player.jumpSpeedBoost += 2f;
        player.gravity = 0f;
        player.maxFallSpeed = 0f;
        player.endurance += 0.05f;
        player.armorEffectDrawShadow = true;
        Lighting.AddLight(player.Center, new Vector3(0.26f, 0.34f, 0.38f));
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        TerraspinHoverPlayer hoverPlayer = player.GetModPlayer<TerraspinHoverPlayer>();
        if (!omp.PrimaryAbilityEnabled && !hoverPlayer.IsLiftOffHoverActive)
            return;

        ApplyShellTurbineHover(player, hoverPlayer, omp.PrimaryAbilityEnabled);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = ResolveAimDirection(player, velocity);

        if (omp.ultimateAttack) {
            Projectile.NewProjectile(source, player.Center + direction * 26f, direction,
                ModContent.ProjectileType<TerraspinUltimateProjectile>(), damage, knockback + 2f, player.whoAmI);
            return false;
        }

        if (omp.IsSecondaryAbilityAttackLoaded) {
            Vector2 fieldCenter = player.Center + direction * 118f;
            Projectile.NewProjectile(source, fieldCenter, Vector2.Zero, ModContent.ProjectileType<TerraspinVortexFieldProjectile>(),
                damage, knockback, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            int burstDamage = System.Math.Max(1, (int)System.Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, player.Center + direction * 12f, Vector2.Zero,
                ModContent.ProjectileType<TerraspinBurstProjectile>(), burstDamage, knockback + 1.5f, player.whoAmI);
            return false;
        }

        player.GetModPlayer<TerraspinHoverPlayer>().BeginLiftOffHover(player);

        Vector2 windCannonSpawnCenter = GetWindCannonSpawnCenter(player);
        int projectileIndex = Projectile.NewProjectile(source, windCannonSpawnCenter, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<TerraspinGustProjectile>(), damage, knockback + 0.5f, player.whoAmI);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Main.projectile[projectileIndex].Center = windCannonSpawnCenter;
            Main.projectile[projectileIndex].netUpdate = true;
        }

        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.SilverHelmet;
        player.body = ArmorIDs.Body.SilverChainmail;
        player.legs = ArmorIDs.Legs.SilverGreaves;
        if (omp.PrimaryAbilityEnabled)
            player.armorEffectDrawShadow = true;
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

    private static Vector2 GetWindCannonSpawnCenter(Player player) {
        return player.Center + new Vector2(0f, -player.height * 0.18f);
    }

    private static void ApplyShellTurbineHover(Player player, TerraspinHoverPlayer hoverPlayer, bool sustainedHover) {
        UpdateHoverAnchor(player, hoverPlayer, sustainedHover);
        HoldHoverHeight(player, hoverPlayer.HoverBottomY);

        player.fallStart = (int)(player.position.Y / 16f);
        player.maxFallSpeed = sustainedHover ? 5.8f : 4.6f;
    }

    private static void UpdateHoverAnchor(Player player, TerraspinHoverPlayer hoverPlayer, bool sustainedHover) {
        float hoverHeight = sustainedHover ? SustainedHoverHeight : LiftOffHoverHeight;
        if (TryFindHoverSurface(player, out float surfaceTop)) {
            float desiredBottomY = surfaceTop - hoverHeight;
            hoverPlayer.HoverBottomY = hoverPlayer.HasHoverAnchor
                ? MathHelper.Lerp(hoverPlayer.HoverBottomY, desiredBottomY, 0.35f)
                : desiredBottomY;
            hoverPlayer.HasHoverAnchor = true;
            return;
        }

        if (!hoverPlayer.HasHoverAnchor) {
            hoverPlayer.HoverBottomY = player.Bottom.Y;
            hoverPlayer.HasHoverAnchor = true;
        }
    }

    private static void HoldHoverHeight(Player player, float targetBottomY) {
        float delta = targetBottomY - player.Bottom.Y;
        float desiredVelocity = MathHelper.Clamp(delta * 0.18f, -3.1f, 3.1f);
        float smoothing = System.Math.Abs(delta) < 1.5f ? 0.2f : 0.34f;
        player.velocity.Y = MathHelper.Lerp(player.velocity.Y, desiredVelocity, smoothing);

        if (System.Math.Abs(delta) < 0.35f)
            player.velocity.Y *= 0.35f;
    }

    private static bool TryFindHoverSurface(Player player, out float surfaceTop) {
        int startTileY = (int)System.Math.Floor(player.Bottom.Y / 16f);
        int leftTileX = (int)System.Math.Floor((player.position.X + 2f) / 16f);
        int centerTileX = (int)System.Math.Floor(player.Center.X / 16f);
        int rightTileX = (int)System.Math.Floor((player.position.X + player.width - 2f) / 16f);

        for (int tileY = startTileY; tileY <= startTileY + 10; tileY++) {
            if (TryGetSurfaceTop(leftTileX, tileY, out surfaceTop) ||
                TryGetSurfaceTop(centerTileX, tileY, out surfaceTop) ||
                TryGetSurfaceTop(rightTileX, tileY, out surfaceTop))
                return true;
        }

        surfaceTop = 0f;
        return false;
    }

    private static bool TryGetSurfaceTop(int tileX, int tileY, out float surfaceTop) {
        Tile tile = Framing.GetTileSafely(tileX, tileY);
        if (!tile.HasTile) {
            surfaceTop = 0f;
            return false;
        }

        if (WorldGen.SolidTileAllowBottomSlope(tileX, tileY)) {
            surfaceTop = tileY * 16f;
            return true;
        }

        if (!Main.tileSolidTop[tile.TileType]) {
            surfaceTop = 0f;
            return false;
        }

        surfaceTop = tileY * 16f;
        return true;
    }
}

public class TerraspinHoverPlayer : ModPlayer {
    private const int LiftOffDuration = 22;
    private const float LiftOffHeight = 32f;

    public int LiftOffHoverTime;
    public float HoverBottomY;
    public bool HasHoverAnchor;

    public bool IsLiftOffHoverActive => LiftOffHoverTime > 0;

    public void BeginLiftOffHover(Player player) {
        LiftOffHoverTime = LiftOffDuration;
        HoverBottomY = player.Bottom.Y - LiftOffHeight;
        HasHoverAnchor = false;
    }

    public void ClearHoverAnchor() {
        HasHoverAnchor = false;
    }

    public override void PostUpdate() {
        if (LiftOffHoverTime > 0)
            LiftOffHoverTime--;
    }

    public override void UpdateDead() {
        LiftOffHoverTime = 0;
        HoverBottomY = 0f;
        HasHoverAnchor = false;
    }
}
