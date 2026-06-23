using Ben10Mod.Common.Absorption;
using Ben10Mod.Common.Omnitrix;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Items.Accessories;

namespace Ben10Mod {
    public partial class OmnitrixPlayer : ModPlayer {
        public enum AttackSelection {
            Primary,
            Secondary,
            PrimaryAbility,
            SecondaryAbility,
            TertiaryAbility,
            Ultimate
        }

        public readonly struct ActiveAbilityStatus {
            public ActiveAbilityStatus(AttackSelection selection, string displayName, int remainingTicks, Color accentColor) {
                Selection = selection;
                DisplayName = displayName;
                RemainingTicks = remainingTicks;
                AccentColor = accentColor;
            }

            public AttackSelection Selection { get; }
            public string DisplayName { get; }
            public int RemainingTicks { get; }
            public Color AccentColor { get; }
            public string RemainingText => RemainingTicks > 0 ? FormatCooldownTicks(RemainingTicks) : "Active";
        }

        public bool masterControl = false;

        public OmnitrixEnergyController Energy { get; } = new();
        public TransformationRosterState Roster { get; } = new();
        public AbilityController Abilities { get; } = new();
        public AttackSelectionController AttackSelectionState { get; } = new();
        public TransformationCustomizationStore Customization { get; } = new();
        public PossessionController Possession { get; } = new();
        public OmnitrixInputController Input { get; } = new();
        internal TransformationProgressionSystem Progression { get; } = new();
        private MaterialAbsorptionPlayer Absorption => Player.GetModPlayer<MaterialAbsorptionPlayer>();

        public bool omnitrixEquipped = false;
        public bool isTransformed = false;
        public bool wasTransformed = false;
        public bool onCooldown = false;
        public bool osmosianEquipped {
            get => Absorption.OsmosianEquipped;
            set => Absorption.OsmosianEquipped = value;
        }
        public bool anoditeCatalystEquipped {
            get => Absorption.AnoditeCatalystEquipped;
            set => Absorption.AnoditeCatalystEquipped = value;
        }
        public float absorptionDurationMultiplier {
            get => Absorption.DurationMultiplier;
            set => Absorption.DurationMultiplier = value;
        }
        public float absorptionStrengthMultiplier {
            get => Absorption.StrengthMultiplier;
            set => Absorption.StrengthMultiplier = value;
        }
        public float absorptionCostMultiplier {
            get => Absorption.CostMultiplier;
            set => Absorption.CostMultiplier = value;
        }
        public float absorptionDebuffDurationMultiplier {
            get => Absorption.DebuffDurationMultiplier;
            set => Absorption.DebuffDurationMultiplier = value;
        }
        public int absorptionCritChanceBonus {
            get => Absorption.CritChanceBonus;
            set => Absorption.CritChanceBonus = value;
        }
        public int absorptionArmorPenBonus {
            get => Absorption.ArmorPenBonus;
            set => Absorption.ArmorPenBonus = value;
        }
        public float absorptionMeleeSpeedBonus {
            get => Absorption.MeleeSpeedBonus;
            set => Absorption.MeleeSpeedBonus = value;
        }
        public float absorptionMeleeKnockbackBonus {
            get => Absorption.MeleeKnockbackBonus;
            set => Absorption.MeleeKnockbackBonus = value;
        }
        public float absorptionMoveSpeedBonus {
            get => Absorption.MoveSpeedBonus;
            set => Absorption.MoveSpeedBonus = value;
        }
        public int absorptionLifeRegenBonus {
            get => Absorption.LifeRegenBonus;
            set => Absorption.LifeRegenBonus = value;
        }
        public int absorptionMaxLifeBonus {
            get => Absorption.MaxLifeBonus;
            set => Absorption.MaxLifeBonus = value;
        }
        public int absorptionFlatDefenseBonus {
            get => Absorption.FlatDefenseBonus;
            set => Absorption.FlatDefenseBonus = value;
        }
        public AttackSelection setAttack {
            get => AttackSelectionState.Current;
            set => AttackSelectionState.Current = value;
        }
        private AttackSelection baseAttackSelection {
            get => AttackSelectionState.Base;
            set => AttackSelectionState.Base = value;
        }
        public bool loadedAbilityAttackUsed {
            get => AttackSelectionState.LoadedAbilityAttackUsed;
            set => AttackSelectionState.LoadedAbilityAttackUsed = value;
        }
        public int transformationAttackSerial {
            get => AttackSelectionState.AttackSerial;
            set => AttackSelectionState.AttackSerial = value;
        }
        public int transformationAttackDamage {
            get => AttackSelectionState.AttackDamage;
            set => AttackSelectionState.AttackDamage = value;
        }
        public int ultimateEchoEchoSpeakerSpawnSerial {
            get => AttackSelectionState.UltimateEchoEchoSpeakerSpawnSerial;
            set => AttackSelectionState.UltimateEchoEchoSpeakerSpawnSerial = value;
        }
        public int absorbedMaterialItemType {
            get => Absorption.AbsorbedMaterialItemType;
            set => Absorption.AbsorbedMaterialItemType = value;
        }
        public int absorbedMaterialTime {
            get => Absorption.AbsorbedMaterialTime;
            set => Absorption.AbsorbedMaterialTime = value;
        }
        public int attackSelectionPulseTime {
            get => AttackSelectionState.PulseTime;
            set => AttackSelectionState.PulseTime = value;
        }
        private int attackEnergyGainLockTime {
            get => AttackSelectionState.EnergyGainLockTime;
            set => AttackSelectionState.EnergyGainLockTime = value;
        }
        private int markAttackProjectilesNoEnergyGainTime {
            get => AttackSelectionState.MarkProjectilesNoEnergyGainTime;
            set => AttackSelectionState.MarkProjectilesNoEnergyGainTime = value;
        }

