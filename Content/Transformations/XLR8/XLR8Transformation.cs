using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.XLR8;

public class XLR8Transformation : Transformation {
    public const string TransformationId = "Ben10Mod:XLR8";

    private const int VectorDashEnergyCost = 18;
    private const int VectorDashCooldown = 14 * 60;
    private const float BaseAttackUseTimeMultiplier = 0.88f;
    private const float OverdriveAttackUseTimeMultiplier = 0.72f;
    private const float BaseVectorDashRange = 420f;
    private const float OverdriveVectorDashRange = 600f;
    private const int PrimaryStrikeBurstCount = 5;
    private const int PrimaryStrikeSpacing = 2;
    private const int PotisVectorDashEnergyCost = 15;
    private const int PotisVectorDashCooldown = 11 * 60;
    private const float PotisBaseAttackUseTimeMultiplier = 0.78f;
    private const float PotisOverdriveAttackUseTimeMultiplier = 0.62f;
    private const float PotisBaseVectorDashRange = 540f;
    private const float PotisOverdriveVectorDashRange = 760f;
    private const int PotisPrimaryStrikeBurstCount = 7;
    private const int PotisPrimaryStrikeSpacing = 1;
    private const float PotisPrimaryDamageMultiplier = 0.72f;
    private const float PotisSecondaryDamageMultiplier = 1.18f;
    private const float PotisVectorDashDamageMultiplier = 1.08f;
    private const int PotisPrimaryAttackSpeed = 8;
    private const int PotisPrimaryShootSpeed = 22;
    private const int PotisSecondaryAttackSpeed = 66;
    private const int PotisSecondaryShootSpeed = 16;
    private const int PotisUltimateAbilityCost = 75;
    private const int PotisUltimateAbilityDuration = 5 * 60;
    private const int PotisUltimateAbilityCooldown = 52 * 60;

    public override string FullID                  => TransformationId;
    public override string TransformationName      => "XLR8";
    public override string IconPath                => "Ben10Mod/Content/Interface/XLR8Select";
    public override int    TransformationBuffId    => ModContent.BuffType<XLR8_Buff>();
    public override string Description =>
        "A Kineceleran momentum assassin built around ground-speed execution, direction-change strike windows, precision dashes, and timebreak pass-throughs.";

    public override List<string> Abilities => new() {
        "Speed Strike hits harder shortly after reversing direction",
        "Velocity Dash damage scales with recent ground speed",
        "Overdrive opens stronger speed windows",
        "Vector Dash to the cursor, scaling with distance and momentum",
        "Water running at speed",
        "Temporal Distortion rewards passing through enemies and projectiles"
    };

    public override string PrimaryAttackName       => "Speed Strike";
    public override string SecondaryAttackName     => "Velocity Dash";
    public override string PrimaryAbilityName      => "Overdrive";
    public override string SecondaryAbilityAttackName => "Vector Dash";
    public override string UltimateAbilityName     => "Temporal Distortion";
    public override int    PrimaryAbilityDuration  => 10 * 60;
    public override int    PrimaryAbilityCooldown  => 30 * 60;
    public override int    PrimaryAttack           => ModContent.ProjectileType<XLR8StarlightProjectile>();
    public override int    PrimaryAttackSpeed      => 10;
    public override int    PrimaryShootSpeed       => 20;
    public override int    PrimaryUseStyle         => ItemUseStyleID.Shoot;
    public override float  PrimaryAttackModifier   => 0.5f;
    public override int    SecondaryAttack         => ModContent.ProjectileType<XLR8DashProjectile>();
    public override int    SecondaryAttackSpeed    => 82;
    public override int    SecondaryShootSpeed     => 14;
    public override int    SecondaryUseStyle       => ItemUseStyleID.Shoot;
    public override float  SecondaryAttackModifier => 1.35f;
    public override int    SecondaryAbilityAttack => ModContent.ProjectileType<XLR8VectorDashProjectile>();
    public override int    SecondaryAbilityAttackSpeed => 16;
    public override int    SecondaryAbilityAttackShootSpeed => 0;
    public override int    SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float  SecondaryAbilityAttackModifier => 1.18f;
    public override int    SecondaryAbilityAttackEnergyCost => VectorDashEnergyCost;
    public override int    SecondaryAbilityCooldown => VectorDashCooldown;
    public override bool   SecondaryAbilityAttackSingleUse => true;
    public override bool   HasUltimateAbility      => true;
    public override int    UltimateAbilityCost     => 85;
    public override int    UltimateAbilityDuration => 4 * 60;
    public override int    UltimateAbilityCooldown => 60 * 60;

