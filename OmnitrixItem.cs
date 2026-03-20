using System;
using Ben10Mod.Content.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
    public class OmnitrixItem : GlobalItem {
        public override void SetDefaults(Item entity) {
            if (entity.type == ItemID.FrostCore) {
                entity.accessory = true;
            }
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual) {
            
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            
            if (item.type == ItemID.FrostCore) {
                omp.snowflake = true;
            }
        }

        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
            if (noHitbox || item.noMelee || item.damage <= 0)
                return;

            var omp = player.GetModPlayer<OmnitrixPlayer>();
            float scale = omp.CurrentTransformationScale;
            if (scale <= 1f)
                return;

            int scaledWidth = Math.Max(hitbox.Width, (int)Math.Round(hitbox.Width * scale));
            int scaledHeight = Math.Max(hitbox.Height, (int)Math.Round(hitbox.Height * scale));
            Point center = hitbox.Center;
            hitbox = new Rectangle(center.X - scaledWidth / 2, center.Y - scaledHeight / 2, scaledWidth, scaledHeight);
        }
    }
}
