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

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
            progress.Message = "Calling down a strange meteor...";

            // --- 0) Your tiles ---
            int meteorOreTile   = TileID.Silver;
            int meteorRockTile  = ModContent.TileType<CongealedCodonOreTile>();
            int capsuleTile     = ModContent.TileType<PlumberCapsulePod>(); // 1x1 capsule
            
            var rand = WorldGen.genRand;

                // --- 1) Pick a surface position ---

                // Avoid very close to edges to prevent clipping into oceans.
                int centerX = rand.Next(300, Main.maxTilesX - 300);

                // Find the "surface" at this X: first solid tile going down.
                int surfaceY = -1;
                for (int y = 0; y < Main.maxTilesY - 300; y++) {
                    if (WorldGen.SolidTile(centerX, y)) {
                        surfaceY = y;
                        if (Main.rand.Next(101) == 1) {
                            break;
                        }
                    }
                }

                // --- 2) Define meteor shape parameters ---

                // Overall horizontal and vertical extents of the meteor blob
                int halfWidth  = 12;
                int halfHeight = 5;

                // The center of the meteor mass is a bit below the surface
                int meteorCenterY = surfaceY + 10;

                // Crater radius around the surface
                int craterRadius = 26;

                // Clamp Y so we don't go out of world bounds
                if (meteorCenterY > Main.maxTilesY - 60)
                    meteorCenterY = Main.maxTilesY - 60;

                // --- 3) Carve a crater on the surface (kill tiles in a circle) ---

                // int craterMinX = centerX - craterRadius - 5;
                // int craterMaxX = centerX + craterRadius + 5;
                // int craterMinY = surfaceY - 10;
                // int craterMaxY = surfaceY + 10;
                //
                // for (int x = craterMinX; x <= craterMaxX; x++) {
                //     for (int y = craterMinY; y <= craterMaxY; y++) {
                //         if (!WorldGen.InWorld(x, y, 20))
                //             continue;
                //
                //         float dx   = x - centerX;
                //         float dy   = y - surfaceY;
                //         float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);
                //
                //         // Slightly flattened crater
                //         float craterShape = craterRadius * (1f + 0.15f * (float)System.Math.Sin(dx / 4f));
                //         if (dist <= craterShape) {
                //             WorldGen.KillTile(x, y, noItem: true);
                //         }
                //     }
                // }

                // --- 4) Build a lumpy meteor blob under the crater ---

                int meteorMinX = centerX - halfWidth;
                int meteorMaxX = centerX + halfWidth;
                int meteorMinY = meteorCenterY - halfHeight;
                int meteorMaxY = meteorCenterY + halfHeight;

                if (meteorMinY < 10) meteorMinY                  = 10;
                if (meteorMaxY > Main.maxTilesY - 30) meteorMaxY = Main.maxTilesY - 30;

                for (int x = meteorMinX; x <= meteorMaxX; x++) {
                    for (int y = meteorMinY; y <= meteorMaxY; y++) {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        float dx     = (x - centerX) / (float)halfWidth;
                        float dy     = (y - meteorCenterY) / (float)halfHeight;
                        float distSq = dx * dx + dy * dy;

                        if (distSq > 1.2f)
                            continue; // outside the main ellipse

                        // Add some noise so the outline isn't perfectly smooth
                        float noise = (float)(rand.NextDouble() * 0.35 - 0.15); // [-0.15, +0.20]
                        if (distSq > 1f + noise)
                            continue;

                        Tile t = Main.tile[x, y];
                        if (t == null)
                            continue;

                        t.HasTile = true;

                        // About 1/6 chance to be the special codon ore
                        if (rand.Next(6) == 0)
                            t.TileType = (ushort)meteorOreTile;
                        else
                            t.TileType = (ushort)meteorRockTile;
                    }
                }

                // --- 5) Fix framing in the area so it looks correct ---

                int frameMinX = meteorMinX - 2;
                int frameMaxX = meteorMaxX + 2;
                int frameMinY = meteorMinY - 2;
                int frameMaxY = meteorMaxY + 2;

                for (int x = frameMinX; x <= frameMaxX; x++) {
                    for (int y = frameMinY; y <= frameMaxY; y++) {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        WorldGen.SquareTileFrame(x, y, resetFrame: true);
                    }
                }

                // --- 6) Find a top spot on the meteor for the capsule ---

                int bestX = -1;
                int bestY = -1;

                int searchHalfWidth = 20;
                int searchTopY      = surfaceY - 10;
                int searchBottomY   = meteorMinY + (meteorMaxY - meteorMinY) / 2;

                if (searchTopY < 10) searchTopY                        = 10;
                if (searchBottomY > Main.maxTilesY - 30) searchBottomY = Main.maxTilesY - 30;

                for (int x = centerX - searchHalfWidth; x <= centerX + searchHalfWidth; x++) {
                    if (x < meteorMinX || x > meteorMaxX)
                        continue;

                    // scan downward from just above the crater
                    for (int y = searchTopY; y <= searchBottomY; y++) {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        Tile t = Main.tile[x, y];
                        if (t == null || !t.HasTile)
                            continue;

                        if (t.TileType == meteorRockTile || t.TileType == meteorOreTile) {
                            Tile above = Main.tile[x, y - 1];
                            if (above != null && !above.HasTile) {
                                bestX = x;
                                bestY = y - 1;
                                break;
                            }
                        }
                    }

                    if (bestX != -1)
                        break;
                }

                if (bestX == -1) {
                    // Couldn't find a clean top; meteor still exists, just no capsule.
                    return;
                }

                // --- 7) Place the 1x1 PlumberCapsulePod tile ---

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