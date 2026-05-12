using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.Items.Consumable;
using Ben10Mod.Content.Projectiles;

namespace Ben10Mod.Content.NPCs.Bosses {
    public class AlbedoBoss : ModNPC {
        private enum AlbedoPhase {
            IntroHuman,
            UltimateHumungousaur,
            UltimateEchoEcho,
            UltimatrixSwap
        }

        private enum AlbedoAttack {
            Intro,
            HumungousaurRocketBurst,
            HumungousaurGroundSlam,
            HumungousaurCharge,
            HumungousaurAirBarrage,
            EchoSpeakerLanes,
            EchoAlternatingHorizontalLanes,
            EchoAlternatingVerticalLanes,
            EchoOrbitingSpeakers,
            EchoPulseRings,
            EchoCrossfire
        }

        private enum SwapForm {
            Humungousaur,
            EchoEcho
        }

        private const int SonicShotMode = 0;
        private const int SonicPulseMode = 1;
        private const float AlternatingHorizontalHalfWidth = 1400f;
        private const float AlternatingVerticalHalfHeight = 980f;
        private const float AlternatingHorizontalWarningLength = AlternatingHorizontalHalfWidth * 2f;
        private const float AlternatingVerticalWarningLength = AlternatingVerticalHalfHeight * 2f;

        private ref float PhaseState => ref NPC.ai[0];
        private ref float AttackState => ref NPC.ai[1];
        private ref float AttackTimer => ref NPC.ai[2];
        private ref float AttackVariant => ref NPC.ai[3];
        private ref float HumungousaurAttackIndex => ref NPC.localAI[0];
        private ref float EchoAttackIndex => ref NPC.localAI[1];
        private ref float IntroStage => ref NPC.localAI[2];
        private ref float SwapAttackIndex => ref NPC.localAI[3];
        private float MovementSeed => NPC.whoAmI * 0.73f;
        private Vector2 _moveTarget;
        private int _moveTargetCooldown;
        private bool _hasMoveTarget;
        private readonly Vector2[] _lockedRocketOrigins = new Vector2[12];
        private readonly Vector2[] _lockedRocketTargets = new Vector2[12];
        private int _lockedRocketCount;
        private Vector2 _rushLaneStart;
        private Vector2 _rushLaneEnd;
        private Vector2 _stompStartCenter;
        private Vector2 _stompApexCenter;
        private Vector2 _stompOrigin;

        public override string Texture => "Ben10Mod/Content/Items/Vanity/Ben10Shirt";

        public override void SetStaticDefaults() {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
        }

        public override void SetDefaults() {
            NPC.width = 72;
            NPC.height = 120;
            NPC.damage = 80;
            NPC.defense = 24;
            NPC.lifeMax = 42000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = Item.buyPrice(gold: 15);
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
            Music = MusicID.Boss3;

            if (Main.BigBossProgressBar.TryGetSpecialVanillaBossBar(NPCID.EyeofCthulhu, out IBigProgressBar bossBar))
                NPC.BossBar = bossBar;
        }

        public override void OnSpawn(IEntitySource source) {
            PhaseState = (float)AlbedoPhase.IntroHuman;
            AttackState = (float)AlbedoAttack.Intro;
            AttackTimer = 0f;
            AttackVariant = 0f;
            HumungousaurAttackIndex = 0f;
            EchoAttackIndex = 0f;
            IntroStage = 0f;
            SwapAttackIndex = 0f;
            ResetMovementTarget();
        }

        public override void AI() {
            if (!TryGetTarget(out Player target)) {
                Despawn();
                return;
            }

            NPC.TargetClosest();
            NPC.direction = NPC.spriteDirection = target.Center.X >= NPC.Center.X ? 1 : -1;

            UpdatePhaseThresholds();

            AlbedoPhase phase = GetCurrentPhase();
            ApplyCurrentFormDimensions(phase);

            switch (phase) {
                case AlbedoPhase.IntroHuman:
                    RunIntroPhase(target);
                    break;
                case AlbedoPhase.UltimateHumungousaur:
                    RunUltimateHumungousaurPhase(target, finalPhase: false);
                    break;
                case AlbedoPhase.UltimateEchoEcho:
                    RunUltimateEchoEchoPhase(target, finalPhase: false);
                    break;
                case AlbedoPhase.UltimatrixSwap:
                    RunUltimatrixSwapPhase(target);
                    break;
            }
        }

        private void RunIntroPhase(Player target) {
            SetMovementProfile(canFly: false);
            NPC.damage = 0;
            NPC.defense = 24;
            NPC.velocity.X *= 0.9f;
            AttackTimer++;

            if (IntroStage == 0f) {
                ShowActionText("Behold the Ultimatrix!");
                IntroStage = 1f;
                NPC.netUpdate = true;
            }

            if (IntroStage == 1f && AttackTimer >= 75f) {
                ShowActionText("Humungousaur!");
                PlayTransformationEffect();
                IntroStage = 2f;
                NPC.netUpdate = true;
            }

            if (IntroStage == 2f && AttackTimer >= 145f) {
                ShowActionText("I can go ultimate!");
                IntroStage = 3f;
                NPC.netUpdate = true;
            }

            if (IntroStage == 3f && AttackTimer >= 215f) {
                ShowActionText("Ultimate Humungousaur!");
                PlayTransformationEffect();
                IntroStage = 4f;
                NPC.netUpdate = true;
            }

            if (AttackTimer >= 230f)
                TransitionToPhase(AlbedoPhase.UltimateHumungousaur);
        }

        private void RunUltimateHumungousaurPhase(Player target, bool finalPhase) {
            SetMovementProfile(canFly: false);
            NPC.damage = finalPhase ? 110 : 100;
            NPC.defense = finalPhase ? 32 : 30;

            if (!IsHumungousaurAttack(GetCurrentAttack()))
                StartNextHumungousaurAttack(target, finalPhase);

            AttackTimer++;
            switch (GetCurrentAttack()) {
                case AlbedoAttack.HumungousaurRocketBurst:
                    RunRocketBurstAttack(target, finalPhase);
                    break;
                case AlbedoAttack.HumungousaurGroundSlam:
                    RunGroundSlamAttack(target, finalPhase);
                    break;
                case AlbedoAttack.HumungousaurCharge:
                    RunChargeAttack(target, finalPhase);
                    break;
                case AlbedoAttack.HumungousaurAirBarrage:
                    RunAirBarrageAttack(target, finalPhase);
                    break;
                default:
                    StartNextHumungousaurAttack(target, finalPhase);
                    break;
            }
        }

        private void RunUltimateEchoEchoPhase(Player target, bool finalPhase) {
            SetMovementProfile(canFly: true);
            NPC.damage = finalPhase ? 84 : 75;
            NPC.defense = finalPhase ? 25 : 22;

            if (!IsEchoAttack(GetCurrentAttack()))
                StartNextEchoAttack(target, finalPhase);

            AttackTimer++;
            switch (GetCurrentAttack()) {
                case AlbedoAttack.EchoSpeakerLanes:
                    RunSpeakerLaneAttack(target, finalPhase);
                    break;
                case AlbedoAttack.EchoAlternatingHorizontalLanes:
                    RunAlternatingLaneAttack(target, finalPhase, vertical: false);
                    break;
                case AlbedoAttack.EchoAlternatingVerticalLanes:
                    RunAlternatingLaneAttack(target, finalPhase, vertical: true);
                    break;
                case AlbedoAttack.EchoOrbitingSpeakers:
                    RunOrbitingSpeakerAttack(target, finalPhase);
                    break;
                case AlbedoAttack.EchoPulseRings:
                    RunPulseRingAttack(target, finalPhase);
                    break;
                case AlbedoAttack.EchoCrossfire:
                    RunCrossfireAttack(target, finalPhase);
                    break;
                default:
                    StartNextEchoAttack(target, finalPhase);
                    break;
            }
        }

