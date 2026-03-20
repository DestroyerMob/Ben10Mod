using Ben10Mod.Common.Absorption;
using Ben10Mod.Keybinds;
using System;
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
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Accessories;
using Terraria.Audio;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Common.CustomVisuals;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Events;

namespace Ben10Mod {
    public class OmnitrixPlayer : ModPlayer {
        public enum AttackSelection {
            Primary,
            Secondary,
            PrimaryAbility,
            SecondaryAbility,
            TertiaryAbility,
            Ultimate
        }

        public bool masterControl = false;

        public bool omnitrixEquipped = false;
        public bool isTransformed = false;
        public bool wasTransformed = false;
        public bool onCooldown = false;
        public bool osmosianEquipped = false;
        public float absorptionDurationMultiplier = 1f;
        public float absorptionStrengthMultiplier = 1f;
        public float absorptionCostMultiplier = 1f;
        public float absorptionDebuffDurationMultiplier = 1f;
        public int absorptionCritChanceBonus = 0;
        public int absorptionArmorPenBonus = 0;
        public float absorptionMeleeSpeedBonus = 0f;
        public float absorptionMeleeKnockbackBonus = 0f;
        public float absorptionMoveSpeedBonus = 0f;
        public int absorptionLifeRegenBonus = 0;
        public int absorptionMaxLifeBonus = 0;
        public int absorptionFlatDefenseBonus = 0;
        public AttackSelection setAttack = AttackSelection.Primary;
        private AttackSelection baseAttackSelection = AttackSelection.Primary;
        public bool loadedAbilityAttackUsed = false;
        public int transformationAttackSerial = 0;
        public int transformationAttackDamage = 0;
        public int ultimateEchoEchoSpeakerSpawnSerial = 0;
        public int absorbedMaterialItemType = 0;
        public int absorbedMaterialTime = 0;

        public int cooldownTime = 120;
        public int transformationTime = 300;
        public float CurrentTransformationScale = 1f;
        public Vector2 CurrentTransformationHitboxScale = Vector2.One;

        public bool PrimaryAbilityEnabled = false;
        public bool PrimaryAbilityWasEnabled = false;
        public bool SecondaryAbilityEnabled = false;
        public bool SecondaryAbilityWasEnabled = false;
        public bool TertiaryAbilityEnabled = false;
        public bool TertiaryAbilityWasEnabled = false;
        public bool UltimateAbilityEnabled = false;
        public bool UltimateAbilityWasEnabled = false;
        public string primaryAbilityTransformationId = "";
        public string secondaryAbilityTransformationId = "";
        public string tertiaryAbilityTransformationId = "";
        public string ultimateAbilityTransformationId = "";

        public int DashDir = -1;
        public const int DashDown = 0;
        public const int DashUp = 1;
        public const int DashRight = 2;
        public const int DashLeft = 3;
        public int DashVelocity = 15;
        public int DashDelay = 0;
        public int DashTimer = 0;
        public const int DashCooldown = 15;
        public const int DashDuration = 15;

        public string[] transformationSlots = { "Ben10Mod:HeatBlast", "", "", "", "" };
        public string currentTransformationId = "";
        public List<string> unlockedTransformations = new() { "Ben10Mod:HeatBlast" };

        public bool showingUI = false;

        public bool omnitrixUpdating = false;
        public bool omnitrixWasUpdating = false;
        public float omnitrixEnergy = 0f;
        public float omnitrixEnergyMax = 0f;
        public float omnitrixEnergyRegen = 0f;
        public float transformationDurationMultiplier = 1f;
        public float cooldownDurationMultiplier = 1f;
        public float activeTransformationDurationMultiplier = 1f;
        public float activeCooldownDurationMultiplier = 1f;
        public float primaryAbilityCooldownMultiplier = 1f;
        public float secondaryAbilityCooldownMultiplier = 1f;
        public float tertiaryAbilityCooldownMultiplier = 1f;
        public float ultimateAbilityCooldownMultiplier = 1f;
        public int heroCritChanceBonus = 0;
        public int heroArmorPenBonus = 0;
        public float heroAttackSpeedBonus = 0f;
        public float heroKnockbackBonus = 0f;
        public int transformedDefenseBonus = 0;
        public float transformedEnduranceBonus = 0f;
        public float transformedMoveSpeedBonus = 0f;
        public float transformedRunAccelerationBonus = 0f;
        public float transformedJumpSpeedBonus = 0f;
        public int omnitrixEnergyMaxBonus = 0;
        public int omnitrixEnergyRegenBonus = 0;
        public int pendingEvolutionStepDownTime = 0;
        public string pendingEvolutionStepDownTransformationId = "";
        public Omnitrix equippedOmnitrix = null;

        public bool inPossessionMode = false;
        public Vector2 prePossessionPosition = Vector2.Zero;
        public int possessedTargetIndex = -1;
        public int possessionTimer = 0;
        private const int PossessionDuration = 360;

        public bool snowflake = false;
        public bool advancedCircuitMatrix = false;
        public bool advancedCircuitMatrixEquippedWhileTransformed = false;

        private readonly HashSet<int> participatedEvents = new();
        private readonly HashSet<int> activeEvents = new();
        private const int BaseTransformationWidth = 20;
        private const int BaseTransformationHeight = 42;
        private float requestedTransformationScale = 1f;
        private int requestedTransformationScaleTime = 1;
        private Vector2 requestedTransformationHitboxScale = Vector2.One;
        private float lastExpandedTransformationScale = 1f;
        private Vector2 lastExpandedTransformationHitboxScale = Vector2.One;