        public int cooldownTime = 120;
        public int transformationTime = 300;
        public float CurrentTransformationScale = 1f;
        public Vector2 CurrentTransformationHitboxScale = Vector2.One;
        public Vector2 GoopVisualScale = Vector2.One;

        public bool PrimaryAbilityEnabled {
            get => Abilities.PrimaryEnabled;
            set => Abilities.PrimaryEnabled = value;
        }
        public bool PrimaryAbilityWasEnabled {
            get => Abilities.PrimaryWasEnabled;
            set => Abilities.PrimaryWasEnabled = value;
        }
        public bool SecondaryAbilityEnabled {
            get => Abilities.SecondaryEnabled;
            set => Abilities.SecondaryEnabled = value;
        }
        public bool SecondaryAbilityWasEnabled {
            get => Abilities.SecondaryWasEnabled;
            set => Abilities.SecondaryWasEnabled = value;
        }
        public bool TertiaryAbilityEnabled {
            get => Abilities.TertiaryEnabled;
            set => Abilities.TertiaryEnabled = value;
        }
        public bool TertiaryAbilityWasEnabled {
            get => Abilities.TertiaryWasEnabled;
            set => Abilities.TertiaryWasEnabled = value;
        }
        public bool UltimateAbilityEnabled {
            get => Abilities.UltimateEnabled;
            set => Abilities.UltimateEnabled = value;
        }
        public bool UltimateAbilityWasEnabled {
            get => Abilities.UltimateWasEnabled;
            set => Abilities.UltimateWasEnabled = value;
        }
        public string primaryAbilityTransformationId {
            get => Abilities.PrimaryTransformationId;
            set => Abilities.PrimaryTransformationId = value ?? "";
        }
        public string secondaryAbilityTransformationId {
            get => Abilities.SecondaryTransformationId;
            set => Abilities.SecondaryTransformationId = value ?? "";
        }
        public string tertiaryAbilityTransformationId {
            get => Abilities.TertiaryTransformationId;
            set => Abilities.TertiaryTransformationId = value ?? "";
        }
        public string ultimateAbilityTransformationId {
            get => Abilities.UltimateTransformationId;
            set => Abilities.UltimateTransformationId = value ?? "";
        }

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
        private const float Xlr8DashAccessoryVelocity = 22f;
        private const int Xlr8DashAccessoryDuration = 24;
        private const int Xlr8DashAccessoryVisualDuration = 36;

