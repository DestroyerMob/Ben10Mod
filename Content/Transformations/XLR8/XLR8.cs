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
            "A Kineceleran momentum assassin that turns ground speed, sharp reversals, targeted dashes, and timebreak pass-throughs into burst damage.";

        public static IReadOnlyList<string> TransformationAbilities => new[] {
            "Speed Strike hits harder shortly after reversing direction.",
            "Velocity Dash slices through enemies and scales with recent ground speed.",
            "Speed overdrive opens stronger momentum and reversal windows.",
            "Vector Dash for targeted repositioning, scaling with cursor distance.",
            "Extreme movement speed, boosted jumping, and water running at full pace.",
            "Horizontal dash by double tapping left or right.",
            "Temporal Distortion freezes the fight and builds flow when XLR8 passes through threats."
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