        private const int EventBloodMoon = -1;
        private const int EventSolarEclipse = -2;
        private const int EventSlimeRain = -3;
        private const int EventPumpkinMoon = -4;
        private const int EventFrostMoon = -5;
        private const int EventOldOnesArmy = -6;

        public Transformation CurrentTransformation
            => TransformationLoader.Get(currentTransformationId);

        public bool IsTransformed => !string.IsNullOrEmpty(currentTransformationId);
        public bool IsPrimaryAbilityActive => PrimaryAbilityEnabled || Player.HasBuff<PrimaryAbility>();
        public bool IsSecondaryAbilityActive => SecondaryAbilityEnabled || Player.HasBuff<SecondaryAbility>();
        public bool IsTertiaryAbilityActive => TertiaryAbilityEnabled || Player.HasBuff<TertiaryAbility>();
        public bool IsUltimateAbilityActive => UltimateAbilityEnabled || Player.HasBuff<UltimateAbility>();
        public bool altAttack => setAttack == AttackSelection.Secondary;
        public bool ultimateAttack => setAttack == AttackSelection.Ultimate;
        public bool IsPrimaryAbilityAttackLoaded => setAttack == AttackSelection.PrimaryAbility;
        public bool IsSecondaryAbilityAttackLoaded => setAttack == AttackSelection.SecondaryAbility;
        public bool IsTertiaryAbilityAttackLoaded => setAttack == AttackSelection.TertiaryAbility;
        public bool HasLoadedAbilityAttack => IsAbilityAttackSelection(setAttack);
        public bool HasLoadedBadgeAttack => setAttack is not AttackSelection.Primary and not AttackSelection.Secondary;

        public Omnitrix GetActiveOmnitrix() {
            if (equippedOmnitrix != null)
                return equippedOmnitrix;

            var omnitrixSlot = ModContent.GetInstance<OmnitrixSlot>();
            return omnitrixSlot?.FunctionalItem?.ModItem as Omnitrix;
        }

        public bool HasEquippedOsmosianHarness() {
            for (int i = 0; i < Player.armor.Length; i++) {
                if (Player.armor[i]?.ModItem is OsmosianHarness)
                    return true;
            }

            return false;
        }

        public bool HasAnyEquippedOmnitrix() {
            if (GetActiveOmnitrix() != null)
                return true;

            for (int i = 0; i < Player.armor.Length; i++) {
                if (Player.armor[i]?.ModItem is Omnitrix)
                    return true;
            }

            return false;
        }

        public override void SaveData(TagCompound tag) {
            tag["masterControl"] = masterControl;
            tag["currentTransformationId"] = currentTransformationId;
            tag["omnitrixEnergy"] = omnitrixEnergy;
            tag["absorbedMaterialItemType"] = absorbedMaterialItemType;
            tag["absorbedMaterialTime"] = absorbedMaterialTime;

            tag["transformationRoster"] = transformationSlots;
            tag["unlockedTransformationRoster"] = unlockedTransformations.ToArray();
        }

        public override void LoadData(TagCompound tag) {
            tag.TryGet("masterControl", out masterControl);
            omnitrixEnergy = tag.TryGet("omnitrixEnergy", out omnitrixEnergy) ? omnitrixEnergy : 0f;
            absorbedMaterialItemType = tag.TryGet("absorbedMaterialItemType", out absorbedMaterialItemType) ? absorbedMaterialItemType : 0;
            absorbedMaterialTime = tag.TryGet("absorbedMaterialTime", out absorbedMaterialTime) ? absorbedMaterialTime : 0;

            tag.TryGet("currentTransformationId", out currentTransformationId);
            int[] oldUnlockedRoster;
            int[] oldTransformationRoster;
            int oldCurrentTransformation;

            if (tag.TryGet("transformationRoster", out string[] rosterArray))
                transformationSlots = rosterArray;
            else if (tag.TryGet("transformationRoster", out oldTransformationRoster)) {
                transformationSlots = new string[oldTransformationRoster.Length];
                for (int i = 0; i < oldTransformationRoster.Length; i++)
                    transformationSlots[i] = MapOldTransformationId((TransformationEnumOld)oldTransformationRoster[i]);
            }
            else if (tag.TryGet("roster", out oldTransformationRoster)) {
                transformationSlots = new string[oldTransformationRoster.Length];
                for (int i = 0; i < oldTransformationRoster.Length; i++)
                    transformationSlots[i] = MapOldTransformationId((TransformationEnumOld)oldTransformationRoster[i]);
            }

            if (string.IsNullOrEmpty(currentTransformationId) &&
                tag.TryGet("currentTransformation", out oldCurrentTransformation)) {
                currentTransformationId = MapOldTransformationId((TransformationEnumOld)oldCurrentTransformation);
            }
            else if (string.IsNullOrEmpty(currentTransformationId) &&
                     tag.TryGet("currTransformation", out oldCurrentTransformation)) {
                currentTransformationId = MapOldTransformationId((TransformationEnumOld)oldCurrentTransformation);
            }

            unlockedTransformations.Clear();
            if (tag.TryGet("unlockedTransformationRoster", out string[] unlockedArray))
                unlockedTransformations.AddRange(unlockedArray);

            if (tag.TryGet("unlockedRoster", out oldUnlockedRoster)) {
                for (int i = 0; i < oldUnlockedRoster.Length; i++) {
                    string migratedId = MapOldTransformationId((TransformationEnumOld)oldUnlockedRoster[i]);
                    if (!string.IsNullOrEmpty(migratedId) && !unlockedTransformations.Contains(migratedId))
                        unlockedTransformations.Insert(Math.Min(i, unlockedTransformations.Count), migratedId);
                }
            }

            if (!unlockedTransformations.Contains("Ben10Mod:HeatBlast"))
                unlockedTransformations.Insert(0, "Ben10Mod:HeatBlast");
        }

