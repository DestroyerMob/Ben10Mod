using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Ben10Mod.Content.Items.Placeables;

namespace Ben10Mod.Content.Transformations.XLR8
{
    public class XLR8 : ModItem {
        public static string TransformationDescription =>
            "A Kineceleran speedster that turns movement into offense. XLR8 dominates with acceleration, rapid strikes, piercing dashes, evasiveness, and a time-bending ultimate that freezes enemies in place.";

        public static IReadOnlyList<string> TransformationAbilities => new[] {
            "Primary ability: speed overdrive that pushes movement and weapon speed even higher.",
            "Main attack: ultra-fast rushing strikes.",
            "Secondary attack: piercing velocity dash that can cut through multiple enemies.",
            "Secondary ability: vector dash to the cursor for a targeted speed burst.",
            "Passive: huge movement speed, rapid acceleration, boosted jumping, and water running at speed.",
            "Mobility: horizontal dash by double tapping left or right.",
            "Ultimate ability: grayscale perception shift that slows hostile NPCs and outside projectiles to a halt."
        };

        public override void Load() {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new XLR8Head());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}_alt", EquipType.Head, this, "XLR8_alt", equipTexture: new XLR8Head());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_Tail", EquipType.Back, this);
            
        }

        private void SetupDrawing() {
            if (Main.netMode == NetmodeID.Server)
                return;
            int equipSlotHeadAlt = EquipLoader.GetEquipSlot(Mod, "XLR8_alt", EquipType.Head);
            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            int equipSlotBack = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Back);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
            ArmorIDs.Head.Sets.DrawHead[equipSlotHeadAlt] = false;
            
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
            
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
            
            ArmorIDs.Back.Sets.DrawInTailLayer[equipSlotBack] = true;
        }

        public override void SetStaticDefaults() {
            SetupDrawing();
        }

        public override void SetDefaults() {
            Item.width = 40;
            Item.height = 80;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player) {
            return !TransformationHandler.HasTransformation(player, "Ben10Mod:XLR8");
        }

        public override bool? UseItem(Player player) {
            TransformationHandler.AddTransformation(player, "Ben10Mod:XLR8");
            return true;
        }
    }

    public class XLR8Head : EquipTexture {
        public override bool IsVanitySet(int head, int body, int legs) => true;
    }
}
