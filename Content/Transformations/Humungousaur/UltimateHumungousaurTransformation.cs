using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Humungousaur;

public class UltimateHumungousaurTransformation : HumungousaurTransformation {
    private const int RocketVolleyCount = 2;
    private const int ChargeRocketVolleyCount = 3;
    private const int CataclysmRocketVolleyCount = 4;
    private const float RocketVolleyDamageMultiplier = 0.8f;
    private const float MeteorStompDamageMultiplier = 1.38f;
    private const float FinisherDamageMultiplier = 1.34f;
    private const float CataclysmFinisherDamageMultiplier = 1.5f;
    private const int BreachDurationTicks = 5 * 60;
    private const int ShatteredDurationTicks = 4 * 60;

    public override string FullID => "Ben10Mod:UltimateHumungousaur";
    public override string TransformationName => "Ultimate Humungousaur";
    public override int TransformationBuffId => ModContent.BuffType<UltimateHumungousaur_Buff>();
    public override Transformation ParentTransformation => ModContent.GetInstance<HumungousaurTransformation>();
    public override Transformation ChildTransformation => null;
    public override bool HasPrimaryAbility => true;

    public override string Description =>
        "A siege-bred Vaxasaurian juggernaut that batters enemies with breach-building combos, crushes them with stomps, and cashes that setup out in explosive cataclysm bursts.";

    public override List<string> Abilities => new() {
        "Siege Combo builds Breach with heavy rocket-assisted punches",
        "Bunker Rockets cashes out broken targets with explosive follow-up shockwaves",
        "Titan Charge turns the form into a faster armored bruiser for a short window",
        "Meteor Stomp erupts shockwaves around you and punishes grouped enemies",
        "Cataclysm Drive overloads the full kit and ends in a massive shutdown pulse"
    };

    public override string PrimaryAttackName => "Siege Combo";
    public override string SecondaryAttackName => "Bunker Rockets";
    public override string PrimaryAbilityName => "Titan Charge";
    public override string SecondaryAbilityAttackName => "Meteor Stomp";
    public override string UltimateAbilityName => "Cataclysm Drive";

    public override int PrimaryAttack => ModContent.ProjectileType<HumungousaurPunchProjectile>();
    public override int PrimaryAttackSpeed => 14;
    public override int PrimaryShootSpeed => 12;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 1.04f;
    public override int PrimaryArmorPenetration => 12;

    public override int SecondaryAttack => ModContent.ProjectileType<UltimateHumungousaurRocketPlayerProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 18;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => RocketVolleyDamageMultiplier;
    public override int SecondaryArmorPenetration => 10;

    public override int PrimaryAbilityDuration => UltimateHumungousaurStatePlayer.TitanChargeDurationTicks;
    public override int PrimaryAbilityCooldown => UltimateHumungousaurStatePlayer.TitanChargeCooldownTicks;
    public override int PrimaryAbilityCost => 18;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>();
    public override int SecondaryAbilityAttackSpeed => 20;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => MeteorStompDamageMultiplier;
    public override int SecondaryAbilityAttackEnergyCost => 26;
    public override int SecondaryAbilityCooldown => UltimateHumungousaurStatePlayer.MeteorStompCooldownTicks;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAbilityCost => 60;
    public override int UltimateAbilityDuration => UltimateHumungousaurStatePlayer.CataclysmDurationTicks;
    public override int UltimateAbilityCooldown => UltimateHumungousaurStatePlayer.CataclysmCooldownTicks;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        UltimateHumungousaurStatePlayer state = player.GetModPlayer<UltimateHumungousaurStatePlayer>();

        player.statDefense += 20;
        player.GetDamage<HeroDamage>() += 0.2f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
        player.GetKnockback<HeroDamage>() += 0.55f;
        player.GetArmorPenetration<HeroDamage>() += 12;
        player.endurance += 0.08f;
        player.noKnockback = true;
        player.moveSpeed += 0.04f;
        player.maxRunSpeed += 0.4f;

        if (state.TitanChargeActive) {
            player.GetDamage<HeroDamage>() += 0.1f;
            player.GetAttackSpeed<HeroDamage>() += 0.12f;
            player.GetArmorPenetration<HeroDamage>() += 8;
            player.moveSpeed += 0.12f;
            player.maxRunSpeed += 1f;
            player.jumpSpeedBoost += 0.8f;
            player.armorEffectDrawShadow = true;
        }

