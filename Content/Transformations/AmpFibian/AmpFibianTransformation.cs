using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.AmpFibian;

public class AmpFibianTransformation : Transformation {
    private const int PhaseShiftEnergyCost = 15;
    private const int PhaseShiftCooldown = 15 * 60;
    private const int PhaseShiftDuration = 12;
    private const int BarrierDuration = 10 * 60;
    private const int BarrierCooldown = 90 * 60;
    private const int BarrierEnergyCost = 75;

    public override string FullID => "Ben10Mod:AmpFibian";
    public override string TransformationName => "AmpFibian";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<AmpFibian_Buff>();

    public override string Description =>
        "An electrical conductoid that lashes targets with living lightning, bursts power at close range, and slips through solid matter in a flash.";

    public override List<string> Abilities => new() {
        "Sine-wave lightning bolt",
        "Close-range electrical burst",
        "Point-and-click phase shift",
        "Electrical barrier against enemy contact"
    };

    public override string PrimaryAttackName => "Lightning Bolt";
    public override string SecondaryAttackName => "Electrical Burst";
    public override string PrimaryAbilityAttackName => "Phase Shift";
    public override int PrimaryAttack => ModContent.ProjectileType<AmpFibianBoltProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override bool PrimaryNoMelee => true;
    public override int SecondaryAttack => ModContent.ProjectileType<AmpFibianBurstProjectile>();
    public override int SecondaryAttackSpeed => 28;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override bool SecondaryNoMelee => true;
    public override float SecondaryAttackModifier => 1.35f;
    public override int PrimaryAbilityAttack => ModContent.ProjectileType<AmpFibianPhaseShiftMarkerProjectile>();
    public override int PrimaryAbilityAttackSpeed => 18;
    public override int PrimaryAbilityAttackShootSpeed => 0;
    public override int PrimaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override bool PrimaryAbilityAttackSingleUse => true;
    public override int PrimaryAbilityCooldown => PhaseShiftCooldown;
    public override int PrimaryAbilityAttackEnergyCost => PhaseShiftEnergyCost;

    public override bool HasUltimateAbility => true;
    public override int UltimateAbilityDuration => BarrierDuration;
    public override int UltimateAbilityCooldown => BarrierCooldown;
    public override int UltimateAbilityCost => BarrierEnergyCost;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.GetDamage<HeroDamage>() += 0.12f;
        player.GetCritChance<HeroDamage>() += 8f;
        player.GetAttackSpeed<HeroDamage>() += 0.1f;
        player.moveSpeed += 0.12f;
        player.maxRunSpeed += 0.8f;
        player.ignoreWater = true;
        player.noFallDmg = true;
        player.armorEffectDrawShadow = omp.IsUltimateAbilityActive;
        Lighting.AddLight(player.Center, new Vector3(0.18f, 0.42f, 0.75f));

