﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.BigChill
{
    public class BigChill : ModItem {
        public static string TransformationDescription =>
            "A spectral aerial skirmisher built around Hoarfrost marks, phasing through danger, and bursting targets while staying in motion.";

        public static IReadOnlyList<string> TransformationAbilities => new[] {
            "Direct hits apply Hoarfrost, slowing enemies and opening a burst window.",
            "Coldfire Breath is the rapid airborne marking tool.",
            "Black Ice Barrage cashes Hoarfrost out into Shiverburst and splinters.",
            "Spectral Phase dashes intangible and turns movement into offense.",
            "Wailing Wake leaves drifting frost clouds behind your movement.",
            "Dead Winter amplifies the whole loop and ends with a freezing pulse.",
            "Ultimate form available through the Ultimatrix branch."
        };

        public override void Load() {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new XLR8Head());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
            
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Ultimate_{EquipType.Head}", EquipType.Head, name: "UltimateBigChill", equipTexture: new XLR8Head());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Ultimate_{EquipType.Body}", EquipType.Body, name: "UltimateBigChill");
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Ultimate_{EquipType.Legs}", EquipType.Legs, name: "UltimateBigChill");
        }

        private void SetupDrawing() {
            if (Main.netMode == NetmodeID.Server)
                return;

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

            int equipSlotHeadUltimate = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Head);
            int equipSlotBodyUltimate = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Body);
            int equipSlotLegsUltimate = EquipLoader.GetEquipSlot(Mod, "UltimateBigChill", EquipType.Legs);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
            
            ArmorIDs.Head.Sets.DrawHead[equipSlotHeadUltimate]        = false;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBodyUltimate]    = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBodyUltimate]       = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegsUltimate] = true;
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
            return !TransformationHandler.HasTransformation(player, "Ben10Mod:BigChill");
        }

        public override bool? UseItem(Player player) {
            TransformationHandler.AddTransformation(player, "Ben10Mod:BigChill");
            return true;
        }
    }

    public class XLR8Head : EquipTexture {
        public override bool IsVanitySet(int head, int body, int legs) => true;
    }
}
