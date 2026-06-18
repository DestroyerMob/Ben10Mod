using System;
using System.Collections.Generic;
using System.Linq;
using Ben10Mod.Common.CustomVisuals;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using OmnitrixPlayerState = global::Ben10Mod.OmnitrixPlayer;

namespace Ben10Mod.Common.Omnitrix;

internal sealed class PalettePresetData {
    public List<TransformationPaletteColorEntry> Entries { get; } = new();
    public List<string> EnabledChannelKeys { get; } = new();
}

public sealed class TransformationCustomizationStore {
    internal Dictionary<string, Dictionary<string, TransformationPaletteChannelSettings>> TransformationPaletteOverrides { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, TransformationPaletteChannelSettings> OmnitrixVisualPaletteOverrides { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal HashSet<string> PaletteEnabledChannels { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, string> SelectedTransformationCostumes { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, string> CustomTransformationNames { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, PalettePresetData[]> PalettePresets { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public bool HasPaletteCustomizationData(OmnitrixPlayerState owner, string transformationId) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null)
            return false;

        if (TransformationPaletteOverrides.TryGetValue(GetActivePaletteOwnerId(transformation),
                out Dictionary<string, TransformationPaletteChannelSettings> overrides) &&
            overrides != null && overrides.Count > 0) {
            return true;
        }

        IReadOnlyList<TransformationPaletteChannel> channels = transformation.GetPaletteChannels(owner);
        for (int i = 0; i < channels.Count; i++) {
            TransformationPaletteChannel channel = channels[i];
            if (channel == null || !channel.IsValid)
                continue;

            if (!IsPaletteChannelEnabled(owner, transformation, channel.Id))
                return true;
        }

        return false;
    }

    public bool HasPaletteCustomizationData(OmnitrixPlayerState owner, Transformation transformation) {
        return transformation != null && HasPaletteCustomizationData(owner, transformation.FullID);
    }

    public string GetSelectedTransformationCustomizationSummary(OmnitrixPlayerState owner) {
        string selectedTransformationId = owner.GetSelectedTransformationId();
        if (string.IsNullOrEmpty(selectedTransformationId))
            return string.Empty;

        List<string> parts = new();
        string paletteText = GetSelectedTransformationPaletteStatusText(owner);
        if (!string.IsNullOrWhiteSpace(paletteText))
            parts.Add(paletteText);

        string costumeName = GetSelectedTransformationCostumeDisplayName(selectedTransformationId);
        if (!string.IsNullOrWhiteSpace(costumeName))
            parts.Add($"Costume: {costumeName}");

        return string.Join("  |  ", parts);
    }

    public string GetCurrentTransformationPaletteStatusText(OmnitrixPlayerState owner) {
        return BuildPaletteStatusText(owner, owner.CurrentTransformation);
    }

    public string GetSelectedTransformationPaletteStatusText(OmnitrixPlayerState owner) {
        return BuildPaletteStatusText(owner, TransformationLoader.Resolve(owner.GetSelectedTransformationId()));
    }

    public Transformation GetPaletteTargetTransformation(OmnitrixPlayerState owner) {
        if (owner.IsTransformed)
            return owner.CurrentTransformation;

        if (owner.GetActiveOmnitrix() == null)
            return null;

        return TransformationLoader.Get(owner.GetSelectedTransformationId());
    }

    public string GetPaletteTargetTransformationId(OmnitrixPlayerState owner) {
        return GetPaletteTargetTransformation(owner)?.FullID ?? string.Empty;
    }

    public string GetPaletteTargetDisplayName(OmnitrixPlayerState owner) {
        return GetPaletteTargetTransformation(owner)?.GetDisplayName(owner) ?? "No Transformation";
    }

    public TransformationCostume GetSelectedTransformationCostume(Transformation transformation) {
        if (transformation == null)
            return null;

        if (!SelectedTransformationCostumes.TryGetValue(transformation.FullID, out string costumeId) ||
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

    public bool SetSelectedTransformationCostume(OmnitrixPlayerState owner, string transformationId, string costumeId,
        bool sync = true) {
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
            bool removed = SelectedTransformationCostumes.Remove(normalizedTransformationId);
            if (removed && sync)
                owner.SyncTransformationPaletteStateToServerOrClients();
            return removed;
        }

        if (SelectedTransformationCostumes.TryGetValue(normalizedTransformationId, out string existingCostumeId) &&
            string.Equals(existingCostumeId, normalizedCostumeId, StringComparison.OrdinalIgnoreCase))
            return false;

        SelectedTransformationCostumes[normalizedTransformationId] = normalizedCostumeId;
        if (sync)
            owner.SyncTransformationPaletteStateToServerOrClients();
        return true;
    }

    public void ApplySelectedTransformationCostumeVisuals(OmnitrixPlayerState owner, Player player,
        Transformation transformation) {
        GetSelectedTransformationCostume(transformation)?.ApplyVisuals(player, owner, transformation);
    }

    public string GetCustomTransformationName(string transformationId) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null)
            return string.Empty;

        return CustomTransformationNames.TryGetValue(transformation.FullID, out string customName)
            ? customName
            : string.Empty;
    }

    public string GetCustomTransformationName(Transformation transformation) {
        return transformation == null ? string.Empty : GetCustomTransformationName(transformation.FullID);
    }

    public string GetTransformationBaseName(Transformation transformation) {
        if (transformation == null)
            return "Unknown Form";

        string customName = GetCustomTransformationName(transformation.FullID);
        return string.IsNullOrWhiteSpace(customName) ? transformation.TransformationName : customName;
    }

    public string ResolveRandomizedTransformationTarget(OmnitrixPlayerState owner, string desiredTransformationId,
        bool randomizerEnabled, out bool wasRandomized) {
        wasRandomized = false;

        Transformation desiredTransformation = TransformationLoader.Resolve(desiredTransformationId);
        if (desiredTransformation == null)
            return string.Empty;

        string desiredId = desiredTransformation.FullID;
        if (!randomizerEnabled)
            return desiredId;

        List<string> candidateIds = new();
        for (int i = 0; i < owner.unlockedTransformations.Count; i++) {
            Transformation unlockedTransformation = TransformationLoader.Resolve(owner.unlockedTransformations[i]);
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
            !string.Equals(id, owner.currentTransformationId, StringComparison.Ordinal)).ToList();

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

    public bool SetCustomTransformationName(string transformationId, string customName) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null)
            return false;

        string normalizedName = NormalizeCustomTransformationName(customName);
        if (string.IsNullOrWhiteSpace(normalizedName))
            return CustomTransformationNames.Remove(transformation.FullID);

        if (CustomTransformationNames.TryGetValue(transformation.FullID, out string existingName) &&
            string.Equals(existingName, normalizedName, StringComparison.Ordinal))
            return false;

        CustomTransformationNames[transformation.FullID] = normalizedName;
        return true;
    }

    internal string GetActivePaletteOwnerId(Transformation transformation) {
        if (transformation == null)
            return string.Empty;

        return GetSelectedTransformationCostume(transformation)?.FullID ?? transformation.FullID;
    }

    internal string GetActivePaletteOwnerId(string transformationId) {
        return GetActivePaletteOwnerId(TransformationLoader.Resolve(transformationId));
    }

    public bool IsPaletteChannelEnabled(OmnitrixPlayerState owner, Transformation transformation, string channelId) {
        if (transformation == null || string.IsNullOrWhiteSpace(channelId))
            return false;

        TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, owner);
        if (channel == null || !channel.IsValid)
            return false;

        return PaletteEnabledChannels.Contains(BuildPaletteChannelKey(GetActivePaletteOwnerId(transformation), channel.Id));
    }

    public bool IsPaletteChannelEnabled(OmnitrixPlayerState owner, string transformationId, string channelId) {
        return IsPaletteChannelEnabled(owner, TransformationLoader.Resolve(transformationId), channelId);
    }

    public bool SetPaletteChannelEnabled(OmnitrixPlayerState owner, string transformationId, string channelId,
        bool enabled, bool sync = true) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null || string.IsNullOrWhiteSpace(channelId))
            return false;

        TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, owner);
        if (channel == null || !channel.IsValid)
            return false;

        string key = BuildPaletteChannelKey(GetActivePaletteOwnerId(transformation), channel.Id);
        bool changed = enabled
            ? PaletteEnabledChannels.Add(key)
            : PaletteEnabledChannels.Remove(key);

        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public TransformationPaletteChannelSettings GetPaletteSettings(OmnitrixPlayerState owner,
        Transformation transformation, string channelId) {
        if (transformation == null || string.IsNullOrWhiteSpace(channelId))
            return new TransformationPaletteChannelSettings(Color.White);

        TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, owner);
        if (channel == null)
            return new TransformationPaletteChannelSettings(Color.White);

        string ownerId = GetActivePaletteOwnerId(transformation);
        if (TransformationPaletteOverrides.TryGetValue(ownerId,
                out Dictionary<string, TransformationPaletteChannelSettings> channelSettings) &&
            channelSettings.TryGetValue(channel.Id, out TransformationPaletteChannelSettings storedSettings)) {
            return NormalizePaletteSettings(storedSettings);
        }

        return new TransformationPaletteChannelSettings(channel.DefaultColor);
    }

    public Color GetPaletteColor(OmnitrixPlayerState owner, Transformation transformation, string channelId) {
        return GetPaletteSettings(owner, transformation, channelId).Color;
    }

    public byte GetPaletteHue(OmnitrixPlayerState owner, string transformationId, string channelId) {
        return GetPaletteSettings(owner, TransformationLoader.Resolve(transformationId), channelId).Hue;
    }

    public byte GetPaletteSaturation(OmnitrixPlayerState owner, string transformationId, string channelId) {
        return GetPaletteSettings(owner, TransformationLoader.Resolve(transformationId), channelId).Saturation;
    }

    public byte GetPaletteBrightness(OmnitrixPlayerState owner, string transformationId, string channelId) {
        return GetPaletteSettings(owner, TransformationLoader.Resolve(transformationId), channelId).Brightness;
    }

    public bool SetPaletteColor(OmnitrixPlayerState owner, string transformationId, string channelId, Color color,
        bool sync = true) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null || string.IsNullOrWhiteSpace(channelId))
            return false;

        TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, owner);
        if (channel == null || !channel.IsValid)
            return false;

        TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(owner, transformation, channel.Id);
        TransformationPaletteChannelSettings newSettings = new(color, currentSettings.Hue, currentSettings.Saturation,
            currentSettings.Brightness);
        bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel, newSettings);

        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool ResetPaletteColor(OmnitrixPlayerState owner, string transformationId, string channelId, bool sync = true) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null || string.IsNullOrWhiteSpace(channelId))
            return false;

        TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, owner);
        if (channel == null)
            return false;

        TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(owner, transformation, channel.Id);
        TransformationPaletteChannelSettings newSettings = new(channel.DefaultColor, currentSettings.Hue,
            currentSettings.Saturation, currentSettings.Brightness);
        bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel, newSettings);
        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool SetPaletteHue(OmnitrixPlayerState owner, string transformationId, string channelId, byte hue,
        bool sync = true) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null || string.IsNullOrWhiteSpace(channelId))
            return false;

        TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, owner);
        if (channel == null || !channel.IsValid)
            return false;

        TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(owner, transformation, channel.Id);
        bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel,
            new TransformationPaletteChannelSettings(currentSettings.Color, hue, currentSettings.Saturation,
                currentSettings.Brightness));
        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool SetPaletteSaturation(OmnitrixPlayerState owner, string transformationId, string channelId,
        byte saturation, bool sync = true) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null || string.IsNullOrWhiteSpace(channelId))
            return false;

        TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, owner);
        if (channel == null || !channel.IsValid)
            return false;

        TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(owner, transformation, channel.Id);
        bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel,
            new TransformationPaletteChannelSettings(currentSettings.Color, currentSettings.Hue, saturation,
                currentSettings.Brightness));
        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool SetPaletteBrightness(OmnitrixPlayerState owner, string transformationId, string channelId,
        byte brightness, bool sync = true) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null || string.IsNullOrWhiteSpace(channelId))
            return false;

        TransformationPaletteChannel channel = transformation.GetPaletteChannel(channelId, owner);
        if (channel == null || !channel.IsValid)
            return false;

        TransformationPaletteChannelSettings currentSettings = GetPaletteSettings(owner, transformation, channel.Id);
        bool changed = SetPaletteSettings(GetActivePaletteOwnerId(transformation), channel,
            new TransformationPaletteChannelSettings(currentSettings.Color, currentSettings.Hue, currentSettings.Saturation,
                brightness));
        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool ResetPaletteColors(OmnitrixPlayerState owner, string transformationId, bool sync = true) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null)
            return false;

        bool changed = TransformationPaletteOverrides.Remove(GetActivePaletteOwnerId(transformation));
        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public IReadOnlyList<OmnitrixVisualPaletteChannel> GetOmnitrixVisualPaletteChannels() {
        return OmnitrixVisualPalette.Channels;
    }

    public bool HasOmnitrixVisualPaletteCustomizationData(OmnitrixPlayerState owner) {
        return BuildNormalizedOmnitrixVisualPaletteEntries(owner).Count > 0;
    }

    public TransformationPaletteChannelSettings GetOmnitrixVisualPaletteSettings(string channelId) {
        if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
            return new TransformationPaletteChannelSettings(Color.White);

        if (OmnitrixVisualPaletteOverrides.TryGetValue(channel.Id, out TransformationPaletteChannelSettings settings))
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

        if (!OmnitrixVisualPaletteOverrides.TryGetValue(channel.Id, out TransformationPaletteChannelSettings settings))
            return fallbackColor;

        settings = NormalizeOmnitrixVisualPaletteSettings(settings, channel.DefaultColor);
        return TransformationPaletteMath.ApplyHueSaturationAndBrightness(settings.Color, settings.Hue,
            settings.Saturation, settings.Brightness);
    }

    public bool SetOmnitrixVisualPaletteColor(OmnitrixPlayerState owner, string channelId, Color color,
        bool sync = true) {
        if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
            return false;

        TransformationPaletteChannelSettings currentSettings = GetOmnitrixVisualPaletteSettings(channel.Id);
        bool changed = SetOmnitrixVisualPaletteSettings(channel.Id,
            new TransformationPaletteChannelSettings(color, currentSettings.Hue, currentSettings.Saturation,
                currentSettings.Brightness));

        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool SetOmnitrixVisualPaletteHue(OmnitrixPlayerState owner, string channelId, byte hue, bool sync = true) {
        if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
            return false;

        TransformationPaletteChannelSettings currentSettings = GetOmnitrixVisualPaletteSettings(channel.Id);
        bool changed = SetOmnitrixVisualPaletteSettings(channel.Id,
            new TransformationPaletteChannelSettings(currentSettings.Color, hue, currentSettings.Saturation,
                currentSettings.Brightness));

        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool SetOmnitrixVisualPaletteSaturation(OmnitrixPlayerState owner, string channelId, byte saturation,
        bool sync = true) {
        if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
            return false;

        TransformationPaletteChannelSettings currentSettings = GetOmnitrixVisualPaletteSettings(channel.Id);
        bool changed = SetOmnitrixVisualPaletteSettings(channel.Id,
            new TransformationPaletteChannelSettings(currentSettings.Color, currentSettings.Hue, saturation,
                currentSettings.Brightness));

        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool SetOmnitrixVisualPaletteBrightness(OmnitrixPlayerState owner, string channelId, byte brightness,
        bool sync = true) {
        if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
            return false;

        TransformationPaletteChannelSettings currentSettings = GetOmnitrixVisualPaletteSettings(channel.Id);
        bool changed = SetOmnitrixVisualPaletteSettings(channel.Id,
            new TransformationPaletteChannelSettings(currentSettings.Color, currentSettings.Hue,
                currentSettings.Saturation, brightness));

        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool ResetOmnitrixVisualPaletteChannel(OmnitrixPlayerState owner, string channelId, bool sync = true) {
        if (!OmnitrixVisualPalette.TryGetChannel(channelId, out OmnitrixVisualPaletteChannel channel))
            return false;

        bool changed = OmnitrixVisualPaletteOverrides.Remove(channel.Id);
        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool ResetOmnitrixVisualPalette(OmnitrixPlayerState owner, bool sync = true) {
        bool changed = OmnitrixVisualPaletteOverrides.Count > 0;
        OmnitrixVisualPaletteOverrides.Clear();
        if (changed && sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return changed;
    }

    public bool HasPalettePreset(OmnitrixPlayerState owner, string transformationId, int presetIndex) {
        return TryGetPalettePreset(owner, transformationId, presetIndex, out _);
    }

    public string GetPalettePresetLabel(OmnitrixPlayerState owner, string transformationId, int presetIndex) {
        int displayIndex = Utils.Clamp(presetIndex, 0, OmnitrixPlayerState.PalettePresetSlotCount - 1) + 1;
        return HasPalettePreset(owner, transformationId, presetIndex)
            ? $"Preset {displayIndex}"
            : $"Preset {displayIndex} (Empty)";
    }

    public bool SavePalettePreset(OmnitrixPlayerState owner, string transformationId, int presetIndex) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null || presetIndex < 0 || presetIndex >= OmnitrixPlayerState.PalettePresetSlotCount)
            return false;

        string ownerId = GetActivePaletteOwnerId(transformation);
        if (!PalettePresets.TryGetValue(ownerId, out PalettePresetData[] presets) || presets == null ||
            presets.Length != OmnitrixPlayerState.PalettePresetSlotCount) {
            presets = new PalettePresetData[OmnitrixPlayerState.PalettePresetSlotCount];
            PalettePresets[ownerId] = presets;
        }

        PalettePresetData preset = new();
        List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntriesFor(owner, ownerId);
        for (int i = 0; i < entries.Count; i++)
            preset.Entries.Add(entries[i]);

        List<string> enabledKeys = BuildNormalizedPaletteEnabledChannelKeysFor(owner, ownerId);
        for (int i = 0; i < enabledKeys.Count; i++)
            preset.EnabledChannelKeys.Add(enabledKeys[i]);

        presets[presetIndex] = preset;
        return true;
    }

    public bool ApplyPalettePreset(OmnitrixPlayerState owner, string transformationId, int presetIndex, bool sync = true) {
        if (!TryGetPalettePreset(owner, transformationId, presetIndex, out PalettePresetData preset))
            return false;

        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null)
            return false;

        string ownerId = GetActivePaletteOwnerId(transformation);
        TransformationPaletteOverrides.Remove(ownerId);
        SetAllPaletteChannelsEnabledForOwner(owner, ownerId, false);

        for (int i = 0; i < preset.Entries.Count; i++)
            AddNormalizedTransformationPaletteEntry(owner, preset.Entries[i]);

        for (int i = 0; i < preset.EnabledChannelKeys.Count; i++) {
            if (TryNormalizePaletteChannelKey(owner, preset.EnabledChannelKeys[i], out string normalizedKey))
                PaletteEnabledChannels.Add(normalizedKey);
        }

        NormalizeTransformationPaletteState(owner);
        if (sync)
            owner.SyncTransformationPaletteStateToServerOrClients();

        return true;
    }

    internal void NormalizeTransformationPaletteState(OmnitrixPlayerState owner) {
        List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntries(owner);
        List<string> enabledChannelKeys = BuildNormalizedPaletteEnabledChannelKeys(owner);
        TransformationPaletteOverrides.Clear();
        PaletteEnabledChannels.Clear();

        for (int i = 0; i < entries.Count; i++)
            AddNormalizedTransformationPaletteEntry(owner, entries[i]);

        for (int i = 0; i < enabledChannelKeys.Count; i++)
            PaletteEnabledChannels.Add(enabledChannelKeys[i]);
    }

    internal void NormalizeOmnitrixVisualPaletteState(OmnitrixPlayerState owner) {
        List<OmnitrixVisualPaletteColorEntry> entries = BuildNormalizedOmnitrixVisualPaletteEntries(owner);
        OmnitrixVisualPaletteOverrides.Clear();

        for (int i = 0; i < entries.Count; i++)
            AddNormalizedOmnitrixVisualPaletteEntry(entries[i]);
    }

    internal List<TransformationPaletteColorEntry> BuildNormalizedTransformationPaletteEntries(
        OmnitrixPlayerState owner) {
        List<TransformationPaletteColorEntry> entries = new();

        foreach ((string ownerId, Dictionary<string, TransformationPaletteChannelSettings> channelSettings) in TransformationPaletteOverrides) {
            if (channelSettings == null)
                continue;

            foreach ((string channelId, TransformationPaletteChannelSettings settings) in channelSettings) {
                if (!TryResolvePaletteChannelOwner(owner, ownerId, channelId, out string normalizedOwnerId,
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

    internal List<TransformationPaletteColorEntry> BuildNormalizedTransformationPaletteEntriesFor(
        OmnitrixPlayerState owner, string ownerId) {
        if (!TryResolvePaletteOwner(owner, ownerId, out string normalizedOwnerId, out _, out _))
            return new List<TransformationPaletteColorEntry>();

        List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntries(owner);
        entries.RemoveAll(entry => !string.Equals(entry.TransformationId, normalizedOwnerId, StringComparison.OrdinalIgnoreCase));
        return entries;
    }

    internal List<string> BuildNormalizedPaletteEnabledChannelKeys(OmnitrixPlayerState owner) {
        List<string> enabledChannelKeys = new();

        foreach (string enabledChannelKey in PaletteEnabledChannels) {
            if (TryNormalizePaletteChannelKey(owner, enabledChannelKey, out string normalizedKey))
                enabledChannelKeys.Add(normalizedKey);
        }

        enabledChannelKeys.Sort(StringComparer.OrdinalIgnoreCase);
        return enabledChannelKeys;
    }

    internal List<string> BuildNormalizedPaletteEnabledChannelKeysFor(OmnitrixPlayerState owner, string ownerId) {
        if (!TryResolvePaletteOwner(owner, ownerId, out string normalizedOwnerId, out _, out _))
            return new List<string>();

        List<string> enabledChannelKeys = BuildNormalizedPaletteEnabledChannelKeys(owner);
        enabledChannelKeys.RemoveAll(key => !key.StartsWith(normalizedOwnerId + "|", StringComparison.OrdinalIgnoreCase));
        return enabledChannelKeys;
    }

    internal List<OmnitrixVisualPaletteColorEntry> BuildNormalizedOmnitrixVisualPaletteEntries(
        OmnitrixPlayerState owner) {
        List<OmnitrixVisualPaletteColorEntry> entries = new();

        foreach ((string channelId, TransformationPaletteChannelSettings settings) in OmnitrixVisualPaletteOverrides) {
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

    internal IEnumerable<TagCompound> BuildPalettePresetTagEntries(OmnitrixPlayerState owner) {
        NormalizePalettePresets(owner);

        foreach ((string ownerId, PalettePresetData[] presets) in PalettePresets) {
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

    internal void ClearPaletteSyncState() {
        TransformationPaletteOverrides.Clear();
        OmnitrixVisualPaletteOverrides.Clear();
        PaletteEnabledChannels.Clear();
        SelectedTransformationCostumes.Clear();
    }

    internal void ClearAllState() {
        ClearPaletteSyncState();
        CustomTransformationNames.Clear();
        PalettePresets.Clear();
    }

    internal void AddNormalizedPaletteEnabledChannelKey(OmnitrixPlayerState owner, string key) {
        if (TryNormalizePaletteChannelKey(owner, key, out string normalizedKey))
            PaletteEnabledChannels.Add(normalizedKey);
    }

    internal void LoadPalettePresetTagEntry(OmnitrixPlayerState owner, TagCompound presetEntry) {
        if (presetEntry == null)
            return;

        string ownerId = presetEntry.GetString("transformationId");
        int presetIndex = presetEntry.GetInt("presetIndex");
        if (!TryResolvePaletteOwner(owner, ownerId, out string normalizedOwnerId, out _, out _) ||
            presetIndex < 0 || presetIndex >= OmnitrixPlayerState.PalettePresetSlotCount)
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

        if (!PalettePresets.TryGetValue(normalizedOwnerId, out PalettePresetData[] presets) || presets == null ||
            presets.Length != OmnitrixPlayerState.PalettePresetSlotCount) {
            presets = new PalettePresetData[OmnitrixPlayerState.PalettePresetSlotCount];
            PalettePresets[normalizedOwnerId] = presets;
        }

        presets[presetIndex] = preset;
    }

    internal bool TryNormalizePaletteChannelKey(OmnitrixPlayerState owner, string key, out string normalizedKey) {
        normalizedKey = string.Empty;
        if (!TrySplitPaletteChannelKey(key, out string ownerId, out string channelId))
            return false;

        if (!TryResolvePaletteChannelOwner(owner, ownerId, channelId, out string normalizedOwnerId,
                out TransformationPaletteChannel channel))
            return false;

        normalizedKey = BuildPaletteChannelKey(normalizedOwnerId, channel.Id);
        return true;
    }

    internal bool TryResolvePaletteChannel(OmnitrixPlayerState owner, string ownerId, string channelId,
        out string normalizedOwnerId, out TransformationPaletteChannel channel) {
        normalizedOwnerId = string.Empty;
        channel = null;

        return TryResolvePaletteChannelOwner(owner, ownerId, channelId, out normalizedOwnerId, out channel);
    }

    internal void AddNormalizedTransformationPaletteEntry(OmnitrixPlayerState owner,
        TransformationPaletteColorEntry entry) {
        if (string.IsNullOrWhiteSpace(entry.TransformationId) || string.IsNullOrWhiteSpace(entry.ChannelId))
            return;

        if (!TryResolvePaletteChannelOwner(owner, entry.TransformationId, entry.ChannelId, out string normalizedOwnerId,
                out TransformationPaletteChannel channel))
            return;

        TransformationPaletteChannelSettings normalizedSettings =
            NormalizePaletteSettings(new TransformationPaletteChannelSettings(entry.Color, entry.Hue,
                entry.Saturation, entry.Brightness), channel.DefaultColor);
        if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
            return;

        if (!TransformationPaletteOverrides.TryGetValue(normalizedOwnerId,
                out Dictionary<string, TransformationPaletteChannelSettings> channelSettings)) {
            channelSettings = new Dictionary<string, TransformationPaletteChannelSettings>(StringComparer.OrdinalIgnoreCase);
            TransformationPaletteOverrides[normalizedOwnerId] = channelSettings;
        }

        channelSettings[channel.Id] = normalizedSettings;
    }

    internal void AddNormalizedOmnitrixVisualPaletteEntry(OmnitrixVisualPaletteColorEntry entry) {
        if (!OmnitrixVisualPalette.TryGetChannel(entry.ChannelId, out OmnitrixVisualPaletteChannel channel))
            return;

        TransformationPaletteChannelSettings normalizedSettings =
            NormalizeOmnitrixVisualPaletteSettings(new TransformationPaletteChannelSettings(entry.Color, entry.Hue,
                entry.Saturation, entry.Brightness), channel.DefaultColor);
        if (normalizedSettings.Color == channel.DefaultColor && normalizedSettings.HasNeutralAdjustments)
            return;

        OmnitrixVisualPaletteOverrides[channel.Id] = normalizedSettings;
    }

    internal void LoadLegacyPaletteDisabledChannels(OmnitrixPlayerState owner, string[] disabledPaletteArray) {
        if (disabledPaletteArray == null)
            return;

        HashSet<string> normalizedDisabledKeys = new(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < disabledPaletteArray.Length; i++) {
            if (TryNormalizePaletteChannelKey(owner, disabledPaletteArray[i], out string normalizedDisabledKey))
                normalizedDisabledKeys.Add(normalizedDisabledKey);
        }

        foreach (Transformation transformation in TransformationLoader.All) {
            if (transformation == null)
                continue;

            IReadOnlyList<TransformationPaletteChannel> channels = transformation.GetPaletteChannels(owner);
            for (int i = 0; i < channels.Count; i++) {
                TransformationPaletteChannel channel = channels[i];
                if (channel == null || !channel.IsValid)
                    continue;

                string key = BuildPaletteChannelKey(transformation.FullID, channel.Id);
                if (!normalizedDisabledKeys.Contains(key))
                    PaletteEnabledChannels.Add(key);
            }
        }
    }

    internal void SetAllPaletteChannelsEnabled(OmnitrixPlayerState owner, string transformationId, bool enabled) {
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
                PaletteEnabledChannels.Add(key);
            else
                PaletteEnabledChannels.Remove(key);
        }
    }

    internal void NormalizeSelectedTransformationCostumes() {
        Dictionary<string, string> normalizedCostumes = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string transformationId, string costumeId) in SelectedTransformationCostumes) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                continue;

            TransformationCostume costume = TransformationCostumeLoader.Resolve(costumeId);
            if (costume == null ||
                !string.Equals(costume.TargetTransformationId, transformation.FullID, StringComparison.OrdinalIgnoreCase))
                continue;

            normalizedCostumes[transformation.FullID] = costume.FullID;
        }

        SelectedTransformationCostumes.Clear();
        foreach ((string transformationId, string costumeId) in normalizedCostumes)
            SelectedTransformationCostumes[transformationId] = costumeId;
    }

    internal IEnumerable<KeyValuePair<string, string>> BuildNormalizedSelectedTransformationCostumes() {
        NormalizeSelectedTransformationCostumes();
        List<KeyValuePair<string, string>> entries = new(SelectedTransformationCostumes);
        entries.Sort(static (left, right) => string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase));
        foreach (KeyValuePair<string, string> entry in entries)
            yield return entry;
    }

    internal void NormalizePalettePresets(OmnitrixPlayerState owner) {
        Dictionary<string, PalettePresetData[]> normalizedPresets = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string ownerId, PalettePresetData[] presets) in PalettePresets) {
            if (!TryResolvePaletteOwner(owner, ownerId, out string normalizedOwnerId, out _, out _) || presets == null)
                continue;

            PalettePresetData[] normalizedPresetArray = new PalettePresetData[OmnitrixPlayerState.PalettePresetSlotCount];
            for (int presetIndex = 0; presetIndex < Math.Min(presets.Length, OmnitrixPlayerState.PalettePresetSlotCount); presetIndex++) {
                PalettePresetData preset = presets[presetIndex];
                if (preset == null)
                    continue;

                PalettePresetData normalizedPreset = new();
                for (int i = 0; i < preset.Entries.Count; i++) {
                    TransformationPaletteColorEntry entry = preset.Entries[i];
                    if (!string.Equals(entry.TransformationId, normalizedOwnerId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!TryResolvePaletteChannel(owner, entry.TransformationId, entry.ChannelId, out string resolvedOwnerId,
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
                    if (TryNormalizePaletteChannelKey(owner, preset.EnabledChannelKeys[i], out string normalizedKey) &&
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

        PalettePresets.Clear();
        foreach ((string transformationId, PalettePresetData[] presets) in normalizedPresets)
            PalettePresets[transformationId] = presets;
    }

    internal void NormalizeCustomTransformationNames() {
        Dictionary<string, string> normalizedNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string> entry in CustomTransformationNames) {
            Transformation transformation = TransformationLoader.Resolve(entry.Key);
            if (transformation == null)
                continue;

            string normalizedName = NormalizeCustomTransformationName(entry.Value);
            if (string.IsNullOrWhiteSpace(normalizedName))
                continue;

            normalizedNames[transformation.FullID] = normalizedName;
        }

        CustomTransformationNames.Clear();
        foreach (KeyValuePair<string, string> entry in normalizedNames)
            CustomTransformationNames[entry.Key] = entry.Value;
    }

    internal IEnumerable<KeyValuePair<string, string>> BuildNormalizedCustomTransformationNames() {
        NormalizeCustomTransformationNames();
        foreach (KeyValuePair<string, string> entry in CustomTransformationNames)
            yield return entry;
    }

    private string BuildPaletteStatusText(OmnitrixPlayerState owner, Transformation transformation) {
        if (transformation == null)
            return string.Empty;

        if (!transformation.SupportsPaletteCustomization(owner))
            return "Palette: None";

        IReadOnlyList<TransformationPaletteChannel> channels = transformation.GetPaletteChannels(owner);
        int enabledCount = 0;
        int validChannelCount = 0;
        for (int i = 0; i < channels.Count; i++) {
            TransformationPaletteChannel channel = channels[i];
            if (channel == null || !channel.IsValid)
                continue;

            validChannelCount++;
            if (IsPaletteChannelEnabled(owner, transformation, channel.Id))
                enabledCount++;
        }

        if (validChannelCount == 0)
            return "Palette: None";

        if (!HasPaletteCustomizationData(owner, transformation))
            return "Palette: Default";

        if (enabledCount <= 0)
            return "Palette: Original";

        if (enabledCount >= validChannelCount)
            return "Palette: Custom";

        return "Palette: Mixed";
    }

    private bool TryResolvePaletteOwner(OmnitrixPlayerState owner, string ownerId, out string normalizedOwnerId,
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

    private IReadOnlyList<TransformationPaletteChannel> GetPaletteChannelsForOwner(OmnitrixPlayerState owner,
        string ownerId) {
        if (!TryResolvePaletteOwner(owner, ownerId, out _, out Transformation transformation, out TransformationCostume costume))
            return Array.Empty<TransformationPaletteChannel>();

        return costume?.GetMergedPaletteChannels(transformation, owner) ?? transformation.PaletteChannels;
    }

    private void SetAllPaletteChannelsEnabledForOwner(OmnitrixPlayerState owner, string ownerId, bool enabled) {
        IReadOnlyList<TransformationPaletteChannel> channels = GetPaletteChannelsForOwner(owner, ownerId);
        for (int i = 0; i < channels.Count; i++) {
            TransformationPaletteChannel channel = channels[i];
            if (channel == null || !channel.IsValid)
                continue;

            string key = BuildPaletteChannelKey(ownerId, channel.Id);
            if (enabled)
                PaletteEnabledChannels.Add(key);
            else
                PaletteEnabledChannels.Remove(key);
        }
    }

    private bool TryResolvePaletteChannelOwner(OmnitrixPlayerState owner, string ownerId, string channelId,
        out string normalizedOwnerId, out TransformationPaletteChannel channel) {
        normalizedOwnerId = string.Empty;
        channel = null;

        if (string.IsNullOrWhiteSpace(channelId) ||
            !TryResolvePaletteOwner(owner, ownerId, out normalizedOwnerId, out Transformation transformation,
                out TransformationCostume costume))
            return false;

        if (costume != null) {
            channel = costume.GetMergedPaletteChannel(transformation, channelId, owner);
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

    private bool TryGetPalettePreset(OmnitrixPlayerState owner, string transformationId, int presetIndex,
        out PalettePresetData preset) {
        preset = null;
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null || presetIndex < 0 || presetIndex >= OmnitrixPlayerState.PalettePresetSlotCount)
            return false;

        string ownerId = GetActivePaletteOwnerId(transformation);
        return PalettePresets.TryGetValue(ownerId, out PalettePresetData[] presets) &&
               presets != null &&
               presetIndex < presets.Length &&
               (preset = presets[presetIndex]) != null;
    }

    private static string BuildPaletteChannelKey(string ownerId, string channelId) {
        if (string.IsNullOrWhiteSpace(ownerId) || string.IsNullOrWhiteSpace(channelId))
            return string.Empty;

        return ownerId.Trim() + "|" + channelId.Trim();
    }

    private static bool TrySplitPaletteChannelKey(string key, out string ownerId, out string channelId) {
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

    private bool RemovePaletteOverride(string transformationId, string channelId) {
        if (!TransformationPaletteOverrides.TryGetValue(transformationId,
                out Dictionary<string, TransformationPaletteChannelSettings> channelColors))
            return false;

        bool removed = channelColors.Remove(channelId);
        if (channelColors.Count == 0)
            TransformationPaletteOverrides.Remove(transformationId);

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

        if (!TransformationPaletteOverrides.TryGetValue(transformationId,
                out Dictionary<string, TransformationPaletteChannelSettings> channelSettings)) {
            channelSettings = new Dictionary<string, TransformationPaletteChannelSettings>(StringComparer.OrdinalIgnoreCase);
            TransformationPaletteOverrides[transformationId] = channelSettings;
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
            return OmnitrixVisualPaletteOverrides.Remove(channel.Id);

        if (OmnitrixVisualPaletteOverrides.TryGetValue(channel.Id,
                out TransformationPaletteChannelSettings existingSettings) &&
            NormalizeOmnitrixVisualPaletteSettings(existingSettings, channel.DefaultColor).Equals(settings))
            return false;

        OmnitrixVisualPaletteOverrides[channel.Id] = settings;
        return true;
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
        if (sanitizedName.Length > OmnitrixPlayerState.MaxCustomTransformationNameLength)
            sanitizedName = sanitizedName[..OmnitrixPlayerState.MaxCustomTransformationNameLength].TrimEnd();

        return sanitizedName;
    }
}
