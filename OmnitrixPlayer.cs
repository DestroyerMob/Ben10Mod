using Ben10Mod.Common.Absorption;
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
using Terraria.Audio;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Common.CustomVisuals;
using Ben10Mod.Content.Projectiles;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Events;

namespace Ben10Mod {
    public class OmnitrixPlayer : ModPlayer {
        private sealed class PalettePresetData {
            public List<TransformationPaletteColorEntry> Entries { get; } = new();
            public List<string> EnabledChannelKeys { get; } = new();
        }

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
        public int attackSelectionPulseTime = 0;

        public int cooldownTime = 120;
        public int transformationTime = 300;
        public float CurrentTransformationScale = 1f;
        public Vector2 CurrentTransformationHitboxScale = Vector2.One;
        public Vector2 GoopVisualScale = Vector2.One;

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

        public const int TransformationSlotCount = 5;
        public const int PalettePresetSlotCount = 3;
        public const int MaxCustomTransformationNameLength = 24;
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
        public bool transformationFailsafeEquipped = false;

        private readonly HashSet<int> participatedEvents = new();
        private readonly HashSet<int> activeEvents = new();
        private readonly Dictionary<string, Dictionary<string, TransformationPaletteChannelSettings>> transformationPaletteOverrides =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> paletteEnabledChannels =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> customTransformationNames =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> favoriteTransformations =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> newlyUnlockedTransformations =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PalettePresetData[]> palettePresets =
            new(StringComparer.OrdinalIgnoreCase);
        private const int BaseTransformationWidth = 20;
        private const int BaseTransformationHeight = 42;
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
        public float AttackSelectionPulseProgress => attackSelectionPulseTime / (float)AttackSelectionPulseDuration;
        public float UltimateReadyCueProgress => ultimateReadyCueTime / (float)UltimateReadyCueDuration;

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

        public override void Initialize() {
            ResetAirborneLungeState();
        }