        private void RunUltimatrixSwapPhase(Player target) {
            NPC.defense = 28;

            AlbedoAttack attack = GetCurrentAttack();
            if (!IsHumungousaurAttack(attack) && !IsEchoAttack(attack)) {
                SwapAttackIndex = 0f;
                StartNextSwapAttack(target);
                attack = GetCurrentAttack();
            }

            if (IsHumungousaurAttack(attack))
                RunUltimateHumungousaurPhase(target, finalPhase: true);
            else
                RunUltimateEchoEchoPhase(target, finalPhase: true);
        }

        private void RunRocketBurstAttack(Player target, bool finalPhase) {
            int windup = finalPhase ? 42 : 56;
            int shotSpacing = finalPhase ? 9 : 14;
            int shotCount = finalPhase ? 10 : 6;
            int recoveryStart = windup + shotSpacing * (shotCount - 1) + 22;
            int duration = recoveryStart + (finalPhase ? 28 : 36);

            if (AttackTimer == 1f) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                LockBunkerRockets(target, shotCount, windup, shotSpacing, finalPhase);
                SoundEngine.PlaySound(SoundID.Item61 with { Pitch = -0.18f, Volume = 0.5f }, NPC.Center);
            }

            if (AttackTimer < windup) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                PlantHumungousaur();
                SpawnBunkerLockDust();
                return;
            }

            if (AttackTimer < recoveryStart) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                PlantHumungousaur();
                int elapsed = (int)AttackTimer - windup;
                if (elapsed >= 0 && elapsed % shotSpacing == 0) {
                    int rocketIndex = elapsed / shotSpacing;
                    if (rocketIndex < _lockedRocketCount)
                        FireLockedRocket(rocketIndex, finalPhase ? 11f : 10f, finalPhase ? 31 : 28);
                }