        if (!state.CataclysmActive)
            return;

        player.GetDamage<HeroDamage>() += 0.16f;
        player.GetAttackSpeed<HeroDamage>() += 0.18f;
        player.GetArmorPenetration<HeroDamage>() += 12;
        player.statDefense += 8;
        player.endurance += 0.04f;
        player.moveSpeed += 0.08f;
        player.maxRunSpeed += 0.8f;
        player.armorEffectDrawShadow = true;
    }

    public override string GetDisplayName(OmnitrixPlayer omp) {
        if (omp.IsUltimateAbilityActive)
            return "Ultimate Humungousaur (Cataclysm)";

        return omp.IsPrimaryAbilityActive ? "Ultimate Humungousaur (Titan Charge)" : base.GetDisplayName(omp);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        UltimateHumungousaurStatePlayer state = player.GetModPlayer<UltimateHumungousaurStatePlayer>();
        Vector2 direction = ResolveAimDirection(player, velocity);
        Vector2 forwardSpawn = player.MountedCenter + direction * 22f;

        if (omp.IsSecondaryAbilityAttackLoaded) {
            TriggerMeteorStomp(player, source, damage, knockback, state.CataclysmActive);
            return false;
        }

        if (omp.altAttack) {
            FireRocketVolley(player, source, forwardSpawn, direction, damage, knockback, state);
            return false;
        }

        int comboStep = state.ConsumeComboStep();
        bool finisher = comboStep >= 2;
        float punchScale = finisher
            ? state.CataclysmActive ? 1.42f : 1.26f
            : state.TitanChargeActive ? 1.12f : 1f;
        float damageMultiplier = finisher
            ? state.CataclysmActive ? CataclysmFinisherDamageMultiplier : FinisherDamageMultiplier
            : comboStep == 1 ? 1.08f : 1f;
        int punchDamage = ScaleDamage(damage, damageMultiplier);
        int projectileIndex = Projectile.NewProjectile(source, player.Center + direction * (4f * punchScale),
            direction * PrimaryShootSpeed, PrimaryAttack, punchDamage, knockback + (finisher ? 1.2f : 0.4f),
            player.whoAmI, punchScale, finisher ? 1f : 0f);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].localAI[0] = comboStep;

        if (finisher) {
            SpawnShockwaveBurst(player, source, player.Bottom + new Vector2(0f, -8f), ScaleDamage(damage,
                    state.CataclysmActive ? 0.56f : 0.42f),
                knockback + 0.8f, state.CataclysmActive ? 1.35f : 1.1f, state.CataclysmActive ? 2 : 1);
        }

        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (!IsUltimateHumungousaurProjectile(projectile.type))
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (identity.IsHumungousaurShatteredFor(player.whoAmI)) {
            modifiers.ArmorPenetration += 10;
            modifiers.FinalDamage *= projectile.type == SecondaryAttack ? 1.16f : 1.12f;
        }
        else if (identity.IsHumungousaurBreachedFor(player.whoAmI) && projectile.ai[1] > 0f) {
            modifiers.FinalDamage *= 1.08f;
        }
    }

    public override void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        NPC.HitInfo hit, int damageDone) {
        if (!IsUltimateHumungousaurProjectile(projectile.type))
            return;

        UltimateHumungousaurStatePlayer state = player.GetModPlayer<UltimateHumungousaurStatePlayer>();
        switch (projectile.type) {
            case var _ when projectile.type == PrimaryAttack:
                ApplyBreachHit(player, target, projectile.ai[1] > 0f ? 2 : 1, BreachDurationTicks);
                if (projectile.ai[1] > 0f)
                    TryConsumeShattered(player, target, projectile.GetSource_FromThis(),
                        ScaleDamage(projectile.damage, state.CataclysmActive ? 0.95f : 0.72f), projectile.knockBack + 0.8f,
                        state.CataclysmActive);
                break;
            case var _ when projectile.type == SecondaryAttack:
                ApplyBreachHit(player, target, 2, BreachDurationTicks);
                TryConsumeShattered(player, target, projectile.GetSource_FromThis(),
                    ScaleDamage(projectile.damage, state.CataclysmActive ? 1.12f : 0.86f), projectile.knockBack + 0.9f,
                    state.CataclysmActive);
                break;
            case var _ when projectile.type == SecondaryAbilityAttack:
                ApplyBreachHit(player, target, projectile.ai[1] >= 2f ? 2 : 1, BreachDurationTicks);
                TryConsumeShattered(player, target, projectile.GetSource_FromThis(),
                    ScaleDamage(projectile.damage, projectile.ai[1] >= 2f ? 1f : 0.7f), projectile.knockBack + 0.9f,
                    state.CataclysmActive);
                break;
        }
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.PlatinumHelmet;
        player.body = ArmorIDs.Body.PlatinumChainmail;
        player.legs = ArmorIDs.Legs.PlatinumGreaves;
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        UltimateHumungousaurStatePlayer state = player.GetModPlayer<UltimateHumungousaurStatePlayer>();
        if (!state.TitanChargeActive && !state.CataclysmActive)
            return;

        if (Main.rand.NextBool(state.CataclysmActive ? 2 : 3)) {
            int dustType = state.CataclysmActive ? DustID.Torch : DustID.Smoke;
            Color dustColor = state.CataclysmActive ? new Color(255, 170, 105) : new Color(220, 170, 120);
            Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, dustType, Scale: state.CataclysmActive ? 1.35f : 1.1f);
            dust.velocity *= 0.25f;
            dust.color = dustColor;
            dust.noGravity = true;
        }
    }

    internal static void TriggerCataclysmShutdownPulse(Player player) {
        if (player == null || !player.active || player.dead || player.whoAmI != Main.myPlayer)
            return;

        int pulseDamage = ResolveHeroDamage(player, 0.72f);
        SpawnShockwaveBurst(player, player.GetSource_FromThis(), player.Bottom + new Vector2(0f, -8f), pulseDamage, 7f, 1.55f, 2,
            variant: 3f);

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.18f, Volume = 0.78f }, player.Center);
            for (int i = 0; i < 30; i++) {
                Dust dust = Dust.NewDustPerfect(player.Bottom + Main.rand.NextVector2Circular(42f, 14f),
                    i % 3 == 0 ? DustID.Smoke : DustID.Torch,
                    new Vector2(Main.rand.NextFloat(-4.6f, 4.6f), Main.rand.NextFloat(-3.8f, -0.2f)),
                    110, new Color(255, 175, 110), Main.rand.NextFloat(1.15f, 1.6f));
                dust.noGravity = true;
            }
        }
    }

    private static void TriggerMeteorStomp(Player player, IEntitySource source, int damage, float knockback, bool cataclysm) {
        float scale = cataclysm ? 1.4f : 1.16f;
        int stompDamage = ScaleDamage(damage, cataclysm ? 1.16f : 1f);
        SpawnShockwaveBurst(player, source, player.Bottom + new Vector2(0f, -8f), stompDamage, knockback + 1.4f, scale, 2,
            variant: cataclysm ? 2f : 1f);
        player.velocity.Y = Math.Min(player.velocity.Y, cataclysm ? -5.2f : -3.6f);
        player.fallStart = (int)(player.position.Y / 16f);

        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.08f, Volume = 0.65f }, player.Center);
        for (int i = 0; i < 20; i++) {
            Dust dust = Dust.NewDustPerfect(player.Bottom + Main.rand.NextVector2Circular(32f, 10f),
                i % 4 == 0 ? DustID.Smoke : DustID.Torch,
                new Vector2(Main.rand.NextFloat(-3.5f, 3.5f), Main.rand.NextFloat(-2.6f, -0.1f)),
                115, new Color(255, 160, 105), Main.rand.NextFloat(1.05f, 1.42f));
            dust.noGravity = true;
        }
    }

    private static void FireRocketVolley(Player player, IEntitySource source, Vector2 spawnPosition, Vector2 direction, int damage,
        float knockback, UltimateHumungousaurStatePlayer state) {
        int rocketCount = state.CataclysmActive ? CataclysmRocketVolleyCount
            : state.TitanChargeActive ? ChargeRocketVolleyCount
            : RocketVolleyCount;
        float spread = rocketCount >= 4 ? 9f : rocketCount == 3 ? 7.5f : 6f;
        float speed = state.CataclysmActive ? 20f : state.TitanChargeActive ? 18.5f : 17f;
        float variant = state.CataclysmActive ? 2f : state.TitanChargeActive ? 1f : 0f;
        int rocketDamage = ScaleDamage(damage, state.CataclysmActive ? 0.88f : 1f);

        for (int i = 0; i < rocketCount; i++) {
            float offsetIndex = i - (rocketCount - 1) / 2f;
            Vector2 rocketVelocity = direction.RotatedBy(MathHelper.ToRadians(spread * offsetIndex)) * speed;
            Projectile.NewProjectile(source, spawnPosition + rocketVelocity.SafeNormalize(direction) * 10f, rocketVelocity,
                ModContent.ProjectileType<UltimateHumungousaurRocketPlayerProjectile>(), rocketDamage, knockback + 0.8f,
                player.whoAmI, variant);
        }
    }

    private static void SpawnShockwaveBurst(Player player, IEntitySource source, Vector2 origin, int damage, float knockback,
        float scale, int wavePairs, float variant = 0f) {
        if (player.whoAmI != Main.myPlayer)
            return;

        for (int pair = 0; pair < wavePairs; pair++) {
            float pairScale = scale * (1f + pair * 0.16f);
            Projectile.NewProjectile(source, origin + new Vector2(8f + pair * 8f, 0f), Vector2.Zero,
                ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>(), damage, knockback, player.whoAmI,
                pairScale, variant);
            Projectile.NewProjectile(source, origin + new Vector2(-8f - pair * 8f, 0f), Vector2.Zero,
                ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>(), damage, knockback, player.whoAmI,
                -pairScale, variant);
        }
    }

    internal static void ApplyBreachHit(Player owner, NPC target, int stacks, int time) {
        if (owner == null || !owner.active || target == null || !target.active)
            return;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        identity.ApplyHumungousaurBreach(owner.whoAmI, stacks, time, ShatteredDurationTicks);
    }

    internal static bool TryConsumeShattered(Player owner, NPC target, IEntitySource source, int bonusDamage, float knockback,
        bool cataclysm) {
        if (owner == null || !owner.active || target == null || !target.active)
            return false;

        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (!identity.IsHumungousaurShatteredFor(owner.whoAmI))
            return false;

        int residualStacks = cataclysm ? 2 : 0;
        int consumedStacks = identity.ConsumeHumungousaurShattered(owner.whoAmI, residualStacks);
        float scale = 1.1f + consumedStacks * 0.08f;
        int pairCount = cataclysm ? 2 : 1;
        SpawnShockwaveBurst(owner, source, target.Bottom + new Vector2(0f, -8f), bonusDamage, knockback, scale, pairCount,
            variant: cataclysm ? 2f : 1f);

        if (!Main.dedServ) {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.05f, Volume = 0.58f }, target.Center);
            for (int i = 0; i < 22; i++) {
                Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(18f, 18f),
                    i % 4 == 0 ? DustID.Smoke : DustID.Torch,
                    Main.rand.NextVector2Circular(3.6f, 3.6f), 110, new Color(255, 175, 120),
                    Main.rand.NextFloat(1f, 1.45f));
                dust.noGravity = true;
            }
        }

        return true;
    }

    internal static int ResolveHeroDamage(Player player, float ratio) {
        Item heldItem = player.HeldItem;
        int baseDamage = heldItem != null && !heldItem.IsAir ? heldItem.damage : 20;
        float heroDamage = player.GetDamage<HeroDamage>().ApplyTo(baseDamage);
        return Math.Max(1, (int)Math.Round(heroDamage * ratio));
    }

    private static bool IsUltimateHumungousaurProjectile(int projectileType) {
        return projectileType == ModContent.ProjectileType<HumungousaurPunchProjectile>()
               || projectileType == ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>()
               || projectileType == ModContent.ProjectileType<UltimateHumungousaurRocketPlayerProjectile>();
    }

    private static int ScaleDamage(int damage, float multiplier) {
        return Math.Max(1, (int)Math.Round(damage * multiplier));
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
}
