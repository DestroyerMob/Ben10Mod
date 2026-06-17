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
        public bool HasPaletteCustomizationData(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            if (transformationPaletteOverrides.TryGetValue(GetActivePaletteOwnerId(transformation),
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

        public string GetSelectedTransformationCustomizationSummary() {
            string selectedTransformationId = GetSelectedTransformationId();
            if (string.IsNullOrEmpty(selectedTransformationId))
                return string.Empty;

            List<string> parts = new();
            string paletteText = GetSelectedTransformationPaletteStatusText();
            if (!string.IsNullOrWhiteSpace(paletteText))
                parts.Add(paletteText);

            string costumeName = GetSelectedTransformationCostumeDisplayName(selectedTransformationId);
            if (!string.IsNullOrWhiteSpace(costumeName))
                parts.Add($"Costume: {costumeName}");

            return string.Join("  |  ", parts);
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

        public TransformationCostume GetSelectedTransformationCostume(Transformation transformation) {
            if (transformation == null)
                return null;

            if (!selectedTransformationCostumes.TryGetValue(transformation.FullID, out string costumeId) ||
                string.IsNullOrWhiteSpace(costumeId))
                return null;

            TransformationCostume costume = TransformationCostumeLoader.Resolve(costumeId);
            return costume != null &&
                   string.Equals(costume.TargetTransformationId, transformation.FullID, StringComparison.OrdinalIgnoreCase)
                ? costume
                : null;
        }

        public string GetSelectedTransformationCostumeId(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            return GetSelectedTransformationCostume(transformation)?.FullID ?? string.Empty;
        }

        public string GetSelectedTransformationCostumeDisplayName(string transformationId) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            TransformationCostume costume = GetSelectedTransformationCostume(transformation);
            return costume?.DisplayName ?? "Default";
        }

        public bool SetSelectedTransformationCostume(string transformationId, string costumeId, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            string normalizedTransformationId = transformation.FullID;
            string normalizedCostumeId = string.Empty;
            if (!string.IsNullOrWhiteSpace(costumeId)) {
                TransformationCostume costume = TransformationCostumeLoader.Resolve(costumeId);
                if (costume == null ||
                    !string.Equals(costume.TargetTransformationId, normalizedTransformationId, StringComparison.OrdinalIgnoreCase))
                    return false;

                normalizedCostumeId = costume.FullID;
            }

            if (string.IsNullOrWhiteSpace(normalizedCostumeId)) {
                bool removed = selectedTransformationCostumes.Remove(normalizedTransformationId);
                if (removed && sync)
                    SyncTransformationPaletteStateToServerOrClients();
                return removed;
            }

            if (selectedTransformationCostumes.TryGetValue(normalizedTransformationId, out string existingCostumeId) &&
                string.Equals(existingCostumeId, normalizedCostumeId, StringComparison.OrdinalIgnoreCase))
                return false;

            selectedTransformationCostumes[normalizedTransformationId] = normalizedCostumeId;
            if (sync)
                SyncTransformationPaletteStateToServerOrClients();
            return true;
        }

        internal void ApplySelectedTransformationCostumeVisuals(Player player, Transformation transformation) {
            GetSelectedTransformationCostume(transformation)?.ApplyVisuals(player, this, transformation);
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

        public bool IsTransformationRandomizerEnabled() {
            return Main.netMode != NetmodeID.Server &&
                   ModContent.GetInstance<Ben10ClientConfig>().EnableTransformationRandomizer;
        }

        public string ResolveRandomizedTransformationTarget(string desiredTransformationId, out bool wasRandomized) {
            wasRandomized = false;

            Transformation desiredTransformation = TransformationLoader.Resolve(desiredTransformationId);
            if (desiredTransformation == null)
                return string.Empty;

            string desiredId = desiredTransformation.FullID;
            if (!IsTransformationRandomizerEnabled())
                return desiredId;

            List<string> candidateIds = new();
            for (int i = 0; i < unlockedTransformations.Count; i++) {
                Transformation unlockedTransformation = TransformationLoader.Resolve(unlockedTransformations[i]);
                if (unlockedTransformation == null)
                    continue;

                string unlockedId = unlockedTransformation.FullID;
                if (!candidateIds.Contains(unlockedId))
                    candidateIds.Add(unlockedId);
            }

            if (candidateIds.Count == 0)
                return desiredId;

            List<string> pool = candidateIds.Where(id =>
                !string.Equals(id, desiredId, StringComparison.Ordinal) &&
                !string.Equals(id, currentTransformationId, StringComparison.Ordinal)).ToList();

            if (pool.Count == 0)
                pool = candidateIds.Where(id => !string.Equals(id, desiredId, StringComparison.Ordinal)).ToList();

            if (pool.Count == 0)
                return desiredId;

            string randomizedId = pool[Main.rand.Next(pool.Count)];
            if (string.Equals(randomizedId, desiredId, StringComparison.Ordinal))
                return desiredId;

            wasRandomized = true;
            return randomizedId;
        }

        public void ShowTransformationRandomizerFeedback(string transformationId) {
            if (Main.netMode == NetmodeID.Server || Player.whoAmI != Main.myPlayer)
                return;

            string formName = GetTransformationBaseName(transformationId);
            Main.NewText($"Mistransformed into {formName}!", new Color(255, 205, 95));
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

        private string GetActivePaletteOwnerId(Transformation transformation) {
            if (transformation == null)
                return string.Empty;

            return GetSelectedTransformationCostume(transformation)?.FullID ?? transformation.FullID;
        }

        private string GetActivePaletteOwnerId(string transformationId) {
            return GetActivePaletteOwnerId(TransformationLoader.Resolve(transformationId));
        }

        private bool TryResolvePaletteOwner(string ownerId, out string normalizedOwnerId,
            out Transformation transformation, out TransformationCostume costume) {
            normalizedOwnerId = string.Empty;
            transformation = null;
            costume = null;

            if (string.IsNullOrWhiteSpace(ownerId))
                return false;

            transformation = TransformationLoader.Resolve(ownerId);
            if (transformation != null) {
                normalizedOwnerId = transformation.FullID;
                return true;
            }

            costume = TransformationCostumeLoader.Resolve(ownerId);
            if (costume == null)
                return false;

            transformation = TransformationLoader.Resolve(costume.TargetTransformationId);
            if (transformation == null)
                return false;

            normalizedOwnerId = costume.FullID;
            return true;
        }

        private IReadOnlyList<TransformationPaletteChannel> GetPaletteChannelsForOwner(string ownerId) {
            if (!TryResolvePaletteOwner(ownerId, out _, out Transformation transformation, out TransformationCostume costume))
                return Array.Empty<TransformationPaletteChannel>();

            return costume?.GetMergedPaletteChannels(transformation, this) ?? transformation.PaletteChannels;
        }

        private void SetAllPaletteChannelsEnabledForOwner(string ownerId, bool enabled) {
            IReadOnlyList<TransformationPaletteChannel> channels = GetPaletteChannelsForOwner(ownerId);
            for (int i = 0; i < channels.Count; i++) {
                TransformationPaletteChannel channel = channels[i];
                if (channel == null || !channel.IsValid)
                    continue;

                string key = BuildPaletteChannelKey(ownerId, channel.Id);
                if (enabled)
                    paletteEnabledChannels.Add(key);
                else
                    paletteEnabledChannels.Remove(key);
            }
        }

        private bool TryResolvePaletteChannelOwner(string ownerId, string channelId, out string normalizedOwnerId,
            out TransformationPaletteChannel channel) {
            normalizedOwnerId = string.Empty;
            channel = null;

            if (string.IsNullOrWhiteSpace(channelId) ||
                !TryResolvePaletteOwner(ownerId, out normalizedOwnerId, out Transformation transformation,
                    out TransformationCostume costume))
                return false;

            if (costume != null) {
                channel = costume.GetMergedPaletteChannel(transformation, channelId, this);
                return channel != null && channel.IsValid;
            }

            IReadOnlyList<TransformationPaletteChannel> channels = transformation.PaletteChannels;
            for (int i = 0; i < channels.Count; i++) {
                TransformationPaletteChannel candidate = channels[i];
                if (candidate != null && candidate.IsValid &&
                    string.Equals(candidate.Id, channelId, StringComparison.OrdinalIgnoreCase)) {
                    channel = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool IsPaletteChannelEnabled(Transformation transformation, string channelId) {
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return false;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null || !channel.IsValid)
                return false;

            return paletteEnabledChannels.Contains(BuildPaletteChannelKey(GetActivePaletteOwnerId(transformation), channel.Id));
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

            string key = BuildPaletteChannelKey(GetActivePaletteOwnerId(transformation), channel.Id);
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

            string ownerId = GetActivePaletteOwnerId(transformation);
            if (transformationPaletteOverrides.TryGetValue(ownerId,
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

        public byte GetPaletteBrightness(string transformationId, string channelId) {
            return GetPaletteSettings(TransformationLoader.Resolve(transformationId), channelId).Brightness;
        }

        public bool SetPaletteColor(string transformationId, string channelId, Color color, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return false;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null || !channel.IsValid)
                return false;

            TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(transformation, channel.Id);
            TransformationPaletteChannelSettings newSettings = new(color, currentSettings.Hue, currentSettings.Saturation,
                currentSettings.Brightness);
            bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel, newSettings);

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
                currentSettings.Saturation, currentSettings.Brightness);
            bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel, newSettings);
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
            bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel,
                new TransformationPaletteChannelSettings(currentSettings.Color, hue, currentSettings.Saturation,
                    currentSettings.Brightness));
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
            bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel,
                new TransformationPaletteChannelSettings(currentSettings.Color, currentSettings.Hue, saturation,
                    currentSettings.Brightness));
            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool SetPaletteBrightness(string transformationId, string channelId, byte brightness, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || string.IsNullOrWhiteSpace(channelId))
                return false;

            TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, this);
            if (channel == null || !channel.IsValid)
                return false;

            TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(transformation, channel.Id);
            bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel,
                new TransformationPaletteChannelSettings(currentSettings.Color, currentSettings.Hue, currentSettings.Saturation,
                    brightness));
            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool ResetPaletteColors(string transformationId, bool sync = true) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return false;

            bool changed = transformationPaletteOverrides.Remove(GetActivePaletteOwnerId(transformation));
            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public IReadOnlyList<OmnitrixVisualPaletteChannel> GetOmnitrixVisualPaletteChannels() {
            return OmnitrixVisualPalette.Channels;
        }

        public bool HasOmnitrixVisualPaletteCustomizationData() {
            return BuildNormalizedOmnitrixVisualPaletteEntries().Count > 0;
        }

        public TransformationPaletteChannelSettings GetOmnitrixVisualPaletteSettings(string channelId) {
            if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
                return new TransformationPaletteChannelSettings(Color.White);

            if (omnitrixVisualPaletteOverrides.TryGetValue(channel.Id, out TransformationPaletteChannelSettings settings))
                return NormalizeOmnitrixVisualPaletteSettings(settings, channel.DefaultColor);

            return new TransformationPaletteChannelSettings(channel.DefaultColor);
        }

        public Color GetOmnitrixVisualColor(string channelId) {
            TransformationPaletteChannelSettings settings = GetOmnitrixVisualPaletteSettings(channelId);
            return TransformationPaletteMath.ApplyHueSaturationAndBrightness(settings.Color, settings.Hue,
                settings.Saturation, settings.Brightness);
        }

        public Color GetOmnitrixVisualColor(string channelId, Color fallbackColor) {
            if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
                return fallbackColor;

            if (!omnitrixVisualPaletteOverrides.TryGetValue(channel.Id, out TransformationPaletteChannelSettings settings))
                return fallbackColor;

            settings = NormalizeOmnitrixVisualPaletteSettings(settings, channel.DefaultColor);
            return TransformationPaletteMath.ApplyHueSaturationAndBrightness(settings.Color, settings.Hue,
                settings.Saturation, settings.Brightness);
        }

        public bool SetOmnitrixVisualPaletteColor(string channelId, Color color, bool sync = true) {
            if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
                return false;

            TransformationPaletteChannelSettings currentSettings = GetOmnitrixVisualPaletteSettings(channel.Id);
            bool changed = SetOmnitrixVisualPaletteSettings(channel.Id,
                new TransformationPaletteChannelSettings(color, currentSettings.Hue, currentSettings.Saturation,
                    currentSettings.Brightness));

            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool SetOmnitrixVisualPaletteHue(string channelId, byte hue, bool sync = true) {
            if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
                return false;

            TransformationPaletteChannelSettings currentSettings = GetOmnitrixVisualPaletteSettings(channel.Id);
            bool changed = SetOmnitrixVisualPaletteSettings(channel.Id,
                new TransformationPaletteChannelSettings(currentSettings.Color, hue, currentSettings.Saturation,
                    currentSettings.Brightness));

            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool SetOmnitrixVisualPaletteSaturation(string channelId, byte saturation, bool sync = true) {
            if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
                return false;

            TransformationPaletteChannelSettings currentSettings = GetOmnitrixVisualPaletteSettings(channel.Id);
            bool changed = SetOmnitrixVisualPaletteSettings(channel.Id,
                new TransformationPaletteChannelSettings(currentSettings.Color, currentSettings.Hue, saturation,
                    currentSettings.Brightness));

            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool SetOmnitrixVisualPaletteBrightness(string channelId, byte brightness, bool sync = true) {
            if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
                return false;

            TransformationPaletteChannelSettings currentSettings = GetOmnitrixVisualPaletteSettings(channel.Id);
            bool changed = SetOmnitrixVisualPaletteSettings(channel.Id,
                new TransformationPaletteChannelSettings(currentSettings.Color, currentSettings.Hue,
                    currentSettings.Saturation, brightness));

            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool ResetOmnitrixVisualPaletteChannel(string channelId, bool sync = true) {
            if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
                return false;

            bool changed = omnitrixVisualPaletteOverrides.Remove(channel.Id);
            if (changed && sync)
                SyncTransformationPaletteStateToServerOrClients();

            return changed;
        }

        public bool ResetOmnitrixVisualPalette(bool sync = true) {
            bool changed = omnitrixVisualPaletteOverrides.Count > 0;
            omnitrixVisualPaletteOverrides.Clear();
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

            string ownerId = GetActivePaletteOwnerId(transformation);
            if (!palettePresets.TryGetValue(ownerId, out PalettePresetData[] presets) || presets == null ||
                presets.Length != PalettePresetSlotCount) {
                presets = new PalettePresetData[PalettePresetSlotCount];
                palettePresets[ownerId] = presets;
            }

            PalettePresetData preset = new();
            List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntriesFor(ownerId);
            for (int i = 0; i < entries.Count; i++)
                preset.Entries.Add(entries[i]);

            List<string> enabledKeys = BuildNormalizedPaletteEnabledChannelKeysFor(ownerId);
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

            string ownerId = GetActivePaletteOwnerId(transformation);
            transformationPaletteOverrides.Remove(ownerId);
            SetAllPaletteChannelsEnabledForOwner(ownerId, false);

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

        private void NormalizeOmnitrixVisualPaletteState() {
            List<OmnitrixVisualPaletteColorEntry> entries = BuildNormalizedOmnitrixVisualPaletteEntries();
            omnitrixVisualPaletteOverrides.Clear();

            for (int i = 0; i < entries.Count; i++)
                AddNormalizedOmnitrixVisualPaletteEntry(entries[i]);
        }

        private List<TransformationPaletteColorEntry> BuildNormalizedTransformationPaletteEntries() {
            List<TransformationPaletteColorEntry> entries = new();

            foreach ((string ownerId, Dictionary<string, TransformationPaletteChannelSettings> channelSettings) in transformationPaletteOverrides) {
                if (channelSettings == null)
                    continue;

                foreach ((string channelId, TransformationPaletteChannelSettings settings) in channelSettings) {
                    if (!TryResolvePaletteChannelOwner(ownerId, channelId, out string normalizedOwnerId,
                            out TransformationPaletteChannel channel))
                        continue;

                    TransformationPaletteChannelSettings normalizedSettings =
                        NormalizePaletteSettings(settings, channel.DefaultColor);
                    if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
                        continue;

                    entries.Add(new TransformationPaletteColorEntry(normalizedOwnerId, channel.Id,
                        normalizedSettings.Color, normalizedSettings.Hue, normalizedSettings.Saturation,
                        normalizedSettings.Brightness));
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

        private List<TransformationPaletteColorEntry> BuildNormalizedTransformationPaletteEntriesFor(string ownerId) {
            if (!TryResolvePaletteOwner(ownerId, out string normalizedOwnerId, out _, out _))
                return new List<TransformationPaletteColorEntry>();

            List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntries();
            entries.RemoveAll(entry => !string.Equals(entry.TransformationId, normalizedOwnerId, StringComparison.OrdinalIgnoreCase));
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

        private List<string> BuildNormalizedPaletteEnabledChannelKeysFor(string ownerId) {
            if (!TryResolvePaletteOwner(ownerId, out string normalizedOwnerId, out _, out _))
                return new List<string>();

            List<string> enabledChannelKeys = BuildNormalizedPaletteEnabledChannelKeys();
            enabledChannelKeys.RemoveAll(key => !key.StartsWith(normalizedOwnerId + "|", StringComparison.OrdinalIgnoreCase));
            return enabledChannelKeys;
        }

        private List<OmnitrixVisualPaletteColorEntry> BuildNormalizedOmnitrixVisualPaletteEntries() {
            List<OmnitrixVisualPaletteColorEntry> entries = new();

            foreach ((string channelId, TransformationPaletteChannelSettings settings) in omnitrixVisualPaletteOverrides) {
                if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
                    continue;

                TransformationPaletteChannelSettings normalizedSettings =
                    NormalizeOmnitrixVisualPaletteSettings(settings, channel.DefaultColor);
                if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
                    continue;

                entries.Add(new OmnitrixVisualPaletteColorEntry(channel.Id, normalizedSettings.Color,
                    normalizedSettings.Hue, normalizedSettings.Saturation, normalizedSettings.Brightness));
            }

            entries.Sort(static (left, right) => string.Compare(left.ChannelId, right.ChannelId,
                StringComparison.OrdinalIgnoreCase));
            return entries;
        }

        private IEnumerable<TagCompound> BuildPalettePresetTagEntries() {
            NormalizePalettePresets();

            foreach ((string ownerId, PalettePresetData[] presets) in palettePresets) {
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
                            ["saturation"] = (int)entry.Saturation,
                            ["brightness"] = (int)entry.Brightness
                        });
                    }

                    yield return new TagCompound {
                        ["transformationId"] = ownerId,
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

            string ownerId = presetEntry.GetString("transformationId");
            int presetIndex = presetEntry.GetInt("presetIndex");
            if (!TryResolvePaletteOwner(ownerId, out string normalizedOwnerId, out _, out _) ||
                presetIndex < 0 || presetIndex >= PalettePresetSlotCount)
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
                    byte brightness = entryTag.ContainsKey("brightness")
                        ? (byte)entryTag.GetInt("brightness")
                        : TransformationPaletteColorEntry.NeutralBrightness;
                    preset.Entries.Add(new TransformationPaletteColorEntry(entryTransformationId, channelId,
                        new Color(r, g, b), hue, saturation, brightness));
                }
            }

            if (presetEntry.TryGet("enabledChannels", out string[] enabledChannels)) {
                for (int i = 0; i < enabledChannels.Length; i++)
                    preset.EnabledChannelKeys.Add(enabledChannels[i] ?? string.Empty);
            }

            if (!palettePresets.TryGetValue(normalizedOwnerId, out PalettePresetData[] presets) || presets == null ||
                presets.Length != PalettePresetSlotCount) {
                presets = new PalettePresetData[PalettePresetSlotCount];
                palettePresets[normalizedOwnerId] = presets;
            }

            presets[presetIndex] = preset;
        }

        private bool TryGetPalettePreset(string transformationId, int presetIndex, out PalettePresetData preset) {
            preset = null;
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null || presetIndex < 0 || presetIndex >= PalettePresetSlotCount)
                return false;

            string ownerId = GetActivePaletteOwnerId(transformation);
            return palettePresets.TryGetValue(ownerId, out PalettePresetData[] presets) &&
                   presets != null &&
                   presetIndex < presets.Length &&
                   (preset = presets[presetIndex]) != null;
        }

        private void SetAllPaletteChannelsEnabled(string transformationId, bool enabled) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                return;

            IReadOnlyList<TransformationPaletteChannel> channels = transformation.PaletteChannels;
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
            if (!TrySplitPaletteChannelKey(key, out string ownerId, out string channelId))
                return false;

            if (!TryResolvePaletteChannelOwner(ownerId, channelId, out string normalizedOwnerId,
                    out TransformationPaletteChannel channel))
                return false;

            normalizedKey = BuildPaletteChannelKey(normalizedOwnerId, channel.Id);
            return true;
        }

        private bool TryResolvePaletteChannel(string ownerId, string channelId,
            out string normalizedOwnerId, out TransformationPaletteChannel channel) {
            normalizedOwnerId = string.Empty;
            channel = null;

            return TryResolvePaletteChannelOwner(ownerId, channelId, out normalizedOwnerId, out channel);
        }

        private static string BuildPaletteChannelKey(string ownerId, string channelId) {
            if (string.IsNullOrWhiteSpace(ownerId) || string.IsNullOrWhiteSpace(channelId))
                return string.Empty;

            return ownerId.Trim() + "|" + channelId.Trim();
        }

        private static bool TrySplitPaletteChannelKey(string key, out string ownerId,
            out string channelId) {
            ownerId = string.Empty;
            channelId = string.Empty;

            if (string.IsNullOrWhiteSpace(key))
                return false;

            int separatorIndex = key.IndexOf('|');
            if (separatorIndex <= 0 || separatorIndex >= key.Length - 1)
                return false;

            ownerId = key[..separatorIndex].Trim();
            channelId = key[(separatorIndex + 1)..].Trim();
            return !string.IsNullOrEmpty(ownerId) && !string.IsNullOrEmpty(channelId);
        }

        private void AddNormalizedTransformationPaletteEntry(TransformationPaletteColorEntry entry) {
            if (string.IsNullOrWhiteSpace(entry.TransformationId) || string.IsNullOrWhiteSpace(entry.ChannelId))
                return;

            if (!TryResolvePaletteChannelOwner(entry.TransformationId, entry.ChannelId, out string normalizedOwnerId,
                    out TransformationPaletteChannel channel))
                return;

            TransformationPaletteChannelSettings normalizedSettings =
                NormalizePaletteSettings(new TransformationPaletteChannelSettings(entry.Color, entry.Hue,
                    entry.Saturation, entry.Brightness), channel.DefaultColor);
            if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
                return;

            if (!transformationPaletteOverrides.TryGetValue(normalizedOwnerId,
                    out Dictionary<string, TransformationPaletteChannelSettings> channelSettings)) {
                channelSettings = new Dictionary<string, TransformationPaletteChannelSettings>(StringComparer.OrdinalIgnoreCase);
                transformationPaletteOverrides[normalizedOwnerId] = channelSettings;
            }

            channelSettings[channel.Id] = normalizedSettings;
        }

        private void AddNormalizedOmnitrixVisualPaletteEntry(OmnitrixVisualPaletteColorEntry entry) {
            if (!OmnitrixVisualPalette.TryGetChannel(entry.ChannelId, out OmnitrixVisualPaletteChannel channel))
                return;

            TransformationPaletteChannelSettings normalizedSettings =
                NormalizeOmnitrixVisualPaletteSettings(new TransformationPaletteChannelSettings(entry.Color, entry.Hue,
                    entry.Saturation, entry.Brightness), channel.DefaultColor);
            if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
                return;

            omnitrixVisualPaletteOverrides[channel.Id] = normalizedSettings;
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
                settings.Saturation,
                settings.Brightness
            );
        }

        private static TransformationPaletteChannelSettings NormalizeOmnitrixVisualPaletteSettings(
            TransformationPaletteChannelSettings settings, Color fallbackColor) {
            return new TransformationPaletteChannelSettings(
                NormalizePaletteColor(settings.Color == default ? fallbackColor : settings.Color),
                settings.Hue,
                settings.Saturation,
                settings.Brightness
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

        private bool SetOmnitrixVisualPaletteSettings(string channelId, TransformationPaletteChannelSettings settings) {
            if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
                return false;

            settings = NormalizeOmnitrixVisualPaletteSettings(settings, channel.DefaultColor);
            bool isDefault = settings.Color == channel.DefaultColor && settings.HasNeutralAdjustments;
            if (isDefault)
                return omnitrixVisualPaletteOverrides.Remove(channel.Id);

            if (omnitrixVisualPaletteOverrides.TryGetValue(channel.Id,
                    out TransformationPaletteChannelSettings existingSettings) &&
                NormalizeOmnitrixVisualPaletteSettings(existingSettings, channel.DefaultColor).Equals(settings))
                return false;

            omnitrixVisualPaletteOverrides[channel.Id] = settings;
            return true;
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

        private void NormalizeSelectedTransformationCostumes() {
            Dictionary<string, string> normalizedCostumes = new(StringComparer.OrdinalIgnoreCase);
            foreach ((string transformationId, string costumeId) in selectedTransformationCostumes) {
                Transformation transformation = TransformationLoader.Resolve(transformationId);
                if (transformation == null)
                    continue;

                TransformationCostume costume = TransformationCostumeLoader.Resolve(costumeId);
                if (costume == null ||
                    !string.Equals(costume.TargetTransformationId, transformation.FullID, StringComparison.OrdinalIgnoreCase))
                    continue;

                normalizedCostumes[transformation.FullID] = costume.FullID;
            }

            selectedTransformationCostumes.Clear();
            foreach ((string transformationId, string costumeId) in normalizedCostumes)
                selectedTransformationCostumes[transformationId] = costumeId;
        }

        private IEnumerable<KeyValuePair<string, string>> BuildNormalizedSelectedTransformationCostumes() {
            NormalizeSelectedTransformationCostumes();
            List<KeyValuePair<string, string>> entries = new(selectedTransformationCostumes);
            entries.Sort(static (left, right) => string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase));
            foreach (KeyValuePair<string, string> entry in entries)
                yield return entry;
        }

        private void NormalizePalettePresets() {
            Dictionary<string, PalettePresetData[]> normalizedPresets = new(StringComparer.OrdinalIgnoreCase);

            foreach ((string ownerId, PalettePresetData[] presets) in palettePresets) {
                if (!TryResolvePaletteOwner(ownerId, out string normalizedOwnerId, out _, out _) || presets == null)
                    continue;

                PalettePresetData[] normalizedPresetArray = new PalettePresetData[PalettePresetSlotCount];
                for (int presetIndex = 0; presetIndex < Math.Min(presets.Length, PalettePresetSlotCount); presetIndex++) {
                    PalettePresetData preset = presets[presetIndex];
                    if (preset == null)
                        continue;

                    PalettePresetData normalizedPreset = new();
                    for (int i = 0; i < preset.Entries.Count; i++) {
                        TransformationPaletteColorEntry entry = preset.Entries[i];
                        if (!string.Equals(entry.TransformationId, normalizedOwnerId, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!TryResolvePaletteChannel(entry.TransformationId, entry.ChannelId, out string resolvedOwnerId,
                                out TransformationPaletteChannel channel))
                            continue;

                        TransformationPaletteChannelSettings normalizedSettings =
                            NormalizePaletteSettings(new TransformationPaletteChannelSettings(entry.Color, entry.Hue,
                                    entry.Saturation, entry.Brightness),
                                channel.DefaultColor);
                        if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
                            continue;

                        normalizedPreset.Entries.Add(new TransformationPaletteColorEntry(resolvedOwnerId, channel.Id,
                            normalizedSettings.Color, normalizedSettings.Hue, normalizedSettings.Saturation,
                            normalizedSettings.Brightness));
                    }

                    for (int i = 0; i < preset.EnabledChannelKeys.Count; i++) {
                        if (TryNormalizePaletteChannelKey(preset.EnabledChannelKeys[i], out string normalizedKey) &&
                            normalizedKey.StartsWith(normalizedOwnerId + "|", StringComparison.OrdinalIgnoreCase) &&
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
                    normalizedPresets[normalizedOwnerId] = normalizedPresetArray;
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
    }
}
