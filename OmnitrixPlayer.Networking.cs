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
        public override void OnEnterWorld() {
            ModContent.GetInstance<UISystem>().HideMyUI();
            ResetAirborneLungeState();
            if (!isTransformed)
                currentTransformationId = "";

            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer) {
                SyncTransformationStateToServer();
                SyncTransformationPaletteStateToServer();
                SyncTransformationSpeedBoostSettingToServer();
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

        public void SyncTransformationSpeedBoostSettingToServer() {
            if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI != Main.myPlayer)
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.RequestSyncTransformationSpeedBoostSetting);
            packet.Write(transformationSpeedBoostPercent);
            packet.Send();
        }

        internal void SyncTransformationSpeedBoostSettingToServerOrClients() {
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer) {
                SyncTransformationSpeedBoostSettingToServer();
                return;
            }

            if (Main.netMode == NetmodeID.Server)
                SyncTransformationSpeedBoostSetting();
        }

        public void ApplyTransformationStateSync(string[] slots, string[] unlocked) {
            unlockedTransformations.Clear();
            if (unlocked != null)
                unlockedTransformations.AddRange(unlocked);

            transformationSlots = slots ?? Array.Empty<string>();
            NormalizeStoredTransformationData();
        }

        public void ApplyClientTransformationSlotSync(string[] slots) {
            NormalizeStoredTransformationData();
            transformationSlots = NormalizeTransformationSlots(slots, unlockedTransformations);
            NormalizeStoredTransformationData();
        }

        public void SyncTransformationPaletteStateToServer() {
            if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI != Main.myPlayer)
                return;

            List<TransformationPaletteColorEntry> entries = BuildNormalizedTransformationPaletteEntries();
            List<string> enabledChannelKeys = BuildNormalizedPaletteEnabledChannelKeys();
            List<KeyValuePair<string, string>> selectedCostumeEntries = new(BuildNormalizedSelectedTransformationCostumes());
            List<OmnitrixVisualPaletteColorEntry> visualPaletteEntries = BuildNormalizedOmnitrixVisualPaletteEntries();
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.RequestSyncTransformationPaletteState);
            OmnitrixPacketRouter.WriteTransformationPaletteEntries(packet, entries);
            OmnitrixPacketRouter.WritePaletteChannelKeys(packet, enabledChannelKeys);
            OmnitrixPacketRouter.WriteSelectedTransformationCostumes(packet, selectedCostumeEntries);
            OmnitrixPacketRouter.WriteOmnitrixVisualPaletteEntries(packet, visualPaletteEntries);
            packet.Send();
        }

        public void ApplyTransformationPaletteStateSync(IReadOnlyList<TransformationPaletteColorEntry> entries,
            IReadOnlyList<string> enabledChannelKeys = null,
            IReadOnlyList<KeyValuePair<string, string>> selectedCostumeEntries = null,
            IReadOnlyList<OmnitrixVisualPaletteColorEntry> visualPaletteEntries = null) {
            ClearTransformationPaletteSyncState();

            if (entries != null) {
                for (int i = 0; i < entries.Count; i++)
                    AddNormalizedTransformationPaletteEntry(entries[i]);
            }

            if (enabledChannelKeys != null) {
                for (int i = 0; i < enabledChannelKeys.Count; i++) {
                    AddNormalizedPaletteEnabledChannelKey(enabledChannelKeys[i]);
                }
            }

            if (selectedCostumeEntries != null) {
                for (int i = 0; i < selectedCostumeEntries.Count; i++) {
                    KeyValuePair<string, string> entry = selectedCostumeEntries[i];
                    SetSelectedTransformationCostume(entry.Key, entry.Value, sync: false);
                }
            }

            if (visualPaletteEntries != null) {
                for (int i = 0; i < visualPaletteEntries.Count; i++)
                    AddNormalizedOmnitrixVisualPaletteEntry(visualPaletteEntries[i]);
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
            if (Main.netMode == NetmodeID.Server) {
                SyncTransformationSpeedBoostSetting(toWho, fromWho);
                SyncTransformationPaletteState(toWho, fromWho);
                SyncAbsorbedMaterial(showEffects: false, toWho: toWho, ignoreClient: fromWho);
            }
        }

        internal void SyncTransformationSpeedBoostSetting(int toWho = -1, int ignoreClient = -1) {
            if (Main.netMode != NetmodeID.Server)
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.SyncTransformationSpeedBoostSetting);
            packet.Write((byte)Player.whoAmI);
            packet.Write(transformationSpeedBoostPercent);
            packet.Send(toWho, ignoreClient);
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
            List<KeyValuePair<string, string>> selectedCostumeEntries = new(BuildNormalizedSelectedTransformationCostumes());
            List<OmnitrixVisualPaletteColorEntry> visualPaletteEntries = BuildNormalizedOmnitrixVisualPaletteEntries();

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.SyncTransformationPaletteState);
            packet.Write((byte)Player.whoAmI);
            OmnitrixPacketRouter.WriteTransformationPaletteEntries(packet, entries);
            OmnitrixPacketRouter.WritePaletteChannelKeys(packet, enabledChannelKeys);
            OmnitrixPacketRouter.WriteSelectedTransformationCostumes(packet, selectedCostumeEntries);
            OmnitrixPacketRouter.WriteOmnitrixVisualPaletteEntries(packet, visualPaletteEntries);
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
    }
}
