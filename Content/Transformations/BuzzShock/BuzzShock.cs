using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Ben10Mod.Enums;

namespace Ben10Mod.Content.Transformations.BuzzShock
{
    public class BuzzShock : ModItem {
        public static string TransformationDescription =>
            "A living electric spark that mixes mobility, summoning, and projectile pressure. Buzzshock can blink across the arena, fire lightning, and call in support units.";

        public static IReadOnlyList<string> TransformationAbilities => new[] {
            "Primary ability: instant short-range teleport to the cursor.",
            "Main attack: electric projectile bolt.",
            "Alt attack: summon a Buzzshock minion to fight for you.",
            "Ultimate attack: multi-shot lightning barrage.",
            "Role: mobile caster-summoner hybrid with high utility."
        };

        public override void Load() {
            // The code below runs only if we're not loading on a server
            if (Main.netMode == NetmodeID.Server)
                return;

            // Add equip textures
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this, equipTexture: new XLR8Head());
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);

            //Add a separate set of equip textures by providing a custom name reference instead of an item reference
            //EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.Head}", EquipType.Head, name: "BlockyAlt", equipTexture: new BlockyHead());
            //EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.Body}", EquipType.Body, name: "BlockyAlt");
            //EquipLoader.AddEquipTexture(Mod, $"{Texture}Alt_{EquipType.Legs}", EquipType.Legs, name: "BlockyAlt");
        }

        // Called in SetStaticDefaults
        private void SetupDrawing() {
            // Since the equipment textures weren't loaded on the server, we can't have this code running server-side
            if (Main.netMode == NetmodeID.Server)
                return;

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

            //int equipSlotHeadAlt = EquipLoader.GetEquipSlot(Mod, "BlockyAlt", EquipType.Head);
            //int equipSlotBodyAlt = EquipLoader.GetEquipSlot(Mod, "BlockyAlt", EquipType.Body);
            //int equipSlotLegsAlt = EquipLoader.GetEquipSlot(Mod, "BlockyAlt", EquipType.Legs);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
            //ArmorIDs.Head.Sets.DrawHead[equipSlotHeadAlt] = false;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
            //ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBodyAlt] = true;
            //ArmorIDs.Body.Sets.HidesArms[equipSlotBodyAlt] = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
            //ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegsAlt] = true;
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

        public override bool CanUseItem(Player player) => !TransformationHandler.HasTransformation(player, TransformationEnum.BuzzShock);

        public override bool? UseItem(Player player) {
            player.GetModPlayer<OmnitrixPlayer>().unlockedTransformation.Add(TransformationEnum.BuzzShock);
            return true;
        }
    }

    public class XLR8Head : EquipTexture {
        public override bool IsVanitySet(int head, int body, int legs) => true;
    }
}
