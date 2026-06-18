using System;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Ben10Mod.Content.Transformations;

namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        public override void SaveData(TagCompound tag) {
            NormalizeStoredTransformationData();
            tag["masterControl"] = masterControl;
            tag["omnitrixEnergy"] = omnitrixEnergy;
            tag["completedOmnitrixRevivalCooldown"] = completedOmnitrixRevivalCooldown;
            tag["absorbedMaterialItemType"] = absorbedMaterialItemType;
            tag["absorbedMaterialTime"] = absorbedMaterialTime;
            tag["transformationSpeedBoostPercent"] = (int)transformationSpeedBoostPercent;

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
                    ["saturation"] = (int)entry.Saturation,
                    ["brightness"] = (int)entry.Brightness
                });
            }

            tag["transformationPalette"] = paletteEntries;
            tag["paletteEnabledChannels"] = BuildNormalizedPaletteEnabledChannelKeys().ToArray();

            List<TagCompound> visualPaletteEntries = new();
            foreach (OmnitrixVisualPaletteColorEntry entry in BuildNormalizedOmnitrixVisualPaletteEntries()) {
                visualPaletteEntries.Add(new TagCompound {
                    ["channelId"] = entry.ChannelId,
                    ["r"] = (int)entry.Color.R,
                    ["g"] = (int)entry.Color.G,
                    ["b"] = (int)entry.Color.B,
                    ["hue"] = (int)entry.Hue,
                    ["saturation"] = (int)entry.Saturation,
                    ["brightness"] = (int)entry.Brightness
                });
            }

            tag["omnitrixVisualPalette"] = visualPaletteEntries;

            List<TagCompound> costumeEntries = new();
            foreach (KeyValuePair<string, string> entry in BuildNormalizedSelectedTransformationCostumes()) {
                costumeEntries.Add(new TagCompound {
                    ["transformationId"] = entry.Key,
                    ["costumeId"] = entry.Value
                });
            }

            tag["selectedTransformationCostumes"] = costumeEntries;

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
            omnitrixEnergy = tag.TryGet("omnitrixEnergy", out float savedOmnitrixEnergy)
                ? savedOmnitrixEnergy
                : 0f;
            completedOmnitrixRevivalCooldown = tag.TryGet("completedOmnitrixRevivalCooldown",
                out int savedCompletedOmnitrixRevivalCooldown)
                ? Math.Max(0, savedCompletedOmnitrixRevivalCooldown)
                : 0;
            absorbedMaterialItemType = tag.TryGet("absorbedMaterialItemType", out int savedAbsorbedMaterialItemType)
                ? savedAbsorbedMaterialItemType
                : 0;
            absorbedMaterialTime = tag.TryGet("absorbedMaterialTime", out int savedAbsorbedMaterialTime)
                ? savedAbsorbedMaterialTime
                : 0;
            transformationSpeedBoostPercent = tag.TryGet("transformationSpeedBoostPercent", out int savedTransformationSpeedBoostPercent)
                ? NormalizeTransformationSpeedBoostPercent(savedTransformationSpeedBoostPercent)
                : DefaultTransformationSpeedBoostPercent;

            currentTransformationId = string.Empty;
            isTransformed = false;
            wasTransformed = false;

            int[] oldUnlockedRoster;
            int[] oldTransformationRoster;

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

            ClearAllCustomizationState();
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
                    byte brightness = paletteEntry.ContainsKey("brightness")
                        ? (byte)paletteEntry.GetInt("brightness")
                        : TransformationPaletteColorEntry.NeutralBrightness;
                    AddNormalizedTransformationPaletteEntry(new TransformationPaletteColorEntry(
                        transformationId,
                        channelId,
                        new Color(r, g, b),
                        hue,
                        saturation,
                        brightness
                    ));
                }
            }

            if (tag.TryGet("omnitrixVisualPalette", out List<TagCompound> visualPaletteEntries)) {
                foreach (TagCompound visualPaletteEntry in visualPaletteEntries) {
                    string channelId = visualPaletteEntry.GetString("channelId");
                    byte r = (byte)visualPaletteEntry.GetInt("r");
                    byte g = (byte)visualPaletteEntry.GetInt("g");
                    byte b = (byte)visualPaletteEntry.GetInt("b");
                    byte hue = visualPaletteEntry.ContainsKey("hue")
                        ? (byte)visualPaletteEntry.GetInt("hue")
                        : TransformationPaletteColorEntry.NeutralHue;
                    byte saturation = visualPaletteEntry.ContainsKey("saturation")
                        ? (byte)visualPaletteEntry.GetInt("saturation")
                        : TransformationPaletteColorEntry.NeutralSaturation;
                    byte brightness = visualPaletteEntry.ContainsKey("brightness")
                        ? (byte)visualPaletteEntry.GetInt("brightness")
                        : TransformationPaletteColorEntry.NeutralBrightness;
                    AddNormalizedOmnitrixVisualPaletteEntry(new OmnitrixVisualPaletteColorEntry(
                        channelId,
                        new Color(r, g, b),
                        hue,
                        saturation,
                        brightness
                    ));
                }
            }

            if (tag.TryGet("paletteEnabledChannels", out string[] enabledPaletteArray)) {
                for (int i = 0; i < enabledPaletteArray.Length; i++) {
                    AddNormalizedPaletteEnabledChannelKey(enabledPaletteArray[i]);
                }
            }
            else if (tag.TryGet("paletteDisabledChannels", out string[] disabledPaletteArray)) {
                LoadLegacyPaletteDisabledChannels(disabledPaletteArray);
            }

            if (tag.TryGet("paletteDisabledTransformations", out string[] disabledTransformationArray)) {
                for (int i = 0; i < disabledTransformationArray.Length; i++)
                    SetAllPaletteChannelsEnabled(disabledTransformationArray[i], false);
            }

            if (tag.TryGet("selectedTransformationCostumes", out List<TagCompound> costumeEntries)) {
                foreach (TagCompound costumeEntry in costumeEntries) {
                    string transformationId = costumeEntry.GetString("transformationId");
                    string costumeId = costumeEntry.GetString("costumeId");
                    SetSelectedTransformationCostume(transformationId, costumeId, sync: false);
                }
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

            if (TransformationLoader.Resolve("Ben10Mod:HeatBlast") != null &&
                !unlockedTransformations.Contains("Ben10Mod:HeatBlast"))
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

        private void NormalizeStoredTransformationData() {
            Roster.Normalize(TransformationSlotCount);
            NormalizeSelectedTransformationCostumes();
            NormalizeTransformationPaletteState();
            NormalizeOmnitrixVisualPaletteState();
            NormalizePalettePresets();
            NormalizeCustomTransformationNames();

            Transformation currentTransformation = TransformationLoader.Resolve(currentTransformationId);
            currentTransformationId = currentTransformation != null &&
                                      unlockedTransformations.Contains(currentTransformation.FullID)
                ? currentTransformation.FullID
                : string.Empty;
        }
    }
}
