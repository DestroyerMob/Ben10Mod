using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Ben10Mod.Content.Tiles;

public class PlumberCapsulePod : ModTile {
        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type]          = false;   // capsule is non-solid
            Main.tileNoAttach[Type]       = true;

            AddMapEntry(new Color(255, 255, 255), CreateMapEntryName());
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;

            // Require holding any Plumber's Badge (check base class or specific types)
            if (player.HeldItem.ModItem is PlumbersBadge)
            {
                // Activation effects
                SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 1f, Pitch = 0.3f }, new Vector2(i * 16, j * 16));
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f }, new Vector2(i * 16, j * 16)); // Unlock/explosion

                // Big energy burst
                for (int d = 0; d < 50; d++)
                {
                    Dust dust = Dust.NewDustPerfect(new Vector2(i * 16 + 16, j * 16 + 16), DustID.Electric, 
                        Main.rand.NextVector2Circular(8f, 8f), Scale: 2f);
                    dust.noGravity = true;
                }

                // Kill the 2x2 tile
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        WorldGen.KillTile(i + x, j + y, noItem: true);
                    }
                }

                // Give the Prototype Omnitrix
                Item omnitrix = new Item(ModContent.ItemType<PrototypeOmnitrix>());
                player.QuickSpawnItem(player.GetSource_FromThis(), omnitrix);

                return true;
            }

            return false; // No action if not holding badge
        }

        public override bool CanDrop(int i, int j) {
            return false;
        }
        
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow               = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID      = ModContent.ItemType<PlumberCadetBadge>();
        }
}