                return;
            }

            SetHumungousaurContactDamage(finalPhase, active: false);
            NPC.velocity.X *= 0.82f;
            if (AttackTimer >= duration)
                CompleteCurrentAttack(target);
        }

        private void RunAirBarrageAttack(Player target, bool finalPhase) {
            int windup = finalPhase ? 44 : 58;
            int shotSpacing = finalPhase ? 9 : 13;
            int shotCount = finalPhase ? 9 : 6;
            int recoveryStart = windup + shotSpacing * (shotCount - 1) + 24;
            int duration = recoveryStart + (finalPhase ? 30 : 38);

            if (AttackTimer == 1f) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                LockAirBarrageRockets(target, shotCount, windup, shotSpacing, finalPhase);
                SoundEngine.PlaySound(SoundID.Item61 with { Pitch = -0.02f, Volume = 0.48f }, NPC.Center);
            }

            if (AttackTimer < windup) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                PlantHumungousaur();
                SpawnBunkerLockDust();
                return;
            }

            if (AttackTimer < recoveryStart) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                PlantHumungousaur();
                int elapsed = (int)AttackTimer - windup;
                if (elapsed >= 0 && elapsed % shotSpacing == 0) {
                    int rocketIndex = elapsed / shotSpacing;
                    if (rocketIndex < _lockedRocketCount)
                        FireLockedRocket(rocketIndex, finalPhase ? 12f : 10.8f, finalPhase ? 31 : 28);
                }

                return;
            }

            SetHumungousaurContactDamage(finalPhase, active: false);
            NPC.velocity.X *= 0.82f;
            if (AttackTimer >= duration)
                CompleteCurrentAttack(target);
        }

        private void RunGroundSlamAttack(Player target, bool finalPhase) {
            int crouchEnd = finalPhase ? 18 : 24;
            int impactTime = finalPhase ? 54 : 68;
            int duration = finalPhase ? 112 : 132;

            if (AttackTimer == 1f) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                PrepareMeteorStomp(impactTime, finalPhase);
                SoundEngine.PlaySound(SoundID.Item37 with { Pitch = -0.55f, Volume = 0.48f }, NPC.Center);
            }

            if (AttackTimer < impactTime) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                NPC.noTileCollide = true;
                NPC.noGravity = true;

                if (AttackTimer < crouchEnd) {
                    Vector2 crouchCenter = _stompStartCenter + new Vector2(0f, 8f);
                    MoveScriptedTowards(crouchCenter, 0.18f);
                    SpawnMeteorStompDust();
                    return;
                }

                float leapProgress = MathHelper.Clamp((AttackTimer - crouchEnd) / Math.Max(1f, impactTime - crouchEnd - 1f), 0f, 1f);
                float arcProgress = MathF.Sin(leapProgress * MathHelper.Pi);
                Vector2 desiredCenter = Vector2.Lerp(_stompStartCenter, _stompApexCenter, arcProgress);
                MoveScriptedTowards(desiredCenter, leapProgress > 0.72f ? 0.72f : 0.42f);

                SpawnMeteorStompDust();
                if (AttackTimer >= impactTime - 18f)
                    SpawnMeteorImpactWarningDust(finalPhase);
                if (AttackTimer == impactTime - 14f)
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.55f, Volume = 0.36f }, _stompOrigin);
                return;
            }

            if (AttackTimer == impactTime) {
                SetHumungousaurContactDamage(finalPhase, active: true);
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                NPC.Center = _stompStartCenter;
                NPC.velocity = Vector2.Zero;
                SpawnMeteorImpactBurstDust(finalPhase);
                PerformMeteorStomp(projectileDamage: finalPhase ? 39 : 36, shockwavePairs: finalPhase ? 5 : 3,
                    speed: finalPhase ? 9.4f : 8.1f);
                return;
            }

            SetHumungousaurContactDamage(finalPhase, active: AttackTimer < impactTime + 8f);
            if (AttackTimer < impactTime + 18f) {
                NPC.noTileCollide = true;
                NPC.noGravity = true;
            }
            NPC.velocity.X *= 0.80f;
            if (AttackTimer >= duration)
                CompleteCurrentAttack(target);
        }

        private void RunChargeAttack(Player target, bool finalPhase) {
            int windup = finalPhase ? 34 : 44;
            int activeFrames = finalPhase ? 36 : 42;
            int chargeEnd = windup + activeFrames;
            int duration = chargeEnd + (finalPhase ? 30 : 38);
            float direction = AttackVariant == 0f ? ResolveAttackDirection(target) : Math.Sign(AttackVariant);
            if (direction == 0f)
                direction = NPC.direction == 0 ? 1f : NPC.direction;

            if (AttackTimer == 1f) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                PrepareTitanRush(direction, windup, finalPhase);
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.35f, Volume = 0.48f }, NPC.Center);
            }

            if (AttackTimer < windup) {
                SetHumungousaurContactDamage(finalPhase, active: false);
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                MoveScriptedTowards(_rushLaneStart, 0.20f);
                SpawnChargeTelegraphDust(direction, finalPhase ? 500f : 430f);
                return;
            }

            if (AttackTimer < chargeEnd) {
                SetHumungousaurContactDamage(finalPhase, active: true);
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                float rushProgress = MathHelper.Clamp((AttackTimer - windup) / Math.Max(1f, activeFrames - 1f), 0f, 1f);
                Vector2 previousCenter = NPC.Center;
                NPC.Center = Vector2.Lerp(_rushLaneStart, _rushLaneEnd, rushProgress);
                NPC.velocity = NPC.Center - previousCenter;
                SpawnAfterimageDust();
                if (AttackTimer == windup)
                    SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.25f, Volume = 0.58f }, NPC.Center);
                return;
            }

            SetHumungousaurContactDamage(finalPhase, active: false);
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.velocity = Vector2.Lerp(NPC.velocity, new Vector2(direction * 2f, -1.1f), 0.10f);
            if (AttackTimer >= duration)
                CompleteCurrentAttack(target);
        }

        private void RunSpeakerLaneAttack(Player target, bool finalPhase) {
            int duration = finalPhase ? 178 : 168;
            MoveAirbornePattern(target, finalPhase ? 310f : 340f, 175f, finalPhase ? 8.5f : 7.6f, 0.05f, 0.032f);

            if (AttackTimer == 1f) {
                ClearAlbedoSpeakers();
                SpawnLaneSpeakers(target, (int)AttackVariant, finalPhase);
                SoundEngine.PlaySound(SoundID.Item43 with { Pitch = -0.15f, Volume = 0.48f }, NPC.Center);
            }

            if (AttackTimer < 58f ||
                (finalPhase && AttackTimer > 72f && AttackTimer < 94f) ||
                (finalPhase && AttackTimer > 108f && AttackTimer < 130f)) {
                SpawnSpeakerWarningDust();
                SpawnLanePathWarningDust(target);
            }

            if (AttackTimer == 58f || (finalPhase && (AttackTimer == 94f || AttackTimer == 130f)))
                FireLanePattern(target, damage: finalPhase ? 29 : 25, delayTicks: finalPhase ? 28f : 34f);

            if (!finalPhase && AttackTimer == 102f)
                FireLanePattern(target, damage: 25, delayTicks: 34f);

            if (AttackTimer >= duration)
                CompleteCurrentAttack(target);
        }

        private void RunPulseRingAttack(Player target, bool finalPhase) {
            int duration = finalPhase ? 188 : 166;
            MoveAirbornePattern(target, 280f, finalPhase ? 160f : 185f, finalPhase ? 8.2f : 7.4f, 0.054f, 0.036f);

            if (AttackTimer == 1f) {
                ClearAlbedoSpeakers();
                SpawnPulseSpeakers(target, finalPhase);
                SoundEngine.PlaySound(SoundID.Item38 with { Pitch = -0.3f, Volume = 0.46f }, NPC.Center);
            }

            if (AttackTimer < 46f) {
                SpawnSpeakerWarningDust();
                SpawnPulseRingWarningDust();
            }

            if (AttackTimer == 46f)
                FirePulsePattern(damage: finalPhase ? 31 : 27, delayTicks: 32f, staggerTicks: 10f);

            if (finalPhase && AttackTimer > 64f && AttackTimer < 94f) {
                SpawnSpeakerWarningDust();
                SpawnPulseRingWarningDust(64f, 30f);
            }

            if (finalPhase && AttackTimer == 94f)
                FirePulsePattern(damage: 29, delayTicks: 24f, staggerTicks: 7f);

            if (finalPhase && AttackTimer > 108f && AttackTimer < 132f) {
                SpawnSpeakerWarningDust();
                SpawnPulseRingWarningDust(108f, 24f);
            }

            if (finalPhase && AttackTimer == 132f)
                FirePulsePattern(damage: 29, delayTicks: 20f, staggerTicks: 6f);

            if (AttackTimer >= duration)
                CompleteCurrentAttack(target);
        }

        private void RunAlternatingLaneAttack(Player target, bool finalPhase, bool vertical) {
            int firstFire = finalPhase ? 50 : 58;
            int repositionTime = firstFire + (finalPhase ? 16 : 18);
            int secondFire = finalPhase ? 102 : 116;
            int duration = finalPhase ? 154 : 176;
            MoveAirbornePattern(target, vertical ? 250f : 330f, vertical ? 190f : 150f,
                finalPhase ? 8.7f : 7.8f, 0.054f, 0.036f);

            if (AttackTimer == 1f) {
                ClearAlbedoSpeakers();
                SpawnAlternatingLaneSpeakers(target, vertical, finalPhase);
                SpawnAlternatingLanePositionDust(target, vertical, setIndex: 0, finalPhase);
                SoundEngine.PlaySound(SoundID.Item43 with { Pitch = vertical ? 0.05f : -0.05f, Volume = 0.48f }, NPC.Center);
            }

            if (AttackTimer < firstFire) {
                SpawnAlternatingLaneWarningDust(vertical);
                return;
            }

            if (AttackTimer == firstFire)
                FireAlternatingLanePattern(damage: finalPhase ? 30 : 26, vertical: vertical, delayTicks: 0f);

            if (AttackTimer == repositionTime) {
                MoveAlternatingLaneSpeakers(target, vertical, setIndex: 1, finalPhase);
                SpawnAlternatingLanePositionDust(target, vertical, setIndex: 1, finalPhase);
                SoundEngine.PlaySound(SoundID.Item43 with { Pitch = vertical ? 0.18f : 0.08f, Volume = 0.36f }, NPC.Center);
            }

            if (AttackTimer > repositionTime && AttackTimer < secondFire) {
                SpawnAlternatingLaneWarningDust(vertical);
                return;
            }

            if (AttackTimer == secondFire)
                FireAlternatingLanePattern(damage: finalPhase ? 30 : 26, vertical: vertical, delayTicks: 0f);

            if (AttackTimer >= duration)
                CompleteCurrentAttack(target);
        }

        private void RunOrbitingSpeakerAttack(Player target, bool finalPhase) {
            int windup = finalPhase ? 52 : 62;
            int fireSpacing = finalPhase ? 40 : 54;
            int duration = finalPhase ? 196 : 214;
            MoveAirbornePattern(target, finalPhase ? 260f : 300f, finalPhase ? 155f : 175f,
                finalPhase ? 8.7f : 7.8f, 0.054f, 0.038f);

            if (AttackTimer == 1f) {
                ClearAlbedoSpeakers();
                SpawnOrbitSpeakers(finalPhase ? 9 : 6, finalPhase);
                SoundEngine.PlaySound(SoundID.Item38 with { Pitch = -0.18f, Volume = 0.46f }, NPC.Center);
            }

            PositionOrbitSpeakers(finalPhase);
            SpawnOrbitWarningDust(finalPhase);

            if (AttackTimer >= windup && AttackTimer < duration - 18 &&
                ((int)AttackTimer - windup) % fireSpacing == 0)
                FireOrbitingSpeakerPattern(damage: finalPhase ? 29 : 25, delayTicks: finalPhase ? 24f : 30f);

            if (AttackTimer >= duration)
                CompleteCurrentAttack(target);
        }

        private void RunCrossfireAttack(Player target, bool finalPhase) {
            int duration = finalPhase ? 178 : 170;
            MoveAirbornePattern(target, finalPhase ? 360f : 330f, 190f, finalPhase ? 8.6f : 7.8f, 0.052f, 0.034f);

            if (AttackTimer == 1f) {
                ClearAlbedoSpeakers();
                SpawnCrossfireSpeakers(target, finalPhase);
                SoundEngine.PlaySound(SoundID.Item92 with { Pitch = -0.15f, Volume = 0.44f }, NPC.Center);
            }

            if (AttackTimer < 54f) {
                SpawnSpeakerWarningDust();
                SpawnCrossfireWarningDust(target);
            }

            if (AttackTimer == 54f)
                FireCrossfirePattern(target, damage: finalPhase ? 30 : 26, delayTicks: finalPhase ? 28f : 34f);

            if (AttackTimer > 70f && AttackTimer < 100f)
                SpawnCrossfireWarningDust(target, invert: true);

            if (AttackTimer == 100f)
                FireCrossfirePattern(target, damage: finalPhase ? 30 : 26, delayTicks: finalPhase ? 26f : 32f,
                    invert: true);

            if (finalPhase && AttackTimer > 112f && AttackTimer < 134f)
                SpawnCrossfireWarningDust(target);

            if (finalPhase && AttackTimer == 134f)
                FireCrossfirePattern(target, damage: 30, delayTicks: 24f);

            if (AttackTimer >= duration)
                CompleteCurrentAttack(target);
        }

        private void UpdatePhaseThresholds() {
            float lifeRatio = NPC.life / (float)NPC.lifeMax;
            AlbedoPhase currentPhase = GetCurrentPhase();

            if (currentPhase == AlbedoPhase.IntroHuman)
                return;

            if (lifeRatio <= 0.20f && currentPhase != AlbedoPhase.UltimatrixSwap) {
                ShowActionText("Ultimatrix swap!");
                TransitionToPhase(AlbedoPhase.UltimatrixSwap);
            }
            else if (lifeRatio <= 0.45f && currentPhase == AlbedoPhase.UltimateHumungousaur) {
                ShowActionText("Ultimate Echo Echo!");
                TransitionToPhase(AlbedoPhase.UltimateEchoEcho);
            }
        }

        private AlbedoPhase GetCurrentPhase() {
            return (AlbedoPhase)(int)MathHelper.Clamp(PhaseState, 0f, (float)AlbedoPhase.UltimatrixSwap);
        }

        private AlbedoAttack GetCurrentAttack() {
            return Enum.IsDefined(typeof(AlbedoAttack), (int)AttackState)
                ? (AlbedoAttack)(int)AttackState
                : AlbedoAttack.Intro;
        }

        private void TransitionToPhase(AlbedoPhase nextPhase) {
            PhaseState = (float)nextPhase;
            AttackTimer = 0f;
            AttackVariant = 0f;
            IntroStage = nextPhase == AlbedoPhase.IntroHuman ? 0f : 4f;
            ResetMovementTarget();
            ClearAlbedoProjectiles();

            switch (nextPhase) {
                case AlbedoPhase.UltimateHumungousaur:
                    HumungousaurAttackIndex = 1f;
                    StartAttack(AlbedoAttack.HumungousaurRocketBurst, 0f);
                    break;
                case AlbedoPhase.UltimateEchoEcho:
                    EchoAttackIndex = 1f;
                    StartAttack(AlbedoAttack.EchoSpeakerLanes, 0f);
                    break;
                case AlbedoPhase.UltimatrixSwap:
                    SwapAttackIndex = 1f;
                    StartAttack(AlbedoAttack.HumungousaurCharge, ResolveAttackDirection(Main.player[NPC.target]));
                    ShowActionText("Ultimate Humungousaur!");
                    break;
                default:
                    StartAttack(AlbedoAttack.Intro, 0f);
                    break;
            }

            NPC.netUpdate = true;
            PlayTransformationEffect();
        }

        private void StartNextHumungousaurAttack(Player target, bool finalPhase) {
            int cycle = (int)HumungousaurAttackIndex++ % 4;
            AlbedoAttack nextAttack = cycle switch {
                0 => AlbedoAttack.HumungousaurRocketBurst,
                1 => AlbedoAttack.HumungousaurGroundSlam,
                2 => AlbedoAttack.HumungousaurCharge,
                _ => AlbedoAttack.HumungousaurAirBarrage
            };

            float variant = nextAttack == AlbedoAttack.HumungousaurCharge ? ResolveAttackDirection(target) : cycle;
            StartAttack(nextAttack, variant);
        }

        private void StartNextEchoAttack(Player target, bool finalPhase) {
            int cycle = (int)EchoAttackIndex++ % 6;
            AlbedoAttack nextAttack = cycle switch {
                0 => AlbedoAttack.EchoSpeakerLanes,
                1 => AlbedoAttack.EchoAlternatingHorizontalLanes,
                2 => AlbedoAttack.EchoAlternatingVerticalLanes,
                3 => AlbedoAttack.EchoOrbitingSpeakers,
                4 => AlbedoAttack.EchoPulseRings,
                _ => AlbedoAttack.EchoCrossfire
            };

            StartAttack(nextAttack, cycle);
        }

        private void StartNextSwapAttack(Player target) {
            int cycle = (int)SwapAttackIndex++ % 10;
            switch (cycle) {
                case 0:
                    ShowActionText("Ultimate Humungousaur!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.HumungousaurCharge, ResolveAttackDirection(target));
                    break;
                case 1:
                    ShowActionText("Ultimate Echo Echo!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.EchoSpeakerLanes, cycle);
                    break;
                case 2:
                    ShowActionText("Ultimate Humungousaur!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.HumungousaurRocketBurst, cycle);
                    break;
                case 3:
                    ShowActionText("Ultimate Echo Echo!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.EchoAlternatingHorizontalLanes, cycle);
                    break;
                case 4:
                    ShowActionText("Ultimate Humungousaur!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.HumungousaurGroundSlam, cycle);
                    break;
                case 5:
                    ShowActionText("Ultimate Echo Echo!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.EchoAlternatingVerticalLanes, cycle);
                    break;
                case 6:
                    ShowActionText("Ultimate Humungousaur!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.HumungousaurCharge, ResolveAttackDirection(target));
                    break;
                case 7:
                    ShowActionText("Ultimate Echo Echo!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.EchoOrbitingSpeakers, cycle);
                    break;
                case 8:
                    ShowActionText("Ultimate Humungousaur!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.HumungousaurAirBarrage, cycle);
                    break;
                default:
                    ShowActionText("Ultimate Echo Echo!");
                    PlayTransformationEffect();
                    StartAttack(AlbedoAttack.EchoCrossfire, cycle);
                    break;
            }
        }

        private void StartAttack(AlbedoAttack attack, float variant) {
            AttackState = (float)attack;
            AttackTimer = 0f;
            AttackVariant = variant;
            ResetMovementTarget();
            NPC.netUpdate = true;
        }

        private void CompleteCurrentAttack(Player target) {
            ClearAlbedoSpeakers();
            ClearAlbedoWarnings();

            if (GetCurrentPhase() == AlbedoPhase.UltimatrixSwap)
                StartNextSwapAttack(target);
            else if (IsHumungousaurAttack(GetCurrentAttack()))
                StartNextHumungousaurAttack(target, finalPhase: false);
            else
                StartNextEchoAttack(target, finalPhase: false);
        }

        private static bool IsHumungousaurAttack(AlbedoAttack attack) {
            return attack is AlbedoAttack.HumungousaurRocketBurst
                or AlbedoAttack.HumungousaurGroundSlam
                or AlbedoAttack.HumungousaurCharge
                or AlbedoAttack.HumungousaurAirBarrage;
        }

        private static bool IsEchoAttack(AlbedoAttack attack) {
            return attack is AlbedoAttack.EchoSpeakerLanes
                or AlbedoAttack.EchoAlternatingHorizontalLanes
                or AlbedoAttack.EchoAlternatingVerticalLanes
                or AlbedoAttack.EchoOrbitingSpeakers
                or AlbedoAttack.EchoPulseRings
                or AlbedoAttack.EchoCrossfire;
        }

        private SwapForm GetCurrentSwapForm() {
            return IsEchoAttack(GetCurrentAttack()) ? SwapForm.EchoEcho : SwapForm.Humungousaur;
        }

        private void MoveTowards(Vector2 targetPosition, float maxSpeed, float inertia) {
            if (Vector2.Distance(NPC.Center, targetPosition) < 48f) {
                NPC.velocity *= 0.92f;
                return;
            }

            Vector2 desiredVelocity = NPC.DirectionTo(targetPosition) * maxSpeed;
            NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, inertia);
        }

        private void MoveGroundedTowards(Player target, float orbitDistance, float maxSpeed, float acceleration) {
            if (!_hasMoveTarget || _moveTargetCooldown <= 0 || Math.Abs(_moveTarget.X - NPC.Center.X) < 28f) {
                float phase = Main.GameUpdateCount * 0.045f + MovementSeed;
                float desiredHorizontalOffset = MathF.Sin(phase) * orbitDistance;
                _moveTarget = new Vector2(target.Center.X + desiredHorizontalOffset, NPC.Center.Y);
                _moveTargetCooldown = 40;
                _hasMoveTarget = true;
            }

            _moveTargetCooldown--;

            float horizontalDistance = _moveTarget.X - NPC.Center.X;
            float desiredVelocityX = MathHelper.Clamp(horizontalDistance * 0.04f, -maxSpeed, maxSpeed);

            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, desiredVelocityX, acceleration);

            if (Math.Abs(horizontalDistance) < 36f)
                NPC.velocity.X *= 0.85f;

            bool shouldHop = (NPC.collideX || Math.Abs(horizontalDistance) < 80f || target.Center.Y < NPC.Center.Y - 32f) &&
                             NPC.velocity.Y == 0f &&
                             AttackTimer % 75f < 2f;
            if (shouldHop)
                NPC.velocity.Y = -9.5f;
        }

        private void MoveAirbornePattern(Player target, float horizontalRadius, float verticalRadius, float maxSpeed, float inertia, float bobSpeed) {
            if (!_hasMoveTarget || _moveTargetCooldown <= 0 || Vector2.Distance(NPC.Center, _moveTarget) < 36f) {
                float time = Main.GameUpdateCount * bobSpeed + MovementSeed;
                _moveTarget = target.Center + new Vector2(
                    MathF.Sin(time) * horizontalRadius,
                    -260f + MathF.Cos(time * 1.35f) * verticalRadius);
                _moveTargetCooldown = 55;
                _hasMoveTarget = true;
            }

            _moveTargetCooldown--;
            MoveTowards(_moveTarget, maxSpeed, inertia);
        }

        private void ResetMovementTarget() {
            _moveTarget = NPC.Center;
            _moveTargetCooldown = 0;
            _hasMoveTarget = false;
        }

        private void SetMovementProfile(bool canFly) {
            NPC.noGravity = canFly;
            NPC.noTileCollide = canFly;
        }

        private void SetHumungousaurContactDamage(bool finalPhase, bool active) {
            NPC.damage = active ? (finalPhase ? 110 : 100) : 0;
        }

        private float ResolveAttackDirection(Player target) {
            float direction = Math.Sign(target.Center.X - NPC.Center.X);
            if (direction == 0f)
                direction = NPC.direction == 0 ? 1f : NPC.direction;

            return direction;
        }

        private void PlantHumungousaur() {
            NPC.velocity.X *= 0.62f;
            if (Math.Abs(NPC.velocity.X) < 0.12f)
                NPC.velocity.X = 0f;
        }

        private void MoveScriptedTowards(Vector2 destination, float smoothing) {
            Vector2 previousCenter = NPC.Center;
            NPC.Center = Vector2.Lerp(NPC.Center, destination, smoothing);
            NPC.velocity = NPC.Center - previousCenter;
        }

        private void PrepareTitanRush(float direction, int windup, bool finalPhase) {
            float backstepDistance = finalPhase ? 190f : 165f;
            float forwardDistance = finalPhase ? 780f : 690f;
            _rushLaneStart = new Vector2(NPC.Center.X - direction * backstepDistance, NPC.Center.Y);
            _rushLaneEnd = new Vector2(NPC.Center.X + direction * forwardDistance, NPC.Center.Y);

            SpawnWarningProjectile(AlbedoWarningProjectile.ModeRushLane, _rushLaneStart,
                _rushLaneEnd - _rushLaneStart, windup + 4, finalPhase ? 52f : 46f);
        }

        private void LockBunkerRockets(Player target, int shotCount, int windup, int shotSpacing, bool finalPhase) {
            Vector2 center = target.Center;
            Vector2[] offsets = finalPhase
                ? new Vector2[] {
                    new(-250f, -150f),
                    new(-80f, -175f),
                    new(90f, -170f),
                    new(260f, -135f),
                    new(-190f, 40f),
                    new(0f, 72f),
                    new(195f, 38f),
                    new(65f, -55f),
                    new(-300f, -15f),
                    new(305f, -20f)
                }
                : new Vector2[] {
                    new(-220f, -135f),
                    new(-70f, -160f),
                    new(85f, -155f),
                    new(230f, -120f),
                    new(-125f, 52f),
                    new(135f, 50f)
                };

            _lockedRocketCount = Math.Min(shotCount, _lockedRocketTargets.Length);
            for (int i = 0; i < _lockedRocketCount; i++) {
                _lockedRocketOrigins[i] = NPC.Center;
                _lockedRocketTargets[i] = center + offsets[i % offsets.Length];
                int warningTime = windup + i * shotSpacing + 12;
                SpawnWarningProjectile(AlbedoWarningProjectile.ModeRocketLine, _lockedRocketOrigins[i],
                    _lockedRocketTargets[i] - _lockedRocketOrigins[i], warningTime, finalPhase ? 12f : 10f);
            }
        }

        private void LockAirBarrageRockets(Player target, int shotCount, int windup, int shotSpacing, bool finalPhase) {
            Vector2 center = target.Center;
            Vector2[] origins = finalPhase
                ? new Vector2[] {
                    new(-620f, -330f),
                    new(620f, -310f),
                    new(-560f, 105f),
                    new(560f, 120f),
                    new(0f, -430f),
                    new(-340f, -20f),
                    new(340f, -35f),
                    new(-120f, 260f),
                    new(120f, 245f)
                }
                : new Vector2[] {
                    new(-560f, -300f),
                    new(560f, -285f),
                    new(-520f, 95f),
                    new(520f, 110f),
                    new(0f, -390f),
                    new(320f, -25f)
                };
            Vector2[] targetOffsets = finalPhase
                ? new Vector2[] {
                    new(-150f, -185f),
                    new(145f, -160f),
                    new(-210f, 35f),
                    new(220f, 55f),
                    new(0f, -45f),
                    new(-70f, 160f),
                    new(85f, 135f),
                    new(-255f, -70f),
                    new(260f, -80f)
                }
                : new Vector2[] {
                    new(-135f, -165f),
                    new(135f, -145f),
                    new(-185f, 35f),
                    new(190f, 48f),
                    new(0f, -40f),
                    new(80f, 130f)
                };

            _lockedRocketCount = Math.Min(shotCount, _lockedRocketTargets.Length);
            for (int i = 0; i < _lockedRocketCount; i++) {
                _lockedRocketOrigins[i] = center + origins[i % origins.Length];
                _lockedRocketTargets[i] = center + targetOffsets[i % targetOffsets.Length];
                int warningTime = windup + i * shotSpacing + 14;
                SpawnWarningProjectile(AlbedoWarningProjectile.ModeRocketLine, _lockedRocketOrigins[i],
                    _lockedRocketTargets[i] - _lockedRocketOrigins[i], warningTime, finalPhase ? 12f : 10f);
            }
        }

        private void FireLockedRocket(int rocketIndex, float speed, int damage) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 targetPoint = rocketIndex >= 0 && rocketIndex < _lockedRocketCount
                ? _lockedRocketTargets[rocketIndex]
                : NPC.Center + new Vector2(NPC.direction == 0 ? 1f : NPC.direction, 0f) * 320f;
            Vector2 origin = rocketIndex >= 0 && rocketIndex < _lockedRocketCount
                ? _lockedRocketOrigins[rocketIndex]
                : NPC.Center;
            Vector2 velocity = (targetPoint - origin).SafeNormalize(Vector2.UnitX * (NPC.direction == 0 ? 1f : NPC.direction)) * speed;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), origin, velocity,
                ModContent.ProjectileType<AlbedoRocketProjectile>(), damage, 0f, Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.08f, Volume = 0.52f }, NPC.Center);
        }

        private void PrepareMeteorStomp(int impactTime, bool finalPhase) {
            _stompStartCenter = NPC.Center;
            _stompApexCenter = NPC.Center - new Vector2(0f, finalPhase ? 170f : 145f);
            _stompOrigin = NPC.Bottom + new Vector2(0f, -10f);
            float warningLength = finalPhase ? 520f : 450f;

            SpawnWarningProjectile(AlbedoWarningProjectile.ModeShockwaveLane, _stompOrigin,
                new Vector2(warningLength, 0f), impactTime + 4, finalPhase ? 30f : 26f);
            SpawnWarningProjectile(AlbedoWarningProjectile.ModeShockwaveLane, _stompOrigin,
                new Vector2(-warningLength, 0f), impactTime + 4, finalPhase ? 30f : 26f);
        }

        private void PerformMeteorStomp(int projectileDamage, int shockwavePairs, float speed) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 spawnPosition = _stompOrigin == Vector2.Zero ? NPC.Bottom + new Vector2(0f, -10f) : _stompOrigin;
            for (int i = 0; i < shockwavePairs; i++) {
                float projectileSpeed = speed + i * 1.1f;
                float delayOffset = i * 14f;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition + new Vector2(14f + delayOffset, 0f),
                    new Vector2(projectileSpeed, 0f), ModContent.ProjectileType<AlbedoShockwaveProjectile>(),
                    projectileDamage, 0f, Main.myPlayer, 1f, i);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition + new Vector2(-14f - delayOffset, 0f),
                    new Vector2(-projectileSpeed, 0f), ModContent.ProjectileType<AlbedoShockwaveProjectile>(),
                    projectileDamage, 0f, Main.myPlayer, -1f, i);
            }

            NPC.velocity.Y = -4.2f;
            NPC.velocity.X *= 0.38f;
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.12f, Volume = 0.62f }, NPC.Center);
        }

        private void SpawnWarningProjectile(int mode, Vector2 origin, Vector2 warningVector, int timeLeft, float width) {
            if (Main.netMode == NetmodeID.MultiplayerClient || warningVector.LengthSquared() <= 0.01f)
                return;

            int projectileIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), origin, Vector2.Zero,
                ModContent.ProjectileType<AlbedoWarningProjectile>(), 0, 0f, Main.myPlayer, mode, width);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile warning = Main.projectile[projectileIndex];
                warning.localAI[0] = warningVector.X;
                warning.localAI[1] = warningVector.Y;
                warning.timeLeft = Math.Max(6, timeLeft);
                warning.netUpdate = true;
            }
        }

        private void SpawnBunkerLockDust() {
            if (Main.dedServ || !Main.rand.NextBool(2))
                return;

            for (int i = 0; i < _lockedRocketCount; i++) {
                Vector2 targetPoint = _lockedRocketTargets[i];
                Dust dust = Dust.NewDustPerfect(targetPoint + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.Firework_Red, Main.rand.NextVector2Circular(0.4f, 0.4f), 110,
                    new Color(255, 80, 64), 0.95f);
                dust.noGravity = true;
            }
        }

        private void SpawnMeteorStompDust() {
            if (Main.dedServ || !Main.rand.NextBool(2))
                return;

            for (int i = 0; i < 2; i++) {
                float direction = i == 0 ? -1f : 1f;
                Vector2 position = _stompOrigin + new Vector2(direction * Main.rand.NextFloat(36f, 420f), Main.rand.NextFloat(-6f, 10f));
                Dust dust = Dust.NewDustPerfect(position, DustID.Smoke, new Vector2(-direction * 0.2f, -0.65f),
                    105, new Color(255, 142, 80), 1.2f);
                dust.noGravity = true;
            }
        }

        private void SpawnMeteorImpactWarningDust(bool finalPhase) {
            if (Main.dedServ)
                return;

            int dustCount = finalPhase ? 5 : 4;
            for (int i = 0; i < dustCount; i++) {
                float angle = MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.12f, 0.12f);
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(24f, finalPhase ? 58f : 48f);
                Dust dust = Dust.NewDustPerfect(_stompOrigin + offset, DustID.Torch, -offset.SafeNormalize(Vector2.UnitY) * 0.7f,
                    90, new Color(255, 165, 80), finalPhase ? 1.45f : 1.25f);
                dust.noGravity = true;
            }
        }

        private void SpawnMeteorImpactBurstDust(bool finalPhase) {
            if (Main.dedServ)
                return;

            int dustCount = finalPhase ? 42 : 34;
            for (int i = 0; i < dustCount; i++) {
                Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 0.55f).SafeNormalize(Vector2.UnitY);
                Vector2 position = _stompOrigin + new Vector2(Main.rand.NextFloat(-28f, 28f), Main.rand.NextFloat(-10f, 10f));
                int dustType = i % 3 == 0 ? DustID.Smoke : DustID.Torch;
                Dust dust = Dust.NewDustPerfect(position, dustType,
                    new Vector2(direction.X * Main.rand.NextFloat(2f, 6f), -Math.Abs(direction.Y) * Main.rand.NextFloat(1.8f, 5.5f)),
                    80, new Color(255, 142, 70), finalPhase ? 1.65f : 1.45f);
                dust.noGravity = true;
            }
        }

        private void SpawnLaneSpeakers(Player target, int variant, bool finalPhase) {
            Vector2 center = target.Center;
            float side = variant % 2 == 0 ? 1f : -1f;
            Vector2[] offsets = finalPhase
                ? new Vector2[] {
                    new(-470f * side, -310f),
                    new(-470f * side, -115f),
                    new(-470f * side, 80f),
                    new(-470f * side, 275f),
                    new(470f * side, -245f),
                    new(470f * side, -50f),
                    new(470f * side, 145f),
                    new(470f * side, 340f)
                }
                : new Vector2[] {
                    new(-430f * side, -260f),
                    new(-430f * side, -65f),
                    new(-430f * side, 130f),
                    new(430f * side, -160f),
                    new(430f * side, 35f),
                    new(430f * side, 230f)
                };

            foreach (Vector2 offset in offsets)
                SpawnSpeakerAt(center + offset, 24);
        }

        private void SpawnPulseSpeakers(Player target, bool finalPhase) {
            Vector2 center = target.Center;
            Vector2[] offsets = finalPhase
                ? new Vector2[] {
                    new(0f, -255f),
                    new(-285f, -70f),
                    new(285f, -70f),
                    new(0f, 145f)
                }
                : new Vector2[] {
                    new(0f, -230f),
                    new(-250f, -45f),
                    new(250f, -45f)
                };

            foreach (Vector2 offset in offsets)
                SpawnSpeakerAt(center + offset, 26);
        }

        private void SpawnCrossfireSpeakers(Player target, bool finalPhase) {
            Vector2 center = target.Center;
            Vector2[] offsets = finalPhase
                ? new Vector2[] {
                    new(-350f, -250f),
                    new(350f, -250f),
                    new(-350f, -40f),
                    new(350f, -40f),
                    new(-350f, 170f),
                    new(350f, 170f)
                }
                : new Vector2[] {
                    new(-300f, -190f),
                    new(300f, -190f),
                    new(-300f, 95f),
                    new(300f, 95f)
                };

            foreach (Vector2 offset in offsets)
                SpawnSpeakerAt(center + offset, 25);
        }

        private void SpawnAlternatingLaneSpeakers(Player target, bool vertical, bool finalPhase) {
            Vector2[] anchors = GetAlternatingLaneAnchors(target.Center, vertical, setIndex: 0, finalPhase);

            foreach (Vector2 anchor in anchors)
                SpawnSpeakerAt(anchor, 24);
        }

        private void MoveAlternatingLaneSpeakers(Player target, bool vertical, int setIndex, bool finalPhase) {
            Vector2[] anchors = GetAlternatingLaneAnchors(target.Center, vertical, setIndex, finalPhase);
            int speakerIndex = 0;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                if (speakerIndex >= anchors.Length) {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        speaker.Kill();
                    continue;
                }

                SetSpeakerAnchor(speaker, anchors[speakerIndex]);
                speakerIndex++;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            while (speakerIndex < anchors.Length) {
                SpawnSpeakerAt(anchors[speakerIndex], 24);
                speakerIndex++;
            }
        }

        private static Vector2[] GetAlternatingLaneAnchors(Vector2 center, bool vertical, int setIndex,
            bool finalPhase = false) {
            float[] laneOffsets;
            if (finalPhase) {
                laneOffsets = vertical
                    ? (setIndex == 0
                        ? new[] { -840f, -280f, 280f, 840f }
                        : new[] { -600f, -80f, 440f, 960f })
                    : (setIndex == 0
                        ? new[] { -760f, -300f, 160f, 620f }
                        : new[] { -530f, -70f, 390f, 850f });
            }
            else {
                laneOffsets = vertical
                    ? (setIndex == 0
                        ? new[] { -720f, -120f, 480f }
                        : new[] { -480f, 120f, 720f })
                    : (setIndex == 0
                        ? new[] { -600f, -120f, 360f }
                        : new[] { -360f, 120f, 600f });
            }

            Vector2[] anchors = new Vector2[laneOffsets.Length * 2];
            for (int i = 0; i < laneOffsets.Length; i++) {
                if (vertical) {
                    anchors[i * 2] = center + new Vector2(laneOffsets[i], -AlternatingVerticalHalfHeight);
                    anchors[i * 2 + 1] = center + new Vector2(laneOffsets[i], AlternatingVerticalHalfHeight);
                }
                else {
                    anchors[i * 2] = center + new Vector2(-AlternatingHorizontalHalfWidth, laneOffsets[i]);
                    anchors[i * 2 + 1] = center + new Vector2(AlternatingHorizontalHalfWidth, laneOffsets[i]);
                }
            }

            return anchors;
        }

        private void SpawnOrbitSpeakers(int speakerCount, bool finalPhase) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < speakerCount; i++)
                SpawnSpeakerAt(GetOrbitSpeakerAnchor(i, speakerCount, finalPhase), 24);
        }

        private void PositionOrbitSpeakers(bool finalPhase) {
            int speakerCount = CountAlbedoSpeakers();
            if (speakerCount <= 0)
                return;

            int speakerIndex = 0;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                SetSpeakerAnchor(speaker, GetOrbitSpeakerAnchor(speakerIndex, speakerCount, finalPhase));
                speakerIndex++;
            }
        }

        private Vector2 GetOrbitSpeakerAnchor(int speakerIndex, int speakerCount, bool finalPhase) {
            float radius = finalPhase ? 260f : 230f;
            float spinSpeed = finalPhase ? 0.046f : 0.036f;
            float angle = MathHelper.TwoPi * speakerIndex / Math.Max(1, speakerCount) +
                          AttackTimer * spinSpeed + MovementSeed;
            return NPC.Center + angle.ToRotationVector2() * radius;
        }

        private int CountAlbedoSpeakers() {
            int speakerCount = 0;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                if (IsAlbedoSpeaker(Main.projectile[i]))
                    speakerCount++;
            }

            return speakerCount;
        }

        private static void SetSpeakerAnchor(Projectile speaker, Vector2 anchor) {
            speaker.ai[0] = anchor.X;
            speaker.ai[1] = anchor.Y;
            speaker.netUpdate = true;
        }

        private void SpawnSpeakerAt(Vector2 anchor, int damage) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 velocity = NPC.DirectionTo(anchor) * 8f;
            int projectileIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                ModContent.ProjectileType<AlbedoSpeakerProjectile>(), damage, 0f, Main.myPlayer, anchor.X, anchor.Y);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Projectile speaker = Main.projectile[projectileIndex];
                speaker.timeLeft = 180;
                speaker.netUpdate = true;
            }
        }

        private void FireLanePattern(Player target, int damage, float delayTicks) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                float direction = speaker.Center.X < target.Center.X ? 1f : -1f;
                FireSonicShot(speaker.Center, new Vector2(direction, 0f), 11.5f, damage, delayTicks, SonicShotMode);
            }
        }

        private void FirePulsePattern(int damage, float delayTicks, float staggerTicks) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int speakerIndex = 0;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                FireSonicShot(speaker.Center, Vector2.Zero, 0f, damage, delayTicks + speakerIndex * staggerTicks,
                    SonicPulseMode);
                speakerIndex++;
            }
        }

        private void FireCrossfirePattern(Player target, int damage, float delayTicks, bool invert = false) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 center = target.Center;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                Vector2 direction = center - speaker.Center;
                if (invert)
                    direction = new Vector2(direction.X, -direction.Y);

                FireSonicShot(speaker.Center, direction.SafeNormalize(Vector2.UnitX), 12.2f, damage, delayTicks,
                    SonicShotMode);
            }
        }

        private void FireAlternatingLanePattern(int damage, bool vertical, float delayTicks) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (!TryGetSpeakerCenter(out Vector2 speakerCenter))
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                if (!TryGetAlternatingLaneData(speaker, speakerCenter, vertical, out Vector2 direction))
                    continue;

                FireSonicShot(speaker.Center, direction, vertical ? 17f : 17.5f, damage, delayTicks, SonicShotMode);
            }
        }

        private void FireOrbitingSpeakerPattern(int damage, float delayTicks) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                Vector2 direction = (GetSpeakerAnchor(speaker) - NPC.Center).SafeNormalize(Vector2.UnitY);
                FireSonicShot(speaker.Center, direction, 12.6f, damage, delayTicks, SonicShotMode);
            }
        }

        private void FireSonicShot(Vector2 origin, Vector2 direction, float speed, int damage, float delayTicks, int mode) {
            Vector2 velocity = direction == Vector2.Zero ? Vector2.Zero : direction.SafeNormalize(Vector2.UnitX) * speed;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), origin, velocity,
                ModContent.ProjectileType<AlbedoSonicBlastProjectile>(), damage, 0f, Main.myPlayer, delayTicks, mode);
        }

        private static bool IsAlbedoSpeaker(Projectile projectile) {
            return projectile.active && projectile.type == ModContent.ProjectileType<AlbedoSpeakerProjectile>();
        }

        private static bool IsAlbedoWarning(Projectile projectile) {
            return projectile.active && projectile.type == ModContent.ProjectileType<AlbedoWarningProjectile>();
        }

        private bool TryGetSpeakerCenter(out Vector2 speakerCenter) {
            speakerCenter = Vector2.Zero;
            int speakerCount = 0;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                speakerCenter += GetSpeakerAnchor(speaker);
                speakerCount++;
            }

            if (speakerCount <= 0)
                return false;

            speakerCenter /= speakerCount;
            return true;
        }

        private static bool TryGetAlternatingLaneData(Projectile speaker, Vector2 speakerCenter, bool vertical,
            out Vector2 direction) {
            direction = Vector2.Zero;
            Vector2 anchor = GetSpeakerAnchor(speaker);

            if (vertical) {
                direction = anchor.Y < speakerCenter.Y ? Vector2.UnitY : -Vector2.UnitY;
                return true;
            }

            direction = anchor.X < speakerCenter.X ? Vector2.UnitX : -Vector2.UnitX;
            return true;
        }

        private static Vector2 GetSpeakerAnchor(Projectile speaker) {
            Vector2 anchor = new(speaker.ai[0], speaker.ai[1]);
            return anchor == Vector2.Zero ? speaker.Center : anchor;
        }

        private void ClearAlbedoSpeakers() {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile projectile = Main.projectile[i];
                if (IsAlbedoSpeaker(projectile))
                    projectile.Kill();
            }
        }

        private void ClearAlbedoWarnings() {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile projectile = Main.projectile[i];
                if (IsAlbedoWarning(projectile))
                    projectile.Kill();
            }
        }

        private void ClearAlbedoProjectiles() {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active)
                    continue;

                if (projectile.type == ModContent.ProjectileType<AlbedoSpeakerProjectile>() ||
                    projectile.type == ModContent.ProjectileType<AlbedoSonicBlastProjectile>() ||
                    projectile.type == ModContent.ProjectileType<AlbedoWarningProjectile>())
                    projectile.Kill();
            }
        }

        private void SpawnAimTelegraphDust(Vector2 origin, Vector2 direction, float length, Color color) {
            if (Main.dedServ || !Main.rand.NextBool(2))
                return;

            Vector2 normalized = direction.SafeNormalize(Vector2.UnitX);
            float distance = Main.rand.NextFloat(40f, length);
            Dust dust = Dust.NewDustPerfect(origin + normalized * distance + Main.rand.NextVector2Circular(4f, 4f),
                DustID.Firework_Red, -normalized * 0.5f, 120, color, 1.05f);
            dust.noGravity = true;
        }

        private void SpawnGroundTelegraphDust(float direction, float length) {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 3; i++) {
                float distance = Main.rand.NextFloat(36f, length);
                Vector2 position = NPC.Bottom + new Vector2(direction * distance, Main.rand.NextFloat(-8f, 10f));
                Dust dust = Dust.NewDustPerfect(position, DustID.Smoke, new Vector2(-direction * 0.25f, -0.6f),
                    110, new Color(255, 120, 90), 1.25f);
                dust.noGravity = true;
            }
        }

        private void SpawnChargeTelegraphDust(float direction, float length) {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 2; i++) {
                float distance = Main.rand.NextFloat(56f, length);
                Vector2 position = NPC.Center + new Vector2(direction * distance, Main.rand.NextFloat(-42f, 42f));
                Dust dust = Dust.NewDustPerfect(position, DustID.Firework_Red, new Vector2(-direction * 0.8f, 0f),
                    120, new Color(255, 95, 80), 1f);
                dust.noGravity = true;
            }
        }

        private void SpawnAfterimageDust() {
            if (Main.dedServ || !Main.rand.NextBool(2))
                return;

            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Firework_Red,
                -NPC.velocity.X * 0.12f, -NPC.velocity.Y * 0.12f, 120, new Color(255, 90, 80), 1.1f);
            dust.noGravity = true;
        }

        private void SpawnSpeakerWarningDust() {
            if (Main.dedServ || !Main.rand.NextBool(2))
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                Dust dust = Dust.NewDustPerfect(speaker.Center + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.GemSapphire, Main.rand.NextVector2Circular(0.4f, 0.4f), 125,
                    new Color(170, 225, 255), 1.05f);
                dust.noGravity = true;
            }
        }

        private void SpawnLanePathWarningDust(Player target) {
            if (Main.dedServ)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                Vector2 direction = new(speaker.Center.X < target.Center.X ? 1f : -1f, 0f);
                SpawnSonicLineDust(speaker.Center, direction, 1180f, 2, new Color(185, 232, 255), 0.92f);
            }
        }

        private void SpawnCrossfireWarningDust(Player target, bool invert = false) {
            if (Main.dedServ)
                return;

            Vector2 center = target.Center;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                Vector2 direction = center - speaker.Center;
                if (invert)
                    direction = new Vector2(direction.X, -direction.Y);

                SpawnSonicLineDust(speaker.Center, direction, 1120f, 2, new Color(180, 228, 255), 0.94f);
            }
        }

        private void SpawnPulseRingWarningDust(float startTime = 0f, float chargeLength = 46f) {
            if (Main.dedServ)
                return;

            float chargeProgress = MathHelper.Clamp((AttackTimer - startTime) / Math.Max(1f, chargeLength), 0f, 1f);
            float radius = MathHelper.Lerp(34f, 112f, chargeProgress);
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                for (int dustIndex = 0; dustIndex < 2; dustIndex++) {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius);
                    Dust dust = Dust.NewDustPerfect(speaker.Center + offset, DustID.GemSapphire,
                        offset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * 0.55f,
                        130, new Color(190, 232, 255), 0.9f);
                    dust.noGravity = true;
                }
            }
        }

        private void SpawnAlternatingLanePositionDust(Player target, bool vertical, int setIndex, bool finalPhase) {
            if (Main.dedServ)
                return;

            Vector2[] anchors = GetAlternatingLaneAnchors(target.Center, vertical, setIndex, finalPhase);
            foreach (Vector2 anchor in anchors) {
                for (int dustIndex = 0; dustIndex < 8; dustIndex++) {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(24f, 24f);
                    Dust dust = Dust.NewDustPerfect(anchor + offset, DustID.GemSapphire,
                        -offset.SafeNormalize(Vector2.UnitY) * 0.75f, 120,
                        new Color(185, 232, 255), 1.06f);
                    dust.noGravity = true;
                }
            }
        }

        private void SpawnAlternatingLaneWarningDust(bool vertical) {
            if (Main.dedServ || !TryGetSpeakerCenter(out Vector2 speakerCenter))
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                if (!TryGetAlternatingLaneData(speaker, speakerCenter, vertical, out Vector2 direction))
                    continue;

                Vector2 speakerAnchor = GetSpeakerAnchor(speaker);
                for (int dustIndex = 0; dustIndex < 2; dustIndex++) {
                    float distance = Main.rand.NextFloat(36f,
                        vertical ? AlternatingVerticalWarningLength : AlternatingHorizontalWarningLength);
                    Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
                    Vector2 position = speakerAnchor + direction * distance +
                                       normal * Main.rand.NextFloatDirection() * (vertical ? 7f : 9f);
                    Dust dust = Dust.NewDustPerfect(position, DustID.GemSapphire, -direction * 0.45f,
                        125, new Color(175, 230, 255), 0.98f);
                    dust.noGravity = true;
                }
            }
        }

        private void SpawnOrbitWarningDust(bool finalPhase) {
            if (Main.dedServ)
                return;

            float warningLength = finalPhase ? 720f : 640f;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile speaker = Main.projectile[i];
                if (!IsAlbedoSpeaker(speaker))
                    continue;

                Vector2 direction = (GetSpeakerAnchor(speaker) - NPC.Center).SafeNormalize(Vector2.UnitY);
                SpawnSonicLineDust(speaker.Center, direction, warningLength, 2, new Color(170, 225, 255), 0.94f);
            }
        }

        private void SpawnSonicLineDust(Vector2 origin, Vector2 direction, float length, int dustCount, Color color,
            float scale) {
            if (Main.dedServ)
                return;

            Vector2 normalized = direction.SafeNormalize(Vector2.UnitX);
            Vector2 normal = normalized.RotatedBy(MathHelper.PiOver2);
            for (int dustIndex = 0; dustIndex < dustCount; dustIndex++) {
                float distance = Main.rand.NextFloat(34f, length);
                Vector2 position = origin + normalized * distance +
                                   normal * Main.rand.NextFloatDirection() * 7f;
                Dust dust = Dust.NewDustPerfect(position, DustID.GemSapphire, -normalized * 0.35f,
                    128, color, scale);
                dust.noGravity = true;
            }
        }

        private void PlayTransformationEffect() {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 24; i++) {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Firework_Red, Scale: 1.5f);
                dust.velocity *= 1.8f;
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(new SoundStyle("Ben10Mod/Content/Sounds/OmnitrixTransformation"), NPC.Center);
        }

        private void ShowActionText(string text) {
            if (Main.netMode == NetmodeID.Server)
                return;

            CombatText.NewText(NPC.Hitbox, new Color(255, 80, 80), text, dramatic: true);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            int npcId = GetCurrentDisplayNpcId();
            Texture2D texture = Main.Assets.Request<Texture2D>($"Images/NPC_{npcId}").Value;
            int frameCount = Main.npcFrameCount[npcId];
            if (frameCount <= 0)
                frameCount = 1;

            Rectangle frame = new(0, 0, texture.Width, texture.Height / frameCount);
            Vector2 drawPosition = NPC.Center - screenPos;
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(texture, drawPosition, frame, drawColor, 0f, frame.Size() * 0.5f, 1f, effects, 0f);
            return false;
        }

        private int GetCurrentDisplayNpcId() {
            AlbedoPhase phase = GetCurrentPhase();
            if (phase == AlbedoPhase.IntroHuman)
                return NPCID.Guide;

            if (phase == AlbedoPhase.UltimateEchoEcho)
                return NPCID.Cyborg;

            if (phase == AlbedoPhase.UltimatrixSwap && GetCurrentSwapForm() == SwapForm.EchoEcho)
                return NPCID.Cyborg;

            return NPCID.Golem;
        }

        private void ApplyCurrentFormDimensions(AlbedoPhase phase) {
            int targetWidth;
            int targetHeight;

            if (phase == AlbedoPhase.IntroHuman) {
                targetWidth = 34;
                targetHeight = 48;
            }
            else if (phase == AlbedoPhase.UltimateEchoEcho ||
                     (phase == AlbedoPhase.UltimatrixSwap && GetCurrentSwapForm() == SwapForm.EchoEcho)) {
                targetWidth = 44;
                targetHeight = 72;
            }
            else {
                targetWidth = 92;
                targetHeight = 110;
            }

            if (NPC.width == targetWidth && NPC.height == targetHeight)
                return;

            Vector2 bottom = NPC.Bottom;
            NPC.width = targetWidth;
            NPC.height = targetHeight;
            NPC.position = bottom - new Vector2(NPC.width * 0.5f, NPC.height);
        }

        private bool TryGetTarget(out Player target) {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            return target.active && !target.dead;
        }

        private void Despawn() {
            ClearAlbedoProjectiles();
            NPC.velocity.Y -= 0.2f;
            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<UltimatrixCore>()));
        }
    }
}
