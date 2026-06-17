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
    internal const string TransformationId = "Ben10Mod:AmpFibian";
    internal const int PhaseDischargeWindowTicks = 5 * 60;
    internal const int PhaseWakePulseIntervalTicks = 7;
    internal const int PhaseWakePulseBaseDamage = 20;
    internal const int BarrierContactPulseBaseDamage = 24;
    internal const int BarrierArcBaseDamage = 30;
    internal const int BarrierMaxCharge = 100;
    internal const int BarrierAutoArcThreshold = 34;
    internal const int BarrierBurstMinimumSpend = 18;
    internal const int BarrierBurstMaxSpend = 48;
    internal const float PhaseChargedBurstMultiplier = 1.48f;

    private const int PhaseShiftEnergyCost = 14;
    private const int PhaseShiftCooldown = 12 * 60;
    private const int PhaseShiftDuration = 14;
    private const int BarrierDuration = 10 * 60;
    private const int BarrierCooldown = 90 * 60;
    private const int BarrierEnergyCost = 75;
    private const float PrimaryDamageMultiplier = 0.82f;
    private const float SecondaryBurstDamageMultiplier = 0.92f;

    public override string FullID => TransformationId;
    public override string TransformationName => "AmpFibian";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<AmpFibian_Buff>();

    public override string Description =>
        "A ghostly phase conductor who slips through danger, stores charge in a defensive barrier, and punishes close enemies with controlled electrical discharges.";

    public override List<string> Abilities => new() {
        "Precise piercing sine-wave lightning",
        "Close-range discharge spikes after Phase Shift",
        "Phase Shift passes through attacks and charges the next burst",
        "Electrical Barrier absorbs projectiles and stores retaliatory arcs"
    };

    public override string PrimaryAttackName => "Lightning Bolt";
    public override string SecondaryAttackName => "Electrical Burst";
    public override string PrimaryAbilityAttackName => "Phase Shift";
    public override string UltimateAbilityName => "Electrical Barrier";
    public override int PrimaryAttack => ModContent.ProjectileType<AmpFibianBoltProjectile>();
    public override int PrimaryAttackSpeed => 20;
    public override int PrimaryShootSpeed => 18;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override bool PrimaryNoMelee => true;
    public override float PrimaryAttackModifier => PrimaryDamageMultiplier;
    public override int SecondaryAttack => ModContent.ProjectileType<AmpFibianBurstProjectile>();
    public override int SecondaryAttackSpeed => 28;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.HoldUp;
    public override bool SecondaryNoMelee => true;
    public override float SecondaryAttackModifier => SecondaryBurstDamageMultiplier;
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

    public override void OnDetransform(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<AmpFibianPhaseShiftPlayer>().ClearConductiveState();
        KillOwnedProjectiles(player,
            ModContent.ProjectileType<AmpFibianBarrierProjectile>(),
            ModContent.ProjectileType<AmpFibianArcProjectile>());
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        AmpFibianPhaseShiftPlayer state = player.GetModPlayer<AmpFibianPhaseShiftPlayer>();
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetAttackSpeed<HeroDamage>() += 0.06f;
        player.moveSpeed += 0.12f;
        player.maxRunSpeed += 0.8f;
        player.ignoreWater = true;
        player.noFallDmg = true;
        player.buffImmune[BuffID.Electrified] = true;
        player.armorEffectDrawShadow = omp.IsUltimateAbilityActive;

        if (state.PhaseBurstCharged) {
            player.moveSpeed += 0.08f;
            player.maxRunSpeed += 0.45f;
            player.GetAttackSpeed<HeroDamage>() += 0.08f;
        }

        if (omp.IsUltimateAbilityActive) {
            player.statDefense += 8;
            player.endurance += 0.06f;
            player.aggro -= 280;
        }

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

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        player.GetModPlayer<AmpFibianPhaseShiftPlayer>().UpdateConductiveState(player, omp);
    }

    public override bool? CanBeHitByNPC(Player player, OmnitrixPlayer omp, NPC npc, ref int cooldownSlot) {
        AmpFibianPhaseShiftPlayer state = player.GetModPlayer<AmpFibianPhaseShiftPlayer>();
        if (state.IsPhaseShifting) {
            state.RegisterPhaseContact(player, npc.Center);
            return false;
        }

        if (omp.IsUltimateAbilityActive) {
            state.AbsorbBarrierContact(player, npc);
            return false;
        }

        return base.CanBeHitByNPC(player, omp, npc, ref cooldownSlot);
    }

    public override bool? CanBeHitByProjectile(Player player, OmnitrixPlayer omp, Projectile projectile) {
        AmpFibianPhaseShiftPlayer state = player.GetModPlayer<AmpFibianPhaseShiftPlayer>();
        if (state.IsPhaseShifting) {
            state.RegisterPhaseProjectile(player, projectile);
            return false;
        }

        if (omp.IsUltimateAbilityActive && state.AbsorbBarrierProjectile(player, projectile))
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

        AmpFibianPhaseShiftPlayer state = player.GetModPlayer<AmpFibianPhaseShiftPlayer>();
        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (omp.altAttack) {
            float phaseMultiplier = state.ConsumePhaseBurstMultiplier();
            int barrierChargeSpent = state.ConsumeBarrierBurstCharge();
            float barrierMultiplier = barrierChargeSpent > 0 ? 1f + barrierChargeSpent * 0.012f : 1f;
            float burstMultiplier = SecondaryAttackModifier * barrierMultiplier;
            float burstMode = AmpFibianBurstProjectile.NormalMode;

            if (phaseMultiplier > 0f) {
                burstMultiplier *= phaseMultiplier;
                burstMode = AmpFibianBurstProjectile.PhaseDischargeMode;
            }
            else if (barrierChargeSpent > 0) {
                burstMode = AmpFibianBurstProjectile.BarrierPulseMode;
            }

            int burstDamage = System.Math.Max(1, (int)System.Math.Round(damage * burstMultiplier));
            Projectile.NewProjectile(source, player.MountedCenter + direction * 10f, Vector2.Zero,
                ModContent.ProjectileType<AmpFibianBurstProjectile>(), burstDamage,
                knockback + (phaseMultiplier > 0f ? 1.55f : 1f), player.whoAmI, 0f, burstMode, barrierChargeSpent);
            SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with {
                Pitch = phaseMultiplier > 0f ? 0.12f : -0.15f,
                Volume = phaseMultiplier > 0f ? 0.95f : 0.82f
            }, player.Center);
            return false;
        }

        int boltDamage = System.Math.Max(1, (int)System.Math.Round(damage * PrimaryAttackModifier));
        Projectile.NewProjectile(source, player.MountedCenter + direction * 14f, direction * PrimaryShootSpeed,
            ModContent.ProjectileType<AmpFibianBoltProjectile>(), boltDamage, knockback, player.whoAmI);
        SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.18f, Volume = 0.8f }, player.Center);
        return false;
    }

    public override void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile, NPC target,
        ref NPC.HitModifiers modifiers) {
        if (projectile.type == PrimaryAttack) {
            modifiers.ArmorPenetration += 5;
            return;
        }

        if (projectile.type != SecondaryAttack)
            return;

        if (projectile.ai[1] == AmpFibianBurstProjectile.PhaseDischargeMode) {
            modifiers.ArmorPenetration += 12;
            modifiers.FinalDamage *= 1.08f;
        }
        else if (projectile.ai[1] == AmpFibianBurstProjectile.BarrierPulseMode) {
            modifiers.ArmorPenetration += 8;
        }
    }

    public override string GetAttackResourceSummary(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp,
        bool compact = false) {
        OmnitrixPlayer.AttackSelection resolvedSelection = ResolveAttackSelection(selection, omp);
        if (resolvedSelection != OmnitrixPlayer.AttackSelection.Primary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Secondary &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.PrimaryAbility &&
            resolvedSelection != OmnitrixPlayer.AttackSelection.Ultimate)
            return base.GetAttackResourceSummary(selection, omp, compact);

        AmpFibianPhaseShiftPlayer state = omp.Player.GetModPlayer<AmpFibianPhaseShiftPlayer>();
        string baseText = base.GetAttackResourceSummary(selection, omp, compact);
        string phaseText = state.PhaseBurstCharged
            ? compact ? "Phase ready" : "Phase-charged burst ready"
            : compact ? "No phase" : "phase to empower burst";
        string barrierText = compact ? $"Barrier {state.BarrierCharge}" : $"barrier charge {state.BarrierCharge}/{BarrierMaxCharge}";
        string identityText = resolvedSelection switch {
            OmnitrixPlayer.AttackSelection.Primary => compact
                ? "Pierce"
                : "precise piercing conductor bolt",
            OmnitrixPlayer.AttackSelection.Secondary => $"{phaseText} • {barrierText}",
            OmnitrixPlayer.AttackSelection.PrimaryAbility => compact
                ? "Charge burst"
                : "phases through danger and primes your next discharge",
            OmnitrixPlayer.AttackSelection.Ultimate => compact
                ? barrierText
                : $"{barrierText} • absorbs projectiles into stored arcs",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(baseText) ? identityText : $"{baseText} • {identityText}";
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

    internal static int ResolveHeroDamage(Player player, float baseDamage) {
        return System.Math.Max(1, (int)System.Math.Round(player.GetDamage<HeroDamage>().ApplyTo(baseDamage)));
    }

    private static void KillOwnedProjectiles(Player player, params int[] projectileTypes) {
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.owner != player.whoAmI)
                continue;

            foreach (int projectileType in projectileTypes) {
                if (projectile.type == projectileType) {
                    projectile.Kill();
                    break;
                }
            }
        }
    }

    internal static void EmitPhaseShiftBurst(Player player, Vector2 destination) {
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
    private int _phaseBurstChargeTime;
    private int _phaseConductionBonus;
    private int _phaseWakePulseTimer;
    private int _barrierCharge;
    private int _barrierArcCooldown;
    private int _barrierContactCooldown;

    public bool IsPhaseShifting => _timeLeft > 0;
    public bool PhaseBurstCharged => _phaseBurstChargeTime > 0;
    public int BarrierCharge => _barrierCharge;
    public float BarrierChargeRatio => _barrierCharge / (float)AmpFibianTransformation.BarrierMaxCharge;

    public void BeginPhaseShift(Player player, Vector2 destination, int duration) {
        _startCenter = player.Center;
        _targetCenter = destination;
        _duration = System.Math.Max(1, duration);
        _timeLeft = _duration;
        _started = false;
        _phaseWakePulseTimer = 0;
        _phaseConductionBonus = 0;
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

        Vector2 previousCenter = player.Center;
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

        TrySpawnPhaseWakePulse(player, previousCenter);

        _timeLeft--;
        if (_timeLeft <= 0) {
            player.Center = _targetCenter;
            SpawnPhaseDust(player.Center);
            ArmPhaseBurst(player);
        }
    }

    public void UpdateConductiveState(Player player, OmnitrixPlayer omp) {
        if (omp.currentTransformationId != AmpFibianTransformation.TransformationId || player.dead) {
            ClearConductiveState();
            return;
        }

        if (_phaseBurstChargeTime > 0)
            _phaseBurstChargeTime--;

        if (_barrierArcCooldown > 0)
            _barrierArcCooldown--;

        if (_barrierContactCooldown > 0)
            _barrierContactCooldown--;

        if (!omp.IsUltimateAbilityActive) {
            if (_barrierCharge > 0)
                _barrierCharge--;
            return;
        }

        if (_barrierCharge >= AmpFibianTransformation.BarrierAutoArcThreshold)
            TryReleaseBarrierArc(player, player.Center);
    }

    public void ClearConductiveState() {
        _timeLeft = 0;
        _duration = 0;
        _started = false;
        _phaseBurstChargeTime = 0;
        _phaseConductionBonus = 0;
        _phaseWakePulseTimer = 0;
        _barrierCharge = 0;
        _barrierArcCooldown = 0;
        _barrierContactCooldown = 0;
    }

    public float ConsumePhaseBurstMultiplier() {
        if (_phaseBurstChargeTime <= 0)
            return 0f;

        float bonus = System.Math.Min(0.28f, _phaseConductionBonus * 0.01f);
        _phaseBurstChargeTime = 0;
        _phaseConductionBonus = 0;
        return AmpFibianTransformation.PhaseChargedBurstMultiplier + bonus;
    }

    public int ConsumeBarrierBurstCharge() {
        if (_barrierCharge < AmpFibianTransformation.BarrierBurstMinimumSpend)
            return 0;

        int spent = System.Math.Min(_barrierCharge, AmpFibianTransformation.BarrierBurstMaxSpend);
        _barrierCharge -= spent;
        return spent;
    }

    public void RegisterPhaseProjectile(Player player, Projectile projectile) {
        if (!ShouldHandleHostileProjectile(projectile))
            return;

        _phaseConductionBonus = Utils.Clamp(_phaseConductionBonus + Utils.Clamp(projectile.damage / 5, 4, 14), 0, 40);
        if (Main.netMode != NetmodeID.Server)
            SpawnAbsorbDust(projectile.Center, 8, new Color(160, 235, 255));
    }

    public void RegisterPhaseContact(Player player, Vector2 contactCenter) {
        _phaseConductionBonus = Utils.Clamp(_phaseConductionBonus + 5, 0, 40);

        if (_phaseWakePulseTimer <= 0)
            SpawnPhaseContactPulse(player, contactCenter);
    }

    public bool AbsorbBarrierProjectile(Player player, Projectile projectile) {
        if (!ShouldHandleHostileProjectile(projectile))
            return false;

        AddBarrierCharge(Utils.Clamp(10 + projectile.damage / 4, 12, 30));
        SpawnAbsorbDust(projectile.Center, 12, new Color(135, 215, 255));
        projectile.Kill();

        if (Main.netMode != NetmodeID.MultiplayerClient)
            TryReleaseBarrierArc(player, projectile.Center, force: _barrierCharge >= AmpFibianTransformation.BarrierAutoArcThreshold);

        return true;
    }

    public void AbsorbBarrierContact(Player player, NPC npc) {
        if (npc == null || !npc.active || npc.friendly || npc.dontTakeDamage || _barrierContactCooldown > 0)
            return;

        _barrierContactCooldown = 22;
        AddBarrierCharge(Utils.Clamp(9 + npc.damage / 12, 10, 24));

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            int damage = AmpFibianTransformation.ResolveHeroDamage(player,
                AmpFibianTransformation.BarrierContactPulseBaseDamage + _barrierCharge * 0.08f);
            Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
                ModContent.ProjectileType<AmpFibianBurstProjectile>(), damage, 1.8f, player.whoAmI, 0f,
                AmpFibianBurstProjectile.BarrierPulseMode, _barrierCharge);
            TryReleaseBarrierArc(player, npc.Center);
        }

        SpawnAbsorbDust(npc.Center, 10, new Color(130, 220, 255));
    }

    private void ArmPhaseBurst(Player player) {
        _phaseBurstChargeTime = AmpFibianTransformation.PhaseDischargeWindowTicks;
        AmpFibianTransformation.EmitPhaseShiftBurst(player, player.Center);
    }

    private void TrySpawnPhaseWakePulse(Player player, Vector2 previousCenter) {
        if (_phaseWakePulseTimer > 0) {
            _phaseWakePulseTimer--;
            return;
        }

        if (Vector2.DistanceSquared(player.Center, previousCenter) < 36f)
            return;

        SpawnPhaseContactPulse(player, Vector2.Lerp(previousCenter, player.Center, 0.5f));
    }

    private void SpawnPhaseContactPulse(Player player, Vector2 center) {
        _phaseWakePulseTimer = AmpFibianTransformation.PhaseWakePulseIntervalTicks;

        if (Main.netMode == NetmodeID.Server || player.whoAmI != Main.myPlayer)
            return;

        int damage = AmpFibianTransformation.ResolveHeroDamage(player,
            AmpFibianTransformation.PhaseWakePulseBaseDamage + _phaseConductionBonus * 0.15f);
        Projectile.NewProjectile(player.GetSource_FromThis(), center, Vector2.Zero,
            ModContent.ProjectileType<AmpFibianBurstProjectile>(), damage, 0.8f, player.whoAmI, 0f,
            AmpFibianBurstProjectile.PhaseContactMode, _phaseConductionBonus);
    }

    private void AddBarrierCharge(int amount) {
        _barrierCharge = Utils.Clamp(_barrierCharge + amount, 0, AmpFibianTransformation.BarrierMaxCharge);
    }

    private bool TryReleaseBarrierArc(Player player, Vector2 origin, bool force = false) {
        if (_barrierArcCooldown > 0)
            return false;

        if (!force && _barrierCharge < AmpFibianTransformation.BarrierAutoArcThreshold)
            return false;

        NPC target = FindBarrierArcTarget(player, origin, 720f);
        if (target == null)
            return false;

        int spent = System.Math.Min(_barrierCharge, 36);
        if (spent <= 0 && !force)
            return false;

        _barrierCharge = System.Math.Max(0, _barrierCharge - spent);
        _barrierArcCooldown = force ? 12 : 18;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return true;

        Vector2 direction = origin.DirectionTo(target.Center);
        if (direction == Vector2.Zero)
            direction = new Vector2(player.direction == 0 ? 1f : player.direction, 0f);

        int damage = AmpFibianTransformation.ResolveHeroDamage(player,
            AmpFibianTransformation.BarrierArcBaseDamage + spent * 0.24f);
        int projectileIndex = Projectile.NewProjectile(player.GetSource_FromThis(), origin, direction * 16f,
            ModContent.ProjectileType<AmpFibianArcProjectile>(), damage, 1.4f, player.whoAmI, target.whoAmI,
            AmpFibianArcProjectile.BarrierArcMode, spent / (float)AmpFibianTransformation.BarrierMaxCharge);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;

        return true;
    }

    private static NPC FindBarrierArcTarget(Player player, Vector2 origin, float range) {
        NPC bestTarget = null;
        float bestScore = range * range;

        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy())
                continue;

            float distanceSquared = Vector2.DistanceSquared(npc.Center, origin);
            if (distanceSquared > bestScore)
                continue;

            bestScore = distanceSquared;
            bestTarget = npc;
        }

        return bestTarget;
    }

    private static bool ShouldHandleHostileProjectile(Projectile projectile) {
        return projectile != null &&
            projectile.active &&
            projectile.hostile &&
            !projectile.friendly &&
            projectile.damage > 0;
    }

    private static void SpawnAbsorbDust(Vector2 center, int count, Color color) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < count; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
            Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(8f, 8f),
                i % 2 == 0 ? DustID.Electric : DustID.BlueTorch, velocity, 90, color,
                Main.rand.NextFloat(0.95f, 1.35f));
            dust.noGravity = true;
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