        private static string MapOldTransformationId(TransformationEnumOld transformation) {
            return transformation switch {
                TransformationEnumOld.Arctiguana => "Ben10Mod:Arctiguana",
                TransformationEnumOld.BigChill => "Ben10Mod:BigChill",
                TransformationEnumOld.BuzzShock => "Ben10Mod:BuzzShock",
                TransformationEnumOld.ChromaStone => "Ben10Mod:ChromaStone",
                TransformationEnumOld.DiamondHead => "Ben10Mod:DiamondHead",
                TransformationEnumOld.EyeGuy => "Ben10Mod:EyeGuy",
                TransformationEnumOld.FourArms => "Ben10Mod:FourArms",
                TransformationEnumOld.GhostFreak => "Ben10Mod:GhostFreak",
                TransformationEnumOld.HeatBlast => "Ben10Mod:HeatBlast",
                TransformationEnumOld.WildVine => "Ben10Mod:WildVine",
                TransformationEnumOld.RipJaws => "Ben10Mod:RipJaws",
                TransformationEnumOld.XLR8 => "Ben10Mod:XLR8",
                TransformationEnumOld.StinkFly => "Ben10Mod:StinkFly",
                _ => string.Empty
            };
        }

        public override void ResetEffects() {
            var trans = CurrentTransformation;

            advancedCircuitMatrix = false;
            snowflake = false;

            omnitrixEnergyMax = 0;
            omnitrixEnergyRegen = 0;
            transformationDurationMultiplier = 1f;
            cooldownDurationMultiplier = 1f;
            primaryAbilityCooldownMultiplier = 1f;
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
            omnitrixEnergyMaxBonus = 0;
            omnitrixEnergyRegenBonus = 0;

            isTransformed = false;
            onCooldown = false;
            osmosianEquipped = false;
            absorptionDurationMultiplier = 1f;
            absorptionStrengthMultiplier = 1f;
            absorptionCostMultiplier = 1f;
            absorptionDebuffDurationMultiplier = 1f;
            absorptionCritChanceBonus = 0;
            absorptionArmorPenBonus = 0;
            absorptionMeleeSpeedBonus = 0f;
            absorptionMeleeKnockbackBonus = 0f;
            absorptionMoveSpeedBonus = 0f;
            absorptionLifeRegenBonus = 0;
            absorptionMaxLifeBonus = 0;
            absorptionFlatDefenseBonus = 0;
            omnitrixEquipped = false;
            equippedOmnitrix = null;

            omnitrixUpdating = false;

            PrimaryAbilityEnabled = false;
            SecondaryAbilityEnabled = false;
            TertiaryAbilityEnabled = false;
            UltimateAbilityEnabled = false;

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

        public void SetTransformationScale(float targetScale, int transitionTicks = 1,
            float? targetHitboxWidthScale = null, float? targetHitboxHeightScale = null) {
            requestedTransformationScale = Math.Max(1f, targetScale);
            requestedTransformationScaleTime = Math.Max(1, transitionTicks);
            requestedTransformationHitboxScale = new Vector2(
                Math.Max(1f, targetHitboxWidthScale ?? targetScale),
                Math.Max(1f, targetHitboxHeightScale ?? targetScale)
            );

            if (requestedTransformationScale > 1f)
                lastExpandedTransformationScale = requestedTransformationScale;

            if (requestedTransformationHitboxScale.X > 1f || requestedTransformationHitboxScale.Y > 1f) {
                lastExpandedTransformationHitboxScale = new Vector2(
                    Math.Max(lastExpandedTransformationHitboxScale.X, requestedTransformationHitboxScale.X),
                    Math.Max(lastExpandedTransformationHitboxScale.Y, requestedTransformationHitboxScale.Y)
                );
            }
        }

        private void UpdateTransformationScale(bool forceReset) {
            if (forceReset) {
                requestedTransformationScale = 1f;
                requestedTransformationScaleTime = 1;
                requestedTransformationHitboxScale = Vector2.One;
            }

            float targetScale = requestedTransformationScale;
            float scaleReference = targetScale > 1f ? targetScale : Math.Max(CurrentTransformationScale, lastExpandedTransformationScale);
            float step = Math.Abs(scaleReference - 1f) / requestedTransformationScaleTime;
            if (step <= 0f)
                step = Math.Abs(targetScale - CurrentTransformationScale);

            CurrentTransformationScale = MoveTowards(CurrentTransformationScale, targetScale, step);
            float hitboxReferenceX = requestedTransformationHitboxScale.X > 1f
                ? requestedTransformationHitboxScale.X
                : Math.Max(CurrentTransformationHitboxScale.X, lastExpandedTransformationHitboxScale.X);
            float hitboxReferenceY = requestedTransformationHitboxScale.Y > 1f
                ? requestedTransformationHitboxScale.Y
                : Math.Max(CurrentTransformationHitboxScale.Y, lastExpandedTransformationHitboxScale.Y);
            float hitboxStepX = Math.Abs(hitboxReferenceX - 1f) / requestedTransformationScaleTime;
            float hitboxStepY = Math.Abs(hitboxReferenceY - 1f) / requestedTransformationScaleTime;

            if (hitboxStepX <= 0f)
                hitboxStepX = Math.Abs(requestedTransformationHitboxScale.X - CurrentTransformationHitboxScale.X);

            if (hitboxStepY <= 0f)
                hitboxStepY = Math.Abs(requestedTransformationHitboxScale.Y - CurrentTransformationHitboxScale.Y);

            CurrentTransformationHitboxScale = new Vector2(
                MoveTowards(CurrentTransformationHitboxScale.X, requestedTransformationHitboxScale.X, hitboxStepX),
                MoveTowards(CurrentTransformationHitboxScale.Y, requestedTransformationHitboxScale.Y, hitboxStepY)
            );

            ApplyHitboxTransformationScale(CurrentTransformationHitboxScale);

            if (CurrentTransformationScale <= 1f)
                lastExpandedTransformationScale = 1f;

            if (CurrentTransformationHitboxScale == Vector2.One)
                lastExpandedTransformationHitboxScale = Vector2.One;

            requestedTransformationScale = 1f;
            requestedTransformationScaleTime = 1;
            requestedTransformationHitboxScale = Vector2.One;
        }

        private void ApplyHitboxTransformationScale(Vector2 scale) {
            int targetWidth = (int)Math.Round(BaseTransformationWidth * scale.X);
            int targetHeight = (int)Math.Round(BaseTransformationHeight * scale.Y);

            if (Player.width == targetWidth && Player.height == targetHeight)
                return;

            float left = Player.position.X;
            float bottom = Player.position.Y + Player.height;
            Player.width = targetWidth;
            Player.height = targetHeight;
            Player.position = new Vector2(left, bottom - targetHeight);
        }

        private static float MoveTowards(float current, float target, float maxDelta) {
            if (Math.Abs(target - current) <= maxDelta)
                return target;

            return current + Math.Sign(target - current) * maxDelta;
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
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();
            var omnitrixSlot = ModContent.GetInstance<OmnitrixSlot>();
            var trans = CurrentTransformation;

            if (wasTransformed && !isTransformed) {
                var customSlot = ModContent.GetInstance<OmnitrixSlot>();
                if (customSlot != null) {
                    var activeOmnitrix = GetActiveOmnitrix();
                    if (masterControl) {
                        TransformationHandler.Detransform(Player, 0, true, false);
                    }
                    else {
                        if (activeOmnitrix != null)
                            activeOmnitrix.HandleForcedDetransform(Player, this);
                        else if (!string.IsNullOrEmpty(currentTransformationId))
                            TransformationHandler.Detransform(Player, 0, showParticles: true, addCooldown: false);
                    }
                }
            }
            wasTransformed = isTransformed;

            // Play the update effect once when the Omnitrix enters its updating state, and complete
            // the item replacement when the updating buff falls off.
            if (omnitrixUpdating != omnitrixWasUpdating) {
                var activeOmnitrix = GetActiveOmnitrix();
                if (omnitrixUpdating) {
                    Random random = new Random();
                    for (int i = 0; i < 25; i++) {
                        int dustNum = Dust.NewDust(Player.position - new Vector2(1, 1), Player.width + 1,
                            Player.height + 1, DustID.BlueTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White,
                            4);
                        Main.dust[dustNum].noGravity = true;
                    }
                }
                else if (omnitrixWasUpdating && activeOmnitrix != null) {
                    activeOmnitrix.CompleteEvolution(Player, this, omnitrixSlot.FunctionalItem);
                }

                omnitrixWasUpdating = omnitrixUpdating;
            }

            if (trans != null)
                trans.UpdateEffects(Player, this);

            trans?.PostUpdateBuffs(Player, this);
        }

        public override void PostUpdate() {
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();
            int materialBuffType = ModContent.BuffType<MaterialAbsorptionBuff>();

            if (!isTransformed) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
                pendingEvolutionStepDownTime = 0;
                pendingEvolutionStepDownTransformationId = "";
            }

            omnitrixEnergy += (omnitrixEnergyRegen / 120f);
            if (omnitrixEnergy > omnitrixEnergyMax) omnitrixEnergy = omnitrixEnergyMax;

            if (KeybindSystem.OpenTransformationScreen.JustPressed && omnitrixEquipped) {
                if (!showingUI) {
                    ModContent.GetInstance<UISystem>().ShowMyUI();
                    showingUI = true;
                }
                else {
                    ModContent.GetInstance<UISystem>().HideMyUI();
                    showingUI = false;
                }
            }

            if (Main.mouseRight && Main.mouseRightRelease && Player.HeldItem.ModItem is PlumbersBadge)
                ToggleBaseAttackSelection();

            if (Player.whoAmI == Main.myPlayer && KeybindSystem.AbsorbMaterial.JustPressed)
                TryAbsorbHeldMaterial();

            var trans = CurrentTransformation;
            if (trans != null)
                trans.PostUpdate(Player, this);

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

                possessionTimer--;
                if (possessionTimer <= 0) {
                    npc.SimpleStrikeNPC(Player.HeldItem.damage * 2, Player.direction, false, 0, DamageClass.Magic);
                    EndPossession();
                }
            }

