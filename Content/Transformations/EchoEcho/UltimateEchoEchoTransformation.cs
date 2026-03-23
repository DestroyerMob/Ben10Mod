using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Summons;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EchoEcho;

public class UltimateEchoEchoTransformation : EchoEchoTransformation {
    private const int SpeakersPerSentrySlot = 3;

    public override string FullID => "Ben10Mod:UltimateEchoEcho";
    public override string TransformationName => "Ultimate Echo Echo";
    public override int TransformationBuffId => ModContent.BuffType<UltimateEchoEcho_Buff>();
    public override Transformation ParentTransformation => ModContent.GetInstance<EchoEchoTransformation>();
    public override Transformation ChildTransformation => null;

    public override string Description =>
        "An evolved sonic form that abandons duplication in favor of detached speaker arrays and heavier resonance fire.";

    public override List<string> Abilities => new() {
        "Enhanced sonic bursts",
        "Detached speaker deployment",
        "Resonance overclock"
    };

    public override string PrimaryAttackName => "Resonance Burst";
    public override string SecondaryAttackName => "Speaker Deployment";
    public override float PrimaryAttackModifier => 1.15f;
    public override int SecondaryAttack => ModContent.ProjectileType<UltimateEchoEchoSpeakerProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 0;
    public override float SecondaryAttackModifier => 0.85f;
    
    

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.18f;
        player.GetAttackSpeed<HeroDamage>() += 0.14f;
        player.maxTurrets += 1;

        if (omp.PrimaryAbilityEnabled)
            player.GetAttackSpeed<HeroDamage>() += 0.24f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (omp.altAttack) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            int speakerType = ModContent.ProjectileType<UltimateEchoEchoSpeakerProjectile>();
            int sentrySlots = Math.Max(1, player.maxTurrets);
            int maxSpeakers = sentrySlots * SpeakersPerSentrySlot;
            int activeSpeakerCount = 0;
            int oldestSpeakerIndex = -1;
            float oldestSpawnOrder = float.MaxValue;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || projectile.owner != player.whoAmI || projectile.type != speakerType)
                    continue;

                activeSpeakerCount++;
                float spawnOrder = projectile.localAI[1] <= 0f ? projectile.identity : projectile.localAI[1];
                if (spawnOrder < oldestSpawnOrder) {
                    oldestSpawnOrder = spawnOrder;
                    oldestSpeakerIndex = i;
                }
            }

            if (activeSpeakerCount >= maxSpeakers && oldestSpeakerIndex != -1) {
                Main.projectile[oldestSpeakerIndex].Kill();
            }

            player.AddBuff(ModContent.BuffType<UltimateEchoEchoSpeakerBuff>(), 2);
            Vector2 anchorPosition = Main.MouseWorld;
            Vector2 spawnVelocity = player.Center.DirectionTo(anchorPosition) * 14f;
            int projectileIndex = Projectile.NewProjectile(source, player.Center, spawnVelocity, speakerType,
                (int)(damage * SecondaryAttackModifier), knockback, player.whoAmI, anchorPosition.X, anchorPosition.Y);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                omp.ultimateEchoEchoSpeakerSpawnSerial++;
                Main.projectile[projectileIndex].originalDamage = (int)(damage * SecondaryAttackModifier);
                Main.projectile[projectileIndex].localAI[1] = omp.ultimateEchoEchoSpeakerSpawnSerial;
                Main.projectile[projectileIndex].netUpdate = true;
            }
            return false;
        }

        omp.transformationAttackSerial++;
        omp.transformationAttackDamage = (int)(damage * PrimaryAttackModifier);

        for (int i = -1; i <= 1; i++) {
            Vector2 spreadVelocity = velocity.RotatedBy(MathHelper.ToRadians(7f * i));
            Projectile.NewProjectile(source, position, spreadVelocity, ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(),
                (int)(damage * (i == 0 ? PrimaryAttackModifier : 0.82f)), knockback, player.whoAmI);
        }
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.PlatinumHelmet;
        player.body = ArmorIDs.Body.PlatinumChainmail;
        player.legs = ArmorIDs.Legs.PlatinumGreaves;
    }
}
