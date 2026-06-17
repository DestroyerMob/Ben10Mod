using Ben10Mod.Common.Absorption;
using Ben10Mod.Common.Networking;
using Ben10Mod.Common.Omnitrix;
using Ben10Mod.Keybinds;
using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Consumable;
using Terraria.Audio;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Common.CustomVisuals;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Content.Transformations.XLR8;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Events;

namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        public bool ActivateUltimateAbility() {
            var trans = CurrentTransformation;
            if (trans == null) return false;

            if (trans.TryActivateUltimateAbility(Player, this))
                return true;

            bool hasUltimateAbility = trans.HasUltimateAbilityForState(this);
            bool hasUltimateAttack = trans.GetUltimateAttackProjectileType(this) > 0;

            if (hasUltimateAbility) {
                if (Player.HasBuff<UltimateAbilityCooldown>() || Player.HasBuff<UltimateAbility>() || ultimateAttack ||
                    HasLoadedAbilityAttack)
                    return false;

                int ultimateAbilityCost = trans.GetUltimateAbilityCost(this);
                if (omnitrixEnergy >= ultimateAbilityCost) {
                    Player.AddBuff(ModContent.BuffType<UltimateAbility>(), trans.GetUltimateAbilityDuration(this));
                    ultimateAbilityTransformationId = currentTransformationId;
                    omnitrixEnergy -= ultimateAbilityCost;
                    return true;
                }

                return false;
            }

            if (!hasUltimateAttack || Player.HasBuff<UltimateAbility>() || Player.HasBuff<UltimateAbilityCooldown>() ||
                HasLoadedAbilityAttack)
                return false;

            int ultimateAttackCost = trans.GetUltimateAbilityCost(this);
            if (omnitrixEnergy >= ultimateAttackCost && !ultimateAttack) {
                if (HasLoadedAbilityAttack)
                    ClearLoadedAbilityAttack(addCooldownIfUsed: loadedAbilityAttackUsed);

                for (int i = 0; i < 50; i++) {
                    Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Firework_Blue,
                        Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                    d.noGravity = true;
                }

                SetAttackSelection(AttackSelection.Ultimate);
                return true;
            }

            if (ultimateAttack) {
                for (int i = 0; i < 50; i++) {
                    Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Firework_Yellow,
                        Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                    d.noGravity = true;
                }

                ResetAttackToBaseSelection();
                return true;
            }

            return false;
        }

        public bool ActivatePrimaryAbility() {
            var trans = CurrentTransformation;
            if (trans == null) return false;

            if (!trans.HasPrimaryAbilityActionForState(this))
                return false;

            if (trans.TryActivatePrimaryAbility(Player, this))
                return true;

            return ActivateAbilitySlot(
                AttackSelection.PrimaryAbility,
                trans.HasPrimaryAbilityForState(this),
                trans.HasPrimaryAbilityAttackForState(this),
                ModContent.BuffType<PrimaryAbility>(),
                ModContent.BuffType<PrimaryAbilityCooldown>(),
                trans.GetPrimaryAbilityDuration(this),
                trans.GetPrimaryAbilityCooldown(this),
                trans.GetPrimaryAbilityCost(this)
            );
        }

        public bool ActivateSecondaryAbility() {
            var trans = CurrentTransformation;
            if (trans == null) return false;

            if (!trans.HasSecondaryAbilityActionForState(this))
                return false;

            if (trans.TryActivateSecondaryAbility(Player, this))
                return true;

            return ActivateAbilitySlot(
                AttackSelection.SecondaryAbility,
                trans.HasSecondaryAbilityForState(this),
                trans.HasSecondaryAbilityAttackForState(this),
                ModContent.BuffType<SecondaryAbility>(),
                ModContent.BuffType<SecondaryAbilityCooldown>(),
                trans.GetSecondaryAbilityDuration(this),
                trans.GetSecondaryAbilityCooldown(this),
                trans.GetSecondaryAbilityCost(this)
            );
        }

        public bool ActivateTertiaryAbility() {
            var trans = CurrentTransformation;
            if (trans == null) return false;

            if (!trans.HasTertiaryAbilityActionForState(this))
                return false;

            if (trans.TryActivateTertiaryAbility(Player, this))
                return true;

            return ActivateAbilitySlot(
                AttackSelection.TertiaryAbility,
                trans.HasTertiaryAbilityForState(this),
                trans.HasTertiaryAbilityAttackForState(this),
                ModContent.BuffType<TertiaryAbility>(),
                ModContent.BuffType<TertiaryAbilityCooldown>(),
                trans.GetTertiaryAbilityDuration(this),
                trans.GetTertiaryAbilityCooldown(this),
                trans.GetTertiaryAbilityCost(this)
            );
        }

        public string GetCurrentAttackSelectionLabel() {
            return CurrentTransformation?.GetAttackSelectionLabel(setAttack, this) ?? "Primary";
        }

        public string GetCurrentAttackDisplayName() {
            return CurrentTransformation?.GetAttackSelectionDisplayName(setAttack, this) ?? "No Attack";
        }

        internal void NotifyCurrentAttackSpentEnergy(int energyCost, int sustainEnergyCost, int attackLockFrames) {
            if (energyCost <= 0 && sustainEnergyCost <= 0)
                return;

            attackEnergyGainLockTime = Math.Max(attackEnergyGainLockTime, Math.Max(8, attackLockFrames));
            markAttackProjectilesNoEnergyGainTime = Math.Max(markAttackProjectilesNoEnergyGainTime, 3);
            AccumulateOmniCoreReactorCharge(energyCost, sustainEnergyCost);
            TryTriggerChronoAccelerator(energyCost, sustainEnergyCost);
        }

        internal bool ShouldMarkSpawnedAttackProjectilesAsNoEnergyGain() {
            return markAttackProjectilesNoEnergyGainTime > 0;
        }

        public string GetAbilityDisplayName(AttackSelection selection) {
            return CurrentTransformation?.GetAbilitySelectionDisplayName(selection, this) ?? selection switch {
                AttackSelection.PrimaryAbility => "Primary Ability",
                AttackSelection.SecondaryAbility => "Secondary Ability",
                AttackSelection.TertiaryAbility => "Tertiary Ability",
                AttackSelection.Ultimate => "Ultimate Ability",
                _ => "Ability"
            };
        }

        public List<ActiveAbilityStatus> GetActiveAbilityStatuses() {
            List<ActiveAbilityStatus> statuses = new();
            if (!IsTransformed)
                return statuses;

            Transformation transformation = CurrentTransformation;
            if (transformation == null)
                return statuses;

            AppendActiveAbilityStatus(statuses, transformation, AttackSelection.PrimaryAbility,
                transformation.HasPrimaryAbilityForState(this), IsPrimaryAbilityActive);
            AppendActiveAbilityStatus(statuses, transformation, AttackSelection.SecondaryAbility,
                transformation.HasSecondaryAbilityForState(this), IsSecondaryAbilityActive);
            AppendActiveAbilityStatus(statuses, transformation, AttackSelection.TertiaryAbility,
                transformation.HasTertiaryAbilityForState(this), IsTertiaryAbilityActive);
            AppendActiveAbilityStatus(statuses, transformation, AttackSelection.Ultimate,
                transformation.HasUltimateAbilityForState(this), IsUltimateAbilityActive);

            return statuses;
        }

        private void AppendActiveAbilityStatus(List<ActiveAbilityStatus> statuses, Transformation transformation,
            AttackSelection selection, bool slotHasTimedAbility, bool isActive) {
            if (!slotHasTimedAbility || !isActive)
                return;

            statuses.Add(new ActiveAbilityStatus(
                selection,
                transformation.GetAbilitySelectionDisplayName(selection, this),
                GetActiveAbilityRemainingTicks(selection),
                GetAbilityAccentColor(selection)
            ));
        }

        public bool CanAffordCurrentAttackForHud() {
            return CurrentTransformation?.CanAffordCurrentAttack(this) ?? true;
        }

        public string GetCurrentAttackModeSummary() {
            return CurrentTransformation?.GetAttackModeSummary(setAttack, this) ?? string.Empty;
        }

        public string GetCurrentAttackResourceSummary(bool compact = false) {
            return CurrentTransformation?.GetAttackResourceSummary(setAttack, this, compact) ?? string.Empty;
        }

        public int GetBuffRemainingTicks(int buffType) {
            if (buffType <= 0)
                return 0;

            for (int i = 0; i < Player.buffType.Length; i++) {
                if (Player.buffType[i] == buffType)
                    return Math.Max(0, Player.buffTime[i]);
            }

            return 0;
        }

        public int GetTransformationCooldownTicks() {
            return GetBuffRemainingTicks(ModContent.BuffType<TransformationCooldown_Buff>());
        }

        public int GetAttackActionCooldownTicks(AttackSelection selection) {
            return selection switch {
                AttackSelection.PrimaryAbility => GetBuffRemainingTicks(ModContent.BuffType<PrimaryAbilityCooldown>()),
                AttackSelection.SecondaryAbility => GetBuffRemainingTicks(ModContent.BuffType<SecondaryAbilityCooldown>()),
                AttackSelection.TertiaryAbility => GetBuffRemainingTicks(ModContent.BuffType<TertiaryAbilityCooldown>()),
                AttackSelection.Ultimate => GetBuffRemainingTicks(ModContent.BuffType<UltimateAbilityCooldown>()),
                _ => 0
            };
        }

        public int GetActiveAbilityRemainingTicks(AttackSelection selection) {
            return selection switch {
                AttackSelection.PrimaryAbility => GetBuffRemainingTicks(ModContent.BuffType<PrimaryAbility>()),
                AttackSelection.SecondaryAbility => GetBuffRemainingTicks(ModContent.BuffType<SecondaryAbility>()),
                AttackSelection.TertiaryAbility => GetBuffRemainingTicks(ModContent.BuffType<TertiaryAbility>()),
                AttackSelection.Ultimate => GetBuffRemainingTicks(ModContent.BuffType<UltimateAbility>()),
                _ => 0
            };
        }

        public static string FormatCooldownTicks(int ticks) {
            if (ticks <= 0)
                return "Ready";

            return $"{ticks / 60f:0.#}s";
        }

        public string GetTransformationCooldownDisplayText() {
            int ticks = GetTransformationCooldownTicks();
            return ticks > 0 ? $"Cooldown {FormatCooldownTicks(ticks)}" : "Ready";
        }

        public string GetAttackHudCooldownSummary() {
            List<string> parts = new();
            int transformCooldown = GetTransformationCooldownTicks();
            if (transformCooldown > 0)
                parts.Add($"Transform {FormatCooldownTicks(transformCooldown)}");

            Transformation transformation = CurrentTransformation;
            if (transformation == null)
                return parts.Count == 0 ? string.Empty : string.Join("  ", parts);

            if (transformation.HasPrimaryAbilityActionForState(this))
                parts.Add($"P {FormatCooldownTicks(GetAttackActionCooldownTicks(AttackSelection.PrimaryAbility))}");
            if (transformation.HasSecondaryAbilityActionForState(this))
                parts.Add($"S {FormatCooldownTicks(GetAttackActionCooldownTicks(AttackSelection.SecondaryAbility))}");
            if (transformation.HasTertiaryAbilityActionForState(this))
                parts.Add($"T {FormatCooldownTicks(GetAttackActionCooldownTicks(AttackSelection.TertiaryAbility))}");
            if (transformation.HasUltimateAbilityForState(this) || transformation.HasUltimateAttack)
                parts.Add($"U {FormatCooldownTicks(GetAttackActionCooldownTicks(AttackSelection.Ultimate))}");

            return string.Join("  ", parts);
        }

        public string GetSelectionHudCooldownSummary() {
            int transformCooldown = GetTransformationCooldownTicks();
            return transformCooldown > 0 ? $"Transform {FormatCooldownTicks(transformCooldown)}" : string.Empty;
        }

        public void ShowTransformFailureFeedback(string message, Color? color = null) {
            if (string.IsNullOrWhiteSpace(message) || Main.netMode == NetmodeID.Server || Player.whoAmI != Main.myPlayer)
                return;

            ulong now = Main.GameUpdateCount;
            if (string.Equals(message, lastTransformFailureMessage, StringComparison.Ordinal) &&
                now - lastTransformFailureTick < TransformFailureFeedbackCooldownTicks)
                return;

            if (now - lastTransformFailureTick < 10)
                return;

            lastTransformFailureMessage = message;
            lastTransformFailureTick = now;
            Main.NewText(message, color ?? new Color(255, 150, 90));
        }

        public Color GetCurrentAttackAccentColor() {
            return CurrentTransformation?.ResolveAttackSelection(setAttack, this) switch {
                AttackSelection.Secondary => new Color(120, 200, 255),
                AttackSelection.PrimaryAbility => new Color(120, 255, 170),
                AttackSelection.SecondaryAbility => new Color(255, 190, 90),
                AttackSelection.TertiaryAbility => new Color(210, 140, 255),
                AttackSelection.Ultimate => new Color(255, 210, 80),
                _ => new Color(120, 255, 120)
            };
        }

        public Color GetAbilityAccentColor(AttackSelection selection) {
            return selection switch {
                AttackSelection.PrimaryAbility => new Color(120, 255, 170),
                AttackSelection.SecondaryAbility => new Color(255, 190, 90),
                AttackSelection.TertiaryAbility => new Color(210, 140, 255),
                AttackSelection.Ultimate => new Color(255, 210, 80),
                _ => new Color(160, 175, 190)
            };
        }

        private bool ActivateAbilitySlot(AttackSelection slot, bool hasTimedAbility, bool hasAttackMode,
            int activeBuffType, int cooldownBuffType, int duration, int cooldown, int activationCost) {
            if (hasTimedAbility) {
                if (Player.HasBuff(cooldownBuffType) || Player.HasBuff(activeBuffType))
                    return false;

                if (omnitrixEnergy < activationCost)
                    return false;

                if (activationCost > 0)
                    omnitrixEnergy -= activationCost;

                Player.AddBuff(activeBuffType, duration);
                Abilities.SetTransformationId(slot, currentTransformationId);
                return true;
            }

            if (!hasAttackMode)
                return false;

            if (Player.HasBuff(cooldownBuffType) || Player.HasBuff(activeBuffType))
                return false;

            if (setAttack == slot) {
                ClearLoadedAbilityAttack(addCooldownIfUsed: loadedAbilityAttackUsed);
                return true;
            }

            if (omnitrixEnergy < activationCost)
                return false;

            ClearLoadedAbilityAttack(addCooldownIfUsed: loadedAbilityAttackUsed);

            if (activationCost > 0)
                omnitrixEnergy -= activationCost;

            SetAttackSelection(slot);
            loadedAbilityAttackUsed = false;
            return true;
        }

        public void NotifyLoadedAbilityAttackFired() {
            if (!HasLoadedAbilityAttack)
                return;

            loadedAbilityAttackUsed = true;

            var trans = CurrentTransformation;
            bool singleUse = trans != null && setAttack switch {
                AttackSelection.PrimaryAbility => trans.PrimaryAbilityAttackSingleUse,
                AttackSelection.SecondaryAbility => trans.SecondaryAbilityAttackSingleUse,
                AttackSelection.TertiaryAbility => trans.TertiaryAbilityAttackSingleUse,
                _ => false
            };
            bool channelled = trans != null && setAttack switch {
                AttackSelection.PrimaryAbility => trans.PrimaryAbilityAttackChannel,
                AttackSelection.SecondaryAbility => trans.SecondaryAbilityAttackChannel,
                AttackSelection.TertiaryAbility => trans.TertiaryAbilityAttackChannel,
                _ => false
            };

            if (singleUse || !channelled)
                ClearLoadedAbilityAttack(addCooldownIfUsed: true);
        }

        public void ClearLoadedAbilityAttack(bool addCooldownIfUsed = false) {
            if (!HasLoadedAbilityAttack) {
                loadedAbilityAttackUsed = false;
                return;
            }

            if (addCooldownIfUsed && loadedAbilityAttackUsed)
                ApplyLoadedAbilityAttackCooldown();

            ResetAttackToBaseSelection();
            loadedAbilityAttackUsed = false;
        }

        private void ApplyLoadedAbilityAttackCooldown() {
            var trans = CurrentTransformation;
            if (trans == null)
                return;

            switch (setAttack) {
                case AttackSelection.PrimaryAbility:
                    if (trans.GetPrimaryAbilityCooldown(this) > 0)
                        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(),
                            trans.GetPrimaryAbilityCooldown(this));
                    break;
                case AttackSelection.SecondaryAbility:
                    if (trans.GetSecondaryAbilityCooldown(this) > 0)
                        Player.AddBuff(ModContent.BuffType<SecondaryAbilityCooldown>(),
                            trans.GetSecondaryAbilityCooldown(this));
                    break;
                case AttackSelection.TertiaryAbility:
                    if (trans.GetTertiaryAbilityCooldown(this) > 0)
                        Player.AddBuff(ModContent.BuffType<TertiaryAbilityCooldown>(),
                            trans.GetTertiaryAbilityCooldown(this));
                    break;
            }
        }

        private bool CanSelectAbilityAttackSlot(AttackSelection slot) {
            var trans = CurrentTransformation;
            if (trans == null)
                return false;

            return slot switch {
                AttackSelection.PrimaryAbility => trans.HasPrimaryAbilityAttackForState(this) &&
                                                  !Player.HasBuff<PrimaryAbility>() &&
                                                  !Player.HasBuff<PrimaryAbilityCooldown>(),
                AttackSelection.SecondaryAbility => trans.HasSecondaryAbilityAttackForState(this) &&
                                                    !Player.HasBuff<SecondaryAbility>() &&
                                                    !Player.HasBuff<SecondaryAbilityCooldown>(),
                AttackSelection.TertiaryAbility => trans.HasTertiaryAbilityAttackForState(this) &&
                                                   !Player.HasBuff<TertiaryAbility>() &&
                                                   !Player.HasBuff<TertiaryAbilityCooldown>(),
                _ => false
            };
        }

        private void HandleBadgeRightClickSelection() {
            if (HasLoadedAbilityAttack) {
                AttackSelection targetSelection = AttackSelection.Primary;

                if (setAttack == targetSelection)
                    return;

                ClearLoadedAbilityAttack(addCooldownIfUsed: loadedAbilityAttackUsed);
                SetAttackSelection(targetSelection);
                return;
            }

            if (setAttack is not AttackSelection.Primary and not AttackSelection.Secondary)
                return;

            baseAttackSelection = baseAttackSelection == AttackSelection.Primary
                ? AttackSelection.Secondary
                : AttackSelection.Primary;
            SetAttackSelection(baseAttackSelection);
        }

        private bool CanHandleBadgeRightClickSelection() {
            if (Player.whoAmI != Main.myPlayer ||
                !Main.mouseRight ||
                !Main.mouseRightRelease ||
                Player.HeldItem.ModItem is not PlumbersBadge)
                return false;

            // Let vanilla inventory and UI right-click interactions win while a badge is held.
            if (Main.playerInventory || Player.mouseInterface || Main.LocalPlayer.mouseInterface)
                return false;

            return true;
        }

        private void SetAttackSelection(AttackSelection selection) {
            bool changed = setAttack != selection;
            setAttack = selection;
            if (selection is AttackSelection.Primary or AttackSelection.Secondary)
                baseAttackSelection = selection;

            if (changed)
                TriggerAttackSelectionPulse();
        }

        public void ResetAttackToBaseSelection() {
            SetAttackSelection(baseAttackSelection);
        }

        private void NormalizeAttackSelectionForCurrentTransformation(Transformation trans) {
            if (trans == null) {
                loadedAbilityAttackUsed = false;
                if (setAttack is not AttackSelection.Primary and not AttackSelection.Secondary)
                    ResetAttackToBaseSelection();
                return;
            }

            AttackSelection resolvedSelection = trans.ResolveAttackSelection(setAttack, this);
            bool invalidUltimateSelection = setAttack == AttackSelection.Ultimate && resolvedSelection != AttackSelection.Ultimate;
            bool invalidLoadedSelection = IsAbilityAttackSelection(setAttack) && !IsAbilityAttackSelection(resolvedSelection);

            if (!invalidUltimateSelection && !invalidLoadedSelection)
                return;

            loadedAbilityAttackUsed = false;
            ResetAttackToBaseSelection();
        }

        private void UpdateUltimateReadyCueState() {
            string cueTransformationId = isTransformed ? currentTransformationId : string.Empty;
            bool ultimateReadyNow = IsUltimateReadyForCue();

            if (!string.Equals(lastUltimateReadyCueTransformationId, cueTransformationId, StringComparison.OrdinalIgnoreCase)) {
                lastUltimateReadyCueTransformationId = cueTransformationId;
                wasUltimateReady = ultimateReadyNow;
                return;
            }

            if (Player.whoAmI == Main.myPlayer && !wasUltimateReady && ultimateReadyNow)
                TriggerUltimateReadyCue();

            wasUltimateReady = ultimateReadyNow;
        }

        private bool IsUltimateReadyForCue() {
            Transformation transformation = CurrentTransformation;
            if (!isTransformed || transformation == null)
                return false;

            bool hasUltimateAction = transformation.HasUltimateAbilityForState(this) ||
                                     transformation.GetUltimateAttackProjectileType(this) > 0;
            if (!hasUltimateAction)
                return false;

            return !ultimateAttack && !Player.HasBuff<UltimateAbility>() && !Player.HasBuff<UltimateAbilityCooldown>();
        }

        private void TriggerUltimateReadyCue() {
            ultimateReadyCueTime = UltimateReadyCueDuration;
            if (Main.dedServ || Player.whoAmI != Main.myPlayer)
                return;

            SoundEngine.PlaySound(SoundID.MaxMana, Player.Center);
        }

        private void RefreshRemoteHeldBadgeStats() {
            if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI == Main.myPlayer)
                return;

            if (Player.HeldItem?.ModItem is PlumbersBadge badge)
                badge.RefreshHeldStats(Player, this);
        }

        private static bool IsAbilityAttackSelection(AttackSelection selection) {
            return AttackSelectionController.IsAbilityAttackSelection(selection);
        }

        private void TriggerAttackSelectionPulse() {
            attackSelectionPulseTime = AttackSelectionPulseDuration;
        }

        public override bool CanUseItem(Item item) {
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI != Main.myPlayer)
                return true;

            var trans = CurrentTransformation;
            return trans?.CanUseItem(Player, this, item) ?? true;
        }

        private void HandleAbilityCooldownExpiration(bool wasEnabled, bool isEnabled, string transformationId,
            int cooldownBuffType, Func<Transformation, OmnitrixPlayer, int> getCooldown) {
            if (!wasEnabled || isEnabled || Player.HasBuff(cooldownBuffType))
                return;

            var abilityTransformation = TransformationLoader.Get(transformationId);
            int cooldown = abilityTransformation == null ? 0 : getCooldown(abilityTransformation, this);
            if (cooldown > 0)
                Player.AddBuff(cooldownBuffType, cooldown);
        }
    }
}