            // Drop out of ultimate attack mode immediately when the Omnitrix can no longer sustain it.
            if (isTransformed && ultimateAttack &&
                omnitrixEnergy < (CurrentTransformation?.GetUltimateAbilityCost(this) ?? 50)) {
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

            UpdateEventTransformationUnlocks();
        }

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
                trans.GetPrimaryAbilityCost(this),
                ref primaryAbilityTransformationId
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
                trans.GetSecondaryAbilityCost(this),
                ref secondaryAbilityTransformationId
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
                trans.GetTertiaryAbilityCost(this),
                ref tertiaryAbilityTransformationId
            );
        }

        private bool ActivateAbilitySlot(AttackSelection slot, bool hasTimedAbility, bool hasAttackMode,
            int activeBuffType, int cooldownBuffType, int duration, int cooldown, int activationCost,
            ref string transformationIdStorage) {
            if (hasTimedAbility) {
                if (Player.HasBuff(cooldownBuffType) || Player.HasBuff(activeBuffType))
                    return false;

                if (omnitrixEnergy < activationCost)
                    return false;

                if (activationCost > 0)
                    omnitrixEnergy -= activationCost;

                Player.AddBuff(activeBuffType, duration);
                transformationIdStorage = currentTransformationId;
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

            if (singleUse)
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

        private void ToggleBaseAttackSelection() {
            if (setAttack is not AttackSelection.Primary and not AttackSelection.Secondary)
                return;

            baseAttackSelection = baseAttackSelection == AttackSelection.Primary
                ? AttackSelection.Secondary
                : AttackSelection.Primary;
            setAttack = baseAttackSelection;
        }

        private void SetAttackSelection(AttackSelection selection) {
            setAttack = selection;
            if (selection is AttackSelection.Primary or AttackSelection.Secondary)
                baseAttackSelection = selection;
        }

        public void ResetAttackToBaseSelection() {
            setAttack = baseAttackSelection;
        }

        private static bool IsAbilityAttackSelection(AttackSelection selection) {
            return selection is AttackSelection.PrimaryAbility or AttackSelection.SecondaryAbility or AttackSelection.TertiaryAbility;
        }

        public override bool CanUseItem(Item item) {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return trans?.CanUseItem(Player, this, item) ?? true;
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return trans?.CanBeHitByNPC(Player, this, npc, ref cooldownSlot) ?? true;
        }

        public override bool CanBeHitByProjectile(Projectile proj) {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return trans?.CanBeHitByProjectile(Player, this, proj) ?? true;
        }

        public override bool FreeDodge(Player.HurtInfo info) {
            var trans = CurrentTransformation;
            return trans?.FreeDodge(Player, this, info) ?? false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
            var trans = CurrentTransformation;
            trans?.ModifyHurt(Player, this, ref modifiers);
        }

        public override void OnHurt(Player.HurtInfo info) {
            CurrentTransformation?.OnHurt(Player, this, info);
            base.OnHurt(info);
        }

        public override void PostHurt(Player.HurtInfo info) {
            CurrentTransformation?.PostHurt(Player, this, info);
            base.PostHurt(info);
        }

        public override void OnHitAnything(float x, float y, Entity victim) {
            CurrentTransformation?.OnHitAnything(Player, this, victim, x, y);
        }

        public override bool? CanHitNPCWithItem(Item item, NPC target) {
            var trans = CurrentTransformation;
            return trans?.CanHitNPCWithItem(Player, this, item, target);
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
            var trans = CurrentTransformation;
            trans?.ModifyHitNPCWithItem(Player, this, item, target, ref modifiers);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
            var trans = CurrentTransformation;
            trans?.OnHitNPCWithItem(Player, this, item, target, hit, damageDone);
            ApplyAbsorptionHitEffects(target);
            base.OnHitNPCWithItem(item, target, hit, damageDone);
        }

        public override bool? CanHitNPCWithProj(Projectile proj, NPC target) {
            var trans = CurrentTransformation;
            return trans?.CanHitNPCWithProjectile(Player, this, proj, target);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
            var trans = CurrentTransformation;
            trans?.ModifyHitNPCWithProjectile(Player, this, proj, target, ref modifiers);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
            var trans = CurrentTransformation;
            trans?.OnHitNPCWithProjectile(Player, this, proj, target, hit, damageDone);
            ApplyAbsorptionHitEffects(target);
            base.OnHitNPCWithProj(proj, target, hit, damageDone);
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            var trans = CurrentTransformation;
            if (trans == null) return;

            trans.ModifyDrawInfo(Player, this, ref drawInfo);
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a,
            ref bool fullBright) {
            var customSlot = ModContent.GetInstance<OmnitrixSlot>();
            GetActiveOmnitrix()?.ApplyHandVisuals(Player, this, customSlot.HideVisuals);

            if (TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile absorptionProfile)) {
                Color tint = absorptionProfile.TintColor;
                Vector3 vividTint = Color.Lerp(tint, Color.White, 0.18f).ToVector3();
                r = MathHelper.Lerp(r, MathHelper.Clamp(vividTint.X * 1.8f, 0f, 1f), 0.97f);
                g = MathHelper.Lerp(g, MathHelper.Clamp(vividTint.Y * 1.8f, 0f, 1f), 0.97f);
                b = MathHelper.Lerp(b, MathHelper.Clamp(vividTint.Z * 1.8f, 0f, 1f), 0.97f);
                a = 1f;
                fullBright = true;

                if (Main.rand.NextBool(3)) {
                    Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(18f, 26f), DustID.GemDiamond,
                        Main.rand.NextVector2Circular(1.5f, 1.5f), 70, Color.Lerp(tint, Color.White, 0.22f), 1.2f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.1f;
                }
            }

            var trans = CurrentTransformation;
            if (trans != null)
                trans.DrawEffects(ref drawInfo);
        }

        public override void FrameEffects() {
            var customSlot = ModContent.GetInstance<OmnitrixSlot>();
            GetActiveOmnitrix()?.ApplyHandVisuals(Player, this, customSlot.HideVisuals);

            if (!customSlot.HideVisuals && isTransformed) {
                Player.wings = -1;
                Player.shoe = -1;
                Player.handoff = -1;
                Player.handon = -1;
                Player.back = -1;
                Player.waist = -1;
                Player.shield = -1;
            }

            CurrentTransformation?.FrameEffects(Player, this);
        }

        public override void PreUpdateMovement() {
            DashMovement();

            var trans = CurrentTransformation;
            if (trans != null)
                trans.PreUpdateMovement(Player, this);
        }

        private void DashMovement() {
            if (CanUseDash() && DashDir != -1 && DashDelay == 0) {
                var trans = CurrentTransformation;
                if (trans?.FullID == "Ben10Mod:XLR8") {
                    Vector2 newVelocity = Player.velocity;

                    switch (DashDir) {
                        case DashLeft when Player.velocity.X > -DashVelocity:
                        case DashRight when Player.velocity.X < DashVelocity:
                            float dashDirection = DashDir == DashRight ? 1 : -1;
                            newVelocity.X = dashDirection * DashVelocity;
                            break;
                        default:
                            return;
                    }

                    DashDelay = DashCooldown;
                    DashTimer = DashDuration;
                    Player.velocity = newVelocity;
                }
            }

            if (DashDelay > 0) DashDelay--;
            if (DashTimer > 0) {
                Player.eocDash = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;
                Player.GiveImmuneTimeForCollisionAttack(40);
                DashTimer--;
            }
        }

        private bool CanUseDash() {
            return Player.dashType == 0 && !Player.setSolar && !Player.mount.Active;
        }

        public override void OnEnterWorld() {
            ModContent.GetInstance<UISystem>().HideMyUI();
            if (!isTransformed)
                currentTransformationId = "";

            CurrentTransformation?.OnEnterWorld(Player, this);
        }

        private void EndPossession() {
            if (!inPossessionMode) return;

            inPossessionMode = false;
            possessedTargetIndex = -1;

            Player.position = prePossessionPosition;
            Player.invis = false;
            Player.immune = true;
            Player.immuneTime = 60;

            SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.3f, Volume = 0.8f }, Player.Center);
            for (int i = 0; i < 30; i++) {
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(6f, 6f),
                    Scale: 1.8f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            var trans = CurrentTransformation;
            trans?.OnHitNPC(Player, this, target, hit, damageDone);
            ApplyAbsorptionHitEffects(target);

            base.OnHitNPC(target, hit, damageDone);

            var activeOmnitrix = GetActiveOmnitrix();
            if (isTransformed && !ultimateAttack && !IsUltimateAbilityActive && activeOmnitrix != null)
                omnitrixEnergy += activeOmnitrix.GetEnergyGainFromDamage(hit.Damage);
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

                if (Main.GameUpdateCount % 60 == 0 && npc.active && npc.life > 0) {
                    int dotDamage = 35;
                    npc.life -= dotDamage;
                    if (npc.life < 1) npc.life = 1;
                    CombatText.NewText(npc.Hitbox, new Color(180, 80, 255), dotDamage, dramatic: true);
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

        public bool TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile profile) {
            if (absorbedMaterialTime > 0 && absorbedMaterialItemType > 0)
                return MaterialAbsorptionRegistry.TryGetProfile(absorbedMaterialItemType, out profile);

            profile = null;
            return false;
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

        private void ApplyAbsorptionHitEffects(NPC target) {
            if (target == null || !target.active || target.life <= 0)
                return;

            if (!TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile profile))
                return;

            foreach (MaterialAbsorptionHitEffect effect in profile.HitEffects) {
                if (effect.BuffType <= 0 || effect.BuffTime <= 0)
                    continue;

                int buffTime = Math.Max(1, (int)Math.Round(effect.BuffTime * absorptionDebuffDurationMultiplier));
                target.AddBuff(effect.BuffType, buffTime);
            }
        }

        private void TryAbsorbHeldMaterial() {
            if (!osmosianEquipped) {
                if (Player.whoAmI == Main.myPlayer)
                    Main.NewText("You need an Osmosian Harness equipped to absorb materials.", new Color(230, 120, 120));
                return;
            }

            if (omnitrixEquipped) {
                if (Player.whoAmI == Main.myPlayer)
                    Main.NewText("Osmosian absorption cannot be used while an Omnitrix is equipped.", new Color(230, 120, 120));
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.RequestAbsorbMaterial);
                packet.Send();
                return;
            }

            TryAbsorbHeldMaterialDirect();
        }

        public void HandleAbsorbMaterialRequest() {
            TryAbsorbHeldMaterialDirect();
        }

        private void TryAbsorbHeldMaterialDirect() {
            Item heldItem = Player.HeldItem;
            if (heldItem == null || heldItem.IsAir) {
                if (absorbedMaterialTime > 0)
                    ClearAbsorbedMaterial();
                return;
            }

            if (absorbedMaterialTime > 0 && heldItem.type == absorbedMaterialItemType) {
                ClearAbsorbedMaterial();
                return;
            }

            if (!MaterialAbsorptionRegistry.TryGetProfile(heldItem.type, out MaterialAbsorptionProfile profile)) {
                if (absorbedMaterialTime > 0)
                    ClearAbsorbedMaterial();
                else
                    Main.NewText("That material cannot be absorbed.", new Color(230, 120, 120));
                return;
            }

            int consumeAmount = Math.Max(1, (int)Math.Round(profile.ConsumeAmount * absorptionCostMultiplier));
            int durationTicks = Math.Max(60, (int)Math.Round(profile.DurationTicks * absorptionDurationMultiplier));

            if (heldItem.stack < consumeAmount) {
                Main.NewText($"You need {consumeAmount} {profile.DisplayName} to absorb it.", new Color(255, 210, 110));
                return;
            }

            heldItem.stack -= consumeAmount;
            if (heldItem.stack <= 0)
                heldItem.TurnToAir();

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Player.whoAmI, Player.selectedItem);

            SetAbsorbedMaterial(profile.SourceItemType, durationTicks, showEffects: true);
        }

        private void ClearAbsorbedMaterial(bool showEffects = true) {
            if (!TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile profile)) {
                absorbedMaterialItemType = 0;
                absorbedMaterialTime = 0;
                Player.ClearBuff(ModContent.BuffType<MaterialAbsorptionBuff>());

                if (Main.netMode == NetmodeID.Server)
                    SyncAbsorbedMaterial(showEffects);
                return;
            }

            SetAbsorbedMaterial(0, 0, showEffects, profile);
        }

        public void ApplyAbsorbedMaterialSync(int itemType, int timeLeft, bool showEffects) {
            MaterialAbsorptionProfile previousProfile = null;
            if (TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile currentProfile))
                previousProfile = currentProfile;

            SetAbsorbedMaterial(itemType, timeLeft, showEffects, previousProfile, shouldSync: false);
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
            if (Main.netMode == NetmodeID.Server)
                SyncAbsorbedMaterial(showEffects: false, toWho: toWho, ignoreClient: fromWho);
        }

        private void SetAbsorbedMaterial(int itemType, int timeLeft, bool showEffects, MaterialAbsorptionProfile previousProfile = null,
            bool shouldSync = true) {
            if (previousProfile == null && TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile activeProfile))
                previousProfile = activeProfile;

            absorbedMaterialItemType = itemType;
            absorbedMaterialTime = itemType > 0 ? Math.Max(1, timeLeft) : 0;

            int buffType = ModContent.BuffType<MaterialAbsorptionBuff>();
            Player.ClearBuff(buffType);
            if (itemType > 0 && absorbedMaterialTime > 0)
                Player.AddBuff(buffType, absorbedMaterialTime);

            if (showEffects && Main.netMode != NetmodeID.Server) {
                if (itemType > 0 && MaterialAbsorptionRegistry.TryGetProfile(itemType, out MaterialAbsorptionProfile newProfile)) {
                    if (Player.whoAmI == Main.myPlayer) {
                        Main.NewText($"Absorbed {newProfile.DisplayName}.", newProfile.TintColor);
                        CombatText.NewText(Player.getRect(), newProfile.TintColor, newProfile.DisplayName, dramatic: true);
                    }

                    for (int i = 0; i < 30; i++) {
                        Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(24f, 30f), DustID.GemDiamond,
                            Main.rand.NextVector2Circular(2.4f, 2.4f), 80, Color.Lerp(newProfile.TintColor, Color.White, 0.2f), 1.35f);
                        dust.noGravity = true;
                        dust.fadeIn = 1.2f;
                    }
                }
                else if (previousProfile != null) {
                    if (Player.whoAmI == Main.myPlayer)
                        Main.NewText($"{previousProfile.DisplayName} absorption cleared.", new Color(220, 220, 220));

                    for (int i = 0; i < 18; i++) {
                        Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(18f, 22f), DustID.GemDiamond,
                            Main.rand.NextVector2Circular(1.8f, 1.8f), 100, previousProfile.TintColor, 1f);
                        dust.noGravity = true;
                    }
                }
            }

            if (shouldSync && Main.netMode == NetmodeID.Server)
                SyncAbsorbedMaterial(showEffects);
        }

        private void SyncAbsorbedMaterial(bool showEffects, int toWho = -1, int ignoreClient = -1) {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.SyncAbsorbedMaterial);
            packet.Write((byte)Player.whoAmI);
            packet.Write(absorbedMaterialItemType);
            packet.Write(absorbedMaterialTime);
            packet.Write(showEffects);
            packet.Send(toWho, ignoreClient);
        }

        public void RecordEventParticipation(NPC npc) {
            if (npc == null || !npc.active || npc.friendly || npc.townNPC || npc.CountsAsACritter)
                return;

            foreach (int eventId in GetActiveTrackedEvents()) {
                if (DoesNpcCountForEventParticipation(eventId, npc))
                    participatedEvents.Add(eventId);
            }
        }

        public bool UnlockTransformation(string transformationId, bool sync = true, bool showEffects = true) {
            if (TransformationHandler.HasTransformation(Player, transformationId))
                return false;

            unlockedTransformations.Add(transformationId);

            if (showEffects && Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer) {
                string name = TransformationLoader.Get(transformationId)?.TransformationName ?? "Unknown";
                Main.NewText($"{name} has been unlocked!", Color.LimeGreen);
                CombatText.NewText(Player.getRect(), Color.LimeGreen, $"{name}!", dramatic: true);
            }

            if (sync && Main.netMode == NetmodeID.Server) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.UnlockTransformation);
                packet.Write((byte)Player.whoAmI);
                packet.Write(transformationId);
                packet.Send(toClient: Player.whoAmI);
            }

            return true;
        }

        public bool RemoveTransformation(string transformationId, bool sync = true, bool showEffects = true) {
            if (!unlockedTransformations.Contains(transformationId))
                return false;

            unlockedTransformations.Remove(transformationId);

            for (int i = 0; i < transformationSlots.Length; i++) {
                if (transformationSlots[i] == transformationId)
                    transformationSlots[i] = "";
            }

            if (currentTransformationId == transformationId)
                TransformationHandler.Detransform(Player, 0, showParticles: showEffects, addCooldown: false, playSound: showEffects);

            if (showEffects && Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer) {
                string name = TransformationLoader.Get(transformationId)?.TransformationName ?? transformationId;
                Main.NewText($"{name} has been removed.", Color.OrangeRed);
            }

            if (sync && Main.netMode == NetmodeID.Server) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.RemoveTransformation);
                packet.Write((byte)Player.whoAmI);
                packet.Write(transformationId);
                packet.Send(toClient: Player.whoAmI);
            }

            return true;
        }

        private void UpdateEventTransformationUnlocks() {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            HashSet<int> currentlyActiveEvents = new(GetActiveTrackedEvents());

            foreach (int eventId in currentlyActiveEvents)
                activeEvents.Add(eventId);

            List<int> completedEvents = new();
            foreach (int eventId in activeEvents) {
                if (!currentlyActiveEvents.Contains(eventId))
                    completedEvents.Add(eventId);
            }

            foreach (int eventId in completedEvents) {
                if (participatedEvents.Contains(eventId) && DidEventComplete(eventId)) {
                    string transformationId = GetTransformationIdForCompletedEvent(eventId);
                    if (!string.IsNullOrEmpty(transformationId))
                        UnlockTransformation(transformationId);
                }

                participatedEvents.Remove(eventId);
                activeEvents.Remove(eventId);
            }
        }

        private static IEnumerable<int> GetActiveTrackedEvents() {
            if (Main.bloodMoon)
                yield return EventBloodMoon;

            if (Main.eclipse)
                yield return EventSolarEclipse;

            if (Main.slimeRain)
                yield return EventSlimeRain;

            if (Main.pumpkinMoon)
                yield return EventPumpkinMoon;

            if (Main.snowMoon)
                yield return EventFrostMoon;

            if (Main.invasionType == InvasionID.GoblinArmy && Main.invasionSize > 0)
                yield return InvasionID.GoblinArmy;

            if (Main.invasionType == InvasionID.SnowLegion && Main.invasionSize > 0)
                yield return InvasionID.SnowLegion;

            if (Main.invasionType == InvasionID.PirateInvasion && Main.invasionSize > 0)
                yield return InvasionID.PirateInvasion;

            if (Main.invasionType == InvasionID.MartianMadness && Main.invasionSize > 0)
                yield return InvasionID.MartianMadness;

            if (DD2Event.Ongoing)
                yield return EventOldOnesArmy;
        }

        private static bool DoesNpcCountForEventParticipation(int eventId, NPC npc) {
            if (eventId == InvasionID.GoblinArmy)
                return IsGoblinArmyNpc(npc);

            return true;
        }

        private static bool DidEventComplete(int eventId) {
            switch (eventId) {
                case EventBloodMoon:
                case EventSolarEclipse:
                case EventSlimeRain:
                case EventPumpkinMoon:
                case EventFrostMoon:
                    return true;
                case InvasionID.GoblinArmy:
                    return NPC.downedGoblins;
                case InvasionID.SnowLegion:
                case InvasionID.PirateInvasion:
                case InvasionID.MartianMadness:
                case EventOldOnesArmy:
                    return true;
                default:
                    return false;
            }
        }

        private static string GetTransformationIdForCompletedEvent(int eventId) {
            switch (eventId) {
                case EventBloodMoon:
                    return "Ben10Mod:GhostFreak";
                case EventSolarEclipse:
                    return string.Empty;
                case EventSlimeRain:
                    return string.Empty;
                case EventPumpkinMoon:
                    return string.Empty;
                case EventFrostMoon:
                    return string.Empty;
                case InvasionID.GoblinArmy:
                    return "Ben10Mod:RipJaws";
                case InvasionID.SnowLegion:
                    return string.Empty;
                case InvasionID.PirateInvasion:
                    return string.Empty;
                case InvasionID.MartianMadness:
                    return string.Empty;
                case EventOldOnesArmy:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private static bool IsGoblinArmyNpc(NPC npc) {
            return npc.type == NPCID.GoblinPeon ||
                   npc.type == NPCID.GoblinThief ||
                   npc.type == NPCID.GoblinWarrior ||
                   npc.type == NPCID.GoblinSorcerer ||
                   npc.type == NPCID.GoblinArcher ||
                   npc.type == NPCID.GoblinSummoner;
        }
    }
}