        public override void SaveData(TagCompound tag) {
            NormalizeStoredTransformationData();
            tag["masterControl"] = masterControl;
            tag["currentTransformationId"] = currentTransformationId;
            tag["omnitrixEnergy"] = omnitrixEnergy;
            tag["absorbedMaterialItemType"] = absorbedMaterialItemType;
            tag["absorbedMaterialTime"] = absorbedMaterialTime;

            tag["transformationRoster"] = transformationSlots;
            tag["unlockedTransformationRoster"] = unlockedTransformations.ToArray();
            tag["favoriteTransformationRoster"] = BuildNormalizedFavoriteTransformations().ToArray();
            tag["newlyUnlockedTransformationRoster"] = BuildNormalizedNewlyUnlockedTransformations().ToArray();

            List<TagCompound> paletteEntries = new();
            foreach (TransformationPaletteColorEntry entry in BuildNormalizedTransformationPaletteEntries()) {
                paletteEntries.Add(new TagCompound {
                    ["transformationId"] = entry.TransformationId,
                    ["channelId"] = entry.ChannelId,
                    ["r"] = (int)entry.Color.R,
                    ["g"] = (int)entry.Color.G,
                    ["b"] = (int)entry.Color.B,
                    ["hue"] = (int)entry.Hue,
                    ["saturation"] = (int)entry.Saturation
                });
            }

            tag["transformationPalette"] = paletteEntries;
            tag["paletteEnabledChannels"] = BuildNormalizedPaletteEnabledChannelKeys().ToArray();

            List<TagCompound> customNameEntries = new();
            foreach (KeyValuePair<string, string> entry in BuildNormalizedCustomTransformationNames()) {
                customNameEntries.Add(new TagCompound {
                    ["transformationId"] = entry.Key,
                    ["customName"] = entry.Value
                });
            }

            tag["customTransformationNames"] = customNameEntries;

            List<TagCompound> palettePresetEntries = new();
            foreach (TagCompound presetEntry in BuildPalettePresetTagEntries())
                palettePresetEntries.Add(presetEntry);
            tag["transformationPalettePresets"] = palettePresetEntries;
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

            favoriteTransformations.Clear();
            if (tag.TryGet("favoriteTransformationRoster", out string[] favoriteArray)) {
                for (int i = 0; i < favoriteArray.Length; i++) {
                    Transformation favoriteTransformation = TransformationLoader.Resolve(favoriteArray[i]);
                    if (favoriteTransformation != null)
                        favoriteTransformations.Add(favoriteTransformation.FullID);
                }
            }

            newlyUnlockedTransformations.Clear();
            if (tag.TryGet("newlyUnlockedTransformationRoster", out string[] newArray)) {
                for (int i = 0; i < newArray.Length; i++) {
                    Transformation newTransformation = TransformationLoader.Resolve(newArray[i]);
                    if (newTransformation != null)
                        newlyUnlockedTransformations.Add(newTransformation.FullID);
                }
            }

            transformationPaletteOverrides.Clear();
            paletteEnabledChannels.Clear();
            customTransformationNames.Clear();
            palettePresets.Clear();
            if (tag.TryGet("transformationPalette", out List<TagCompound> paletteEntries)) {
                foreach (TagCompound paletteEntry in paletteEntries) {
                    string transformationId = paletteEntry.GetString("transformationId");
                    string channelId = paletteEntry.GetString("channelId");
                    byte r = (byte)paletteEntry.GetInt("r");
                    byte g = (byte)paletteEntry.GetInt("g");
                    byte b = (byte)paletteEntry.GetInt("b");
                    byte hue = paletteEntry.ContainsKey("hue")
                        ? (byte)paletteEntry.GetInt("hue")
                        : TransformationPaletteColorEntry.NeutralHue;
                    byte saturation = paletteEntry.ContainsKey("saturation")
                        ? (byte)paletteEntry.GetInt("saturation")
                        : TransformationPaletteColorEntry.NeutralSaturation;
                    AddNormalizedTransformationPaletteEntry(new TransformationPaletteColorEntry(
                        transformationId,
                        channelId,
                        new Color(r, g, b),
                        hue,
                        saturation
                    ));
                }
            }

            if (tag.TryGet("paletteEnabledChannels", out string[] enabledPaletteArray)) {
                for (int i = 0; i < enabledPaletteArray.Length; i++) {
                    if (TryNormalizePaletteChannelKey(enabledPaletteArray[i], out string enabledChannelKey))
                        paletteEnabledChannels.Add(enabledChannelKey);
                }
            }
            else if (tag.TryGet("paletteDisabledChannels", out string[] disabledPaletteArray)) {
                LoadLegacyPaletteDisabledChannels(disabledPaletteArray);
            }

            if (tag.TryGet("paletteDisabledTransformations", out string[] disabledTransformationArray)) {
                for (int i = 0; i < disabledTransformationArray.Length; i++)
                    SetAllPaletteChannelsEnabled(disabledTransformationArray[i], false);
            }

            if (tag.TryGet("customTransformationNames", out List<TagCompound> customNameEntries)) {
                foreach (TagCompound customNameEntry in customNameEntries) {
                    string transformationId = customNameEntry.GetString("transformationId");
                    string customName = customNameEntry.GetString("customName");
                    SetCustomTransformationName(transformationId, customName);
                }
            }

            if (tag.TryGet("transformationPalettePresets", out List<TagCompound> palettePresetEntries)) {
                foreach (TagCompound presetEntry in palettePresetEntries)
                    LoadPalettePresetTagEntry(presetEntry);
            }

            if (tag.TryGet("unlockedRoster", out oldUnlockedRoster)) {
                for (int i = 0; i < oldUnlockedRoster.Length; i++) {
                    string migratedId = MapOldTransformationId((TransformationEnumOld)oldUnlockedRoster[i]);
                    if (!string.IsNullOrEmpty(migratedId) && !unlockedTransformations.Contains(migratedId))
                        unlockedTransformations.Insert(Math.Min(i, unlockedTransformations.Count), migratedId);
                }
            }

            if (!unlockedTransformations.Contains("Ben10Mod:HeatBlast"))
                unlockedTransformations.Insert(0, "Ben10Mod:HeatBlast");

            NormalizeStoredTransformationData();
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
            transformationFailsafeEquipped = false;
            if (currentTransformationId != "Ben10Mod:Goop") {
                GoopVisualScale = Vector2.One;
                goopWasGrounded = false;
                goopPreviousVerticalVelocity = 0f;
                goopLandingSquish = 0f;
                goopLandingSplashTime = 0;
            }

            omnitrixEnergyMax = 0;
            omnitrixEnergyRegen = 0;
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

        public Vector2 GetScaledVisualPoint(Vector2 worldPoint) {
            if (CurrentTransformationScale <= 1f)
                return worldPoint;

            Vector2 pivot = Player.Bottom;
            return pivot + (worldPoint - pivot) * CurrentTransformationScale;
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
                if (!skipAutomaticForcedDetransformHandling) {
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

                skipAutomaticForcedDetransformHandling = false;
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
            trans?.UpdateActiveAbilityVisuals(Player, this);
        }

        public override void PostUpdate() {
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();
            int materialBuffType = ModContent.BuffType<MaterialAbsorptionBuff>();

            if (activeLungeTime > 0)
                activeLungeTime--;

            if (airborneLungeConsumed && activeLungeTime <= 0 && IsTouchingLungeResetSurface())
                airborneLungeConsumed = false;

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

            if (KeybindSystem.OpenTransformationCodex.JustPressed && omnitrixEquipped && unlockedTransformations.Count > 0) {
                UISystem uiSystem = ModContent.GetInstance<UISystem>();
                if (uiSystem.IsCodexUIOpen()) {
                    uiSystem.HideMyUI();
                    showingUI = false;
                }
                else {
                    uiSystem.ShowCodexUI();
                    showingUI = true;
                }
            }

            if (CanHandleBadgeRightClickSelection()) {
                Main.mouseRightRelease = false;
                HandleBadgeRightClickSelection();
            }

            if (Player.whoAmI == Main.myPlayer && KeybindSystem.AbsorbMaterial.JustPressed)
                TryAbsorbHeldMaterial();

            var trans = CurrentTransformation;
            if (trans != null)
                trans.PostUpdate(Player, this);

            if (attackSelectionPulseTime > 0)
                attackSelectionPulseTime--;
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

            UpdateProgressionTransformationUnlocks();
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

        public string GetCurrentAttackSelectionLabel() {
            return CurrentTransformation?.GetAttackSelectionLabel(setAttack, this) ?? "Primary";
        }

        public string GetCurrentAttackDisplayName() {
            return CurrentTransformation?.GetAttackSelectionDisplayName(setAttack, this) ?? "No Attack";
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

        public int GetSelectedTransformationSlotIndex() {
            Omnitrix activeOmnitrix = GetActiveOmnitrix();
            if (activeOmnitrix == null || transformationSlots.Length == 0)
                return -1;

            return Utils.Clamp(activeOmnitrix.transformationNum, 0, transformationSlots.Length - 1);
        }

        public string GetSelectedTransformationId() {
            int selectedSlot = GetSelectedTransformationSlotIndex();
            if (selectedSlot < 0 || selectedSlot >= transformationSlots.Length)
                return string.Empty;

            return transformationSlots[selectedSlot] ?? string.Empty;
        }

        public string GetSelectedTransformationHudLabel() {
            int selectedSlot = GetSelectedTransformationSlotIndex();
            return selectedSlot >= 0 ? $"Slot {selectedSlot + 1}" : "No Slot";
        }

        public string GetSelectedTransformationDisplayName() {
            string transformationId = GetSelectedTransformationId();
            if (string.IsNullOrEmpty(transformationId))
                return "Empty Slot";

            Transformation selectedTransformation = TransformationLoader.Get(transformationId);
            return selectedTransformation?.GetDisplayName(this) ?? "Unknown Form";
        }

        public Color GetSelectedTransformationAccentColor() {
            string selectedTransformationId = GetSelectedTransformationId();
            if (string.IsNullOrEmpty(selectedTransformationId))
                return new Color(160, 170, 185);

            return IsFavoriteTransformation(selectedTransformationId)
                ? new Color(232, 205, 110)
                : new Color(120, 255, 170);
        }

        public bool HasPaletteCustomizationData(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            if (transformationPaletteOverrides.TryGetValue(transformation.FullID,
                    out Dictionary<string, TransformationPaletteChannelSettings> overrides) &&
                overrides != null && overrides.Count > 0) {
                return true;
            }

            IReadOnlyList<TransformationPaletteChannel> channels = transformation.GetPaletteChannels(this);
            for (int i = 0; i < channels.Count; i++) {
                TransformationPaletteChannel channel = channels[i];
                if (channel == null || !channel.IsValid)
                    continue;

                if (!IsPaletteChannelEnabled(transformation, channel.Id))
                    return true;
            }

            return false;
        }

        public bool HasPaletteCustomizationData(Transformation transformation) {
            return transformation != null && HasPaletteCustomizationData(transformation.FullID);
        }

        public bool CanAffordCurrentAttackForHud() {
            return CurrentTransformation?.CanAffordCurrentAttack(this) ?? true;
        }

        public string GetCurrentTransformationPaletteStatusText() {
            return BuildPaletteStatusText(CurrentTransformation);
        }

        public string GetSelectedTransformationPaletteStatusText() {
            return BuildPaletteStatusText(TransformationLoader.Resolve(GetSelectedTransformationId()));
        }

        private string BuildPaletteStatusText(Transformation transformation) {
            if (transformation == null)
                return string.Empty;

            if (!transformation.SupportsPaletteCustomization(this))
                return "Palette: None";

            IReadOnlyList<TransformationPaletteChannel> channels = transformation.GetPaletteChannels(this);
            int enabledCount = 0;
            int validChannelCount = 0;
            for (int i = 0; i < channels.Count; i++) {
                TransformationPaletteChannel channel = channels[i];
                if (channel == null || !channel.IsValid)
                    continue;

                validChannelCount++;
                if (IsPaletteChannelEnabled(transformation, channel.Id))
                    enabledCount++;
            }

            if (validChannelCount == 0)
                return "Palette: None";

            if (!HasPaletteCustomizationData(transformation))
                return "Palette: Default";

            if (enabledCount <= 0)
                return "Palette: Original";

            if (enabledCount >= validChannelCount)
                return "Palette: Custom";

            return "Palette: Mixed";
        }

        public Transformation GetPaletteTargetTransformation() {
            if (IsTransformed)
                return CurrentTransformation;

            if (GetActiveOmnitrix() == null)
                return null;

            return TransformationLoader.Get(GetSelectedTransformationId());
        }

        public string GetPaletteTargetTransformationId() {
            return GetPaletteTargetTransformation()?.FullID ?? string.Empty;
        }

        public string GetPaletteTargetDisplayName() {
            return GetPaletteTargetTransformation()?.GetDisplayName(this) ?? "No Transformation";
        }

        public string GetCustomTransformationName(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return string.Empty;

            return customTransformationNames.TryGetValue(transformation.FullID, out string customName)
                ? customName
                : string.Empty;
        }

        public string GetCustomTransformationName(Transformation transformation) {
            return transformation == null ? string.Empty : GetCustomTransformationName(transformation.FullID);
        }

        public string GetTransformationBaseName(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            return GetTransformationBaseName(transformation);
        }

        public string GetTransformationBaseName(Transformation transformation) {
            if (transformation == null)
                return "Unknown Form";

            string customName = GetCustomTransformationName(transformation.FullID);
            return string.IsNullOrWhiteSpace(customName) ? transformation.TransformationName : customName;
        }

        public bool IsFavoriteTransformation(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            return transformation != null && favoriteTransformations.Contains(transformation.FullID);
        }

        public bool IsFavoriteTransformation(Transformation transformation) {
            return transformation != null && favoriteTransformations.Contains(transformation.FullID);
        }

        public bool SetFavoriteTransformation(string transformationId, bool isFavorite) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            if (isFavorite)
                return favoriteTransformations.Add(transformation.FullID);

            return favoriteTransformations.Remove(transformation.FullID);
        }

        public bool ToggleFavoriteTransformation(string transformationId) {
            return SetFavoriteTransformation(transformationId, !IsFavoriteTransformation(transformationId));
        }

        public bool IsNewlyUnlockedTransformation(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            return transformation != null && newlyUnlockedTransformations.Contains(transformation.FullID);
        }

        public bool IsNewlyUnlockedTransformation(Transformation transformation) {
            return transformation != null && newlyUnlockedTransformations.Contains(transformation.FullID);
        }

        public bool MarkTransformationAsSeen(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            return transformation != null && newlyUnlockedTransformations.Remove(transformation.FullID);
        }

        public bool HasAnyNewlyUnlockedTransformations() => newlyUnlockedTransformations.Count > 0;

        public bool IsTransformationUnlocked(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            return transformation != null && unlockedTransformations.Contains(transformation.FullID);
        }

        public bool IsTransformationUnlocked(Transformation transformation) {
            return transformation != null && unlockedTransformations.Contains(transformation.FullID);
        }

        public IReadOnlyList<string> GetTransformationsForCodexDisplay() {
            List<string> displayTransformations = new();
            HashSet<string> seenTransformations = new(StringComparer.OrdinalIgnoreCase);

            foreach (Transformation transformation in TransformationLoader.All) {
                if (transformation == null)
                    continue;

                if (seenTransformations.Add(transformation.FullID))
                    displayTransformations.Add(transformation.FullID);
            }

            displayTransformations.Sort(CompareCodexTransformationDisplayOrder);
            return displayTransformations;
        }

        public string GetTransformationUnlockConditionText(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            return GetTransformationUnlockConditionText(transformation);
        }

        public string GetTransformationUnlockConditionText(Transformation transformation) {
            if (transformation == null)
                return "Unlock condition not available.";

            string unlockConditionText = transformation.GetUnlockConditionText(this);
            return string.IsNullOrWhiteSpace(unlockConditionText)
                ? "Unlock condition not yet documented in the codex."
                : unlockConditionText;
        }

        public IReadOnlyList<string> GetUnlockedTransformationsForDisplay() {
            List<string> displayTransformations = new();
            HashSet<string> seenTransformations = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < unlockedTransformations.Count; i++) {
                string transformationId = unlockedTransformations[i];
                Transformation transformation = TransformationLoader.Resolve(transformationId);
                if (transformation == null)
                    continue;

                string resolvedTransformationId = transformation.FullID;
                if (seenTransformations.Add(resolvedTransformationId))
                    displayTransformations.Add(resolvedTransformationId);
            }

            displayTransformations.Sort(CompareUnlockedTransformationDisplayOrder);
            return displayTransformations;
        }

        private int CompareCodexTransformationDisplayOrder(string leftTransformationId, string rightTransformationId) {
            bool leftIsUnlocked = IsTransformationUnlocked(leftTransformationId);
            bool rightIsUnlocked = IsTransformationUnlocked(rightTransformationId);
            if (leftIsUnlocked != rightIsUnlocked)
                return leftIsUnlocked ? -1 : 1;

            bool leftIsFavorite = leftIsUnlocked && IsFavoriteTransformation(leftTransformationId);
            bool rightIsFavorite = rightIsUnlocked && IsFavoriteTransformation(rightTransformationId);
            if (leftIsFavorite != rightIsFavorite)
                return leftIsFavorite ? -1 : 1;

            return CompareTransformationDisplayName(leftTransformationId, rightTransformationId);
        }

        private int CompareUnlockedTransformationDisplayOrder(string leftTransformationId, string rightTransformationId) {
            bool leftIsFavorite = IsFavoriteTransformation(leftTransformationId);
            bool rightIsFavorite = IsFavoriteTransformation(rightTransformationId);
            if (leftIsFavorite != rightIsFavorite)
                return leftIsFavorite ? -1 : 1;

            return CompareTransformationDisplayName(leftTransformationId, rightTransformationId);
        }

        private int CompareTransformationDisplayName(string leftTransformationId, string rightTransformationId) {
            Transformation leftTransformation = TransformationLoader.Resolve(leftTransformationId);
            Transformation rightTransformation = TransformationLoader.Resolve(rightTransformationId);
            string leftName = leftTransformation?.GetDisplayName(this) ?? string.Empty;
            string rightName = rightTransformation?.GetDisplayName(this) ?? string.Empty;
            int nameComparison = string.Compare(leftName, rightName, StringComparison.CurrentCultureIgnoreCase);
            if (nameComparison != 0)
                return nameComparison;

            string leftKey = leftTransformation?.FullID ?? leftTransformationId ?? string.Empty;
            string rightKey = rightTransformation?.FullID ?? rightTransformationId ?? string.Empty;
            return string.Compare(leftKey, rightKey, StringComparison.OrdinalIgnoreCase);
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

        public bool SetCustomTransformationName(string transformationId, string customName) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            string normalizedName = NormalizeCustomTransformationName(customName);
            if (string.IsNullOrWhiteSpace(normalizedName))
                return customTransformationNames.Remove(transformation.FullID);

            if (customTransformationNames.TryGetValue(transformation.FullID, out string existingName) &&
                string.Equals(existingName, normalizedName, StringComparison.Ordinal))
                return false;

            customTransformationNames[transformation.FullID] = normalizedName;
            return true;
        }

        public bool IsPaletteChannelEnabled(Transformation transformation, string channelId) {
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return false;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null || !channel.IsValid)
                return false;

            return paletteEnabledChannels.Contains(BuildPaletteChannelKey(transformation.FullID, channel.Id));
        }

        public bool IsPaletteChannelEnabled(string transformationId, string channelId) {
            return IsPaletteChannelEnabled(TransformationLoader.Resolve(transformationId), channelId);
        }

        public bool SetPaletteChannelEnabled(string transformationId, string channelId, bool enabled, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return false;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null || !channel.IsValid)
                return false;

            string key = BuildPaletteChannelKey(transformation.FullID, channel.Id);
            bool changed = enabled
                ? paletteEnabledChannels.Add(key)
                : paletteEnabledChannels.Remove(key);

            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public TransformationPaletteChannelSettings GetPaletteSettings(Transformation transformation, string channelId) {
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return new TransformationPaletteChannelSettings(Color.White);

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null)
                return new TransformationPaletteChannelSettings(Color.White);

            if (transformationPaletteOverrides.TryGetValue(transformation.FullID,
                    out Dictionary<string, TransformationPaletteChannelSettings> channelSettings) &&
                channelSettings.TryGetValue(channel.Id, out TransformationPaletteChannelSettings storedSettings)) {
                return NormalizePaletteSettings(storedSettings);
            }

            return new TransformationPaletteChannelSettings(channel.DefaultColor);
        }

