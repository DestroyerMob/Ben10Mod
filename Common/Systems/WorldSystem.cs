using Ben10Mod.Common.Systems.GenPasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ben10Mod.Content.Tiles;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace Ben10Mod.Common.Systems {
    public class WorldSystem : ModSystem {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
            int shiniesIndex = tasks.FindIndex(t => t.Name.Equals("Shinies"));
            int microBiomesIndex = tasks.FindIndex(t => t.Name.Equals("Micro Biomes"));
            
            if (shiniesIndex == -1)
                shiniesIndex = tasks.Count - 1;
            tasks.Insert(shiniesIndex + 1, new CongealedCodonOreGenPass("Congealed Ore Pass", 10));
            
            if (microBiomesIndex == -1)
                microBiomesIndex = tasks.Count - 1;

            tasks.Insert(microBiomesIndex + 1, new OmnitrixCapsulePass("Omnitrix Capsule Pass", 200f));
        }
    }
}