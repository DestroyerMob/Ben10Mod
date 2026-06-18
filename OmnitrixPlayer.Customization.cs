using System.Collections.Generic;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        public bool HasPaletteCustomizationData(string transformationId) {
            return Customization.HasPaletteCustomizationData(this, transformationId);
        }

        public bool HasPaletteCustomizationData(Transformation transformation) {
            return Customization.HasPaletteCustomizationData(this, transformation);
        }

        public string GetSelectedTransformationCustomizationSummary() {
            return Customization.GetSelectedTransformationCustomizationSummary(this);
        }

        public string GetCurrentTransformationPaletteStatusText() {
            return Customization.GetCurrentTransformationPaletteStatusText(this);
        }

        public string GetSelectedTransformationPaletteStatusText() {
            return Customization.GetSelectedTransformationPaletteStatusText(this);
        }

        public Transformation GetPaletteTargetTransformation() {
            return Customization.GetPaletteTargetTransformation(this);
        }

        public string GetPaletteTargetTransformationId() {
            return Customization.GetPaletteTargetTransformationId(this);
        }

        public string GetPaletteTargetDisplayName() {
            return Customization.GetPaletteTargetDisplayName(this);
        }

        public TransformationCostume GetSelectedTransformationCostume(Transformation transformation) {
            return Customization.GetSelectedTransformationCostume(transformation);
        }

        public string GetSelectedTransformationCostumeId(string transformationId) {
            return Customization.GetSelectedTransformationCostumeId(transformationId);
        }

        public string GetSelectedTransformationCostumeDisplayName(string transformationId) {
            return Customization.GetSelectedTransformationCostumeDisplayName(transformationId);
        }

        public bool SetSelectedTransformationCostume(string transformationId, string costumeId, bool sync = true) {
            return Customization.SetSelectedTransformationCostume(this, transformationId, costumeId, sync);
        }

        internal void ApplySelectedTransformationCostumeVisuals(Player player, Transformation transformation) {
            Customization.ApplySelectedTransformationCostumeVisuals(this, player, transformation);
        }

        public string GetCustomTransformationName(string transformationId) {
            return Customization.GetCustomTransformationName(transformationId);
        }

        public string GetCustomTransformationName(Transformation transformation) {
            return Customization.GetCustomTransformationName(transformation);
        }

        public string GetTransformationBaseName(string transformationId) {
            return GetTransformationBaseName(TransformationLoader.Resolve(transformationId));
        }

        public string GetTransformationBaseName(Transformation transformation) {
            return Customization.GetTransformationBaseName(transformation);
        }

        public bool IsTransformationRandomizerEnabled() {
            return Main.netMode != NetmodeID.Server &&
                   ModContent.GetInstance<Ben10ClientConfig>().EnableTransformationRandomizer;
        }

        public string ResolveRandomizedTransformationTarget(string desiredTransformationId, out bool wasRandomized) {
            return Customization.ResolveRandomizedTransformationTarget(this, desiredTransformationId,
                IsTransformationRandomizerEnabled(), out wasRandomized);
        }

        public void ShowTransformationRandomizerFeedback(string transformationId) {
            if (Main.netMode == NetmodeID.Server || Player.whoAmI != Main.myPlayer)
                return;

            string formName = GetTransformationBaseName(transformationId);
            Main.NewText($"Mistransformed into {formName}!", new Color(255, 205, 95));
        }

        public bool SetCustomTransformationName(string transformationId, string customName) {
            return Customization.SetCustomTransformationName(transformationId, customName);
        }

        public bool IsPaletteChannelEnabled(Transformation transformation, string channelId) {
            return Customization.IsPaletteChannelEnabled(this, transformation, channelId);
        }

        public bool IsPaletteChannelEnabled(string transformationId, string channelId) {
            return Customization.IsPaletteChannelEnabled(this, transformationId, channelId);
        }

        public bool SetPaletteChannelEnabled(string transformationId, string channelId, bool enabled, bool sync = true) {
            return Customization.SetPaletteChannelEnabled(this, transformationId, channelId, enabled, sync);
        }

        public TransformationPaletteChannelSettings GetPaletteSettings(Transformation transformation, string channelId) {
            return Customization.GetPaletteSettings(this, transformation, channelId);
        }

        public Color GetPaletteColor(Transformation transformation, string channelId) {
            return Customization.GetPaletteColor(this, transformation, channelId);
        }

        public byte GetPaletteHue(string transformationId, string channelId) {
            return Customization.GetPaletteHue(this, transformationId, channelId);
        }

        public byte GetPaletteSaturation(string transformationId, string channelId) {
            return Customization.GetPaletteSaturation(this, transformationId, channelId);
        }

        public byte GetPaletteBrightness(string transformationId, string channelId) {
            return Customization.GetPaletteBrightness(this, transformationId, channelId);
        }

        public bool SetPaletteColor(string transformationId, string channelId, Color color, bool sync = true) {
            return Customization.SetPaletteColor(this, transformationId, channelId, color, sync);
        }

        public bool ResetPaletteColor(string transformationId, string channelId, bool sync = true) {
            return Customization.ResetPaletteColor(this, transformationId, channelId, sync);
        }

        public bool SetPaletteHue(string transformationId, string channelId, byte hue, bool sync = true) {
            return Customization.SetPaletteHue(this, transformationId, channelId, hue, sync);
        }

        public bool SetPaletteSaturation(string transformationId, string channelId, byte saturation, bool sync = true) {
            return Customization.SetPaletteSaturation(this, transformationId, channelId, saturation, sync);
        }

        public bool SetPaletteBrightness(string transformationId, string channelId, byte brightness, bool sync = true) {
            return Customization.SetPaletteBrightness(this, transformationId, channelId, brightness, sync);
        }

        public bool ResetPaletteColors(string transformationId, bool sync = true) {
            return Customization.ResetPaletteColors(this, transformationId, sync);
        }

        public IReadOnlyList<OmnitrixVisualPaletteChannel> GetOmnitrixVisualPaletteChannels() {
            return Customization.GetOmnitrixVisualPaletteChannels();
        }

        public bool HasOmnitrixVisualPaletteCustomizationData() {
            return Customization.HasOmnitrixVisualPaletteCustomizationData(this);
        }

        public TransformationPaletteChannelSettings GetOmnitrixVisualPaletteSettings(string channelId) {
            return Customization.GetOmnitrixVisualPaletteSettings(channelId);
        }

        public Color GetOmnitrixVisualColor(string channelId) {
            return Customization.GetOmnitrixVisualColor(channelId);
        }

        public Color GetOmnitrixVisualColor(string channelId, Color fallbackColor) {
            return Customization.GetOmnitrixVisualColor(channelId, fallbackColor);
        }

        public bool SetOmnitrixVisualPaletteColor(string channelId, Color color, bool sync = true) {
            return Customization.SetOmnitrixVisualPaletteColor(this, channelId, color, sync);
        }

        public bool SetOmnitrixVisualPaletteHue(string channelId, byte hue, bool sync = true) {
            return Customization.SetOmnitrixVisualPaletteHue(this, channelId, hue, sync);
        }

        public bool SetOmnitrixVisualPaletteSaturation(string channelId, byte saturation, bool sync = true) {
            return Customization.SetOmnitrixVisualPaletteSaturation(this, channelId, saturation, sync);
        }

        public bool SetOmnitrixVisualPaletteBrightness(string channelId, byte brightness, bool sync = true) {
            return Customization.SetOmnitrixVisualPaletteBrightness(this, channelId, brightness, sync);
        }

        public bool ResetOmnitrixVisualPaletteChannel(string channelId, bool sync = true) {
            return Customization.ResetOmnitrixVisualPaletteChannel(this, channelId, sync);
        }

        public bool ResetOmnitrixVisualPalette(bool sync = true) {
            return Customization.ResetOmnitrixVisualPalette(this, sync);
        }

        public bool HasPalettePreset(string transformationId, int presetIndex) {
            return Customization.HasPalettePreset(this, transformationId, presetIndex);
        }

        public string GetPalettePresetLabel(string transformationId, int presetIndex) {
            return Customization.GetPalettePresetLabel(this, transformationId, presetIndex);
        }

        public bool SavePalettePreset(string transformationId, int presetIndex) {
            return Customization.SavePalettePreset(this, transformationId, presetIndex);
        }

        public bool ApplyPalettePreset(string transformationId, int presetIndex, bool sync = true) {
            return Customization.ApplyPalettePreset(this, transformationId, presetIndex, sync);
        }

        private string GetActivePaletteOwnerId(Transformation transformation) {
            return Customization.GetActivePaletteOwnerId(transformation);
        }

        private string GetActivePaletteOwnerId(string transformationId) {
            return Customization.GetActivePaletteOwnerId(transformationId);
        }

        private void NormalizeTransformationPaletteState() {
            Customization.NormalizeTransformationPaletteState(this);
        }

        private void NormalizeOmnitrixVisualPaletteState() {
            Customization.NormalizeOmnitrixVisualPaletteState(this);
        }

        private List<TransformationPaletteColorEntry> BuildNormalizedTransformationPaletteEntries() {
            return Customization.BuildNormalizedTransformationPaletteEntries(this);
        }

        private List<TransformationPaletteColorEntry> BuildNormalizedTransformationPaletteEntriesFor(string ownerId) {
            return Customization.BuildNormalizedTransformationPaletteEntriesFor(this, ownerId);
        }

        private List<string> BuildNormalizedPaletteEnabledChannelKeys() {
            return Customization.BuildNormalizedPaletteEnabledChannelKeys(this);
        }

        private List<string> BuildNormalizedPaletteEnabledChannelKeysFor(string ownerId) {
            return Customization.BuildNormalizedPaletteEnabledChannelKeysFor(this, ownerId);
        }

        private List<OmnitrixVisualPaletteColorEntry> BuildNormalizedOmnitrixVisualPaletteEntries() {
            return Customization.BuildNormalizedOmnitrixVisualPaletteEntries(this);
        }

        private IEnumerable<TagCompound> BuildPalettePresetTagEntries() {
            return Customization.BuildPalettePresetTagEntries(this);
        }

        private void ClearTransformationPaletteSyncState() {
            Customization.ClearPaletteSyncState();
        }

        private void ClearAllCustomizationState() {
            Customization.ClearAllState();
        }

        private void AddNormalizedPaletteEnabledChannelKey(string key) {
            Customization.AddNormalizedPaletteEnabledChannelKey(this, key);
        }

        private void LoadPalettePresetTagEntry(TagCompound presetEntry) {
            Customization.LoadPalettePresetTagEntry(this, presetEntry);
        }

        private bool TryNormalizePaletteChannelKey(string key, out string normalizedKey) {
            return Customization.TryNormalizePaletteChannelKey(this, key, out normalizedKey);
        }

        private bool TryResolvePaletteChannel(string ownerId, string channelId,
            out string normalizedOwnerId, out TransformationPaletteChannel channel) {
            return Customization.TryResolvePaletteChannel(this, ownerId, channelId, out normalizedOwnerId, out channel);
        }

        private void AddNormalizedTransformationPaletteEntry(TransformationPaletteColorEntry entry) {
            Customization.AddNormalizedTransformationPaletteEntry(this, entry);
        }

        private void AddNormalizedOmnitrixVisualPaletteEntry(OmnitrixVisualPaletteColorEntry entry) {
            Customization.AddNormalizedOmnitrixVisualPaletteEntry(entry);
        }

        private void LoadLegacyPaletteDisabledChannels(string[] disabledPaletteArray) {
            Customization.LoadLegacyPaletteDisabledChannels(this, disabledPaletteArray);
        }

        private void SetAllPaletteChannelsEnabled(string transformationId, bool enabled) {
            Customization.SetAllPaletteChannelsEnabled(this, transformationId, enabled);
        }

        private void NormalizeSelectedTransformationCostumes() {
            Customization.NormalizeSelectedTransformationCostumes();
        }

        private IEnumerable<KeyValuePair<string, string>> BuildNormalizedSelectedTransformationCostumes() {
            return Customization.BuildNormalizedSelectedTransformationCostumes();
        }

        private void NormalizePalettePresets() {
            Customization.NormalizePalettePresets(this);
        }

        private void NormalizeCustomTransformationNames() {
            Customization.NormalizeCustomTransformationNames();
        }

        private IEnumerable<KeyValuePair<string, string>> BuildNormalizedCustomTransformationNames() {
            return Customization.BuildNormalizedCustomTransformationNames();
        }
    }
}