        public Color GetPaletteColor(Transformation transformation, string channelId) {
            return GetPaletteSettings(transformation, channelId).Color;
        }

        public byte GetPaletteHue(string transformationId, string channelId) {
            return GetPaletteSettings(TransformationLoader.Resolve(transformationId), channelId).Hue;
        }

        public byte GetPaletteSaturation(string transformationId, string channelId) {
            return GetPaletteSettings(TransformationLoader.Resolve(transformationId), channelId).Saturation;
        }

        public bool SetPaletteColor(string transformationId, string channelId, Color color, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return false;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null || !channel.IsValid)
                return false;

            TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(transformation, channel.Id);
            TransformationPaletteChannelSettings newSettings = new(color, currentSettings.Hue, currentSettings.Saturation);
            bool changed = SetPaletteSettings(transformation.FullID, channel, newSettings);

            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool ResetPaletteColor(string transformationId, string channelId, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return false;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null)
                return false;

            TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(transformation, channel.Id);
            TransformationPaletteChannelSettings newSettings = new(channel.DefaultColor, currentSettings.Hue,
                currentSettings.Saturation);
            bool changed = SetPaletteSettings(transformation.FullID, channel, newSettings);
            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool SetPaletteHue(string transformationId, string channelId, byte hue, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return false;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null || !channel.IsValid)
                return false;

            TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(transformation, channel.Id);
            bool changed = SetPaletteSettings(transformation.FullID, channel,
                new TransformationPaletteChannelSettings(currentSettings.Color, hue, currentSettings.Saturation));
            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool SetPaletteSaturation(string transformationId, string channelId, byte saturation, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return false;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null || !channel.IsValid)
                return false;

            TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(transformation, channel.Id);
            bool changed = SetPaletteSettings(transformation.FullID, channel,
                new TransformationPaletteChannelSettings(currentSettings.Color, currentSettings.Hue, saturation));
            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool ResetPaletteColors(string transformationId, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            bool changed = transformationPaletteOverrides.Remove(transformation.FullID);
            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool HasPalettePreset(string transformationId, int presetIndex) {
            return TryGetPalettePreset(transformationId, presetIndex, out _);
        }

        public string GetPalettePresetLabel(string transformationId, int presetIndex) {
            int displayIndex = Utils.Clamp(presetIndex, 0, PalettePresetSlotCount - 1) + 1;
            return HasPalettePreset(transformationId, presetIndex)
                ? $"Preset {displayIndex}"
                : $"Preset {displayIndex} (Empty)";
        }

        public bool SavePalettePreset(string transformationId, int presetIndex) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || presetIndex < 0 || presetIndex >= PalettePresetSlotCount)
                return false;