    public override int GetMoveSetIndex(OmnitrixPlayer omp) => HasPotisAltiare(omp?.Player) ? 1 : 0;

    public override string GetDescription(OmnitrixPlayer omp) {
        if (!HasPotisAltiare(omp?.Player))
            return Description;

        return $"{Description} Potis Altiare turns XLR8 into a slipstream duelist with denser reversal flurries, safer piercing dashes, longer cursor vectors, and a brighter timebreak field.";
    }

    public override List<string> GetAbilities(OmnitrixPlayer omp) {
        if (!HasPotisAltiare(omp?.Player))
            return base.GetAbilities(omp);

        return new List<string> {
            "Afterimage Flurry releases denser Potis speed-strikes that spike after a clean direction change.",
            "Slipstream Cut is a longer, safer piercing dash whose payoff rises with speed.",
            "Slipstream Overdrive makes XLR8 faster, sharper, and better at chaining reversal windows.",
            "Chrono Vector reaches farther, costs less Omnitrix energy, and rewards long cursor vectors.",
            "Water running at speed.",
            "Timebreak Field extends the battlefield freeze and builds flow when XLR8 passes through threats."
        };
    }

    public override string GetAbilitySelectionDisplayName(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp) {
        if (!HasPotisAltiare(omp?.Player))
            return base.GetAbilitySelectionDisplayName(selection, omp);

        return selection switch {
            OmnitrixPlayer.AttackSelection.PrimaryAbility => "Slipstream Overdrive",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => "Chrono Vector",
            OmnitrixPlayer.AttackSelection.Ultimate => "Timebreak Field",
            _ => base.GetAbilitySelectionDisplayName(selection, omp)
        };
    }

    public override int GetSecondaryAbilityCooldown(OmnitrixPlayer omp) {
        int cooldown = HasPotisAltiare(omp?.Player) ? PotisVectorDashCooldown : VectorDashCooldown;
        return ApplyAbilityCooldownMultiplier(cooldown, omp.secondaryAbilityCooldownMultiplier);
    }

    public override int GetUltimateAbilityCost(OmnitrixPlayer omp) {
        return HasPotisAltiare(omp?.Player) ? PotisUltimateAbilityCost : UltimateAbilityCost;
    }

    public override int GetUltimateAbilityDuration(OmnitrixPlayer omp) {
        return HasPotisAltiare(omp?.Player) ? PotisUltimateAbilityDuration : UltimateAbilityDuration;
    }

    public override int GetUltimateAbilityCooldown(OmnitrixPlayer omp) {
        int cooldown = HasPotisAltiare(omp?.Player) ? PotisUltimateAbilityCooldown : UltimateAbilityCooldown;
        return ApplyAbilityCooldownMultiplier(cooldown, omp.ultimateAbilityCooldownMultiplier);
    }

    protected override IReadOnlyList<TransformationAttackProfile> GetPrimaryAttackProfiles() {
        return CreateMoveSetProfiles(
            CreatePrimaryAttackProfile(),
            new TransformationAttackProfile {
                DisplayName = "Afterimage Flurry",
                ProjectileType = PrimaryAttack,
                DamageMultiplier = PotisPrimaryDamageMultiplier,
                UseTime = PotisPrimaryAttackSpeed,
                ShootSpeed = PotisPrimaryShootSpeed,
                UseStyle = PrimaryUseStyle,
                Channel = false,
                NoMelee = true,
                ArmorPenetration = PrimaryArmorPenetration + 4
            });
    }

    protected override IReadOnlyList<TransformationAttackProfile> GetSecondaryAttackProfiles() {
        return CreateMoveSetProfiles(
            CreateSecondaryAttackProfile(),
            new TransformationAttackProfile {
                DisplayName = "Slipstream Cut",
                ProjectileType = SecondaryAttack,
                DamageMultiplier = PotisSecondaryDamageMultiplier,
                UseTime = PotisSecondaryAttackSpeed,
                ShootSpeed = PotisSecondaryShootSpeed,
                UseStyle = SecondaryUseStyle,
                Channel = false,
                NoMelee = true,
                ArmorPenetration = SecondaryArmorPenetration + 5
            });
    }

