using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Networking;

public static class MaterialAbsorptionPacketHandler {
    public static void Handle(global::Ben10Mod.Ben10Mod.MessageType msgType, BinaryReader reader, int whoAmI) {
        switch (msgType) {
            case global::Ben10Mod.Ben10Mod.MessageType.RequestAbsorbMaterial:
                HandleAbsorbMaterialRequest(whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.AbsorbMaterialFeedback:
                HandleAbsorbMaterialFeedback(reader);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.SyncAbsorbedMaterial:
                HandleAbsorbedMaterialSync(reader);
                break;
        }
    }

    private static void HandleAbsorbMaterialRequest(int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (!TryGetActivePlayer(whoAmI, out Player player))
            return;

        player.GetModPlayer<OmnitrixPlayer>().HandleAbsorbMaterialRequest();
    }

    private static void HandleAbsorbMaterialFeedback(BinaryReader reader) {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        string message = reader.ReadString();
        byte r = reader.ReadByte();
        byte g = reader.ReadByte();
        byte b = reader.ReadByte();

        Main.NewText(message, new Color(r, g, b));
    }

    private static void HandleAbsorbedMaterialSync(BinaryReader reader) {
        int playerIndex = reader.ReadByte();
        int itemType = reader.ReadInt32();
        int timeLeft = reader.ReadInt32();
        bool showEffects = reader.ReadBoolean();

        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        if (!TryGetActivePlayer(playerIndex, out Player player))
            return;

        player.GetModPlayer<OmnitrixPlayer>().ApplyAbsorbedMaterialSync(itemType, timeLeft, showEffects);
    }

    private static bool TryGetActivePlayer(int playerIndex, out Player player) {
        player = null;
        if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
            return false;

        player = Main.player[playerIndex];
        return player.active;
    }
}