            if (!palettePresets.TryGetValue(transformation.FullID, out PalettePresetData[] presets) || presets == null ||
                presets.Length != PalettePresetSlotCount) {
                presets = new PalettePresetData[PalettePresetSlotCount];
                palettePresets[transformation.FullID] = presets;
            }

            PalettePresetData preset = new();
            List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntriesFor(transformation.FullID);
            for (int i = 0; i < entries.Count; i++)
                preset.Entries.Add(entries[i]);

            List<string> enabledKeys = BuildNormalizedPaletteEnabledChannelKeysFor(transformation.FullID);
            for (int i = 0; i < enabledKeys.Count; i++)
                preset.EnabledChannelKeys.Add(enabledKeys[i]);

            presets[presetIndex] = preset;
            return true;
        }

        public bool ApplyPalettePreset(string transformationId, int presetIndex, bool sync = true) {
            if (!TryGetPalettePreset(transformationId, presetIndex, out PalettePresetData preset))
                return false;

            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            transformationPaletteOverrides.Remove(transformation.FullID);
            SetAllPaletteChannelsEnabled(transformation.FullID, false);

            for (int i = 0; i < preset.Entries.Count; i++)
                AddNormalizedTransformationPaletteEntry(preset.Entries[i]);

            for (int i = 0; i < preset.EnabledChannelKeys.Count; i++) {
                if (TryNormalizePaletteChannelKey(preset.EnabledChannelKeys[i], out string normalizedKey))
                    paletteEnabledChannels.Add(normalizedKey);
            }

            NormalizeTransformationPaletteState();
            if (sync)
                SyncTransformationPaletteStateToServerOrClients();

            return true;
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

        private static bool IsAbilityAttackSelection(AttackSelection selection) {
            return selection is AttackSelection.PrimaryAbility or AttackSelection.SecondaryAbility or AttackSelection.TertiaryAbility;
        }

        private void TriggerAttackSelectionPulse() {
            attackSelectionPulseTime = AttackSelectionPulseDuration;
        }

        public override bool CanUseItem(Item item) {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return trans?.CanUseItem(Player, this, item) ?? true;
        }

        public override void UpdateDead() {
            ResetAirborneLungeState();
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

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust,
            ref PlayerDeathReason damageSource) {
            if (!transformationFailsafeEquipped || !IsTransformed)
                return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);

            TriggerTransformationFailsafe();
            playSound = false;
            genDust = false;
            return false;
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
            if (trans != null && trans.TryGetTransformationTint(Player, this, out Color transformationTint,
                    out float tintBlendStrength, out bool forceTransformationFullBright)) {
                Vector3 vividTint = Color.Lerp(transformationTint, Color.White, 0.16f).ToVector3();
                float safeBlendStrength = MathHelper.Clamp(tintBlendStrength, 0f, 1f);
                r = MathHelper.Lerp(r, MathHelper.Clamp(vividTint.X * 1.65f, 0f, 1f), safeBlendStrength);
                g = MathHelper.Lerp(g, MathHelper.Clamp(vividTint.Y * 1.65f, 0f, 1f), safeBlendStrength);
                b = MathHelper.Lerp(b, MathHelper.Clamp(vividTint.Z * 1.65f, 0f, 1f), safeBlendStrength);
                a = 1f;
                if (forceTransformationFullBright)
                    fullBright = true;
            }

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

        private bool IsTouchingLungeResetSurface() {
            return Player.velocity.Y >= 0f &&
                   Collision.SolidCollision(Player.position + new Vector2(0f, Player.height - 2f), Player.width, 8);
        }

        private bool IsRestrictedLungeProjectile(int projectileType) {
            return projectileType == ModContent.ProjectileType<RathPounceProjectile>() ||
                   projectileType == ModContent.ProjectileType<XLR8DashProjectile>() ||
                   projectileType == ModContent.ProjectileType<XLR8VectorDashProjectile>() ||
                   projectileType == ModContent.ProjectileType<RipJawsBiteProjectile>() ||
                   projectileType == ModContent.ProjectileType<JetrayDiveProjectile>() ||
                   projectileType == ModContent.ProjectileType<BigChillPhaseStrikeProjectile>() ||
                   projectileType == ModContent.ProjectileType<CannonboltRollProjectile>() ||
                   projectileType == ModContent.ProjectileType<GoopDelugeProjectile>();
        }

        private bool CanIgnoreAirborneLungeLimit(int projectileType) {
            return projectileType == ModContent.ProjectileType<RipJawsBiteProjectile>() && Player.wet;
        }

        private void ResetAirborneLungeState() {
            airborneLungeConsumed = false;
            activeLungeTime = 0;
        }

        public bool CanUseLungeAttack(int projectileType) {
            if (!IsRestrictedLungeProjectile(projectileType) || CanIgnoreAirborneLungeLimit(projectileType))
                return true;

            return !airborneLungeConsumed || (activeLungeTime <= 0 && IsTouchingLungeResetSurface());
        }

        public bool TryConsumeLungeAttack(int projectileType) {
            if (!IsRestrictedLungeProjectile(projectileType) || CanIgnoreAirborneLungeLimit(projectileType))
                return true;

            if (!CanUseLungeAttack(projectileType))
                return false;

            airborneLungeConsumed = true;
            activeLungeTime = Math.Max(activeLungeTime, 2);
            return true;
        }

        public void RegisterActiveLunge() {
            activeLungeTime = Math.Max(activeLungeTime, 2);
            Player.armorEffectDrawShadow = true;
        }

        public override void OnEnterWorld() {
            ModContent.GetInstance<UISystem>().HideMyUI();
            ResetAirborneLungeState();
            if (!isTransformed)
                currentTransformationId = "";

            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer) {
                SyncTransformationStateToServer();
                SyncTransformationPaletteStateToServer();
            }

            CurrentTransformation?.OnEnterWorld(Player, this);
        }

        public void SyncTransformationStateToServer() {
            if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI != Main.myPlayer)
                return;

