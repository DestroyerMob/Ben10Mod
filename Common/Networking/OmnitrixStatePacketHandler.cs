using System;
using System.Collections.Generic;
using System.IO;
using Ben10Mod.Content.Transformations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Networking;

public static class OmnitrixStatePacketHandler {
    public static void Handle(global::Ben10Mod.Ben10Mod.MessageType msgType, BinaryReader reader, int whoAmI) {
        switch (msgType) {
            case global::Ben10Mod.Ben10Mod.MessageType.UnlockTransformation:
                HandleUnlockTransformation(reader);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.RemoveTransformation:
                HandleRemoveTransformation(reader);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.RequestUnlockTransformation:
                HandleUnlockTransformationRequest(reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.RequestRemoveTransformation:
                HandleRemoveTransformationRequest(reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.RecordEventParticipation:
                HandleRecordEventParticipation(reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.SyncOmnitrixEvolution:
                HandleOmnitrixEvolutionSync(reader);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.RequestSyncTransformationState:
                HandleTransformationStateRequest(reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.SyncTransformationState:
                HandleTransformationStateSync(reader);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.RequestSyncTransformationPaletteState:
                HandleTransformationPaletteStateRequest(reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.SyncTransformationPaletteState:
                HandleTransformationPaletteStateSync(reader);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.RequestSyncTransformationSpeedBoostSetting:
                HandleTransformationSpeedBoostSettingRequest(reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.SyncTransformationSpeedBoostSetting:
                HandleTransformationSpeedBoostSettingSync(reader);
                break;
        }
    }

    private static void HandleUnlockTransformation(BinaryReader reader) {
        int playerIndex = reader.ReadByte();
        string transformationId = reader.ReadString();

        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        if (!TryGetActivePlayer(playerIndex, out Player player))
            return;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        bool unlocked = omp.UnlockTransformation(transformationId, sync: false,
            showEffects: playerIndex == Main.myPlayer);
        if (!unlocked && playerIndex == Main.myPlayer)
            omp.ShowTransformationUnlockFeedback(transformationId);
    }

    private static void HandleRemoveTransformation(BinaryReader reader) {
        int playerIndex = reader.ReadByte();
        string transformationId = reader.ReadString();

        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        if (!TryGetActivePlayer(playerIndex, out Player player))
            return;

        player.GetModPlayer<OmnitrixPlayer>().RemoveTransformation(transformationId, sync: false,
            showEffects: playerIndex == Main.myPlayer);
    }

    private static void HandleUnlockTransformationRequest(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (!TryGetActivePlayer(whoAmI, out Player player))
            return;

        string transformationId = reader.ReadString();
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null)
            return;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.CanAcceptClientUnlockRequest(transformation)) {
            omp.SyncTransformationState(toWho: whoAmI);
            return;
        }

        omp.UnlockTransformation(transformation.FullID, sync: true, showEffects: false);
    }

    private static void HandleRemoveTransformationRequest(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (!TryGetActivePlayer(whoAmI, out Player player))
            return;

        string transformationId = reader.ReadString();
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        if (transformation == null)
            return;

        player.GetModPlayer<OmnitrixPlayer>()
            .RemoveTransformation(transformation.FullID, sync: true, showEffects: false);
    }

    private static void HandleRecordEventParticipation(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (!TryGetActivePlayer(whoAmI, out Player player))
            return;

        int eventCount = reader.ReadByte();
        List<int> eventIds = new(eventCount);
        for (int i = 0; i < eventCount; i++)
            eventIds.Add(reader.ReadInt32());

        player.GetModPlayer<OmnitrixPlayer>().ApplyRecordedEventParticipation(eventIds);
    }

    private static void HandleOmnitrixEvolutionSync(BinaryReader reader) {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        int playerIndex = reader.ReadByte();
        int resultType = reader.ReadInt32();
        if (!TryGetActivePlayer(playerIndex, out Player player) || playerIndex != Main.myPlayer)
            return;

        player.GetModPlayer<OmnitrixPlayer>().ApplyOmnitrixEvolutionSync(resultType);
    }

    private static void HandleTransformationStateRequest(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (!TryGetActivePlayer(whoAmI, out Player player))
            return;

        int slotCount = reader.ReadByte();
        string[] slots = new string[Math.Min(slotCount, OmnitrixPlayer.TransformationSlotCount)];
        for (int i = 0; i < slotCount; i++) {
            string slot = reader.ReadString();
            if (i < slots.Length)
                slots[i] = slot;
        }

        int unlockedCount = reader.ReadUInt16();
        for (int i = 0; i < unlockedCount; i++)
            reader.ReadString();

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.ApplyClientTransformationSlotSync(slots);
        omp.SyncTransformationState(toWho: whoAmI);
    }

    private static void HandleTransformationStateSync(BinaryReader reader) {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        int playerIndex = reader.ReadByte();
        if (!TryGetActivePlayer(playerIndex, out Player player))
            return;

        int slotCount = reader.ReadByte();
        string[] slots = new string[slotCount];
        for (int i = 0; i < slotCount; i++)
            slots[i] = reader.ReadString();

        int unlockedCount = reader.ReadUInt16();
        string[] unlocked = new string[unlockedCount];
        for (int i = 0; i < unlockedCount; i++)
            unlocked[i] = reader.ReadString();

        player.GetModPlayer<OmnitrixPlayer>().ApplyTransformationStateSync(slots, unlocked);
    }

    private static void HandleTransformationPaletteStateRequest(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (!TryGetActivePlayer(whoAmI, out Player player))
            return;

        TransformationPaletteColorEntry[] entries = OmnitrixPacketRouter.ReadTransformationPaletteEntries(reader);
        string[] enabledChannelKeys = OmnitrixPacketRouter.ReadPaletteChannelKeys(reader);
        KeyValuePair<string, string>[] selectedCostumeEntries =
            OmnitrixPacketRouter.ReadSelectedTransformationCostumes(reader);
        OmnitrixVisualPaletteColorEntry[] visualPaletteEntries =
            OmnitrixPacketRouter.ReadOmnitrixVisualPaletteEntries(reader);
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.ApplyTransformationPaletteStateSync(entries, enabledChannelKeys, selectedCostumeEntries,
            visualPaletteEntries);
        omp.SyncTransformationPaletteState();
    }

    private static void HandleTransformationPaletteStateSync(BinaryReader reader) {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        int playerIndex = reader.ReadByte();
        if (!TryGetActivePlayer(playerIndex, out Player player))
            return;

        TransformationPaletteColorEntry[] entries = OmnitrixPacketRouter.ReadTransformationPaletteEntries(reader);
        string[] enabledChannelKeys = OmnitrixPacketRouter.ReadPaletteChannelKeys(reader);
        KeyValuePair<string, string>[] selectedCostumeEntries =
            OmnitrixPacketRouter.ReadSelectedTransformationCostumes(reader);
        OmnitrixVisualPaletteColorEntry[] visualPaletteEntries =
            OmnitrixPacketRouter.ReadOmnitrixVisualPaletteEntries(reader);
        player.GetModPlayer<OmnitrixPlayer>().ApplyTransformationPaletteStateSync(entries,
            enabledChannelKeys,
            selectedCostumeEntries,
            visualPaletteEntries);
    }

    private static void HandleTransformationSpeedBoostSettingRequest(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (!TryGetActivePlayer(whoAmI, out Player player))
            return;

        byte speedBoostPercent = reader.ReadByte();
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.ApplyTransformationSpeedBoostSettingSync(speedBoostPercent);
        omp.SyncTransformationSpeedBoostSetting(ignoreClient: whoAmI);
    }

    private static void HandleTransformationSpeedBoostSettingSync(BinaryReader reader) {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        int playerIndex = reader.ReadByte();
        if (!TryGetActivePlayer(playerIndex, out Player player))
            return;

        byte speedBoostPercent = reader.ReadByte();
        player.GetModPlayer<OmnitrixPlayer>().ApplyTransformationSpeedBoostSettingSync(speedBoostPercent);
    }

    private static bool TryGetActivePlayer(int playerIndex, out Player player) {
        player = null;
        if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
            return false;

        player = Main.player[playerIndex];
        return player.active;
    }
}
