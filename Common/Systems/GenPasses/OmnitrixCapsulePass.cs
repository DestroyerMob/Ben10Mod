using Terraria;
using Terraria.ModLoader;
using Terraria.IO;
using Terraria.WorldBuilding;
using Ben10Mod.Content.Tiles;
using Terraria.ID;

namespace Ben10Mod.Common.Systems.GenPasses {
    public class OmnitrixCapsulePass : GenPass {
        public OmnitrixCapsulePass(string name, float loadWeight)
            : base(name, loadWeight) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration config) {
            progress.Message = "Calling down a strange meteor...";

            // Your custom tiles
            int meteorRockTile = TileID.LunarOre; // "meteor rock"
            int meteorOreTile  = ModContent.TileType<CongealedCodonOreTile>(); // special ore
            int capsuleTile    = ModContent.TileType<PlumberCapsulePod>(); // 1x1 capsule

            // --- 1) Choose an impact X away from the very edges ---
            int impactX = WorldGen.genRand.Next(250, Main.maxTilesX - 250);

            // --- 2) Call vanilla meteor generator once ---
            // y argument is mostly ignored; vanilla picks its own vertical area.
            bool spawned = WorldGen.meteor(impactX, 0);
            if (!spawned)
                return; // If vanilla couldn't place a meteor, we can't do anything

            // --- 3) Scan around impactX to find the meteorite area ---
            int scanRadius = 80;
            int minX       = impactX - scanRadius;
            int maxX       = impactX + scanRadius;

            if (minX < 10) minX                  = 10;
            if (maxX > Main.maxTilesX - 10) maxX = Main.maxTilesX - 10;

            bool foundMeteor    = false;
            int  highestMeteorY = Main.maxTilesY;
            int  lowestMeteorY  = 0;

            for (int x = minX; x <= maxX; x++) {
                for (int y = 0; y < Main.maxTilesY - 200; y++) {
                    Tile t = Main.tile[x, y];
                    if (t == null || !t.HasTile)
                        continue;

                    if (t.TileType == TileID.Meteorite) {
                        foundMeteor = true;
                        if (y < highestMeteorY) highestMeteorY = y;
                        if (y > lowestMeteorY) lowestMeteorY   = y;
                    }
                }
            }

            if (!foundMeteor)
                return; // Somehow no meteorite tiles were created, bail

            // --- 4) Convert Meteorite into your meteor rock / ore ---
            for (int x = minX; x <= maxX; x++) {
                for (int y = 0; y < Main.maxTilesY - 200; y++) {
                    Tile t = Main.tile[x, y];
                    if (t == null || !t.HasTile || t.TileType != TileID.Meteorite)
                        continue;

                    // Random ore pockets inside the meteor
                    if (WorldGen.genRand.Next(6) == 0) // ~1/6 chance
                    {
                        t.TileType = (ushort)meteorOreTile;
                    }
                    else {
                        t.TileType = (ushort)meteorRockTile;
                    }
                }
            }

            // --- 5) Fix framing for that region so it looks nice ---
            for (int x = minX - 2; x <= maxX + 2; x++) {
                if (x < 10 || x >= Main.maxTilesX - 10)
                    continue;

                for (int y = highestMeteorY - 20; y <= lowestMeteorY + 20; y++) {
                    if (y < 10 || y >= Main.maxTilesY - 10)
                        continue;

                    WorldGen.SquareTileFrame(x, y, resetFrame: true);
                }
            }

            // --- 6) Find a top spot for the 1x1 capsule near impactX ---

            int bestX = -1;
            int bestY = -1;

            int searchHalfWidth             = 25;
            int topSearchY                  = highestMeteorY - 5;
            if (topSearchY < 10) topSearchY = 10;

            int bottomSearchY                                      = lowestMeteorY + 10;
            if (bottomSearchY > Main.maxTilesY - 10) bottomSearchY = Main.maxTilesY - 10;

            for (int x = impactX - searchHalfWidth; x <= impactX + searchHalfWidth; x++) {
                if (x < minX || x > maxX)
                    continue;

                // Scan from just above the meteor top downward
                for (int y = topSearchY; y <= bottomSearchY; y++) {
                    Tile t = Main.tile[x, y];
                    if (t == null || !t.HasTile)
                        continue;

                    if (t.TileType == meteorRockTile || t.TileType == meteorOreTile) {
                        Tile above = Main.tile[x, y - 1];
                        if (above != null && !above.HasTile) {
                            bestX = x;
                            bestY = y - 1; // capsule goes in this empty tile
                            break;
                        }
                    }
                }

                if (bestX != -1)
                    break;
            }

            if (bestX == -1)
                return; // meteor exists, but we couldn't find a clean top spot for the capsule

            // --- 7) Place the 1x1 PlumberCapsulePod tile directly ---

            Tile cap = Main.tile[bestX, bestY];
            cap.HasTile  = true;
            cap.TileType = (ushort)capsuleTile;

            WorldGen.SquareTileFrame(bestX, bestY, resetFrame: true);

            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendTileSquare(-1, bestX, bestY, 1, 1);
            }
        }
    }
}