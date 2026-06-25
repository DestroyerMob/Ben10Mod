using Ben10Mod.Common.Absorption;
using Ben10Mod.Keybinds;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Common.CustomVisuals;

namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        public override void Initialize() {
            ResetAirborneLungeState();
            NormalizeStoredTransformationData();
            transformationSpeedBoostPercent = DefaultTransformationSpeedBoostPercent;
        }

        public override void ResetEffects() {
            var trans = CurrentTransformation;

            advancedCircuitMatrix = false;
            snowflake = false;
            transformationFailsafeEquipped = false;
            completedOmnitrixEquipped = false;
            chronoAcceleratorEquipped = false;
            heroConvergenceEmblemEquipped = false;
            omniCoreReactorEquipped = false;
            xlr8DashAccessoryEquipped = false;
            if (currentTransformationId != "Ben10Mod:Goop") {
                GoopVisualScale = Vector2.One;
                goopWasGrounded = false;
                goopPreviousVerticalVelocity = 0f;
                goopLandingSquish = 0f;
                goopLandingSplashTime = 0;
            }

            Energy.ResetEffectiveStats();
            transformationDurationMultiplier = 1f;
            cooldownDurationMultiplier = 1f;
            primaryAbilityCooldownMultiplier = 1f;
            secondaryAbilityCooldownMultiplier = 1f;
            tertiaryAbilityCooldownMultiplier = 1f;
            ultimateAbilityCooldownMultiplier = 1f;
            heroCritChanceBonus = 0;
            heroArmorPenBonus = 0;
            heroAttackSpeedBonus = 0f;
            heroKnockbackBonus = 0f;
            transformedDefenseBonus = 0;
            transformedEnduranceBonus = 0f;
            transformedMoveSpeedBonus = 0f;
            transformedRunAccelerationBonus = 0f;
            transformedJumpSpeedBonus = 0f;

            isTransformed = false;
            onCooldown = false;
            Absorption.ResetAccessoryEffects();
            omnitrixEquipped = false;
            equippedOmnitrix = null;
            equippedOmnitrixItem = null;
            activeOmnitrixVisualsHidden = false;
            activeTransformationHeadSlot = -1;
            activeTransformationBodySlot = -1;
            activeTransformationLegsSlot = -1;

            omnitrixUpdating = false;

            Abilities.ResetActiveFlags();

            if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15)
                DashDir = DashDown;
            else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15)
                DashDir = DashUp;
            else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15)
                DashDir = DashRight;
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15)
                DashDir = DashLeft;
            else
                DashDir = -1;

            trans?.ResetEffects(Player, this);
            UpdateTransformationScale(trans == null);
        }

        public override void PostUpdateEquips() {
            Player.GetCritChance<HeroDamage>() += heroCritChanceBonus;
            Player.GetArmorPenetration<HeroDamage>() += heroArmorPenBonus;
            Player.GetAttackSpeed<HeroDamage>() += heroAttackSpeedBonus;
            Player.GetKnockback<HeroDamage>() += heroKnockbackBonus;

            if (IsTransformed) {
                Player.statDefense += transformedDefenseBonus;
                Player.endurance += transformedEnduranceBonus;
                Player.moveSpeed += transformedMoveSpeedBonus;
                Player.maxRunSpeed += transformedMoveSpeedBonus * 1.8f;
                Player.accRunSpeed += transformedMoveSpeedBonus * 2f;
                Player.runAcceleration += transformedRunAccelerationBonus;
                Player.jumpSpeedBoost += transformedJumpSpeedBonus;
            }

            if (CompletedOmnitrixSyncActive && IsTransformed) {
                Player.GetDamage<HeroDamage>() += 0.08f;
                Player.GetAttackSpeed<HeroDamage>() += 0.06f;
                Player.GetArmorPenetration<HeroDamage>() += 8;
                Player.moveSpeed += 0.08f;
                Player.maxRunSpeed += 0.5f;
            }

            if (TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile absorptionProfile)) {
                Player.GetDamage(DamageClass.Generic) += absorptionProfile.GenericDamageBonus * absorptionStrengthMultiplier;
                Player.statDefense += (int)Math.Round(absorptionProfile.DefenseBonus * absorptionStrengthMultiplier);
                Player.endurance += absorptionProfile.EnduranceBonus * absorptionStrengthMultiplier;
                Player.GetKnockback(DamageClass.Melee) += absorptionProfile.MeleeKnockbackBonus * absorptionStrengthMultiplier;
                Player.GetCritChance(DamageClass.Generic) += absorptionCritChanceBonus;
                Player.GetArmorPenetration(DamageClass.Generic) += absorptionArmorPenBonus;
                Player.GetAttackSpeed(DamageClass.Melee) += absorptionMeleeSpeedBonus;
                Player.GetKnockback(DamageClass.Melee) += absorptionMeleeKnockbackBonus;
                Player.moveSpeed += absorptionMoveSpeedBonus;
                Player.maxRunSpeed += absorptionMoveSpeedBonus * 2.2f;
                Player.accRunSpeed += absorptionMoveSpeedBonus * 2.5f;
                Player.lifeRegen += absorptionLifeRegenBonus;
                Player.statLifeMax2 += absorptionMaxLifeBonus;
                Player.statDefense += absorptionFlatDefenseBonus;
            }
        }

        public override void PostUpdateBuffs() {
            var trans = CurrentTransformation;

            bool transformationBuffMissing = !string.IsNullOrEmpty(currentTransformationId) &&
                                             trans?.TransformationBuffId > 0 &&
                                             !Player.HasBuff(trans.TransformationBuffId);
            if ((wasTransformed && !isTransformed) || transformationBuffMissing) {
                HandleAutomaticForcedDetransform();
                trans = CurrentTransformation;
            }
            wasTransformed = isTransformed;

            // Play the update effect once when the Omnitrix enters its updating state, and complete
            // the item replacement when the updating buff falls off.
            if (omnitrixUpdating != omnitrixWasUpdating) {
                Item activeOmnitrixItem = GetActiveOmnitrixItem();
                var activeOmnitrix = activeOmnitrixItem?.ModItem as Omnitrix;
                if (omnitrixUpdating) {
                    Random random = new Random();
                    for (int i = 0; i < 25; i++) {
                        int dustNum = Dust.NewDust(Player.position - new Vector2(1, 1), Player.width + 1,
                            Player.height + 1, DustID.BlueTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White,
                            4);
                        Main.dust[dustNum].noGravity = true;
                    }
                }
                else if (omnitrixWasUpdating && activeOmnitrix != null && activeOmnitrixItem != null) {
                    activeOmnitrix.CompleteEvolution(Player, this, activeOmnitrixItem);
                }

                omnitrixWasUpdating = omnitrixUpdating;
            }

            if (trans != null)
            {
                float baseMoveSpeed = Player.moveSpeed;
                float baseMaxRunSpeed = Player.maxRunSpeed;
                float baseAccRunSpeed = Player.accRunSpeed;
                float baseRunAcceleration = Player.runAcceleration;

                trans.UpdateEffects(Player, this);
                trans.PostUpdateBuffs(Player, this);
                ApplyTransformationMovementBoostScale(baseMoveSpeed, baseMaxRunSpeed, baseAccRunSpeed, baseRunAcceleration);
            }
            trans?.UpdateActiveAbilityVisuals(Player, this);
        }

        private void HandleAutomaticForcedDetransform() {
            if (skipAutomaticForcedDetransformHandling) {
                skipAutomaticForcedDetransformHandling = false;
                return;
            }

            var activeOmnitrix = GetActiveOmnitrix();
            if (HasMasterControlAccess) {
                TransformationHandler.Detransform(Player, 0, true, false);
            }
            else if (activeOmnitrix != null) {
                activeOmnitrix.HandleForcedDetransform(Player, this);
            }
            else if (!string.IsNullOrEmpty(currentTransformationId)) {
                TransformationHandler.Detransform(Player, cooldownTime, showParticles: true, addCooldown: true);
            }

            skipAutomaticForcedDetransformHandling = false;
        }

        public override void PostUpdate() {
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();
            int materialBuffType = ModContent.BuffType<MaterialAbsorptionBuff>();

            if (activeLungeTime > 0)
                activeLungeTime--;

            if (airborneLungeConsumed && activeLungeTime <= 0 && IsTouchingLungeResetSurface())
                airborneLungeConsumed = false;

            if (completedOmnitrixRevivalCooldown > 0)
                completedOmnitrixRevivalCooldown--;

            int previousXlr8DashAccessoryVisualTime = xlr8DashAccessoryVisualTime;
            if (xlr8DashAccessoryVisualTime > 0)
                xlr8DashAccessoryVisualTime--;

            if (previousXlr8DashAccessoryVisualTime > 0 && xlr8DashAccessoryVisualTime == 0)
                PlayXlr8DashAccessoryEndEffects();

            if (!isTransformed) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
                pendingEvolutionStepDownTime = 0;
                pendingEvolutionStepDownTransformationId = "";
            }

            Energy.RegeneratePerTick();
            Input.HandleUiInput(this);

            if (CanHandleBadgeRightClickSelection()) {
                Main.mouseRightRelease = false;
                HandleBadgeRightClickSelection();
            }

            if (Player.whoAmI == Main.myPlayer && KeybindSystem.AbsorbMaterial.JustPressed)
                TryAbsorbHeldMaterial();

            if (Player.whoAmI == Main.myPlayer && KeybindSystem.CycleTransformationSpeedBoost?.JustPressed == true)
                CycleTransformationSpeedBoostPercent();

            var trans = CurrentTransformation;
            NormalizeAttackSelectionForCurrentTransformation(trans);
            RefreshRemoteHeldBadgeStats();
            if (trans != null)
                trans.PostUpdate(Player, this);

            UpdateAccessoryProcStates();

            if (!completedOmnitrixEquipped || !IsTransformed)
                completedOmnitrixSyncTime = 0;
            else if (completedOmnitrixSyncTime > 0)
                completedOmnitrixSyncTime--;

            AttackSelectionState.Tick();
            if (ultimateReadyCueTime > 0)
                ultimateReadyCueTime--;

            string currentAttackTransformationId = isTransformed ? currentTransformationId : "";
            if (lastAttackUiTransformationId != currentAttackTransformationId) {
                lastAttackUiTransformationId = currentAttackTransformationId;
                if (isTransformed)
                    TriggerAttackSelectionPulse();
            }

            UpdateUltimateReadyCueState();

            bool authoritativeBuffTracking = Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI == Main.myPlayer;
            bool absorptionBlocked = !osmosianEquipped || omnitrixEquipped;
            if (absorptionBlocked && absorbedMaterialTime > 0 && authoritativeBuffTracking)
                ClearAbsorbedMaterial(showEffects: false);

            if (absorbedMaterialTime > 0) {
                if (authoritativeBuffTracking && !Player.HasBuff(materialBuffType))
                    ClearAbsorbedMaterial(showEffects: false);
                else if (authoritativeBuffTracking)
                    Player.AddBuff(materialBuffType, absorbedMaterialTime);
            }
            else if (absorbedMaterialItemType != 0) {
                absorbedMaterialItemType = 0;
                absorbedMaterialTime = 0;
            }

            if (pendingEvolutionStepDownTime > 0) {
                bool sameTransformation = currentTransformationId == pendingEvolutionStepDownTransformationId;
                if (!isTransformed || !sameTransformation) {
                    pendingEvolutionStepDownTime = 0;
                    pendingEvolutionStepDownTransformationId = "";
                }
                else if (--pendingEvolutionStepDownTime <= 0) {
                    pendingEvolutionStepDownTime = 0;
                    pendingEvolutionStepDownTransformationId = "";
                    trans?.CompleteEvolutionStepDown(Player, this);
                }
            }

            if (inPossessionMode) {
                if (possessedTargetIndex < 0 || possessedTargetIndex >= Main.maxNPCs) {
                    EndPossession();
                    return;
                }

                NPC npc = Main.npc[possessedTargetIndex];
                if (npc == null || !npc.active || npc.whoAmI != possessedTargetIndex) {
                    EndPossession();
                    return;
                }

                Player.immuneNoBlink = true;
                Player.immuneTime = 999;
                Player.Center = npc.Center;
                Player.velocity = npc.velocity * 0.8f;

                Player.controlJump = false;
                Player.controlDown = false;
                Player.controlLeft = false;
                Player.controlRight = false;
                Player.controlUp = false;
                Player.controlUseItem = false;
                Player.controlUseTile = false;
                Player.controlHook = false;

                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    possessionTimer--;
                    if (possessionTimer <= 0) {
                        int finalStrikeDamage = Math.Max(1, Player.HeldItem.damage * 2);
                        npc.SimpleStrikeNPC(finalStrikeDamage, Player.direction, false, 0,
                            ModContent.GetInstance<HeroDamage>());
                        EndPossession();
                    }
                }
            }

            // Drop out of ultimate attack mode immediately when the Omnitrix can no longer sustain it.
            if (isTransformed && ultimateAttack &&
                !CanSpendOmnitrixEnergy(CurrentTransformation?.GetUltimateAbilityCost(this) ?? 50)) {
                for (int i = 0; i < 50; i++) {
                    Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Firework_Yellow,
                        Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                    d.noGravity = true;
                }

                ResetAttackToBaseSelection();
            }

            if (isTransformed) {
                if (KeybindSystem.PrimaryAbility.JustPressed &&
                    CurrentTransformation?.HasPrimaryAbilityActionForState(this) == true)
                    ActivatePrimaryAbility();

                if (KeybindSystem.SecondaryAbility.JustPressed &&
                    CurrentTransformation?.HasSecondaryAbilityActionForState(this) == true)
                    ActivateSecondaryAbility();

                if (KeybindSystem.TertiaryAbility.JustPressed &&
                    CurrentTransformation?.HasTertiaryAbilityActionForState(this) == true)
                    ActivateTertiaryAbility();

                if (KeybindSystem.UltimateAbility.JustPressed && CurrentTransformation != null)
                    ActivateUltimateAbility();
            }

            HandleAbilityCooldownExpiration(PrimaryAbilityWasEnabled, PrimaryAbilityEnabled,
                primaryAbilityTransformationId, ModContent.BuffType<PrimaryAbilityCooldown>(),
                static (transformation, omp) => transformation.GetPrimaryAbilityCooldown(omp));
            PrimaryAbilityWasEnabled = PrimaryAbilityEnabled;

            HandleAbilityCooldownExpiration(SecondaryAbilityWasEnabled, SecondaryAbilityEnabled,
                secondaryAbilityTransformationId, ModContent.BuffType<SecondaryAbilityCooldown>(),
                static (transformation, omp) => transformation.GetSecondaryAbilityCooldown(omp));
            SecondaryAbilityWasEnabled = SecondaryAbilityEnabled;

            HandleAbilityCooldownExpiration(TertiaryAbilityWasEnabled, TertiaryAbilityEnabled,
                tertiaryAbilityTransformationId, ModContent.BuffType<TertiaryAbilityCooldown>(),
                static (transformation, omp) => transformation.GetTertiaryAbilityCooldown(omp));
            TertiaryAbilityWasEnabled = TertiaryAbilityEnabled;

            HandleAbilityCooldownExpiration(UltimateAbilityWasEnabled, UltimateAbilityEnabled,
                ultimateAbilityTransformationId, ModContent.BuffType<UltimateAbilityCooldown>(),
                static (transformation, omp) => transformation.GetUltimateAbilityCooldown(omp));
            UltimateAbilityWasEnabled = UltimateAbilityEnabled;

            UpdateProgressionTransformationUnlocks();
            UpdateEventTransformationUnlocks();
        }

        public override void UpdateDead() {
            ResetAirborneLungeState();
        }

        public override void PreUpdate() {
            var trans = CurrentTransformation;
            trans?.PreUpdate(Player, this);

            if (inPossessionMode) {
                if (possessedTargetIndex < 0 || possessedTargetIndex >= Main.maxNPCs) {
                    EndPossession();
                    return;
                }

                NPC npc = Main.npc[possessedTargetIndex];
                if (npc == null || !npc.active || npc.whoAmI != possessedTargetIndex || npc.life <= 0) {
                    EndPossession();
                    return;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient && Main.GameUpdateCount % 60 == 0 && npc.active &&
                    npc.life > 1) {
                    int dotDamage = Math.Min(35, npc.life - 1);
                    if (dotDamage > 0) {
                        npc.SimpleStrikeNPC(dotDamage, 0, false, 0f, ModContent.GetInstance<HeroDamage>());
                        npc.netUpdate = true;
                    }
                }
            }

            if (IsPrimaryAbilityActive && trans != null &&
                (trans.TransformationName == "Ghostfreak" || trans.TransformationName == "Bigchill")) {
                Player.gravity = 0f;
                Player.noKnockback = true;
                Player.noFallDmg = true;
                Player.fallStart = (int)(Player.position.Y / 16f);
            }

            if (TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile absorptionProfile))
                Lighting.AddLight(Player.Center, Color.Lerp(absorptionProfile.TintColor, Color.White, 0.15f).ToVector3() * 0.85f);

            ScreenShaderController.UpdateForLocalPlayer(Player);
        }
    }
}
