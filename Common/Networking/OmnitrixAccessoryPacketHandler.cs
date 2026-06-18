using System.IO;
using Ben10Mod.Content.Items.Armour;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Networking;

public static class OmnitrixAccessoryPacketHandler {
    public static void Handle(global::Ben10Mod.Ben10Mod mod, global::Ben10Mod.Ben10Mod.MessageType msgType,
        BinaryReader reader, int whoAmI) {
        switch (msgType) {
            case global::Ben10Mod.Ben10Mod.MessageType.RelayDodgeVisual:
                HandleRelayDodgeVisual(mod, reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.RequestCompletedOmnitrixRevival:
                HandleCompletedOmnitrixRevivalRequest(reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.SyncCompletedOmnitrixRevival:
                HandleCompletedOmnitrixRevivalSync(reader);
                break;
        }
    }

    private static void HandleRelayDodgeVisual(global::Ben10Mod.Ben10Mod mod, BinaryReader reader, int whoAmI) {
        int playerIndex = reader.ReadByte();

        if (!TryGetActivePlayer(playerIndex, out Player player))
            return;

        if (Main.netMode == NetmodeID.Server) {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)global::Ben10Mod.Ben10Mod.MessageType.RelayDodgeVisual);
            packet.Write((byte)playerIndex);
            packet.Send(-1, whoAmI);
            return;
        }

        player.GetModPlayer<HeroPlumberArmorPlayer>().PlayRelayDodgeVisual(sync: false);
    }

    private static void HandleCompletedOmnitrixRevivalRequest(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        string requestedTransformationId = reader.ReadString();
        if (!TryGetActivePlayer(whoAmI, out Player player))
            return;

        player.GetModPlayer<OmnitrixPlayer>()
            .HandleCompletedOmnitrixRevivalRequest(requestedTransformationId);
    }

    private static void HandleCompletedOmnitrixRevivalSync(BinaryReader reader) {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        int playerIndex = reader.ReadByte();
        string transformationId = reader.ReadString();
        int durationSeconds = reader.ReadInt32();
        int cooldownTicks = reader.ReadInt32();
        int statLife = reader.ReadInt32();
        float omnitrixEnergy = reader.ReadSingle();

        if (!TryGetActivePlayer(playerIndex, out Player player))
            return;

        player.GetModPlayer<OmnitrixPlayer>().ApplyCompletedOmnitrixRevivalSync(
            transformationId,
            durationSeconds,
            cooldownTicks,
            statLife,
            omnitrixEnergy);
    }

    private static bool TryGetActivePlayer(int playerIndex, out Player player) {
        player = null;
        if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
            return false;

        player = Main.player[playerIndex];
        return player.active;
    }
}
