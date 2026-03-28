using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Ben10Mod.Content.Items.Placeables;

namespace Ben10Mod.Content.Transformations.HeatBlast
{
    public class HeatBlast : ModItem {
        public static string TransformationDescription =>
            "A Pyronite built for relentless fire control. Heat Blast floods the screen with flames, hurls volatile fire bombs, can superheat the area around him, and summons a blazing halo that spits fire from behind him.";

        public static IReadOnlyList<string> TransformationAbilities => new[] {
            "Primary ability: deploy a flare rod sentry that scorches nearby enemies.",
            "Secondary ability: channel a vertical solar halo behind Heat Blast as five orbiting fire points take turns firing imp fireballs at the cursor.",
            "Tertiary ability: ignite a superheated flame aura that burns enemies inside its radius.",
            "Main attack: rapid flamethrower stream.",
            "Alt attack: hurl explosive fire bombs in quick succession.",
            "Passive: lava immunity, fire walking, and fiery melee hits.",
            "Mobility: gains a powerful flame-propelled extra jump while transformed.",
            "Ultimate attack: sustained high-power fire blast."
        };

        public override void Load() {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new XLR8Head());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.Head}", EquipType.Head, name: "HeatBlastAlt", equipTexture: new XLR8Head());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.Body}", EquipType.Body, name: "HeatBlastAlt");
            EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.Legs}", EquipType.Legs, name: "HeatBlastAlt");
        }

        private void SetupDrawing() {
            if (Main.netMode == NetmodeID.Server)
                return;

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
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
            return !TransformationHandler.HasTransformation(player, "Ben10Mod:HeatBlast");
        }

        public override bool? UseItem(Player player) {
            TransformationHandler.AddTransformation(player, "Ben10Mod:HeatBlast");
            return true;
        }
    }

    public class XLR8Head : EquipTexture {
        public override bool IsVanitySet(int head, int body, int legs) => true;
    }
}
