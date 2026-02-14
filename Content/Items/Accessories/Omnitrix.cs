 using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Transformations.XLR8;
using Ben10Mod.Keybinds;
using Microsoft.Xna.Framework;
using Steamworks;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Ben10Mod.Enums;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using System.Security.Cryptography.X509Certificates;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Buffs.Abilities.ChromaStone;
using Ben10Mod.Content.Buffs.Abilities.DiamondHead;
using Ben10Mod.Content.Buffs.Abilities.HeatBlast;
using Ben10Mod.Content.Buffs.Abilities.XLR8;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.Items.Placeables;
using Ben10Mod.Content.DamageClasses;
using Terraria.ModLoader.Default;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace Ben10Mod.Content.Items.Accessories
{
    public class Omnitrix : ModItem {

        private Player player = null;
        public int transformationNum = 0;

        public override string Texture => $"Terraria/Images/Item_{ItemID.None}";

        public override void SetDefaults() {
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Master;
            Item.DamageType = ModContent.GetInstance<HeroDamage>();
        }
    }
}