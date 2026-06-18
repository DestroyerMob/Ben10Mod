using System.IO;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Networking;

public static class GhostFreakPacketHandler {
    private const string GhostFreakTransformationId = "Ben10Mod:GhostFreak";

    public static void HandlePossessionRequest(BinaryReader reader, int whoAmI) {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
            return;

        int targetIndex = reader.ReadInt32();
        if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
            return;

        Player player = Main.player[whoAmI];
        if (!player.active || player.dead)
            return;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != GhostFreakTransformationId || omp.inPossessionMode)
            return;

        NPC target = Main.npc[targetIndex];
        if (!target.active || !target.CanBeChasedBy())
            return;

        int possessionDuration = GhostFreakPossesionProjectile.ResolvePossessionDuration(target, whoAmI);
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ConsumeGhostFreakHaunt(whoAmI);
        omp.BeginPossession(targetIndex, player.position, possessionDuration);
    }

    public static void HandlePossessionStateSync(BinaryReader reader) {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        int playerIndex = reader.ReadByte();
        bool active = reader.ReadBoolean();
        int targetIndex = reader.ReadInt32();
        Vector2 returnPosition = new(reader.ReadSingle(), reader.ReadSingle());
        int timer = reader.ReadInt32();

        if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
            return;

        Player player = Main.player[playerIndex];
        if (!player.active)
            return;

        player.GetModPlayer<OmnitrixPlayer>().ApplyPossessionStateSync(active, targetIndex, returnPosition, timer);
    }
}