    protected override IReadOnlyList<TransformationAttackProfile> GetSecondaryAbilityAttackProfiles() {
        return CreateMoveSetProfiles(
            CreateSecondaryAbilityAttackProfile(),
            new TransformationAttackProfile {
                DisplayName = "Chrono Vector",
                ProjectileType = SecondaryAbilityAttack,
                DamageMultiplier = PotisVectorDashDamageMultiplier,
                UseTime = SecondaryAbilityAttackSpeed,
                ShootSpeed = SecondaryAbilityAttackShootSpeed,
                UseStyle = SecondaryAbilityAttackUseStyle,
                Channel = false,
                NoMelee = true,
                ArmorPenetration = SecondaryAbilityAttackArmorPenetration + 5,
                EnergyCost = PotisVectorDashEnergyCost,
                SingleUse = true
            });
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        XLR8MomentumPlayer momentum = player.GetModPlayer<XLR8MomentumPlayer>();
        bool potis = HasPotisAltiare(player);
        bool overdrive = omp.PrimaryAbilityEnabled;
        player.moveSpeed *= overdrive ? potis ? 5.65f : 5.2f : potis ? 3.05f : 2.7f;
        player.accRunSpeed *= overdrive ? potis ? 4.65f : 4.3f : potis ? 2.45f : 2.2f;
        player.runAcceleration *= overdrive ? potis ? 2.35f : 2.15f : potis ? 1.62f : 1.45f;
        player.maxRunSpeed += overdrive ? potis ? 2.65f : 2.2f : potis ? 1.42f : 1.1f;
        player.GetAttackSpeed(DamageClass.Generic) += overdrive ? potis ? 1.42f : 1.25f : potis ? 0.88f : 0.75f;
        player.GetCritChance<HeroDamage>() += overdrive ? potis ? 20f : 16f : potis ? 11f : 8f;
        player.pickSpeed *= omp.PrimaryAbilityEnabled ? 0.45f : 0.65f;
        player.tileSpeed *= omp.PrimaryAbilityEnabled ? 0.45f : 0.65f;
        player.wallSpeed *= omp.PrimaryAbilityEnabled ? 0.45f : 0.65f;
        player.jumpSpeedBoost += overdrive ? potis ? 3.35f : 3f : potis ? 1.9f : 1.6f;

        if (!momentum.MovingRecently) {
            player.GetDamage<HeroDamage>() -= 0.08f;
            player.GetAttackSpeed<HeroDamage>() -= 0.1f;
        }
        else {
            player.GetCritChance<HeroDamage>() += 3f + momentum.MomentumRatio * 5f;
        }

        if (momentum.ReversalReady) {
            player.GetAttackSpeed<HeroDamage>() += potis ? 0.12f : 0.08f;
            player.GetArmorPenetration<HeroDamage>() += potis ? 5 : 3;
        }

        if (momentum.TimebreakStacks > 0) {
            float flowRatio = momentum.TimebreakFlowRatio;
            player.GetDamage<HeroDamage>() += flowRatio * (potis ? 0.16f : 0.12f);
            player.GetAttackSpeed<HeroDamage>() += flowRatio * (potis ? 0.2f : 0.15f);
            player.moveSpeed += flowRatio * 0.24f;
            Lighting.AddLight(player.Center, new Vector3(0.06f, 0.42f, 0.72f) * flowRatio);
        }

        if (potis) {
            player.GetDamage<HeroDamage>() += 0.05f;
            player.GetAttackSpeed<HeroDamage>() += 0.08f;
            player.GetArmorPenetration<HeroDamage>() += 4;
            player.statDefense += 3;
            player.endurance += 0.03f;
            Lighting.AddLight(player.Center, new Vector3(0.1f, 0.78f, 0.95f) * 0.2f);
        }

        if (omp.IsUltimateAbilityActive) {
            player.statDefense += potis ? 8 : 5;
            player.endurance += potis ? 0.08f : 0.04f;
            player.armorEffectDrawShadow = true;
        }

        if (Math.Abs(player.velocity.X) > 2) {
            player.waterWalk =  true;
        }
    }

    public override void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
        base.ModifyPlumbersBadgeStats(item, omp);

        bool potis = HasPotisAltiare(omp.Player);
        float speedMultiplier = omp.PrimaryAbilityEnabled
            ? potis ? PotisOverdriveAttackUseTimeMultiplier : OverdriveAttackUseTimeMultiplier
            : potis ? PotisBaseAttackUseTimeMultiplier : BaseAttackUseTimeMultiplier;
        bool firingPrimary = !omp.altAttack && !omp.IsSecondaryAbilityAttackLoaded && !omp.ultimateAttack;
        int minUseTime = firingPrimary ? potis ? 5 : 7 : potis ? 5 : 6;

