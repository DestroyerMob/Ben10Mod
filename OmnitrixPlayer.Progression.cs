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
        internal bool CanAcceptClientUnlockRequest(Transformation transformation) {
            if (transformation == null)
                return false;

            if (IsTransformationUnlocked(transformation))
                return true;

            if (transformation.ParentTransformation != null || transformation.IsAccessoryTransformation(this))
                return false;

            if (transformation.IsStarterTransformation(this))
                return true;

            return IsHeldTransformationUnlockItem(transformation);
        }

        private bool IsHeldTransformationUnlockItem(Transformation transformation) {
            Item heldItem = Player?.HeldItem;
            if (heldItem == null || heldItem.IsAir || heldItem.ModItem == null || heldItem.ModItem.Mod != Mod)
                return false;

            if (heldItem.type == ModContent.ItemType<CelestialsapienDnaSample>())
                return NPC.downedMoonlord &&
                       string.Equals(transformation.FullID, "Ben10Mod:AlienX", StringComparison.OrdinalIgnoreCase);

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

            if (!TransformationHasUnlockCondition(transformation)) {
                if (transformation.ParentTransformation != null) {
                    string parentName = transformation.ParentTransformation.GetDisplayName(this);
                    return $"No separate unlock required. Access this evolution from {parentName} with an Omnitrix that supports evolution.";
                }

                if (transformation.IsAccessoryTransformation(this))
                    return "No unlock required. Equip the Anodite Catalyst in the DNA Alteration slot and press the transformation key to assume this form.";

                if (transformation.IsStarterTransformation(this))
                    return "No unlock required. This starter transformation is available from the beginning.";
            }

            string unlockConditionText = transformation.GetUnlockConditionText(this);
            if (!string.IsNullOrWhiteSpace(unlockConditionText))
                return unlockConditionText;

            if (string.Equals(transformation.FullID, "Ben10Mod:AlienX", StringComparison.OrdinalIgnoreCase))
                return "Defeat the Moon Lord, then use a Celestialsapien DNA Sample.";

            return "Unlock condition not yet documented in the codex.";
        }

        public string GetTransformationUnlockCategoryText(Transformation transformation) {
            if (transformation == null)
                return "Unknown unlock";

            if (transformation.ParentTransformation != null)
                return "Evolution form";

            if (transformation.IsStarterTransformation(this))
                return "Starter transformation";

            if (transformation.IsAccessoryTransformation(this))
                return "DNA alteration form";

            if (string.Equals(transformation.FullID, "Ben10Mod:Upgrade", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(transformation.FullID, "Ben10Mod:AlienX", StringComparison.OrdinalIgnoreCase))
                return "Special unlock";

            return TryGetTrackedEventForTransformation(transformation.FullID, out _)
                ? "Event unlock"
                : "Boss unlock";
        }

        public string GetTransformationUnlockProgressText(Transformation transformation) {
            if (transformation == null)
                return string.Empty;

            if (IsTransformationUnlocked(transformation))
                return "Unlocked on this character.";

            if (!TransformationHasUnlockCondition(transformation) && transformation.ParentTransformation != null) {
                string parentName = transformation.ParentTransformation.GetDisplayName(this);
                if (!IsTransformationUnlocked(transformation.ParentTransformation))
                    return $"Unlock {parentName} on this character first, then evolve into this form.";

                Omnitrix activeOmnitrix = GetActiveOmnitrix();
                return activeOmnitrix?.CanUseEvolutionFeature(Player, this, transformation.ParentTransformation) == true
                    ? $"{parentName} is unlocked. Transform into {parentName} with your current Omnitrix, then press the transformation key again to evolve."
                    : $"{parentName} is unlocked. Equip an Omnitrix with evolution support, such as the Ultimatrix, to access this form.";
            }

            if (!TransformationHasUnlockCondition(transformation) && transformation.IsAccessoryTransformation(this)) {
                return HasEquippedAnoditeCatalyst()
                    ? "Anodite Catalyst equipped. Press the transformation key to assume this form."
                    : "Equip an Anodite Catalyst in the DNA Alteration slot to access this form.";
            }

            if (!TransformationHasUnlockCondition(transformation) && transformation.IsStarterTransformation(this))
                return "Starter transformation.";

            if (string.Equals(transformation.FullID, "Ben10Mod:Upgrade", StringComparison.OrdinalIgnoreCase)) {
                List<string> remainingForms = new();
                if (!IsTransformationUnlocked("Ben10Mod:Humungousaur"))
                    remainingForms.Add("Humungousaur (Destroyer)");
                if (!IsTransformationUnlocked("Ben10Mod:EyeGuy"))
                    remainingForms.Add("Eye Guy (Twins)");
                if (!IsTransformationUnlocked("Ben10Mod:EchoEcho"))
                    remainingForms.Add("Echo Echo (Skeletron Prime)");

                return remainingForms.Count == 0
                    ? "Humungousaur, Eye Guy, and Echo Echo are unlocked on this character. Upgrade should unlock automatically."
                    : $"Still needed on this character: {string.Join(", ", remainingForms)}.";
            }

            if (string.Equals(transformation.FullID, "Ben10Mod:AlienX", StringComparison.OrdinalIgnoreCase)) {
                return NPC.downedMoonlord
                    ? "Moon Lord defeated. Use a Celestialsapien DNA Sample to unlock Alien X."
                    : "Moon Lord must be defeated before the DNA Sample can be used.";
            }

            if (TryGetTrackedEventForTransformation(transformation.FullID, out int eventId)) {
                string eventName = GetTrackedEventDisplayName(eventId);
                if (IsTrackedEventCurrentlyActive(eventId)) {
                    return participatedEvents.Contains(eventId)
                        ? $"Participation recorded for the current {eventName}. Finish it to unlock this form."
                        : $"{eventName} is active. Deal damage during the event to qualify.";
                }
            }

            return string.Empty;
        }

        public string GetTransformationCodexSubtitle(Transformation transformation) {
            if (transformation == null)
                return string.Empty;

            if (IsTransformationUnlocked(transformation)) {
                List<string> parts = new() { "Unlocked" };
                if (IsNewlyUnlockedTransformation(transformation))
                    parts.Add("New");
                if (IsFavoriteTransformation(transformation))
                    parts.Add("Favorite");
                parts.Add(GetTransformationUnlockCategoryText(transformation));
                return string.Join("  |  ", parts);
            }

            if (!TransformationHasUnlockCondition(transformation))
                return $"{GetTransformationAvailabilityStateText(transformation)}  |  {GetTransformationUnlockCategoryText(transformation)}";

            if (TryGetTrackedEventForTransformation(transformation.FullID, out int eventId) &&
                IsTrackedEventCurrentlyActive(eventId)) {
                return participatedEvents.Contains(eventId)
                    ? "Locked  |  Event active  |  Participation recorded"
                    : "Locked  |  Event active  |  Deal damage to qualify";
            }

            if (string.Equals(transformation.FullID, "Ben10Mod:Upgrade", StringComparison.OrdinalIgnoreCase))
                return "Locked  |  Mech trio required";

            if (string.Equals(transformation.FullID, "Ben10Mod:AlienX", StringComparison.OrdinalIgnoreCase)) {
                return NPC.downedMoonlord
                    ? "Locked  |  DNA Sample required"
                    : "Locked  |  Moon Lord required";
            }

            string condensedCondition = CondenseUnlockConditionLabel(GetTransformationUnlockConditionText(transformation));
            if (!string.IsNullOrWhiteSpace(condensedCondition))
                return $"Locked  |  {condensedCondition}";

            return $"Locked  |  {GetTransformationUnlockCategoryText(transformation)}";
        }

        public bool TransformationHasUnlockCondition(Transformation transformation) {
            return transformation?.HasUnlockCondition(this) == true;
        }

        public string GetTransformationAvailabilityStateText(Transformation transformation) {
            if (transformation == null)
                return "Unavailable";

            if (IsTransformationUnlocked(transformation))
                return "Unlocked";

            if (TransformationHasUnlockCondition(transformation))
                return "Locked";

            return transformation.ParentTransformation != null
                ? "No separate unlock"
                : "No unlock required";
        }

        public string GetTransformationAccessHeaderText(Transformation transformation) {
            return TransformationHasUnlockCondition(transformation)
                ? "Unlock Condition"
                : "Access";
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
                EventOldOnesArmy => "Old One's Army",
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
                EventOldOnesArmy => DD2Event.Ongoing,
                InvasionID.GoblinArmy => Main.invasionType == InvasionID.GoblinArmy && Main.invasionSize > 0,
                InvasionID.SnowLegion => Main.invasionType == InvasionID.SnowLegion && Main.invasionSize > 0,
                InvasionID.PirateInvasion => Main.invasionType == InvasionID.PirateInvasion && Main.invasionSize > 0,
                InvasionID.MartianMadness => Main.invasionType == InvasionID.MartianMadness && Main.invasionSize > 0,
                _ => false
            };
        }

        public void RecordEventParticipation(NPC npc) {
            if (npc == null || !npc.active || npc.friendly || npc.townNPC || npc.CountsAsACritter)
                return;

            List<int> newlyRecordedEvents = null;
            foreach (int eventId in GetActiveTrackedEvents()) {
                if (!DoesNpcCountForEventParticipation(eventId, npc))
                    continue;

                if (participatedEvents.Add(eventId) && Main.netMode == NetmodeID.MultiplayerClient &&
                    Player.whoAmI == Main.myPlayer) {
                    newlyRecordedEvents ??= new List<int>();
                    newlyRecordedEvents.Add(eventId);
                }
            }

            if (newlyRecordedEvents is { Count: > 0 })
                RequestServerEventParticipationSync(newlyRecordedEvents);
        }

        internal void ApplyRecordedEventParticipation(IEnumerable<int> eventIds) {
            if (eventIds == null)
                return;

            HashSet<int> activeTrackedEvents = new(GetActiveTrackedEvents());
            foreach (int eventId in eventIds) {
                if (activeTrackedEvents.Contains(eventId) || activeEvents.Contains(eventId))
                    participatedEvents.Add(eventId);
            }
        }

        private void RequestServerEventParticipationSync(IReadOnlyList<int> eventIds) {
            if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI != Main.myPlayer || eventIds == null ||
                eventIds.Count == 0)
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.RecordEventParticipation);
            packet.Write((byte)eventIds.Count);
            for (int i = 0; i < eventIds.Count; i++)
                packet.Write(eventIds[i]);

            packet.Send();
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

            if (HasAllUpgradeRequirementsUnlocked())
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
                case InvasionID.MartianMadness:
                    return "Ben10Mod:Astrodactyl";
                default:
                    return string.Empty;
            }
        }

        private bool HasAllUpgradeRequirementsUnlocked() {
            for (int i = 0; i < UpgradeRequiredTransformationIds.Length; i++) {
                if (!IsTransformationUnlocked(UpgradeRequiredTransformationIds[i]))
                    return false;
            }

            return true;
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
