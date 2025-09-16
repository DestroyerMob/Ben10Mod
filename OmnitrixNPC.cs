using Ben10Mod.Common.Command;
using Ben10Mod.Content;
using Ben10Mod.Content.Items;
using Ben10Mod.Content.Items.Vanity;
using Ben10Mod.Enums;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
    public class OmnitrixNPC : GlobalNPC {

        public override void ModifyShop(NPCShop shop) {
            if (shop.NpcType == NPCID.Clothier) {
                shop.Add(new Item(ModContent.ItemType<Ben10Pants>()));
                shop.Add(new Item(ModContent.ItemType<Ben10Shirt>()));
            }
        }

        public override void OnKill(NPC npc)
        {
            base.OnKill(npc);

            if (npc.boss)
            {
                int killerIndex = npc.lastInteraction;
                if (killerIndex >= 0 && killerIndex < Main.maxPlayers)
                {
                    Player killer = Main.player[killerIndex];
                    if (killer != null && killer.active)
                    {
                        switch (npc.type)
                        {
                            case NPCID.KingSlime:
                                addTransformation(killer, TransformationEnum.DiamondHead);
                                break;
                            case NPCID.EyeofCthulhu:
                                addTransformation(killer, TransformationEnum.XLR8);
                                break;
                            case NPCID.BrainofCthulhu:
                                addTransformation(killer, TransformationEnum.FourArms);
                                break;
                            case NPCID.EaterofWorldsHead:
                                if (NPC.downedBoss2)
                                {
                                    addTransformation(killer, TransformationEnum.FourArms);
                                }
                                break;
                            case NPCID.EaterofWorldsBody:
                                if (NPC.downedBoss2)
                                {
                                    addTransformation(killer, TransformationEnum.FourArms);
                                }
                                break;
                            case NPCID.QueenBee:
                                addTransformation(killer, TransformationEnum.StinkFly);
                                break;
                            case NPCID.SkeletronHead:
                                addTransformation(killer, TransformationEnum.BuzzShock);
                                break;
                            case NPCID.Deerclops:
                                addTransformation(killer, TransformationEnum.WildVine);
                                break;
                            case NPCID.WallofFlesh:
                                addTransformation(killer, TransformationEnum.ChromaStone);
                                break;
                            case NPCID.WallofFleshEye:
                                addTransformation(killer, TransformationEnum.ChromaStone);
                                break;
                            case NPCID.HallowBoss:
                                if (Main.dayTime)
                                {
                                    if (!killer.GetModPlayer<OmnitrixPlayer>().masterControl)
                                    {
                                        Main.NewText(killer.name + " has unlocked master control!", Color.Green);
                                        killer.GetModPlayer<OmnitrixPlayer>().masterControl = true;
                                    }
                                }
                                break;
                        }
                    }
                }

            }
        }

        private void addTransformation(Player player, TransformationEnum transformation) {

            if (!TransformationHandler.HasTransformation(player, transformation))
            {
                player.GetModPlayer<OmnitrixPlayer>().unlockedTransformation.Add(transformation);
                Main.NewText(player.name + " has unlocked " + transformation.GetName(), Color.Green);
            }
        }

    }
}
