using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ben10Mod.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Tiles {
    public class CongealedCodonOreTile : ModTile {
        public override void SetStaticDefaults() {
            TileID.Sets.Ore[Type] = true;

            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileShine[Type] = 900;
            Main.tileShine2[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 400;

            AddMapEntry(new Color(0, 255, 0), CreateMapEntryName());

            DustType = DustID.Chlorophyte;
            HitSound = SoundID.Tink;

            MineResist = 1.5f;
            MinPick = 55;

        }
    }
}