        public const int TransformationSlotCount = 5;
        public const int PalettePresetSlotCount = 3;
        public const int MaxCustomTransformationNameLength = 24;
        public const byte TransformationSpeedBoostPercentStep = 25;
        public const byte TransformationSpeedBoostPercentMax = 100;
        public const byte DefaultTransformationSpeedBoostPercent = 100;
        public string[] transformationSlots {
            get => Roster.Slots;
            set => Roster.Slots = value ?? Array.Empty<string>();
        }
        public string currentTransformationId = "";
        public List<string> unlockedTransformations => Roster.Unlocked;
        public byte transformationSpeedBoostPercent = DefaultTransformationSpeedBoostPercent;

        public bool showingUI = false;

        public bool omnitrixUpdating = false;
        public bool omnitrixWasUpdating = false;
        public float omnitrixEnergy {
            get => Energy.Current;
            set => Energy.Current = value;
        }
        public float omnitrixEnergyMax {
            get => Energy.Max;
            set => Energy.Max = value;
        }
        public float omnitrixEnergyRegen {
            get => Energy.Regen;
            set => Energy.Regen = value;
        }
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
        public int omnitrixEnergyMaxBonus {
            get => Energy.MaxBonus;
            set => Energy.MaxBonus = value;
        }
        public int omnitrixEnergyRegenBonus {
            get => Energy.RegenBonus;
            set => Energy.RegenBonus = value;
        }
        public int pendingEvolutionStepDownTime = 0;
        public string pendingEvolutionStepDownTransformationId = "";
        public Omnitrix equippedOmnitrix = null;
        public Item equippedOmnitrixItem = null;

        public bool inPossessionMode {
            get => Possession.Active;
            set => Possession.Active = value;
        }
        public Vector2 prePossessionPosition {
            get => Possession.ReturnPosition;
            set => Possession.ReturnPosition = value;
        }
        public int possessedTargetIndex {
            get => Possession.TargetIndex;
            set => Possession.TargetIndex = value;
        }
        public int possessionTimer {
            get => Possession.Timer;
            set => Possession.Timer = value;
        }
        private const int PossessionDuration = 360;

        public bool snowflake = false;
        public bool advancedCircuitMatrix = false;
        public bool advancedCircuitMatrixEquippedWhileTransformed = false;
        public bool transformationFailsafeEquipped = false;
        public bool completedOmnitrixEquipped = false;
        public bool chronoAcceleratorEquipped = false;
        public bool heroConvergenceEmblemEquipped = false;
        public bool omniCoreReactorEquipped = false;
        public bool xlr8DashAccessoryEquipped = false;
        private int completedOmnitrixSyncTime = 0;
        private int completedOmnitrixRevivalCooldown = 0;
        private int chronoAcceleratorProcCooldown = 0;
        private int heroConvergenceProcCooldown = 0;
        private int heroConvergenceHitCount = 0;
        private float omniCoreReactorCharge = 0f;
        private int omniCoreReactorPulseCooldown = 0;
        private int xlr8DashAccessoryVisualTime = 0;

        private HashSet<string> favoriteTransformations => Roster.Favorites;
        private HashSet<string> newlyUnlockedTransformations => Roster.NewlyUnlocked;
        private const int BaseTransformationWidth = 20;
        private const int BaseTransformationHeight = 42;
        private const float MinimumTransformationScale = 0.35f;
        private const int AttackSelectionPulseDuration = 24;
        private const int UltimateReadyCueDuration = 72;
        private const int TransformFailureFeedbackCooldownTicks = 30;
        private float requestedTransformationScale = 1f;
        private int requestedTransformationScaleTime = 1;
        private Vector2 requestedTransformationHitboxScale = Vector2.One;
        private float lastExpandedTransformationScale = 1f;
        private Vector2 lastExpandedTransformationHitboxScale = Vector2.One;
        internal bool goopWasGrounded = false;
        internal float goopPreviousVerticalVelocity = 0f;
        internal float goopLandingSquish = 0f;
        internal int goopLandingSplashTime = 0;
        private string lastAttackUiTransformationId = "";
        private string lastUltimateReadyCueTransformationId = "";
        private string lastTransformFailureMessage = "";
        private ulong lastTransformFailureTick = 0;
        private int ultimateReadyCueTime = 0;
        private bool wasUltimateReady = false;
        private bool skipAutomaticForcedDetransformHandling = false;
        private bool airborneLungeConsumed = false;
        private int activeLungeTime = 0;

