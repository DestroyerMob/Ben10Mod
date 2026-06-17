using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.Buffs.Summons;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.BuzzShock;

public class BuzzShockTransformation : Transformation {
    private const int ArcVolleyEnergyCost = 10;
    private const float ShockBoltDamageMultiplier = 0.78f;
    private const float ArcVolleyDamageMultiplier = 0.68f;
    private const int SparkBuddyEnergyCost = 12;
    private const int SparkBuddyCooldown = 8 * 60;
    private const int TeleportAfterimageBaseDamage = 30;
    private const float TaggedStormPulseDamageScale = 0.58f;
    private const float TaggedStormBoltDamageScale = 0.72f;
    private const int MaxTaggedStormTargets = 14;

    public override string FullID => "Ben10Mod:BuzzShock";
    public override string TransformationName => "Buzzshock";
    public override string IconPath => "Ben10Mod/Content/Interface/BuzzShockSelect";
    public override int TransformationBuffId => ModContent.BuffType<BuzzShock_Buff>();

    public override string Description =>
        "A skittish living spark that floods enemies with tags, blinks through danger, and lets small shocks pile into a storm.";

    public override List<string> Abilities => new() {
        "Shock Bolt tag spam with small forked follow-up sparks",
        "Arc Volley that chains harder through tagged groups",
        "Teleport Burst that leaves an electric afterimage",
        "Spark Buddy summon that prioritizes tagged targets",
        "Storm Barrage that shocks every tagged enemy"
    };

    public override string PrimaryAttackName => "Shock Bolt";
    public override string SecondaryAttackName => "Arc Volley";
    public override string PrimaryAbilityName => "Teleport Burst";
    public override string SecondaryAbilityAttackName => "Spark Buddy";
    public override string UltimateAttackName => "Storm Barrage";
    public override int PrimaryAttack => ModContent.ProjectileType<BuzzShockProjectile>();
    public override int PrimaryAttackSpeed => 14;
    public override int PrimaryShootSpeed => 28;

    public override int SecondaryAttack => ModContent.ProjectileType<BuzzShockUltimateProjectile>();
    public override int SecondaryAttackSpeed => 22;
    public override int SecondaryShootSpeed => 20;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => ArcVolleyDamageMultiplier;
    public override int SecondaryEnergyCost => ArcVolleyEnergyCost;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 1;
    public override int PrimaryAbilityCooldown => 10 * 60;

