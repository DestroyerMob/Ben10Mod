using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Consumable
{
    public class MasterControlKey : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = Item.useAnimation = 45; // Dramatic wind-up for unlock
            Item.useTurn = true;
            Item.consumable = true;
            Item.rare = ItemRarityID.Purple; // Feels like a rare/powerful unlock
        }

        public override bool CanUseItem(Player player)
        {
            // Can only use if Master Control is NOT already unlocked
            return !player.GetModPlayer<OmnitrixPlayer>().masterControl;
        }

        public override bool? UseItem(Player player)
        {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            
            omp.masterControl = true;

            // Dramatic effects
            SoundEngine.PlaySound(SoundID.Unlock, player.Center);
            SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.4f }, player.Center); // Omnitrix-like chime

            // Big energy burst
            for (int i = 0; i < 50; i++)
            {
                Dust d = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.Firework_Green, Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                d.noGravity = true;
            }

            // Rainbow pulse for flair
            for (int i = 0; i < 30; i++)
            {
                Dust d = Dust.NewDustPerfect(player.Center, DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(8f, 8f), Scale: 2f);
                d.noGravity = true;
            }

            // Announcement
            Main.NewText("Master Control unlocked!", new Color(0, 255, 0));

            return true; // Item was used successfully
        }

        // Optional: Extra safety - don't consume if already unlocked (CanUseItem already blocks use)
        public override bool ConsumeItem(Player player)
        {
            return !player.GetModPlayer<OmnitrixPlayer>().masterControl;
        }
    }
}