            NormalizeStoredTransformationData();
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.RequestSyncTransformationState);
            packet.Write((byte)transformationSlots.Length);
            for (int i = 0; i < transformationSlots.Length; i++)
                packet.Write(transformationSlots[i] ?? string.Empty);

            packet.Write((ushort)unlockedTransformations.Count);
            for (int i = 0; i < unlockedTransformations.Count; i++)
                packet.Write(unlockedTransformations[i] ?? string.Empty);

            packet.Send();
        }

        public void ApplyTransformationStateSync(string[] slots, string[] unlocked) {
            unlockedTransformations.Clear();
            if (unlocked != null)
                unlockedTransformations.AddRange(unlocked);

            transformationSlots = slots ?? Array.Empty<string>();
            NormalizeStoredTransformationData();
        }

        public void SyncTransformationPaletteStateToServer() {
            if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI != Main.myPlayer)
                return;

            List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntries();
            List<string> enabledChannelKeys = BuildNormalizedPaletteEnabledChannelKeys();
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.RequestSyncTransformationPaletteState);
            WriteTransformationPaletteEntries(packet, entries);
            WritePaletteChannelKeys(packet, enabledChannelKeys);
            packet.Send();
        }

        public void ApplyTransformationPaletteStateSync(IReadOnlyList<TransformationPaletteColorEntry> entries,
            IReadOnlyList<string> enabledChannelKeys = null) {
            transformationPaletteOverrides.Clear();
            paletteEnabledChannels.Clear();

            if (entries != null) {
                for (int i = 0; i < entries.Count; i++)
                    AddNormalizedTransformationPaletteEntry(entries[i]);
            }

            if (enabledChannelKeys == null)
                return;

            for (int i = 0; i < enabledChannelKeys.Count; i++) {
                if (TryNormalizePaletteChannelKey(enabledChannelKeys[i], out string normalizedKey))
                    paletteEnabledChannels.Add(normalizedKey);
            }
        }

        public void BeginPossession(int targetIndex, Vector2 returnPosition, int duration = PossessionDuration,
            bool shouldSync = true, bool playEffects = true) {
            if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
                return;

            NPC target = Main.npc[targetIndex];
            if (target == null || !target.active || target.life <= 0)
                return;

            bool sameTarget = inPossessionMode && possessedTargetIndex == targetIndex;

            prePossessionPosition = returnPosition;
            possessedTargetIndex = targetIndex;
            possessionTimer = Math.Max(1, duration);
            inPossessionMode = true;
            Player.invis = true;
            Player.Center = target.Center;
            Player.velocity = target.velocity * 0.8f;

            if (!sameTarget && playEffects && Main.netMode != NetmodeID.Server) {
                SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.5f, Volume = 0.8f }, Player.Center);
                for (int i = 0; i < 40; i++) {
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                        Main.rand.NextVector2Circular(8f, 8f), Scale: 2f);
                    d.noGravity = true;
                }
            }

            if (shouldSync && Main.netMode == NetmodeID.Server)
                SyncPossessionState();
        }

        public void ApplyPossessionStateSync(bool active, int targetIndex, Vector2 returnPosition, int timer) {
            prePossessionPosition = returnPosition;
            if (active) {
                BeginPossession(targetIndex, returnPosition, timer, shouldSync: false);
                return;
            }

            EndPossession(shouldSync: false);
        }

        private void EndPossession(bool shouldSync = true, bool playEffects = true) {
            if (!inPossessionMode && possessedTargetIndex < 0)
                return;

            inPossessionMode = false;
            possessedTargetIndex = -1;
            possessionTimer = 0;

            Player.position = prePossessionPosition;
            Player.velocity = Vector2.Zero;
            Player.invis = false;
            Player.immune = true;
            Player.immuneNoBlink = true;
            Player.immuneTime = 60;

            if (playEffects && Main.netMode != NetmodeID.Server) {
                SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.3f, Volume = 0.8f }, Player.Center);
                for (int i = 0; i < 30; i++) {
                    Dust d = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(6f, 6f),
                        Scale: 1.8f);
                    d.noGravity = true;
                }
            }

            if (shouldSync && Main.netMode == NetmodeID.Server)
                SyncPossessionState();
        }

        private void SyncPossessionState() {
            if (Main.netMode != NetmodeID.Server)
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.SyncGhostFreakPossessionState);
            packet.Write((byte)Player.whoAmI);
            packet.Write(inPossessionMode);
            packet.Write(possessedTargetIndex);
            packet.Write(prePossessionPosition.X);
            packet.Write(prePossessionPosition.Y);
            packet.Write(possessionTimer);
            packet.Send();
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
                    ShowAbsorptionFeedback("You need an Osmosian Harness equipped to absorb materials.", new Color(230, 120, 120));
                return;
            }

            if (omnitrixEquipped) {
                if (Player.whoAmI == Main.myPlayer)
                    ShowAbsorptionFeedback("Osmosian absorption cannot be used while an Omnitrix is equipped.", new Color(230, 120, 120));
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
                    ShowAbsorptionFeedback("That material cannot be absorbed.", new Color(230, 120, 120));
                return;
            }

            int consumeAmount = Math.Max(1, (int)Math.Round(profile.ConsumeAmount * absorptionCostMultiplier));
            int durationTicks = Math.Max(60, (int)Math.Round(profile.DurationTicks * absorptionDurationMultiplier));

            if (heldItem.stack < consumeAmount) {
                ShowAbsorptionFeedback($"You need {consumeAmount} {profile.DisplayName} to absorb it.", new Color(255, 210, 110));
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
            if (Main.netMode == NetmodeID.Server) {
                SyncTransformationPaletteState(toWho, fromWho);
                SyncAbsorbedMaterial(showEffects: false, toWho: toWho, ignoreClient: fromWho);
            }
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

        private void ShowAbsorptionFeedback(string message, Color color) {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (Main.netMode == NetmodeID.Server) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.AbsorbMaterialFeedback);
                packet.Write(message);
                packet.Write(color.R);
                packet.Write(color.G);
                packet.Write(color.B);
                packet.Send(Player.whoAmI);
                return;
            }

            if (Player.whoAmI == Main.myPlayer)
                Main.NewText(message, color);
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
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            string canonicalTransformationId = transformation.FullID;
            if (unlockedTransformations.Contains(canonicalTransformationId))
                return false;

            unlockedTransformations.Add(canonicalTransformationId);
            newlyUnlockedTransformations.Add(canonicalTransformationId);
            NormalizeStoredTransformationData();

            if (showEffects)
                ShowTransformationUnlockFeedback(transformation);

            if (sync && Main.netMode == NetmodeID.Server) {
                SyncTransformationState(toWho: Player.whoAmI);
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.UnlockTransformation);
                packet.Write((byte)Player.whoAmI);
                packet.Write(canonicalTransformationId);
                packet.Send(toClient: Player.whoAmI);
            }

            return true;
        }

        internal void ShowTransformationUnlockFeedback(string transformationId) {
            ShowTransformationUnlockFeedback(TransformationLoader.Resolve(transformationId));
        }

        private void ShowTransformationUnlockFeedback(Transformation transformation) {
            if (transformation == null || Main.netMode == NetmodeID.Server || Player.whoAmI != Main.myPlayer)
                return;

            string name = GetTransformationBaseName(transformation);
            Main.NewText($"{name} has been unlocked!", Color.LimeGreen);
            CombatText.NewText(Player.getRect(), Color.LimeGreen, $"{name}!", dramatic: true);
        }

        public bool RemoveTransformation(string transformationId, bool sync = true, bool showEffects = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            string canonicalTransformationId = transformation.FullID;
            if (!unlockedTransformations.Contains(canonicalTransformationId))
                return false;

            bool shouldDetransform = currentTransformationId == canonicalTransformationId;
            unlockedTransformations.Remove(canonicalTransformationId);
            newlyUnlockedTransformations.Remove(canonicalTransformationId);

            for (int i = 0; i < transformationSlots.Length; i++) {
                if (transformationSlots[i] == canonicalTransformationId)
                    transformationSlots[i] = "";
            }

            if (shouldDetransform)
                TransformationHandler.Detransform(Player, 0, showParticles: showEffects, addCooldown: false, playSound: showEffects);

            NormalizeStoredTransformationData();

            if (showEffects && Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer) {
                string name = GetTransformationBaseName(transformation);
                Main.NewText($"{name} has been removed.", Color.OrangeRed);
            }

            if (sync && Main.netMode == NetmodeID.Server) {
                SyncTransformationState(toWho: Player.whoAmI);
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.RemoveTransformation);
                packet.Write((byte)Player.whoAmI);
                packet.Write(canonicalTransformationId);
                packet.Send(toClient: Player.whoAmI);
            }

            return true;
        }

        internal void SyncTransformationState(int toWho = -1, int ignoreClient = -1) {
            if (Main.netMode != NetmodeID.Server)
                return;

            NormalizeStoredTransformationData();
            string[] normalizedSlots = (string[])transformationSlots.Clone();

            if (!Main.dedServ && Player.whoAmI == Main.myPlayer) {
                Player localPlayer = Main.LocalPlayer;
                if (localPlayer != null && localPlayer.active) {
                    OmnitrixPlayer localOmnitrixPlayer = localPlayer.GetModPlayer<OmnitrixPlayer>();
                    if (!ReferenceEquals(localOmnitrixPlayer, this))
                        localOmnitrixPlayer.ApplyTransformationStateSync((string[])normalizedSlots.Clone(), unlockedTransformations.ToArray());
                }
            }

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.SyncTransformationState);
            packet.Write((byte)Player.whoAmI);
            packet.Write((byte)normalizedSlots.Length);
            for (int i = 0; i < normalizedSlots.Length; i++)
                packet.Write(normalizedSlots[i] ?? string.Empty);

            packet.Write((ushort)unlockedTransformations.Count);
            for (int i = 0; i < unlockedTransformations.Count; i++)
                packet.Write(unlockedTransformations[i] ?? string.Empty);

            packet.Send(toWho, ignoreClient);
        }

        internal void SyncTransformationPaletteState(int toWho = -1, int ignoreClient = -1) {
            if (Main.netMode != NetmodeID.Server)
                return;

            List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntries();
            List<string> enabledChannelKeys = BuildNormalizedPaletteEnabledChannelKeys();

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.SyncTransformationPaletteState);
            packet.Write((byte)Player.whoAmI);
            WriteTransformationPaletteEntries(packet, entries);
            WritePaletteChannelKeys(packet, enabledChannelKeys);
            packet.Send(toWho, ignoreClient);
        }

        internal void SyncTransformationPaletteStateToServerOrClients() {
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer) {
                SyncTransformationPaletteStateToServer();
                return;
            }

            if (Main.netMode == NetmodeID.Server)
                SyncTransformationPaletteState();
        }

        private void NormalizeTransformationPaletteState() {
            List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntries();
            List<string> enabledChannelKeys = BuildNormalizedPaletteEnabledChannelKeys();
            transformationPaletteOverrides.Clear();
            paletteEnabledChannels.Clear();

            for (int i = 0; i < entries.Count; i++)
                AddNormalizedTransformationPaletteEntry(entries[i]);

            for (int i = 0; i < enabledChannelKeys.Count; i++)
                paletteEnabledChannels.Add(enabledChannelKeys[i]);
        }

        private List<TransformationPaletteColorEntry> BuildNormalizedTransformationPaletteEntries() {
            List<TransformationPaletteColorEntry> entries = new();

            foreach ((string transformationId, Dictionary<string, TransformationPaletteChannelSettings> channelSettings) in transformationPaletteOverrides) {
                Transformation transformation = TransformationLoader.Resolve(transformationId);
                if (transformation == null || channelSettings == null)
                    continue;

                foreach ((string channelId, TransformationPaletteChannelSettings settings) in channelSettings) {
                    TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
                    if (channel == null || !channel.IsValid)
                        continue;

                    TransformationPaletteChannelSettings normalizedSettings =
                        NormalizePaletteSettings(settings, channel.DefaultColor);
                    if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
                        continue;

                    entries.Add(new TransformationPaletteColorEntry(transformation.FullID, channel.Id,
                        normalizedSettings.Color, normalizedSettings.Hue, normalizedSettings.Saturation));
                }
            }

            entries.Sort(static (left, right) => {
                int transformationCompare = string.Compare(left.TransformationId, right.TransformationId,
                    StringComparison.OrdinalIgnoreCase);
                return transformationCompare != 0
                    ? transformationCompare
                    : string.Compare(left.ChannelId, right.ChannelId, StringComparison.OrdinalIgnoreCase);
            });

            return entries;
        }

        private List<TransformationPaletteColorEntry> BuildNormalizedTransformationPaletteEntriesFor(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return new List<TransformationPaletteColorEntry>();

            List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntries();
            entries.RemoveAll(entry => !string.Equals(entry.TransformationId, transformation.FullID, StringComparison.OrdinalIgnoreCase));
            return entries;
        }

        private List<string> BuildNormalizedPaletteEnabledChannelKeys() {
            List<string> enabledChannelKeys = new();

            foreach (string enabledChannelKey in paletteEnabledChannels) {
                if (TryNormalizePaletteChannelKey(enabledChannelKey, out string normalizedKey))
                    enabledChannelKeys.Add(normalizedKey);
            }

            enabledChannelKeys.Sort(StringComparer.OrdinalIgnoreCase);
            return enabledChannelKeys;
        }

        private List<string> BuildNormalizedPaletteEnabledChannelKeysFor(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return new List<string>();

            List<string> enabledChannelKeys = BuildNormalizedPaletteEnabledChannelKeys();
            enabledChannelKeys.RemoveAll(key => !key.StartsWith(transformation.FullID + "|", StringComparison.OrdinalIgnoreCase));
            return enabledChannelKeys;
        }

        private IEnumerable<TagCompound> BuildPalettePresetTagEntries() {
            NormalizePalettePresets();

            foreach ((string transformationId, PalettePresetData[] presets) in palettePresets) {
                if (presets == null)
                    continue;

                for (int presetIndex = 0; presetIndex < presets.Length; presetIndex++) {
                    PalettePresetData preset = presets[presetIndex];
                    if (preset == null)
                        continue;

                    List<TagCompound> entryTags = new();
                    for (int i = 0; i < preset.Entries.Count; i++) {
                        TransformationPaletteColorEntry entry = preset.Entries[i];
                        entryTags.Add(new TagCompound {
                            ["transformationId"] = entry.TransformationId,
                            ["channelId"] = entry.ChannelId,
                            ["r"] = (int)entry.Color.R,
                            ["g"] = (int)entry.Color.G,
                            ["b"] = (int)entry.Color.B,
                            ["hue"] = (int)entry.Hue,
                            ["saturation"] = (int)entry.Saturation
                        });
                    }

                    yield return new TagCompound {
                        ["transformationId"] = transformationId,
                        ["presetIndex"] = presetIndex,
                        ["entries"] = entryTags,
                        ["enabledChannels"] = preset.EnabledChannelKeys.ToArray()
                    };
                }
            }
        }

        private void LoadPalettePresetTagEntry(TagCompound presetEntry) {
            if (presetEntry == null)
                return;

            string transformationId = presetEntry.GetString("transformationId");
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            int presetIndex = presetEntry.GetInt("presetIndex");
            if (transformation == null || presetIndex < 0 || presetIndex >= PalettePresetSlotCount)
                return;

            PalettePresetData preset = new();
            if (presetEntry.TryGet("entries", out List<TagCompound> entryTags)) {
                for (int i = 0; i < entryTags.Count; i++) {
                    TagCompound entryTag = entryTags[i];
                    string entryTransformationId = entryTag.GetString("transformationId");
                    string channelId = entryTag.GetString("channelId");
                    byte r = (byte)entryTag.GetInt("r");
                    byte g = (byte)entryTag.GetInt("g");
                    byte b = (byte)entryTag.GetInt("b");
                    byte hue = entryTag.ContainsKey("hue")
                        ? (byte)entryTag.GetInt("hue")
                        : TransformationPaletteColorEntry.NeutralHue;
                    byte saturation = entryTag.ContainsKey("saturation")
                        ? (byte)entryTag.GetInt("saturation")
                        : TransformationPaletteColorEntry.NeutralSaturation;
                    preset.Entries.Add(new TransformationPaletteColorEntry(entryTransformationId, channelId,
                        new Color(r, g, b), hue, saturation));
                }
            }

            if (presetEntry.TryGet("enabledChannels", out string[] enabledChannels)) {
                for (int i = 0; i < enabledChannels.Length; i++)
                    preset.EnabledChannelKeys.Add(enabledChannels[i] ?? string.Empty);
            }

            if (!palettePresets.TryGetValue(transformation.FullID, out PalettePresetData[] presets) || presets == null ||
                presets.Length != PalettePresetSlotCount) {
                presets = new PalettePresetData[PalettePresetSlotCount];
                palettePresets[transformation.FullID] = presets;
            }

            presets[presetIndex] = preset;
        }

        private bool TryGetPalettePreset(string transformationId, int presetIndex, out PalettePresetData preset) {
            preset = null;
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || presetIndex < 0 || presetIndex >= PalettePresetSlotCount)
                return false;

            return palettePresets.TryGetValue(transformation.FullID, out PalettePresetData[] presets) &&
                   presets != null &&
                   presetIndex < presets.Length &&
                   (preset = presets[presetIndex]) != null;
        }

        private void SetAllPaletteChannelsEnabled(string transformationId, bool enabled) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return;

            IReadOnlyList<TransformationPaletteChannel> channels = transformation.GetPaletteChannels(this);
            for (int i = 0; i < channels.Count; i++) {
                TransformationPaletteChannel channel = channels[i];
                if (channel == null || !channel.IsValid)
                    continue;

                string key = BuildPaletteChannelKey(transformation.FullID, channel.Id);
                if (enabled)
                    paletteEnabledChannels.Add(key);
                else
                    paletteEnabledChannels.Remove(key);
            }
        }

        private bool TryNormalizePaletteChannelKey(string key, out string normalizedKey) {
            normalizedKey = string.Empty;
            if (!TrySplitPaletteChannelKey(key, out string transformationId, out string channelId))
                return false;

            if (!TryResolvePaletteChannel(transformationId, channelId, out Transformation transformation,
                    out TransformationPaletteChannel channel))
                return false;

            normalizedKey = BuildPaletteChannelKey(transformation.FullID, channel.Id);
            return true;
        }

        private bool TryResolvePaletteChannel(string transformationId, string channelId,
            out Transformation transformation, out TransformationPaletteChannel channel) {
            transformation = null;
            channel = null;

            if (string.IsNullOrWhiteSpace(transformationId) || string.IsNullOrWhiteSpace(channelId))
                return false;

            transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            channel = transformation.GetPaletteChannel(channelId, this);
            return channel != null && channel.IsValid;
        }

        private static string BuildPaletteChannelKey(string transformationId, string channelId) {
            if (string.IsNullOrWhiteSpace(transformationId) || string.IsNullOrWhiteSpace(channelId))
                return string.Empty;

            return transformationId.Trim() + "|" + channelId.Trim();
        }

        private static bool TrySplitPaletteChannelKey(string key, out string transformationId,
            out string channelId) {
            transformationId = string.Empty;
            channelId = string.Empty;

            if (string.IsNullOrWhiteSpace(key))
                return false;

            int separatorIndex = key.IndexOf('|');
            if (separatorIndex <= 0 || separatorIndex >= key.Length - 1)
                return false;

            transformationId = key[..separatorIndex].Trim();
            channelId = key[(separatorIndex + 1)..].Trim();
            return !string.IsNullOrEmpty(transformationId) && !string.IsNullOrEmpty(channelId);
        }

        private void AddNormalizedTransformationPaletteEntry(TransformationPaletteColorEntry entry) {
            if (string.IsNullOrWhiteSpace(entry.TransformationId) || string.IsNullOrWhiteSpace(entry.ChannelId))
                return;

            Transformation transformation = TransformationLoader.Resolve(entry.TransformationId);
            if (transformation == null)
                return;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(entry.ChannelId, this);
            if (channel == null || !channel.IsValid)
                return;

            TransformationPaletteChannelSettings normalizedSettings =
                NormalizePaletteSettings(new TransformationPaletteChannelSettings(entry.Color, entry.Hue,
                    entry.Saturation), channel.DefaultColor);
            if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
                return;

            if (!transformationPaletteOverrides.TryGetValue(transformation.FullID,
                    out Dictionary<string, TransformationPaletteChannelSettings> channelSettings)) {
                channelSettings = new Dictionary<string, TransformationPaletteChannelSettings>(StringComparer.OrdinalIgnoreCase);
                transformationPaletteOverrides[transformation.FullID] = channelSettings;
            }

            channelSettings[channel.Id] = normalizedSettings;
        }

        private bool RemovePaletteOverride(string transformationId, string channelId) {
            if (!transformationPaletteOverrides.TryGetValue(transformationId,
                    out Dictionary<string, TransformationPaletteChannelSettings> channelColors))
                return false;

            bool removed = channelColors.Remove(channelId);
            if (channelColors.Count == 0)
                transformationPaletteOverrides.Remove(transformationId);

            return removed;
        }

        private static Color NormalizePaletteColor(Color color) {
            return new Color(color.R, color.G, color.B, 255);
        }

        private static TransformationPaletteChannelSettings NormalizePaletteSettings(
            TransformationPaletteChannelSettings settings, Color? fallbackColor = null) {
            return new TransformationPaletteChannelSettings(
                NormalizePaletteColor(settings.Color == default && fallbackColor.HasValue ? fallbackColor.Value : settings.Color),
                settings.Hue,
                settings.Saturation
            );
        }

        private bool SetPaletteSettings(string transformationId, TransformationPaletteChannel channel,
            TransformationPaletteChannelSettings settings) {
            settings = NormalizePaletteSettings(settings, channel.DefaultColor);
            bool isDefault = settings.Color == channel.DefaultColor && settings.HasNeutralAdjustments;
            if (isDefault)
                return RemovePaletteOverride(transformationId, channel.Id);

            if (!transformationPaletteOverrides.TryGetValue(transformationId,
                    out Dictionary<string, TransformationPaletteChannelSettings> channelSettings)) {
                channelSettings = new Dictionary<string, TransformationPaletteChannelSettings>(StringComparer.OrdinalIgnoreCase);
                transformationPaletteOverrides[transformationId] = channelSettings;
            }

            if (channelSettings.TryGetValue(channel.Id, out TransformationPaletteChannelSettings existingSettings) &&
                NormalizePaletteSettings(existingSettings, channel.DefaultColor).Equals(settings))
                return false;

            channelSettings[channel.Id] = settings;
            return true;
        }

        internal static void WriteTransformationPaletteEntries(BinaryWriter writer,
            IReadOnlyList<TransformationPaletteColorEntry> entries) {
            ushort count = (ushort)Math.Min(entries?.Count ?? 0, ushort.MaxValue);
            writer.Write(count);

            for (int i = 0; i < count; i++) {
                TransformationPaletteColorEntry entry = entries[i];
                writer.Write(entry.TransformationId ?? string.Empty);
                writer.Write(entry.ChannelId ?? string.Empty);
                writer.Write(entry.Color.R);
                writer.Write(entry.Color.G);
                writer.Write(entry.Color.B);
                writer.Write(entry.Hue);
                writer.Write(entry.Saturation);
            }
        }

        internal static void WritePaletteChannelKeys(BinaryWriter writer,
            IReadOnlyList<string> channelKeys) {
            ushort count = (ushort)Math.Min(channelKeys?.Count ?? 0, ushort.MaxValue);
            writer.Write(count);

            for (int i = 0; i < count; i++)
                writer.Write(channelKeys[i] ?? string.Empty);
        }

        internal static TransformationPaletteColorEntry[] ReadTransformationPaletteEntries(BinaryReader reader) {
            ushort count = reader.ReadUInt16();
            TransformationPaletteColorEntry[] entries = new TransformationPaletteColorEntry[count];

            for (int i = 0; i < count; i++) {
                string transformationId = reader.ReadString();
                string channelId = reader.ReadString();
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte hue = reader.ReadByte();
                byte saturation = reader.ReadByte();
                entries[i] = new TransformationPaletteColorEntry(transformationId, channelId, new Color(r, g, b),
                    hue, saturation);
            }

            return entries;
        }

        internal static string[] ReadPaletteChannelKeys(BinaryReader reader) {
            ushort count = reader.ReadUInt16();
            string[] channelKeys = new string[count];

            for (int i = 0; i < count; i++)
                channelKeys[i] = reader.ReadString();

            return channelKeys;
        }

        private void LoadLegacyPaletteDisabledChannels(string[] disabledPaletteArray) {
            if (disabledPaletteArray == null)
                return;

            HashSet<string> normalizedDisabledKeys = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < disabledPaletteArray.Length; i++) {
                if (TryNormalizePaletteChannelKey(disabledPaletteArray[i], out string normalizedDisabledKey))
                    normalizedDisabledKeys.Add(normalizedDisabledKey);
            }

            foreach (Transformation transformation in TransformationLoader.All) {
                if (transformation == null)
                    continue;

                IReadOnlyList<TransformationPaletteChannel> channels = transformation.GetPaletteChannels(this);
                for (int i = 0; i < channels.Count; i++) {
                    TransformationPaletteChannel channel = channels[i];
                    if (channel == null || !channel.IsValid)
                        continue;

                    string key = BuildPaletteChannelKey(transformation.FullID, channel.Id);
                    if (!normalizedDisabledKeys.Contains(key))
                        paletteEnabledChannels.Add(key);
                }
            }
        }

        private void NormalizeStoredTransformationData() {
            string[] normalizedUnlocks = NormalizeUnlockedTransformations(unlockedTransformations);
            unlockedTransformations.Clear();
            unlockedTransformations.AddRange(normalizedUnlocks);
            NormalizeFavoriteTransformations();
            NormalizeNewlyUnlockedTransformations();

            transformationSlots = NormalizeTransformationSlots(transformationSlots, unlockedTransformations);
            NormalizeTransformationPaletteState();
            NormalizePalettePresets();
            NormalizeCustomTransformationNames();

            Transformation currentTransformation = TransformationLoader.Resolve(currentTransformationId);
            currentTransformationId = currentTransformation != null &&
                                      unlockedTransformations.Contains(currentTransformation.FullID)
                ? currentTransformation.FullID
                : string.Empty;
        }

        private static string[] NormalizeUnlockedTransformations(IEnumerable<string> unlocked) {
            List<string> normalizedUnlocks = new();

            if (unlocked != null) {
                foreach (string unlockedId in unlocked) {
                    Transformation transformation = TransformationLoader.Resolve(unlockedId);
                    if (transformation == null || normalizedUnlocks.Contains(transformation.FullID))
                        continue;

                    normalizedUnlocks.Add(transformation.FullID);
                }
            }

            if (!normalizedUnlocks.Contains("Ben10Mod:HeatBlast"))
                normalizedUnlocks.Insert(0, "Ben10Mod:HeatBlast");

            return normalizedUnlocks.ToArray();
        }

        private static string[] NormalizeTransformationSlots(string[] slots, IList<string> unlocked) {
            string[] normalizedSlots = new string[TransformationSlotCount];
            for (int i = 0; i < normalizedSlots.Length; i++)
                normalizedSlots[i] = string.Empty;

            if (slots == null)
                return normalizedSlots;

            for (int i = 0; i < normalizedSlots.Length && i < slots.Length; i++) {
                Transformation transformation = TransformationLoader.Resolve(slots[i]);
                if (transformation == null)
                    continue;

                if (unlocked != null && !unlocked.Contains(transformation.FullID))
                    continue;

                normalizedSlots[i] = transformation.FullID;
            }

            return normalizedSlots;
        }

        private void NormalizeFavoriteTransformations() {
            HashSet<string> normalizedFavorites = new(StringComparer.OrdinalIgnoreCase);
            foreach (string favoriteTransformationId in favoriteTransformations) {
                Transformation transformation = TransformationLoader.Resolve(favoriteTransformationId);
                if (transformation != null)
                    normalizedFavorites.Add(transformation.FullID);
            }

            favoriteTransformations.Clear();
            foreach (string favoriteTransformationId in normalizedFavorites)
                favoriteTransformations.Add(favoriteTransformationId);
        }

        private void NormalizeNewlyUnlockedTransformations() {
            HashSet<string> normalizedNew = new(StringComparer.OrdinalIgnoreCase);
            foreach (string transformationId in newlyUnlockedTransformations) {
                Transformation transformation = TransformationLoader.Resolve(transformationId);
                if (transformation == null || !unlockedTransformations.Contains(transformation.FullID))
                    continue;

                normalizedNew.Add(transformation.FullID);
            }

            newlyUnlockedTransformations.Clear();
            foreach (string transformationId in normalizedNew)
                newlyUnlockedTransformations.Add(transformationId);
        }

        private IReadOnlyList<string> BuildNormalizedFavoriteTransformations() {
            NormalizeFavoriteTransformations();
            List<string> normalizedFavorites = new();
            foreach (string transformationId in favoriteTransformations)
                normalizedFavorites.Add(transformationId);
            normalizedFavorites.Sort(StringComparer.OrdinalIgnoreCase);
            return normalizedFavorites;
        }

        private IReadOnlyList<string> BuildNormalizedNewlyUnlockedTransformations() {
            NormalizeNewlyUnlockedTransformations();
            List<string> normalizedNew = new();
            foreach (string transformationId in newlyUnlockedTransformations)
                normalizedNew.Add(transformationId);
            normalizedNew.Sort(StringComparer.OrdinalIgnoreCase);
            return normalizedNew;
        }

        private void NormalizePalettePresets() {
            Dictionary<string, PalettePresetData[]> normalizedPresets = new(StringComparer.OrdinalIgnoreCase);

            foreach ((string transformationId, PalettePresetData[] presets) in palettePresets) {
                Transformation transformation = TransformationLoader.Resolve(transformationId);
                if (transformation == null || presets == null)
                    continue;

                PalettePresetData[] normalizedPresetArray = new PalettePresetData[PalettePresetSlotCount];
                for (int presetIndex = 0; presetIndex < Math.Min(presets.Length, PalettePresetSlotCount); presetIndex++) {
                    PalettePresetData preset = presets[presetIndex];
                    if (preset == null)
                        continue;

                    PalettePresetData normalizedPreset = new();
                    for (int i = 0; i < preset.Entries.Count; i++) {
                        TransformationPaletteColorEntry entry = preset.Entries[i];
                        if (!string.Equals(entry.TransformationId, transformation.FullID, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!TryResolvePaletteChannel(entry.TransformationId, entry.ChannelId, out Transformation resolvedTransformation,
                                out TransformationPaletteChannel channel))
                            continue;

                        TransformationPaletteChannelSettings normalizedSettings =
                            NormalizePaletteSettings(new TransformationPaletteChannelSettings(entry.Color, entry.Hue, entry.Saturation),
                                channel.DefaultColor);
                        if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
                            continue;

                        normalizedPreset.Entries.Add(new TransformationPaletteColorEntry(resolvedTransformation.FullID, channel.Id,
                            normalizedSettings.Color, normalizedSettings.Hue, normalizedSettings.Saturation));
                    }

                    for (int i = 0; i < preset.EnabledChannelKeys.Count; i++) {
                        if (TryNormalizePaletteChannelKey(preset.EnabledChannelKeys[i], out string normalizedKey) &&
                            normalizedKey.StartsWith(transformation.FullID + "|", StringComparison.OrdinalIgnoreCase) &&
                            !normalizedPreset.EnabledChannelKeys.Contains(normalizedKey))
                            normalizedPreset.EnabledChannelKeys.Add(normalizedKey);
                    }

                    normalizedPresetArray[presetIndex] = normalizedPreset;
                }

                bool hasAnyPreset = false;
                for (int i = 0; i < normalizedPresetArray.Length; i++) {
                    if (normalizedPresetArray[i] != null) {
                        hasAnyPreset = true;
                        break;
                    }
                }

                if (hasAnyPreset)
                    normalizedPresets[transformation.FullID] = normalizedPresetArray;
            }

            palettePresets.Clear();
            foreach ((string transformationId, PalettePresetData[] presets) in normalizedPresets)
                palettePresets[transformationId] = presets;
        }

        private void NormalizeCustomTransformationNames() {
            Dictionary<string, string> normalizedNames = new(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> entry in customTransformationNames) {
                Transformation transformation = TransformationLoader.Resolve(entry.Key);
                if (transformation == null)
                    continue;

                string normalizedName = NormalizeCustomTransformationName(entry.Value);
                if (string.IsNullOrWhiteSpace(normalizedName))
                    continue;

                normalizedNames[transformation.FullID] = normalizedName;
            }

            customTransformationNames.Clear();
            foreach (KeyValuePair<string, string> entry in normalizedNames)
                customTransformationNames[entry.Key] = entry.Value;
        }

        private IEnumerable<KeyValuePair<string, string>> BuildNormalizedCustomTransformationNames() {
            NormalizeCustomTransformationNames();
            foreach (KeyValuePair<string, string> entry in customTransformationNames)
                yield return entry;
        }

        private static string NormalizeCustomTransformationName(string customName) {
            if (string.IsNullOrWhiteSpace(customName))
                return string.Empty;

            string sanitizedName = customName
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Replace('\t', ' ')
                .Trim();

            sanitizedName = string.Join(" ", sanitizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (sanitizedName.Length > MaxCustomTransformationNameLength)
                sanitizedName = sanitizedName[..MaxCustomTransformationNameLength].TrimEnd();

            return sanitizedName;
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

        private void UpdateProgressionTransformationUnlocks() {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                UnlockTransformation("Ben10Mod:Upgrade");
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
                    return "Ben10Mod:Frankenstrike";
                case EventSlimeRain:
                    return "Ben10Mod:Goop";
                case EventPumpkinMoon:
                    return "Ben10Mod:Whampire";
                case EventFrostMoon:
                    return "Ben10Mod:Lodestar";
                case InvasionID.GoblinArmy:
                    return "Ben10Mod:RipJaws";
                case InvasionID.SnowLegion:
                    return "Ben10Mod:Fasttrack";
                case InvasionID.PirateInvasion:
                    return "Ben10Mod:WaterHazard";
                case EventOldOnesArmy:
                    return "Ben10Mod:Astrodactyl";
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

        private void TriggerTransformationFailsafe() {
            Omnitrix activeOmnitrix = GetActiveOmnitrix();
            int cooldownSeconds = activeOmnitrix?.GetDetransformCooldownDuration(this) ?? cooldownTime;
            if (cooldownSeconds <= 0)
                cooldownSeconds = cooldownTime;
            bool showEffects = Player.whoAmI == Main.myPlayer;

            skipAutomaticForcedDetransformHandling = true;
            TransformationHandler.Detransform(Player, cooldownSeconds, showParticles: showEffects, addCooldown: true,
                playSound: showEffects);

            Player.statLife = 1;
            Player.dead = false;
            Player.immuneNoBlink = true;
            Player.immuneTime = Math.Max(Player.immuneTime, 180);

            if (showEffects)
                CombatText.NewText(Player.getRect(), new Color(96, 255, 160), "Failsafe!", dramatic: true);

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, Player.whoAmI);
        }
    }
}
