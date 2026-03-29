using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace Ben10Mod.Content.Transformations.Clockwork;

public class ClockworkTransformation : Transformation {
    private const float ParadoxTeleportDistance = 112f;

    public override string FullID => "Ben10Mod:Clockwork";
    public override string TransformationName => "Clockwork";
    public override int TransformationBuffId => ModContent.BuffType<Clockwork_Buff>();
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";

    public override string Description =>
        "A Chronosapien who manipulates time with scrambling bolts, suspended traps, bursts of temporal acceleration, and paradox dodges.";

    public override List<string> Abilities => new() {
        "Chrono bolts that scramble enemies",
        "Time Snares that suspend targets in place",
        "Accelerate to speed yourself up",
        "Paradox for short-range temporal dodges",
        "Chrono Lock to pin enemies in frozen time"
    };

    public override string PrimaryAttackName => "Chrono Bolt";
    public override string SecondaryAttackName => "Time Snare";
    public override string PrimaryAbilityName => "Accelerate";
    public override string SecondaryAbilityName => "Paradox";
    public override string UltimateAttackName => "Chrono Lock";

    public override int PrimaryAttack => ModContent.ProjectileType<ClockworkBoltProjectile>();
    public override int PrimaryAttackSpeed => 20;
    public override int PrimaryShootSpeed => 15;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.92f;

    public override int SecondaryAttack => ModContent.ProjectileType<ClockworkTimeTrapProjectile>();
    public override int SecondaryAttackSpeed => 34;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 0.72f;

    public override int UltimateAttack => ModContent.ProjectileType<ClockworkChronoLockProjectile>();
    public override int UltimateAttackSpeed => 46;
    public override int UltimateUseStyle => ItemUseStyleID.Shoot;
    public override int UltimateEnergyCost => 70;
    public override int UltimateAbilityCooldown => 65 * 60;
    public override float UltimateAttackModifier => 1.45f;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 8 * 60;
    public override int PrimaryAbilityCooldown => 28 * 60;
    public override bool HasSecondaryAbility => true;
    public override int SecondaryAbilityDuration => 10 * 60;
    public override int SecondaryAbilityCooldown => 28 * 60;
    public override int SecondaryAbilityCost => 25;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.08f;
        player.statDefense += 6;
        player.GetCritChance<HeroDamage>() += 6f;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.moveSpeed += 0.22f;
        player.runAcceleration += 0.16f;
        player.maxRunSpeed += 1.2f;
        player.jumpSpeedBoost += 1.2f;
        player.GetAttackSpeed<HeroDamage>() += 0.24f;
        player.GetDamage<HeroDamage>() += 0.08f;
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.PrimaryAbilityEnabled || Main.rand.NextBool(2))
            return;

        Vector2 offset = Main.rand.NextVector2Circular(player.width * 0.45f, player.height * 0.45f);
        Dust dust = Dust.NewDustPerfect(player.Center + offset, Main.rand.NextBool() ? DustID.GemTopaz : DustID.YellowTorch,
            player.velocity * 0.12f, 95, new Color(240, 215, 120), Main.rand.NextFloat(0.95f, 1.2f));
        dust.noGravity = true;

        if (omp.SecondaryAbilityEnabled && Main.rand.NextBool(2)) {
            Vector2 paradoxOffset = Main.rand.NextVector2Circular(player.width * 0.5f, player.height * 0.6f);
            Dust paradoxDust = Dust.NewDustPerfect(player.Center + paradoxOffset,
                Main.rand.NextBool() ? DustID.GoldFlame : DustID.GemTopaz,
                Main.rand.NextVector2Circular(0.35f, 0.35f), 100, new Color(255, 225, 150), Main.rand.NextFloat(1f, 1.24f));
            paradoxDust.noGravity = true;
        }
    }

    public override bool FreeDodge(Player player, OmnitrixPlayer omp, Player.HurtInfo info) {
        if (!info.Dodgeable || !omp.SecondaryAbilityEnabled)
            return false;

        TriggerParadox(player, omp, info.HitDirection);
        return true;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.ultimateAttack) {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                ModContent.ProjectileType<ClockworkChronoLockProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        if (omp.altAttack) {
            Vector2 lobVelocity = direction * 7f + new Vector2(0f, -3.4f);
            Projectile.NewProjectile(source, player.Center + direction * 12f, lobVelocity,
                ModContent.ProjectileType<ClockworkTimeTrapProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        Projectile.NewProjectile(source, player.Center + direction * 10f, direction * 15f,
            ModContent.ProjectileType<ClockworkBoltProjectile>(), damage, knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.GoldHelmet;
        player.body = ArmorIDs.Body.GoldChainmail;
        player.legs = ArmorIDs.Legs.GoldGreaves;
        if (omp.PrimaryAbilityEnabled)
            player.armorEffectDrawShadow = true;
    }

    private void TriggerParadox(Player player, OmnitrixPlayer omp, int hitDirection) {
        ConsumeParadox(player, omp);

        int retreatDirection = hitDirection == 0 ? -player.direction : -System.Math.Sign(hitDirection);
        Vector2 destination = FindParadoxDestination(player, retreatDirection);

        EmitParadoxBurst(player);
        player.Teleport(destination, TeleportationStyleID.DebugTeleport);
        player.velocity = new Vector2(-retreatDirection * 1.5f, 0f);
        player.immune = true;
        player.immuneNoBlink = true;
        player.immuneTime = System.Math.Max(player.immuneTime, 24);
        player.fallStart = (int)(player.position.Y / 16f);

        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);

        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, destination.X,
                destination.Y, TeleportationStyleID.DebugTeleport);
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
        }

        EmitParadoxBurst(player);
    }

    private void ConsumeParadox(Player player, OmnitrixPlayer omp) {
        omp.SecondaryAbilityEnabled = false;
        omp.SecondaryAbilityWasEnabled = false;
        player.ClearBuff(ModContent.BuffType<SecondaryAbility>());

        int cooldown = GetSecondaryAbilityCooldown(omp);
        if (cooldown > 0)
            player.AddBuff(ModContent.BuffType<SecondaryAbilityCooldown>(), cooldown);
    }

    private static Vector2 FindParadoxDestination(Player player, int retreatDirection) {
        float[] distances = { ParadoxTeleportDistance, 96f, 80f, 64f, 48f };
        float[] verticalOffsets = { -16f, -32f, 0f, -48f, 16f };

        foreach (float distance in distances) {
            foreach (float verticalOffset in verticalOffsets) {
                Vector2 candidate = player.position + new Vector2(retreatDirection * distance, verticalOffset);
                if (!Collision.SolidCollision(candidate, player.width, player.height))
                    return candidate;
            }
        }

        return player.position + new Vector2(retreatDirection * 40f, -16f);
    }

    private static void EmitParadoxBurst(Player player) {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item8, player.position);
        for (int i = 0; i < 22; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.8f, 2.8f);
            Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(player.width * 0.35f, player.height * 0.45f),
                i % 2 == 0 ? DustID.GemTopaz : DustID.GoldFlame, velocity, 95,
                new Color(255, 225, 150), Main.rand.NextFloat(1f, 1.28f));
            dust.noGravity = true;
        }
    }
}
