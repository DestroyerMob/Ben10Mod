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

        public string GetSelectedTransformationStatusSummary() {
            string selectedTransformationId = GetSelectedTransformationId();
            if (string.IsNullOrEmpty(selectedTransformationId))
                return "Assign a form to this slot";

            List<string> parts = new() {
                GetTransformationCooldownTicks() > 0 ? GetTransformationCooldownDisplayText() : "Ready to transform"
            };

            if (IsNewlyUnlockedTransformation(selectedTransformationId))
                parts.Add("New");
            if (IsFavoriteTransformation(selectedTransformationId))
                parts.Add("Favorite");

            return string.Join("  |  ", parts);
        }

        public bool IsFavoriteTransformation(string transformationId) {
            string canonicalTransformationId = TransformationRosterState.ResolveTransformationId(transformationId);
            return canonicalTransformationId.Length > 0 && favoriteTransformations.Contains(canonicalTransformationId);
        }

        public bool IsFavoriteTransformation(Transformation transformation) {
            return transformation != null && favoriteTransformations.Contains(transformation.FullID);
        }

        public bool SetFavoriteTransformation(string transformationId, bool isFavorite) {
            return Roster.SetFavorite(transformationId, isFavorite);
        }

        public bool ToggleFavoriteTransformation(string transformationId) {
            return Roster.ToggleFavorite(transformationId);
        }

        public bool IsNewlyUnlockedTransformation(string transformationId) {
            string canonicalTransformationId = TransformationRosterState.ResolveTransformationId(transformationId);
            return canonicalTransformationId.Length > 0 && newlyUnlockedTransformations.Contains(canonicalTransformationId);
        }

        public bool IsNewlyUnlockedTransformation(Transformation transformation) {
            return transformation != null && newlyUnlockedTransformations.Contains(transformation.FullID);
        }

        public bool MarkTransformationAsSeen(string transformationId) {
            return Roster.MarkSeen(transformationId);
        }

        public bool HasAnyNewlyUnlockedTransformations() => newlyUnlockedTransformations.Count > 0;

        public bool IsTransformationUnlocked(string transformationId) {
            return Roster.IsUnlocked(transformationId);
        }

        public bool IsTransformationUnlocked(Transformation transformation) {
            return transformation != null && unlockedTransformations.Contains(transformation.FullID);
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

        private int CompareUnlockedTransformationDisplayOrder(string leftTransformationId, string rightTransformationId) {
            bool leftIsFavorite = IsFavoriteTransformation(leftTransformationId);
            bool rightIsFavorite = IsFavoriteTransformation(rightTransformationId);
            if (leftIsFavorite != rightIsFavorite)
                return leftIsFavorite ? -1 : 1;

            return CompareTransformationDisplayName(leftTransformationId, rightTransformationId);
        }

        public bool UnlockTransformation(string transformationId, bool sync = true, bool showEffects = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            string canonicalTransformationId = transformation.FullID;
            if (!Roster.TryUnlock(canonicalTransformationId))
                return false;

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
            Main.NewText($"Open the roster with L to slot {name} into your active five.", new Color(180, 255, 210));
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
            Roster.Remove(canonicalTransformationId);

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

        private static string[] NormalizeUnlockedTransformations(IEnumerable<string> unlocked) {
            return TransformationRosterState.NormalizeUnlockedTransformations(unlocked);
        }

        private static string[] NormalizeTransformationSlots(string[] slots, IList<string> unlocked) {
            return TransformationRosterState.NormalizeTransformationSlots(slots, unlocked, TransformationSlotCount);
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
    }
}