        private const int ChronoAcceleratorProcCooldownMax = 78;
        private const int HeroConvergenceHitsRequired = 6;
        private const int HeroConvergenceProcCooldownMax = 24;
        private const float OmniCoreReactorChargeThreshold = 80f;
        private const float OmniCoreReactorHitChargeMultiplier = 0.05f;
        private const float OmniCoreReactorMinHitCharge = 1f;
        private const float OmniCoreReactorMaxHitCharge = 5f;
        private const int OmniCoreReactorPulseCooldownMax = 24;
        public const int CompletedOmnitrixSyncDurationTicks = 5 * 60;
        private const float CompletedOmnitrixSyncRestoreAmount = 20f;
        private const int CompletedOmnitrixRevivalCooldownTicks = 60 * 60;
        private const float CompletedOmnitrixRevivalEnergyRestoreAmount = 10f;
        private const float CompletedOmnitrixRevivalLifeRatio = 0.2f;
        private const int CompletedOmnitrixRevivalMinimumLife = 80;
        private static readonly string[] UpgradeRequiredTransformationIds = {
            "Ben10Mod:Humungousaur",
            "Ben10Mod:EyeGuy",
            "Ben10Mod:EchoEcho"
        };

        public Transformation CurrentTransformation
            => TransformationLoader.Get(currentTransformationId);

        public bool IsTransformed => !string.IsNullOrEmpty(currentTransformationId);
        public bool IsPrimaryAbilityActive => PrimaryAbilityEnabled || Player.HasBuff<PrimaryAbility>();
        public bool IsSecondaryAbilityActive => SecondaryAbilityEnabled || Player.HasBuff<SecondaryAbility>();
        public bool IsTertiaryAbilityActive => TertiaryAbilityEnabled || Player.HasBuff<TertiaryAbility>();
        public bool IsUltimateAbilityActive => UltimateAbilityEnabled || Player.HasBuff<UltimateAbility>();
        public bool HasMasterControlAccess => masterControl;
        public bool altAttack => setAttack == AttackSelection.Secondary;
        public bool ultimateAttack => setAttack == AttackSelection.Ultimate;
        public bool IsPrimaryAbilityAttackLoaded => setAttack == AttackSelection.PrimaryAbility;
        public bool IsSecondaryAbilityAttackLoaded => setAttack == AttackSelection.SecondaryAbility;
        public bool IsTertiaryAbilityAttackLoaded => setAttack == AttackSelection.TertiaryAbility;
        public bool HasLoadedAbilityAttack => AttackSelectionState.HasLoadedAbilityAttack;
        public bool HasLoadedBadgeAttack => AttackSelectionState.HasLoadedBadgeAttack;
        public bool CompletedOmnitrixSyncActive => completedOmnitrixEquipped && completedOmnitrixSyncTime > 0;
        public int CompletedOmnitrixSyncTicksRemaining => completedOmnitrixSyncTime;
        public float AttackSelectionPulseProgress => AttackSelectionState.GetPulseProgress(AttackSelectionPulseDuration);
        public float UltimateReadyCueProgress => ultimateReadyCueTime / (float)UltimateReadyCueDuration;
        public float TransformationSpeedBoostScale => transformationSpeedBoostPercent / (float)TransformationSpeedBoostPercentMax;
        public int HeroConvergenceHitCount => heroConvergenceHitCount;
        public int HeroConvergenceRequiredHits => HeroConvergenceHitsRequired;
        public int HeroConvergenceCooldownTicks => heroConvergenceProcCooldown;
        public int HeroConvergenceCooldownMaxTicks => HeroConvergenceProcCooldownMax;
        public float OmniCoreReactorChargeValue => omniCoreReactorCharge;
        public float OmniCoreReactorChargeThresholdValue => OmniCoreReactorChargeThreshold;


    }
}
