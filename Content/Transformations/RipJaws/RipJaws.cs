using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.RipJaws
{
    public class RipJaws : ModItem {
        public static string TransformationDescription =>
            "An aquatic hunter with a sharp split between land and water combat. Ripjaws becomes terrifying underwater, but struggles badly if you stay dry for too long.";

        public static IReadOnlyList<string> TransformationAbilities => new[] {
            "Main attack: aquatic projectile strike.",
            "Alt attack: high-damage lunging bite.",
            "Passive in water: faster movement, full breathing, brighter vision, and much higher damage.",
            "Passive on land: rapidly loses breath and life if kept dry.",
            "Ultimate attack: empowered offensive burst through the badge attack system."
        };

        public override void Load() {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Waist}", EquipType.Waist, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}_alt", EquipType.Legs, this, "RipJaws_alt");
        }

        private void SetupDrawing() {
            if (Main.netMode == NetmodeID.Server)
                return;

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            int equipSlotLegsAlt = EquipLoader.GetEquipSlot(Mod, "RipJaws_alt", EquipType.Legs);
            int equipSlotWaist = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Waist);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegsAlt] = true;
            ArmorIDs.Legs.Sets.OverridesLegs[equipSlotLegsAlt] = true;
            ArmorIDs.Waist.Sets.IsABelt[equipSlotWaist] = true;
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
            return !TransformationHandler.HasTransformation(player, "Ben10Mod:RipJaws");
        }

        public override bool? UseItem(Player player) {
            TransformationHandler.AddTransformation(player, "Ben10Mod:RipJaws");
            return true;
        }
    }
}
