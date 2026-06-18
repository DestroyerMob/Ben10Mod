using System.IO;

namespace Ben10Mod.Common.Networking;

public static class OmnitrixPacketHandler {
    public static void HandlePacket(global::Ben10Mod.Ben10Mod mod, BinaryReader reader, int whoAmI) {
        global::Ben10Mod.Ben10Mod.MessageType msgType =
            (global::Ben10Mod.Ben10Mod.MessageType)reader.ReadByte();

        switch (msgType) {
            case global::Ben10Mod.Ben10Mod.MessageType.UnlockTransformation:
            case global::Ben10Mod.Ben10Mod.MessageType.RemoveTransformation:
            case global::Ben10Mod.Ben10Mod.MessageType.RequestUnlockTransformation:
            case global::Ben10Mod.Ben10Mod.MessageType.RequestRemoveTransformation:
            case global::Ben10Mod.Ben10Mod.MessageType.RecordEventParticipation:
            case global::Ben10Mod.Ben10Mod.MessageType.SyncOmnitrixEvolution:
            case global::Ben10Mod.Ben10Mod.MessageType.RequestSyncTransformationState:
            case global::Ben10Mod.Ben10Mod.MessageType.SyncTransformationState:
            case global::Ben10Mod.Ben10Mod.MessageType.RequestSyncTransformationPaletteState:
            case global::Ben10Mod.Ben10Mod.MessageType.SyncTransformationPaletteState:
            case global::Ben10Mod.Ben10Mod.MessageType.RequestSyncTransformationSpeedBoostSetting:
            case global::Ben10Mod.Ben10Mod.MessageType.SyncTransformationSpeedBoostSetting:
                OmnitrixStatePacketHandler.Handle(msgType, reader, whoAmI);
                break;

            case global::Ben10Mod.Ben10Mod.MessageType.RequestAbsorbMaterial:
            case global::Ben10Mod.Ben10Mod.MessageType.AbsorbMaterialFeedback:
            case global::Ben10Mod.Ben10Mod.MessageType.SyncAbsorbedMaterial:
                MaterialAbsorptionPacketHandler.Handle(msgType, reader, whoAmI);
                break;

            case global::Ben10Mod.Ben10Mod.MessageType.ExecuteBuzzShockTeleport:
            case global::Ben10Mod.Ben10Mod.MessageType.ExecuteEchoEchoShift:
            case global::Ben10Mod.Ben10Mod.MessageType.ExecuteUltimateEchoEchoRelay:
            case global::Ben10Mod.Ben10Mod.MessageType.ExecuteAmpFibianPhaseShift:
                TransformationAbilityPacketHandler.Handle(msgType, reader, whoAmI);
                break;

            case global::Ben10Mod.Ben10Mod.MessageType.RequestGhostFreakPossession:
                GhostFreakPacketHandler.HandlePossessionRequest(reader, whoAmI);
                break;

            case global::Ben10Mod.Ben10Mod.MessageType.SyncGhostFreakPossessionState:
                GhostFreakPacketHandler.HandlePossessionStateSync(reader);
                break;

            case global::Ben10Mod.Ben10Mod.MessageType.RelayDodgeVisual:
            case global::Ben10Mod.Ben10Mod.MessageType.RequestCompletedOmnitrixRevival:
            case global::Ben10Mod.Ben10Mod.MessageType.SyncCompletedOmnitrixRevival:
                OmnitrixAccessoryPacketHandler.Handle(mod, msgType, reader, whoAmI);
                break;
        }
    }
}