        item.useTime = item.useAnimation = Math.Max(minUseTime, (int)Math.Round(item.useTime * speedMultiplier));
    }

    public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
        if (!base.CanStartCurrentAttack(player, omp))
            return false;

        TransformationAttackProfile profile = GetSelectedAttackProfile(omp);
        if (profile?.ProjectileType == ModContent.ProjectileType<XLR8StarlightProjectile>())
            return !HasActiveOwnedProjectile(player, profile.ProjectileType);

        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        bool potis = HasPotisAltiare(player);
        XLR8MomentumPlayer momentum = player.GetModPlayer<XLR8MomentumPlayer>();
        if (!omp.altAttack && !omp.IsSecondaryAbilityAttackLoaded) {
            if (HasActiveOwnedProjectile(player, ModContent.ProjectileType<XLR8StarlightProjectile>()))
                return false;

            Vector2 attackDirection = ResolveAimDirection(player, velocity);
            float strikeMode = potis
                ? omp.PrimaryAbilityEnabled ? 3f : 2f
                : omp.PrimaryAbilityEnabled ? 1f : 0f;
            if (momentum.ReversalReady)
                strikeMode += 4f;

            int burstCount = potis ? PotisPrimaryStrikeBurstCount : PrimaryStrikeBurstCount;
            int strikeSpacing = potis ? PotisPrimaryStrikeSpacing : PrimaryStrikeSpacing;
            int shootSpeed = potis ? PotisPrimaryShootSpeed : PrimaryShootSpeed;
            float spread = potis ? 0.12f : 0.085f;
            int strikeDamage = potis ? Math.Max(1, (int)Math.Round(damage * PotisPrimaryDamageMultiplier)) : damage;

            for (int i = 0; i < burstCount; i++) {
                Vector2 burstDirection = attackDirection.RotatedBy(Main.rand.NextFloat(-spread, spread));
                omp.transformationAttackSerial++;
                int burstProjectileIndex = Projectile.NewProjectile(source,
                    player.MountedCenter + burstDirection * 12f,
                    burstDirection * shootSpeed,
                    ModContent.ProjectileType<XLR8StarlightProjectile>(),
                    strikeDamage,
                    knockback,
                    player.whoAmI,
                    strikeMode,
                    omp.transformationAttackSerial,
                    i * strikeSpacing);

                if (burstProjectileIndex >= 0 && burstProjectileIndex < Main.maxProjectiles)
                    Main.projectile[burstProjectileIndex].netUpdate = true;
            }

            return false;
        }

        if (!omp.IsSecondaryAbilityAttackLoaded) {
            Vector2 dashDirection = ResolveAimDirection(player, velocity);
            float dashPower = momentum.ResolveVelocityDashPower(player);
            int velocityDashDamage = potis ? Math.Max(1, (int)Math.Round(damage * PotisSecondaryDamageMultiplier)) : damage;
            int shootSpeed = potis ? PotisSecondaryShootSpeed : SecondaryShootSpeed;
            int dashProjectileIndex = Projectile.NewProjectile(source,
                player.MountedCenter + dashDirection * 18f,
                dashDirection * shootSpeed,
                SecondaryAttack,
                velocityDashDamage,
                knockback + (potis ? 0.8f : 0.3f),
                player.whoAmI,
                potis ? 1f : 0f,
                dashPower);

            if (dashProjectileIndex >= 0 && dashProjectileIndex < Main.maxProjectiles)
                Main.projectile[dashProjectileIndex].netUpdate = true;

            return false;
        }

        if (Main.netMode == NetmodeID.Server ||
            (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
            return false;

        Vector2 destination = Main.MouseWorld;
        Vector2 offset = destination - player.MountedCenter;
        if (offset == Vector2.Zero)
            offset = new Vector2(player.direction, 0f);

        float maxRange = omp.PrimaryAbilityEnabled
            ? potis ? PotisOverdriveVectorDashRange : OverdriveVectorDashRange
            : potis ? PotisBaseVectorDashRange : BaseVectorDashRange;
        float requestedDistance = Math.Min(offset.Length(), maxRange);
        Vector2 direction = offset.SafeNormalize(new Vector2(player.direction, 0f));
        bool empowered = omp.PrimaryAbilityEnabled;
        float dashSpeed = XLR8VectorDashProjectile.GetDashSpeed(empowered, potis);
        int maxDashFrames = potis ? XLR8VectorDashProjectile.PotisMaxDashFrames : XLR8VectorDashProjectile.MaxDashFrames;
        int dashFrames = Utils.Clamp((int)Math.Ceiling(requestedDistance / dashSpeed),
            XLR8VectorDashProjectile.MinDashFrames, maxDashFrames);
        float vectorDashPower = momentum.ResolveVectorDashPower(requestedDistance, maxRange);
        float damageMultiplier = potis ? PotisVectorDashDamageMultiplier : SecondaryAbilityAttackModifier;
        int vectorDashDamage = Math.Max(1, (int)Math.Round(damage * damageMultiplier));

        int projectileIndex = Projectile.NewProjectile(source, player.MountedCenter + direction * 18f, direction * dashSpeed,
            SecondaryAbilityAttack, vectorDashDamage, knockback + (potis ? 1.35f : 1f), player.whoAmI,
            empowered ? 1f : 0f, potis ? 1f : 0f, vectorDashPower);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
            Projectile projectile = Main.projectile[projectileIndex];
            projectile.timeLeft = dashFrames;
            projectile.netUpdate = true;
        }

        if (!Main.dedServ) {
            int dustCount = potis ? empowered ? 34 : 26 : empowered ? 22 : 16;
            Color dustColor = potis ? new Color(160, 250, 255) : new Color(120, 210, 255);
            for (int i = 0; i < dustCount; i++) {
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(12f, 18f),
                    potis && i % 4 == 0 ? DustID.WhiteTorch : DustID.BlueCrystalShard,
                    direction.RotatedByRandom(potis ? 0.55f : 0.45f) * Main.rand.NextFloat(1.8f, potis ? 6.2f : 4.8f),
                    110, dustColor, Main.rand.NextFloat(1f, potis ? 1.45f : 1.25f));
                dust.noGravity = true;
            }
        }

        SoundEngine.PlaySound(SoundID.Item8 with { Pitch = potis ? 0.42f : 0.3f, Volume = potis ? 0.9f : 0.82f },
            player.Center);
        return false;
    }

    public override bool? CanBeHitByNPC(Player player, OmnitrixPlayer omp, NPC npc, ref int cooldownSlot) {
        if (omp.IsUltimateAbilityActive) {
            player.GetModPlayer<XLR8MomentumPlayer>().RegisterTimebreakEnemyPass(player, npc);
            return false;
        }

        return base.CanBeHitByNPC(player, omp, npc, ref cooldownSlot);
    }

    public override bool? CanBeHitByProjectile(Player player, OmnitrixPlayer omp, Projectile projectile) {
        if (omp.IsUltimateAbilityActive && projectile.hostile) {
            player.GetModPlayer<XLR8MomentumPlayer>().RegisterTimebreakProjectilePass(player, projectile);
            return false;
        }

        return base.CanBeHitByProjectile(player, omp, projectile);
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (projectile.type != PrimaryAttack &&
            projectile.type != SecondaryAttack &&
            projectile.type != SecondaryAbilityAttack)
            return;

        XLR8MomentumPlayer momentum = player.GetModPlayer<XLR8MomentumPlayer>();

        if (projectile.type == PrimaryAttack) {
            modifiers.FinalDamage *= momentum.StationaryDamageScale;
            if (XLR8MomentumPlayer.IsReversalStrike(projectile)) {
                float reversalScale = 1.22f + Math.Max(momentum.MomentumRatio, 0.55f) * 0.18f;
                modifiers.FinalDamage *= reversalScale;
                modifiers.ArmorPenetration += HasPotisAltiare(player) ? 8 : 5;
            }
        }
        else {
            float dashPower = projectile.type == SecondaryAbilityAttack
                ? MathHelper.Clamp(projectile.ai[2], 0f, 1f)
                : MathHelper.Clamp(projectile.ai[1], 0f, 1f);
            modifiers.FinalDamage *= 0.84f + dashPower * 0.56f;
            modifiers.ArmorPenetration += (int)Math.Round(dashPower * (HasPotisAltiare(player) ? 9f : 6f));
        }

        if (momentum.TimebreakStacks > 0)
            modifiers.FinalDamage *= momentum.TimebreakDamageScale;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        if (resolvedSelection != OmnitrixPlayer.AttackSelection.Primary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Secondary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.SecondaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Ultimate)
            return base.GetAttackResourceSummary(selection, omp, compact);

        XLR8MomentumPlayer momentum = omp.Player.GetModPlayer<XLR8MomentumPlayer>();
        string momentumText = compact ? $"Mom {momentum.MomentumPercent}%" : $"Momentum {momentum.MomentumPercent}%";
        string reversalText = momentum.ReversalReady
            ? compact ? "Reverse ready" : "Direction-change strike ready"
            : compact ? "No reverse" : "Reverse direction to spike Speed Strike";
        string flowText = compact
            ? $"Flow {momentum.TimebreakStacks}/{XLR8MomentumPlayer.MaxTimebreakStacks}"
            : $"Timebreak Flow {momentum.TimebreakStacks}/{XLR8MomentumPlayer.MaxTimebreakStacks}";

        string identityText = resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"{momentumText} • {reversalText}"
                : $"{momentumText} • {reversalText} • stationary DPS is weaker",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{momentumText} • Speed scales"
                : $"{momentumText} • dash damage scales with recent ground speed",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"{momentumText} • Distance scales"
                : $"{momentumText} • Vector Dash damage scales with cursor distance and momentum",
            OmnitrixPlayer.AttackSelection.Ultimate => compact
                ? flowText
                : $"{flowText} • pass through enemies and hostile projectiles to build speed payoff",
            _ => momentumText
        };

        string baseText = base.GetAttackResourceSummary(selection, omp, compact);
        return string.IsNullOrWhiteSpace(baseText) ? identityText : $"{baseText} • {identityText}";
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        XLR8MomentumPlayer momentum = player.GetModPlayer<XLR8MomentumPlayer>();
        momentum.UpdateMomentum(player, omp);

        bool potis = HasPotisAltiare(player);
        if ((!potis && !omp.IsUltimateAbilityActive && !momentum.ReversalReady && momentum.TimebreakStacks <= 0) || Main.dedServ)
            return;

        float speed = player.velocity.Length();
        if (speed < 3f && momentum.TimebreakStacks <= 0)
            return;

        int interval = omp.IsUltimateAbilityActive || momentum.TimebreakStacks > 0 ? 1 : 2;
        if (Main.GameUpdateCount % interval != 0)
            return;

        Color trailColor = momentum.TimebreakStacks > 0
            ? new Color(150, 238, 255)
            : potis ? new Color(132, 248, 255) : new Color(96, 184, 255);
        Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(12f, 18f),
            (potis || momentum.TimebreakStacks > 0) && Main.rand.NextBool(3) ? DustID.WhiteTorch : DustID.BlueCrystalShard,
            -player.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.9f, potis ? 3.2f : 2.2f),
            115, trailColor, Main.rand.NextFloat(0.9f, potis ? 1.45f : 1.15f));
        dust.noGravity = true;
        dust.fadeIn = 0.8f;
    }

    public override bool TryGetTransformationTint(Player player, OmnitrixPlayer omp, out Color tint,
        out float blendStrength, out bool forceFullBright) {
        bool potis = HasPotisAltiare(player);
        tint = potis
            ? omp.IsUltimateAbilityActive ? new Color(120, 248, 255) : new Color(80, 220, 255)
            : new Color(80, 180, 255);
        blendStrength = omp.IsUltimateAbilityActive ? potis ? 0.24f : 0.14f : potis ? 0.08f : 0f;
        forceFullBright = omp.IsUltimateAbilityActive && potis;
        return blendStrength > 0f;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<XLR8>();
        player.armorEffectDrawShadow = true;
        player.head                  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        if (omp.PrimaryAbilityEnabled)
            player.head = EquipLoader.GetEquipSlot(Mod, "XLR8_alt", EquipType.Head);
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

    private static bool HasActiveOwnedProjectile(Player player, int projectileType) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
                return true;
        }

        return false;
    }

    private static bool HasPotisAltiare(Player player) {
        return player?.GetModPlayer<PotisAltiarePlayer>()?.potisAltiareEquipped == true;
    }
    
    public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => [
        new TransformationPaletteChannel(
            "base",
            "Base",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Head",
                "Ben10Mod/Content/Transformations/XLR8/XLR8BaseMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Head_alt",
                "Ben10Mod/Content/Transformations/XLR8/XLR8BaseMask_Head_alt"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Body",
                "Ben10Mod/Content/Transformations/XLR8/XLR8BaseMask_Body"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Legs",
                "Ben10Mod/Content/Transformations/XLR8/XLR8BaseMask_Legs")),
        new TransformationPaletteChannel(
            "eye",
            "Eye",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/XLR8/XLR8_Head",
                "Ben10Mod/Content/Transformations/XLR8/XLR8EyeMask_Head"))
    ];
}