    public override int SecondaryAbilityAttack => ModContent.ProjectileType<BuzzShockMinionProjectile>();
    public override int SecondaryAbilityAttackSpeed => 18;
    public override int SecondaryAbilityAttackShootSpeed => 0;
    public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override float SecondaryAbilityAttackModifier => 0.9f;
    public override int SecondaryAbilityAttackEnergyCost => SparkBuddyEnergyCost;
    public override int SecondaryAbilityCooldown => SparkBuddyCooldown;
    public override bool SecondaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<BuzzShockUltimateProjectile>();
    public override int UltimateAttackSpeed => 18;
    public override int UltimateShootSpeed => 27;
    public override int UltimateEnergyCost => 22;
    public override float UltimateAttackModifier => 1.55f;
    public override int UltimateAbilityCooldown => 30 * 60;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.06f;
        player.GetAttackSpeed<HeroDamage>() += 0.2f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.moveSpeed += 0.18f;
        player.maxRunSpeed += 1.35f;
        player.maxMinions += 2;
        player.noFallDmg = true;
        Lighting.AddLight(player.Center, new Vector3(0.2f, 0.45f, 0.7f));
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled || omp.PrimaryAbilityWasEnabled || Main.myPlayer != player.whoAmI)
            return;

        Vector2 destination = Main.MouseWorld;
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            RequestPrimaryAbilityTeleport(destination);
            return;
        }

        ExecutePrimaryAbilityTeleport(player, destination);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (omp.IsSecondaryAbilityAttackLoaded) {
            SoundEngine.PlaySound(SoundID.AbigailSummon, player.position);
            player.AddBuff(ModContent.BuffType<BuzzShockMinionBuff>(), 2);
            player.SpawnMinionOnCursor(source, player.whoAmI, ModContent.ProjectileType<BuzzShockMinionProjectile>(),
                damage, knockback);
            return false;
        }

        if (omp.altAttack && !omp.ultimateAttack) {
            float[] spreads = { -0.26f, 0f, 0.26f };
            int finalDamage = Math.Max(1, (int)Math.Round(damage * SecondaryAttackModifier));
            SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, player.position);

            for (int i = 0; i < spreads.Length; i++) {
                Vector2 boltVelocity = velocity.RotatedBy(spreads[i]);
                Projectile.NewProjectile(source, position, boltVelocity,
                    ModContent.ProjectileType<BuzzShockUltimateProjectile>(), finalDamage, knockback, player.whoAmI,
                    BuzzShockUltimateProjectile.ArcVolleyMode, 0f, -1f);
            }

            return false;
        }

        SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, player.position);

        if (omp.ultimateAttack) {
            float[] spreads = { -0.52f, -0.26f, 0f, 0.26f, 0.52f };
            int finalDamage = Math.Max(1, (int)Math.Round(damage * UltimateAttackModifier));
            int taggedCount = SpawnTaggedStorm(source, player, finalDamage, knockback);
            float boltDamageScale = taggedCount > 0 ? TaggedStormBoltDamageScale : 1f;

            for (int i = 0; i < spreads.Length; i++) {
                Projectile.NewProjectile(source, position, velocity.RotatedBy(spreads[i]),
                    ModContent.ProjectileType<BuzzShockUltimateProjectile>(), Math.Max(1, (int)Math.Round(finalDamage * boltDamageScale)),
                    knockback, player.whoAmI, BuzzShockUltimateProjectile.UltimateMode, 0f, -1f);
            }

            return false;
        }

        Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BuzzShockProjectile>(),
            Math.Max(1, (int)Math.Round(damage * ShockBoltDamageMultiplier)), knockback, player.whoAmI);
        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (projectile.type != PrimaryAttack &&
            projectile.type != SecondaryAttack &&
            projectile.type != UltimateAttack &&
            projectile.type != SecondaryAbilityAttack)
            return;

        if (!target.HasBuff(ModContent.BuffType<BuzzShockTagBuff>()))
            return;

        bool isArcVolleyProjectile = projectile.type == SecondaryAttack && projectile.ai[0] < 0f;
        bool isUltimateProjectile = projectile.type == UltimateAttack && projectile.ai[0] > 0f;

        int nearbyTagged = BuzzShockTargeting.CountTagged(target.Center, 560f);
        float groupBonus = Math.Min(0.3f, nearbyTagged * 0.055f);

        modifiers.FinalDamage *= isUltimateProjectile ? 1.24f + groupBonus
            : isArcVolleyProjectile ? 1.16f + groupBonus
            : projectile.type == SecondaryAbilityAttack ? 1.2f
            : 1.12f;
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        if (resolvedSelection != OmnitrixPlayer.AttackSelection.Primary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Secondary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.SecondaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Ultimate)
            return base.GetAttackResourceSummary(selection, omp, compact);

        int tagged = BuzzShockTargeting.CountTagged(omp.Player.Center, 1400f);
        string tagText = compact ? $"Tags {tagged}" : $"Tagged enemies {tagged}";
        string baseText = base.GetAttackResourceSummary(selection, omp, compact);
        string identityText = resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? $"{tagText} • Spam"
                : $"{tagText} • refresh and spread tags",
            OmnitrixPlayer.AttackSelection.Secondary => compact
                ? $"{tagText} • Chains"
                : $"{tagText} • chains harder through tagged groups",
            OmnitrixPlayer.AttackSelection.SecondaryAbility => compact
                ? $"{tagText} • Buddy"
                : $"{tagText} • Spark Buddy hunts tagged enemies",
            OmnitrixPlayer.AttackSelection.Ultimate => compact
                ? $"{tagText} • Cashout"
                : $"{tagText} • shocks every tagged enemy now",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(baseText) ? identityText : $"{baseText} • {identityText}";
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<BuzzShock>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }

    internal static void ExecutePrimaryAbilityTeleport(Player player, Vector2 destination) {
        SpawnTeleportAfterimage(player, player.Center);
        EmitTeleportBurst(player);
        player.Teleport(destination, TeleportationStyleID.DebugTeleport);
        player.velocity = Vector2.Zero;

        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, destination.X,
                destination.Y, TeleportationStyleID.DebugTeleport);
        }

        EmitTeleportBurst(player);
    }

    private static void SpawnTeleportAfterimage(Player player, Vector2 center) {
        int damage = Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(TeleportAfterimageBaseDamage)));
        Projectile.NewProjectile(player.GetSource_FromThis(), center, Vector2.Zero,
            ModContent.ProjectileType<BuzzShockAfterimageProjectile>(), damage, 1.2f, player.whoAmI,
            BuzzShockAfterimageProjectile.TeleportAfterimageMode);
    }

    private static int SpawnTaggedStorm(EntitySource_ItemUse_WithAmmo source, Player player, int finalDamage,
        float knockback) {
        int spawned = 0;
        for (int i = 0; i < Main.maxNPCs && spawned < MaxTaggedStormTargets; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy() || !BuzzShockTargeting.IsTagged(npc))
                continue;

            int pulseDamage = Math.Max(1, (int)Math.Round(finalDamage * TaggedStormPulseDamageScale));
            Projectile.NewProjectile(source, npc.Center, Vector2.Zero,
                ModContent.ProjectileType<BuzzShockAfterimageProjectile>(), pulseDamage, knockback + 1.1f,
                player.whoAmI, BuzzShockAfterimageProjectile.TaggedDetonationMode);

            Vector2 offset = new(Main.rand.NextFloat(-80f, 80f), Main.rand.NextFloat(-120f, -60f));
            Vector2 boltDirection = (npc.Center - (npc.Center + offset)).SafeNormalize(Vector2.UnitY);
            Projectile.NewProjectile(source, npc.Center + offset, boltDirection * 25f,
                ModContent.ProjectileType<BuzzShockUltimateProjectile>(), Math.Max(1, (int)Math.Round(finalDamage * 0.42f)),
                knockback * 0.75f, player.whoAmI, BuzzShockUltimateProjectile.TaggedStormMode, 0f, npc.whoAmI);

            spawned++;
        }

        return spawned;
    }

    private static void RequestPrimaryAbilityTeleport(Vector2 destination) {
        ModPacket packet = ModContent.GetInstance<global::Ben10Mod.Ben10Mod>().GetPacket();
        packet.Write((byte)global::Ben10Mod.Ben10Mod.MessageType.ExecuteBuzzShockTeleport);
        packet.Write(destination.X);
        packet.Write(destination.Y);
        packet.Send();
    }

    private static void EmitTeleportBurst(Player player) {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item8, player.position);
        for (int i = 0; i < 50; i++) {
            int dustNum = Dust.NewDust(player.position - Vector2.One, player.width + 1, player.height + 1,
                DustID.UltraBrightTorch, Main.rand.Next(-4, 5), Main.rand.Next(-4, 5), 1, Color.White, 2f);
            Main.dust[dustNum].noGravity = true;
        }
    }
    
    public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => new[] {
        new TransformationPaletteChannel(
            "charge",
            "Charge",
            new Color(255, 255, 255),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShock_Head",
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShockChargeMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShock_Body",
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShockChargeMask_Body")
        ),
        new TransformationPaletteChannel(
            "primary",
            "Primary",
            new Color(255, 255, 255),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShock_Head",
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShockBatteryPrimaryMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShock_Body",
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShockBatteryPrimaryMask_Body"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShock_Legs",
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShockBatteryPrimaryMask_Legs")
        ),
        new TransformationPaletteChannel(
            "secondary",
            "Secondary",
            new Color(255, 255, 255),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShock_Body",
                "Ben10Mod/Content/Transformations/BuzzShock/BuzzShockBatterySecondaryMask_Body")
        )
    };
}