        if (omp.IsUltimateAbilityActive &&
            (Main.netMode != NetmodeID.MultiplayerClient || player.whoAmI == Main.myPlayer) &&
            player.ownedProjectileCounts[ModContent.ProjectileType<AmpFibianBarrierProjectile>()] <= 0) {
            Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
                ModContent.ProjectileType<AmpFibianBarrierProjectile>(), 0, 0f, player.whoAmI);
        }
    }

    public override void ModifyDrawInfo(Player player, OmnitrixPlayer omp, ref PlayerDrawSet drawInfo) {
        bool phaseActive = player.GetModPlayer<AmpFibianPhaseShiftPlayer>().IsPhaseShifting;
        if (!phaseActive && !omp.IsUltimateAbilityActive)
            return;

        byte targetAlpha = phaseActive ? (byte)120 : (byte)180;
        drawInfo.colorArmorHead.A = System.Math.Min(drawInfo.colorArmorHead.A, targetAlpha);
        drawInfo.colorArmorBody.A = System.Math.Min(drawInfo.colorArmorBody.A, targetAlpha);
        drawInfo.colorArmorLegs.A = System.Math.Min(drawInfo.colorArmorLegs.A, targetAlpha);
        drawInfo.colorEyeWhites.A = System.Math.Min(drawInfo.colorEyeWhites.A, targetAlpha);
        drawInfo.colorEyes.A = System.Math.Min(drawInfo.colorEyes.A, targetAlpha);
    }

    public override void PreUpdateMovement(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<AmpFibianPhaseShiftPlayer>().UpdatePhaseShift(player);
    }

    public override bool? CanBeHitByNPC(Player player, OmnitrixPlayer omp, NPC npc, ref int cooldownSlot) {
        if (omp.IsUltimateAbilityActive || player.GetModPlayer<AmpFibianPhaseShiftPlayer>().IsPhaseShifting)
            return false;

        return base.CanBeHitByNPC(player, omp, npc, ref cooldownSlot);
    }

    public override bool? CanBeHitByProjectile(Player player, OmnitrixPlayer omp, Projectile projectile) {
        if (player.GetModPlayer<AmpFibianPhaseShiftPlayer>().IsPhaseShifting)
            return false;

        return base.CanBeHitByProjectile(player, omp, projectile);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (omp.IsPrimaryAbilityAttackLoaded) {
            Vector2 destination = Main.MouseWorld;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                RequestPhaseShift(destination);

            ExecutePhaseShift(player, destination);
            return false;
        }

        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.altAttack) {
            int burstDamage = System.Math.Max(1, (int)System.Math.Round(damage * SecondaryAttackModifier));
            Projectile.NewProjectile(source, player.MountedCenter + direction * 10f, Vector2.Zero,
                ModContent.ProjectileType<AmpFibianBurstProjectile>(), burstDamage, knockback + 1f, player.whoAmI);
            SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with { Pitch = -0.15f }, player.Center);
            return false;
        }

        Projectile.NewProjectile(source, player.MountedCenter + direction * 14f, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<AmpFibianBoltProjectile>(), damage, knockback, player.whoAmI);
        SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.18f, Volume = 0.8f }, player.Center);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.GoldHelmet;
        player.body = ArmorIDs.Body.GoldChainmail;
        player.legs = ArmorIDs.Legs.GoldGreaves;
    }

    internal static void ExecutePhaseShift(Player player, Vector2 destination) {
        destination = ClampDestination(destination, player);
        player.GetModPlayer<AmpFibianPhaseShiftPlayer>().BeginPhaseShift(player, destination, PhaseShiftDuration);
    }

    private static void RequestPhaseShift(Vector2 destination) {
        ModPacket packet = ModContent.GetInstance<global::Ben10Mod.Ben10Mod>().GetPacket();
        packet.Write((byte)global::Ben10Mod.Ben10Mod.MessageType.ExecuteAmpFibianPhaseShift);
        packet.Write(destination.X);
        packet.Write(destination.Y);
        packet.Send();
    }

    private static Vector2 ClampDestination(Vector2 destination, Player player) {
        float halfWidth = player.width * 0.5f;
        float halfHeight = player.height * 0.5f;
        float minX = 16f + halfWidth;
        float maxX = Main.maxTilesX * 16f - 16f - halfWidth;
        float minY = 16f + halfHeight;
        float maxY = Main.maxTilesY * 16f - 16f - halfHeight;
        return new Vector2(
            MathHelper.Clamp(destination.X, minX, maxX),
            MathHelper.Clamp(destination.Y, minY, maxY)
        );
    }

    private static void EmitPhaseShiftBurst(Player player, Vector2 destination) {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.24f }, destination);
        for (int i = 0; i < 28; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(3.4f, 3.4f);
            Dust dust = Dust.NewDustPerfect(destination + Main.rand.NextVector2Circular(14f, 18f), DustID.Electric,
                velocity, 90, new Color(110, 220, 255), Main.rand.NextFloat(1.15f, 1.55f));
            dust.noGravity = true;
        }

        for (int i = 0; i < 16; i++) {
            Dust mist = Dust.NewDustPerfect(destination + Main.rand.NextVector2Circular(12f, 16f), DustID.BlueTorch,
                Main.rand.NextVector2Circular(2.1f, 2.1f), 105, new Color(150, 235, 255), Main.rand.NextFloat(0.9f, 1.2f));
            mist.noGravity = true;
        }
    }
}

public class AmpFibianPhaseShiftPlayer : ModPlayer {
    private Vector2 _startCenter;
    private Vector2 _targetCenter;
    private int _timeLeft;
    private int _duration;
    private bool _started;

    public bool IsPhaseShifting => _timeLeft > 0;

    public void BeginPhaseShift(Player player, Vector2 destination, int duration) {
        _startCenter = player.Center;
        _targetCenter = destination;
        _duration = System.Math.Max(1, duration);
        _timeLeft = _duration;
        _started = false;
        player.velocity = Vector2.Zero;
        player.fallStart = (int)(player.position.Y / 16f);
        player.immune = true;
        player.immuneNoBlink = true;
        player.immuneTime = System.Math.Max(player.immuneTime, _duration + 8);
    }

    public void UpdatePhaseShift(Player player) {
        if (!IsPhaseShifting)
            return;

        if (!_started) {
            _started = true;
            SpawnPhaseDust(player.Center);
        }

        float progress = 1f - _timeLeft / (float)_duration;
        float easedProgress = progress * progress * (3f - 2f * progress);
        player.Center = Vector2.SmoothStep(_startCenter, _targetCenter, easedProgress);
        player.velocity = Vector2.Zero;
        player.fallStart = (int)(player.position.Y / 16f);
        player.immune = true;
        player.immuneNoBlink = true;
        player.immuneTime = System.Math.Max(player.immuneTime, 2);

        if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
            SpawnTrailDust(player.Center);

        _timeLeft--;
        if (_timeLeft <= 0) {
            player.Center = _targetCenter;
            SpawnPhaseDust(player.Center);
        }
    }

    private static void SpawnPhaseDust(Vector2 center) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 16; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2.8f, 2.8f);
            Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(14f, 18f), DustID.Electric,
                velocity, 90, new Color(110, 220, 255), Main.rand.NextFloat(1f, 1.4f));
            dust.noGravity = true;
        }
    }

    private static void SpawnTrailDust(Vector2 center) {
        Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(10f, 14f), DustID.BlueTorch,
            Main.rand.NextVector2Circular(1.2f, 1.2f), 110, new Color(190, 245, 255), Main.rand.NextFloat(0.8f, 1.05f));
        dust.noGravity = true;
    }
}
