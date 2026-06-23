using System;
using System.Collections.Generic;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Transformations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Omnitrix;

public sealed class TransformationProgressionSystem {
    private const int EventBloodMoon = -1;
    private const int EventSolarEclipse = -2;
    private const int EventSlimeRain = -3;
    private const int EventPumpkinMoon = -4;
    private const int EventFrostMoon = -5;

    private static readonly string[] UpgradeRequiredTransformationIds = {
        "Ben10Mod:Humungousaur",
        "Ben10Mod:EyeGuy",
        "Ben10Mod:EchoEcho"
    };

    public bool CanAcceptClientUnlockRequest(global::Ben10Mod.OmnitrixPlayer owner, Transformation transformation) {
        if (owner == null || transformation == null)
            return false;

        if (owner.IsTransformationUnlocked(transformation))
            return true;

        if (transformation.ParentTransformation != null || transformation.IsAccessoryTransformation(owner))
            return false;

        if (transformation.IsStarterTransformation(owner))
            return true;

        return IsHeldTransformationUnlockItem(owner, transformation);
    }

    public IReadOnlyList<string> GetTransformationsForCodexDisplay(global::Ben10Mod.OmnitrixPlayer owner) {
        List<string> displayTransformations = new();
        HashSet<string> seenTransformations = new(StringComparer.OrdinalIgnoreCase);

        foreach (Transformation transformation in TransformationLoader.All) {
            if (transformation == null)
                continue;

            if (seenTransformations.Add(transformation.FullID))
                displayTransformations.Add(transformation.FullID);
        }

        displayTransformations.Sort((left, right) => CompareCodexTransformationDisplayOrder(owner, left, right));
        return displayTransformations;
    }

    public string GetTransformationUnlockConditionText(global::Ben10Mod.OmnitrixPlayer owner, string transformationId) {
        return GetTransformationUnlockConditionText(owner, TransformationLoader.Resolve(transformationId));
    }

    public string GetTransformationUnlockConditionText(global::Ben10Mod.OmnitrixPlayer owner, Transformation transformation) {
        if (owner == null || transformation == null)
            return "Unlock condition not available.";

        if (!TransformationHasUnlockCondition(owner, transformation)) {
            if (transformation.ParentTransformation != null) {
                string parentName = transformation.ParentTransformation.GetDisplayName(owner);
                return $"No separate unlock required. Access this evolution from {parentName} with an Omnitrix that supports evolution.";
            }

            if (transformation.IsAccessoryTransformation(owner))
                return "No unlock required. Equip the Anodite Catalyst in the DNA Alteration slot and press the transformation key to assume this form.";

            if (transformation.IsStarterTransformation(owner))
                return "No unlock required. This starter transformation is available from the beginning.";
        }

        string unlockConditionText = transformation.GetUnlockConditionText(owner);
        if (!string.IsNullOrWhiteSpace(unlockConditionText))
            return unlockConditionText;

        if (string.Equals(transformation.FullID, "Ben10Mod:AlienX", StringComparison.OrdinalIgnoreCase))
            return "Defeat the Moon Lord and collect its Soul of Transformation.";

        return "Unlock condition not yet documented in the codex.";
    }

