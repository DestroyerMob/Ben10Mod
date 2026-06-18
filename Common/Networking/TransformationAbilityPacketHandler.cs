using System.IO;
using Ben10Mod.Content.Transformations.AmpFibian;
using Ben10Mod.Content.Transformations.BuzzShock;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Networking;

public static class TransformationAbilityPacketHandler {
    public static void Handle(global::Ben10Mod.Ben10Mod.MessageType msgType, BinaryReader reader, int whoAmI) {
        switch (msgType) {
            case global::Ben10Mod.Ben10Mod.MessageType.ExecuteBuzzShockTeleport:
                HandleBuzzShockTeleport(reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.ExecuteEchoEchoShift:
                HandleEchoEchoShift(whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.ExecuteUltimateEchoEchoRelay:
                HandleUltimateEchoEchoRelay(reader, whoAmI);
                break;
            case global::Ben10Mod.Ben10Mod.MessageType.ExecuteAmpFibianPhaseShift:
                HandleAmpFibianPhaseShift(reader, whoAmI);
                break;
        }
    }

    private static void HandleBuzzShockTeleport(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        Vector2 destination = new(reader.ReadSingle(), reader.ReadSingle());
        if (!TryGetActiveLivingPlayer(whoAmI, out Player player))
            return;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != "Ben10Mod:BuzzShock" || !omp.IsPrimaryAbilityActive)
            return;

        BuzzShockTransformation.ExecutePrimaryAbilityTeleport(player, destination);
    }

    private static void HandleEchoEchoShift(int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (!TryGetActiveLivingPlayer(whoAmI, out Player player))
            return;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != "Ben10Mod:EchoEcho")
            return;

        global::Ben10Mod.Content.Transformations.EchoEcho.EchoEchoTransformation.ExecuteEchoShift(player);
    }

    private static void HandleUltimateEchoEchoRelay(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        Vector2 cursorWorld = new(reader.ReadSingle(), reader.ReadSingle());
        if (!TryGetActiveLivingPlayer(whoAmI, out Player player))
            return;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != "Ben10Mod:UltimateEchoEcho")
            return;

        global::Ben10Mod.Content.Transformations.EchoEcho.UltimateEchoEchoTransformation.ExecuteResonantRelay(
            player, cursorWorld);
    }

    private static void HandleAmpFibianPhaseShift(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        Vector2 destination = new(reader.ReadSingle(), reader.ReadSingle());
        if (!TryGetActiveLivingPlayer(whoAmI, out Player player))
            return;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != "Ben10Mod:AmpFibian")
            return;

        AmpFibianTransformation.ExecutePhaseShift(player, destination);
    }

    private static bool TryGetActiveLivingPlayer(int playerIndex, out Player player) {
        player = null;
        if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
            return false;

        player = Main.player[playerIndex];
        return player.active && !player.dead;
    }
}
