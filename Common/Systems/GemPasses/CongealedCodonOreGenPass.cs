using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.IO;
using Terraria.WorldBuilding;
using Ben10Mod.Content.Tiles;
using Terraria.ID;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ben10Mod.Common.Systems.GemPasses {
    public class CongealedCodonOreGenPass : GenPass {
        public CongealedCodonOreGenPass(string name, float weight) : base(name, weight) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
            progress.Message = "Spawning Congealed Codon Ore";

            int maxToSpawn = WorldGen.genRand.Next(250, 300);
            int numSpawned = 0;
            int attempts = 0;

            while (numSpawned < maxToSpawn) {
                int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                int y = WorldGen.genRand.Next(0, Main.maxTilesY);

                Tile tile = Framing.GetTileSafely(x, y);
                if (tile.TileType == TileID.Stone) {
                    WorldGen.TileRunner(x, y, WorldGen.genRand.Next(5, 15), WorldGen.genRand.Next(1, 4), ModContent.TileType<CongealedCodonOreTile>());
                    numSpawned++;
                }

                if (++attempts >= 100000) { break; }

            }

            

        }
    }
}