    public string GetTransformationUnlockCategoryText(global::Ben10Mod.OmnitrixPlayer owner, Transformation transformation) {
        if (owner == null || transformation == null)
            return "Unknown unlock";

        if (transformation.ParentTransformation != null)
            return "Evolution form";

        if (transformation.IsStarterTransformation(owner))
            return "Starter transformation";

        if (transformation.IsAccessoryTransformation(owner))
            return "DNA alteration form";

        if (string.Equals(transformation.FullID, "Ben10Mod:Upgrade", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(transformation.FullID, "Ben10Mod:AlienX", StringComparison.OrdinalIgnoreCase))
            return "Special unlock";

        return TryGetTrackedEventForTransformation(transformation.FullID, out _)
            ? "Event unlock"
            : "Boss unlock";
    }

    public string GetTransformationUnlockProgressText(global::Ben10Mod.OmnitrixPlayer owner, Transformation transformation) {
        if (owner == null || transformation == null)
            return string.Empty;

        if (owner.IsTransformationUnlocked(transformation))
            return "Unlocked on this character.";

        if (!TransformationHasUnlockCondition(owner, transformation) && transformation.ParentTransformation != null) {
            string parentName = transformation.ParentTransformation.GetDisplayName(owner);
            if (!owner.IsTransformationUnlocked(transformation.ParentTransformation))
                return $"Unlock {parentName} on this character first, then evolve into this form.";

            global::Ben10Mod.Content.Items.Accessories.Omnitrix activeOmnitrix = owner.GetActiveOmnitrix();
            return activeOmnitrix?.CanUseEvolutionFeature(owner.Player, owner, transformation.ParentTransformation) == true
                ? $"{parentName} is unlocked. Transform into {parentName} with your current Omnitrix, then press the transformation key again to evolve."
                : $"{parentName} is unlocked. Equip an Omnitrix with evolution support, such as the Ultimatrix, to access this form.";
        }

        if (!TransformationHasUnlockCondition(owner, transformation) && transformation.IsAccessoryTransformation(owner)) {
            return owner.HasEquippedAnoditeCatalyst()
                ? "Anodite Catalyst equipped. Press the transformation key to assume this form."
                : "Equip an Anodite Catalyst in the DNA Alteration slot to access this form.";
        }

        if (!TransformationHasUnlockCondition(owner, transformation) && transformation.IsStarterTransformation(owner))
            return "Starter transformation.";

        if (string.Equals(transformation.FullID, "Ben10Mod:Upgrade", StringComparison.OrdinalIgnoreCase)) {
            List<string> remainingForms = new();
            if (!owner.IsTransformationUnlocked("Ben10Mod:Humungousaur"))
                remainingForms.Add("Humungousaur (Destroyer)");
            if (!owner.IsTransformationUnlocked("Ben10Mod:EyeGuy"))
                remainingForms.Add("Eye Guy (Twins)");
            if (!owner.IsTransformationUnlocked("Ben10Mod:EchoEcho"))
                remainingForms.Add("Echo Echo (Skeletron Prime)");

            return remainingForms.Count == 0
                ? "Humungousaur, Eye Guy, and Echo Echo are unlocked on this character. Upgrade should unlock automatically."
                : $"Still needed on this character: {string.Join(", ", remainingForms)}.";
        }

        if (string.Equals(transformation.FullID, "Ben10Mod:AlienX", StringComparison.OrdinalIgnoreCase)) {
            return NPC.downedMoonlord
                ? "Moon Lord defeated. Defeat it again and collect its Soul of Transformation to unlock Alien X."
                : "Defeat the Moon Lord and collect its Soul of Transformation to unlock Alien X.";
        }

        if (TryGetTrackedEventForTransformation(transformation.FullID, out int eventId)) {
            string eventName = GetTrackedEventDisplayName(eventId);
            if (IsTrackedEventCurrentlyActive(eventId))
                return $"{eventName} is active. Defeat event enemies and pick up the Soul of Transformation they leave behind.";
        }

        return string.Empty;
    }

    public string GetTransformationCodexSubtitle(global::Ben10Mod.OmnitrixPlayer owner, Transformation transformation) {
        if (owner == null || transformation == null)
            return string.Empty;

        if (owner.IsTransformationUnlocked(transformation)) {
            List<string> parts = new() { "Unlocked" };
            if (owner.IsNewlyUnlockedTransformation(transformation))
                parts.Add("New");
            if (owner.IsFavoriteTransformation(transformation))
                parts.Add("Favorite");
            parts.Add(GetTransformationUnlockCategoryText(owner, transformation));
            return string.Join("  |  ", parts);
        }

        if (!TransformationHasUnlockCondition(owner, transformation))
            return $"{GetTransformationAvailabilityStateText(owner, transformation)}  |  {GetTransformationUnlockCategoryText(owner, transformation)}";

        if (TryGetTrackedEventForTransformation(transformation.FullID, out int eventId) &&
            IsTrackedEventCurrentlyActive(eventId)) {
            return "Locked  |  Event active  |  Soul drops from event enemies";
        }

        if (string.Equals(transformation.FullID, "Ben10Mod:Upgrade", StringComparison.OrdinalIgnoreCase))
            return "Locked  |  Mech trio required";

        if (string.Equals(transformation.FullID, "Ben10Mod:AlienX", StringComparison.OrdinalIgnoreCase)) {
            return NPC.downedMoonlord
                ? "Locked  |  Moon Lord Soul required"
                : "Locked  |  Moon Lord required";
        }

        string condensedCondition = CondenseUnlockConditionLabel(GetTransformationUnlockConditionText(owner, transformation));
        if (!string.IsNullOrWhiteSpace(condensedCondition))
            return $"Locked  |  {condensedCondition}";

        return $"Locked  |  {GetTransformationUnlockCategoryText(owner, transformation)}";
    }

    public bool TransformationHasUnlockCondition(global::Ben10Mod.OmnitrixPlayer owner, Transformation transformation) {
        return owner != null && transformation?.HasUnlockCondition(owner) == true;
    }

    public string GetTransformationAvailabilityStateText(global::Ben10Mod.OmnitrixPlayer owner, Transformation transformation) {
        if (owner == null || transformation == null)
            return "Unavailable";

        if (owner.IsTransformationUnlocked(transformation))
            return "Unlocked";

        if (TransformationHasUnlockCondition(owner, transformation))
            return "Locked";

        return transformation.ParentTransformation != null
            ? "No separate unlock"
            : "No unlock required";
    }

    public string GetTransformationAccessHeaderText(global::Ben10Mod.OmnitrixPlayer owner, Transformation transformation) {
        return TransformationHasUnlockCondition(owner, transformation)
            ? "Unlock Condition"
            : "Access";
    }

    public void RecordEventParticipation(global::Ben10Mod.OmnitrixPlayer owner, NPC npc) {
    }

    public void ApplyRecordedEventParticipation(IEnumerable<int> eventIds) {
    }

    public void UpdateEventTransformationUnlocks(global::Ben10Mod.OmnitrixPlayer owner) {
        // Event rewards are now real item drops from event enemies, handled by bossTrackerNPC.OnKill.
    }

    public void UpdateProgressionTransformationUnlocks(global::Ben10Mod.OmnitrixPlayer owner) {
        if (owner == null || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (HasAllUpgradeRequirementsUnlocked(owner))
            owner.UnlockTransformation("Ben10Mod:Upgrade");
    }

    private static bool IsHeldTransformationUnlockItem(global::Ben10Mod.OmnitrixPlayer owner,
        Transformation transformation) {
        Item heldItem = owner.Player?.HeldItem;
        if (heldItem == null || heldItem.IsAir || heldItem.ModItem == null || heldItem.ModItem.Mod != owner.Mod)
            return false;

        if (!heldItem.consumable)
            return false;

        string heldName = NormalizeUnlockItemToken(heldItem.ModItem.Name);
        return heldName == NormalizeUnlockItemToken(transformation.TransformationName) ||
               heldName == NormalizeUnlockItemToken(GetTransformationIdName(transformation.FullID));
    }

    private static string GetTransformationIdName(string transformationId) {
        if (string.IsNullOrWhiteSpace(transformationId))
            return string.Empty;

        int separatorIndex = transformationId.IndexOf(':');
        return separatorIndex >= 0 && separatorIndex < transformationId.Length - 1
            ? transformationId[(separatorIndex + 1)..]
            : transformationId;
    }

    private static string NormalizeUnlockItemToken(string value) {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        Span<char> buffer = stackalloc char[value.Length];
        int length = 0;
        for (int i = 0; i < value.Length; i++) {
            char c = value[i];
            if (char.IsLetterOrDigit(c))
                buffer[length++] = char.ToUpperInvariant(c);
        }

        return new string(buffer[..length]);
    }

    private static int CompareCodexTransformationDisplayOrder(global::Ben10Mod.OmnitrixPlayer owner,
        string leftTransformationId, string rightTransformationId) {
        bool leftIsUnlocked = owner.IsTransformationUnlocked(leftTransformationId);
        bool rightIsUnlocked = owner.IsTransformationUnlocked(rightTransformationId);
        if (leftIsUnlocked != rightIsUnlocked)
            return leftIsUnlocked ? -1 : 1;

        bool leftIsFavorite = leftIsUnlocked && owner.IsFavoriteTransformation(leftTransformationId);
        bool rightIsFavorite = rightIsUnlocked && owner.IsFavoriteTransformation(rightTransformationId);
        if (leftIsFavorite != rightIsFavorite)
            return leftIsFavorite ? -1 : 1;

        return CompareTransformationDisplayName(owner, leftTransformationId, rightTransformationId);
    }

    public static int CompareTransformationDisplayName(global::Ben10Mod.OmnitrixPlayer owner,
        string leftTransformationId, string rightTransformationId) {
        Transformation leftTransformation = TransformationLoader.Resolve(leftTransformationId);
        Transformation rightTransformation = TransformationLoader.Resolve(rightTransformationId);
        string leftName = leftTransformation?.GetDisplayName(owner) ?? string.Empty;
        string rightName = rightTransformation?.GetDisplayName(owner) ?? string.Empty;
        int nameComparison = string.Compare(leftName, rightName, StringComparison.CurrentCultureIgnoreCase);
        if (nameComparison != 0)
            return nameComparison;

        string leftKey = leftTransformation?.FullID ?? leftTransformationId ?? string.Empty;
        string rightKey = rightTransformation?.FullID ?? rightTransformationId ?? string.Empty;
        return string.Compare(leftKey, rightKey, StringComparison.OrdinalIgnoreCase);
    }

    private static string CondenseUnlockConditionLabel(string unlockConditionText) {
        if (string.IsNullOrWhiteSpace(unlockConditionText))
            return string.Empty;

        string condensed = unlockConditionText.Trim().TrimEnd('.');
        condensed = condensed.Replace("Participate in and complete ", "Complete ", StringComparison.OrdinalIgnoreCase);
        condensed = condensed.Replace("Participate in and defeat ", "Defeat ", StringComparison.OrdinalIgnoreCase);
        condensed = condensed.Replace("Access this evolution from ", "Evolve from ", StringComparison.OrdinalIgnoreCase);
        return condensed;
    }

    private static bool TryGetTrackedEventForTransformation(string transformationId, out int eventId) {
        switch (transformationId) {
            case "Ben10Mod:GhostFreak":
                eventId = EventBloodMoon;
                return true;
            case "Ben10Mod:Frankenstrike":
                eventId = EventSolarEclipse;
                return true;
            case "Ben10Mod:Goop":
                eventId = EventSlimeRain;
                return true;
            case "Ben10Mod:Whampire":
                eventId = EventPumpkinMoon;
                return true;
            case "Ben10Mod:Lodestar":
                eventId = EventFrostMoon;
                return true;
            case "Ben10Mod:RipJaws":
                eventId = InvasionID.GoblinArmy;
                return true;
            case "Ben10Mod:Fasttrack":
                eventId = InvasionID.SnowLegion;
                return true;
            case "Ben10Mod:WaterHazard":
                eventId = InvasionID.PirateInvasion;
                return true;
            case "Ben10Mod:Astrodactyl":
                eventId = InvasionID.MartianMadness;
                return true;
            default:
                eventId = 0;
                return false;
        }
    }

    private static string GetTrackedEventDisplayName(int eventId) {
        return eventId switch {
            EventBloodMoon => "Blood Moon",
            EventSolarEclipse => "Solar Eclipse",
            EventSlimeRain => "Slime Rain",
            EventPumpkinMoon => "Pumpkin Moon",
            EventFrostMoon => "Frost Moon",
            InvasionID.GoblinArmy => "Goblin Army",
            InvasionID.SnowLegion => "Frost Legion",
            InvasionID.PirateInvasion => "Pirate Invasion",
            InvasionID.MartianMadness => "Martian Madness",
            _ => "event"
        };
    }

    private static bool IsTrackedEventCurrentlyActive(int eventId) {
        return eventId switch {
            EventBloodMoon => Main.bloodMoon,
            EventSolarEclipse => Main.eclipse,
            EventSlimeRain => Main.slimeRain,
            EventPumpkinMoon => Main.pumpkinMoon,
            EventFrostMoon => Main.snowMoon,
            InvasionID.GoblinArmy => IsInvasionEventActive(InvasionID.GoblinArmy),
            InvasionID.SnowLegion => IsInvasionEventActive(InvasionID.SnowLegion),
            InvasionID.PirateInvasion => IsInvasionEventActive(InvasionID.PirateInvasion),
            InvasionID.MartianMadness => IsInvasionEventActive(InvasionID.MartianMadness),
            _ => false
        };
    }

    private static bool IsInvasionEventActive(int invasionId) {
        return Main.invasionType == invasionId;
    }

    private static bool HasAllUpgradeRequirementsUnlocked(global::Ben10Mod.OmnitrixPlayer owner) {
        for (int i = 0; i < UpgradeRequiredTransformationIds.Length; i++) {
            if (!owner.IsTransformationUnlocked(UpgradeRequiredTransformationIds[i]))
                return false;
        }

        return true;
    }
}
