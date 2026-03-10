using Terraria;
using Terraria.ModLoader;
using Terraria.IO;
using Terraria.WorldBuilding;
using Ben10Mod.Content.Tiles;
using Terraria.ID;

namespace Ben10Mod.Common.Systems.GenPasses
{
    public class OmnitrixCapsulePass : GenPass
    {
        public OmnitrixCapsulePass(string name, float loadWeight)
            : base(name, loadWeight) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Calling down strange meteors...";

            int meteorOreTile   = TileID.Silver;
            int meteorRockTile  = ModContent.TileType<CongealedCodonOreTile>();
            int capsuleTile     = ModContent.TileType<PlumberCapsulePod>();

            var rand = WorldGen.genRand;

            // === WORLD-SIZE BASED COUNT (exactly what you asked for) ===
            int numMeteors;
            if (Main.maxTilesX < 5000)       // Small world (4200 tiles)
                numMeteors = rand.Next(1, 3);   // 1-2
            else if (Main.maxTilesX < 7000)  // Medium world (6400 tiles)
                numMeteors = rand.Next(3, 5);   // 3-4
            else                             // Large world (8400 tiles)
                numMeteors = rand.Next(5, 7);   // 5-6

            int placed = 0;
            int maxAttempts = numMeteors * 6;   // safety net (you'll never hit it)

            for (int attempt = 0; attempt < maxAttempts && placed < numMeteors; attempt++)
            {
                // === 1) Pick random spot DEEP in the Cavern layer ===
                int centerX = rand.Next(300, Main.maxTilesX - 300);

                int cavernTop    = (int)Main.rockLayer + 60;   // well below surface, into caverns
                int cavernBottom = Main.maxTilesY - 380;       // safe above Underworld

                if (cavernTop >= cavernBottom) continue;

                int centerY = rand.Next(cavernTop, cavernBottom);

                // === 2) Build the lumpy meteor blob (slightly bigger & rounder than before) ===
                int halfWidth  = 13;
                int halfHeight = 8;

                bool meteorPlaced = false;

                for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
                {
                    for (int y = centerY - halfHeight; y <= centerY + halfHeight; y++)
                    {
                        if (!WorldGen.InWorld(x, y, 20)) continue;

                        float dx = (x - centerX) / (float)halfWidth;
                        float dy = (y - centerY) / (float)halfHeight;
                        float distSq = dx * dx + dy * dy;

                        if (distSq > 1.25f) continue;

                        // Organic lumpy edges
                        float noise = (float)(rand.NextDouble() * 0.42 - 0.19);
                        if (distSq > 1.0f + noise) continue;

                        Tile t = Main.tile[x, y];
                        if (t == null) continue;

                        t.HasTile = true;
                        t.TileType = rand.Next(6) == 0 ? (ushort)meteorOreTile : (ushort)meteorRockTile;

                        meteorPlaced = true;
                    }
                }

                if (!meteorPlaced) continue;

                // === 3) Fix framing ===
                for (int x = centerX - halfWidth - 4; x <= centerX + halfWidth + 4; x++)
                    for (int y = centerY - halfHeight - 4; y <= centerY + halfHeight + 4; y++) {
                        if (WorldGen.InWorld(x, y, 15))
                            WorldGen.SquareTileFrame(x, y, true);
                    }

                // === 4) GUARANTEED capsule placement (no more missing capsules!) ===
                // Try up to 40 columns on the meteor until we find a valid top spot
                bool capsulePlaced = false;
                for (int tries = 0; tries < 40 && !capsulePlaced; tries++) {
                    int x = centerX + rand.Next(-halfWidth + 3, halfWidth - 2);

                    // Scan this column from the top of the meteor downward
                    for (int y = centerY - halfHeight - 12; y <= centerY + halfHeight; y++) {
                        if (!WorldGen.InWorld(x, y)) continue;

                        Tile t = Main.tile[x, y];
                        if (t == null || !t.HasTile) continue;
                        if (t.TileType != meteorRockTile && t.TileType != meteorOreTile) continue;

                        // Found a meteor tile → place capsule directly above it
                        int capY = y - 1;
                        if (!WorldGen.InWorld(x, capY)) break;

                        // Carve a clean 1-tile pocket (just in case something is there)
                        WorldGen.KillTile(x, capY, noItem: true);

                        Tile cap = Main.tile[x, capY];
                        cap.HasTile  = true;
                        cap.TileType = (ushort)capsuleTile;

                        WorldGen.SquareTileFrame(x, capY, true);
                        WorldGen.SquareTileFrame(x, y, true);

                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, x - 1, capY - 1, 3, 3);

                        capsulePlaced = true;
                        break;
                    }
                }

                if (capsulePlaced)
                    placed++;
            }
            ModContent.GetInstance<Ben10Mod>().Logger.Info($"OmnitrixCapsulePass: Placed {placed} meteors in cavern layer");
        }
    }
}