using System.Runtime.CompilerServices;
using Ben10Mod.Content;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Enums;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
    public class bossTrackerNPC : GlobalNPC {
        public override bool InstancePerEntity => true;

        // total damage dealt to THIS npc instance by each player
        private readonly int[] _damageByPlayer = new int[Main.maxPlayers];

        // optional: track who last damaged it as a tie-breaker
        private int _lastDamager = -1;

        private static bool CountsAsBoss(NPC npc) {
            // npc.boss is true for most bosses, but this catches extra boss-like NPCs too
            return npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
        }

        private void RecordDamage(int playerIndex, int damage) {
            if (damage <= 0) return;
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers) return;

            Player p = Main.player[playerIndex];
            if (!p.active) return;

            _damageByPlayer[playerIndex] += damage;
            _lastDamager                 =  playerIndex;
        }



        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (!CountsAsBoss(npc)) return;

            // IMPORTANT: use damageDone (actual applied damage)
            RecordDamage(player.whoAmI, damageDone);
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (!CountsAsBoss(npc)) return;
            if (damageDone <= 0) return;

            // Credit only player-owned friendly projectiles (weapons/minions/whips/etc.)
            int owner = projectile.owner;
            if (owner >= 0 && owner < Main.maxPlayers && projectile.friendly && !projectile.hostile) {
                RecordDamage(owner, damageDone);
            }
        }

        public override void OnKill(NPC npc) {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (!CountsAsBoss(npc)) return;

            int credited = GetTopDamager(npc);
            if (credited == -1) return;

            int eaterCount = 0;

            if (npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsHead ||
                npc.type == NPCID.EaterofWorldsTail) {
                for (int i = 0; i < Main.npc.Length; i++) {
                    if (Main.npc[i].type == NPCID.EaterofWorldsBody || Main.npc[i].type == NPCID.EaterofWorldsHead ||
                        Main.npc[i].type == NPCID.EaterofWorldsTail) {
                        if (Main.npc[i].active) {
                            eaterCount++;
                        }
                    }
                }
            }

            if (eaterCount > 1)
                return;

            int twinsCount = 0;

            if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism) {
                for (int i = 0; i < Main.npc.Length; i++) {
                    if (Main.npc[i].type == NPCID.Retinazer || Main.npc[i].type == NPCID.Spazmatism) {
                        if (Main.npc[i].active) {
                            twinsCount++;
                        }
                    }
                }
            }

            if (twinsCount > 1)
                return;

            string msg = $"{Main.player[credited].name} dealt the most damage!";
            
            var player = Main.player[credited];

            // Show message in both SP and MP
            if (Main.netMode == NetmodeID.SinglePlayer) {
                Main.NewText(msg, Color.Cyan);
            }
            else if (Main.netMode == NetmodeID.Server) {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(
                    Terraria.Localization.NetworkText.FromLiteral(msg),
                    Color.Cyan
                );
            }

            switch (npc.type) {
                case NPCID.KingSlime: {
                    player.GetModPlayer<OmnitrixPlayer>()
                        .AddTransformation(TransformationEnum.DiamondHead);
                    break;
                }
                case NPCID.EyeofCthulhu: {
                    player.GetModPlayer<OmnitrixPlayer>()
                        .AddTransformation(TransformationEnum.XLR8);
                    break;
                }
                case NPCID.BrainofCthulhu: {
                    player.GetModPlayer<OmnitrixPlayer>()
                        .AddTransformation(TransformationEnum.FourArms);
                    break;
                }
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsTail:
                case NPCID.EaterofWorldsBody: {
                    player.GetModPlayer<OmnitrixPlayer>()
                        .AddTransformation(TransformationEnum.FourArms);
                    break;
                }
                case NPCID.QueenBee: {
                    player.GetModPlayer<OmnitrixPlayer>()
                        .AddTransformation(TransformationEnum.StinkFly);
                    break;
                }
                case NPCID.SkeletronHead: {
                    player.GetModPlayer<OmnitrixPlayer>()
                        .AddTransformation(TransformationEnum.BuzzShock);
                    break;
                }
                case NPCID.Deerclops: {
                    player.GetModPlayer<OmnitrixPlayer>()
                        .AddTransformation(TransformationEnum.WildVine);
                    break;
                }
                case NPCID.WallofFlesh: {
                    if (player.GetModPlayer<OmnitrixPlayer>().prototypeOmnitrix) {
                        TransformationHandler.Detransform(player, 120);
                        player.AddBuff(ModContent.BuffType<OmnitrixUpdating>(), 120 * 60);
                    }
                    break;
                }
                case NPCID.QueenSlimeBoss: {
                    player.GetModPlayer<OmnitrixPlayer>()
                        .AddTransformation(TransformationEnum.ChromaStone);
                    break;
                }
                case NPCID.Retinazer:
                case NPCID.Spazmatism: 
                    player.GetModPlayer<OmnitrixPlayer>().AddTransformation(TransformationEnum.EyeGuy);
                    break;
                case NPCID.IceQueen:
                    player.GetModPlayer<OmnitrixPlayer>().AddTransformation(TransformationEnum.BigChill);
                    break;
                default: break;
            }
            
            // === SYNC THE UNLOCK TO THE CLIENT ===
            if (Main.netMode == NetmodeID.Server && credited >= 0 && credited < Main.maxPlayers)
            {
                NetMessage.SendData(MessageID.SyncPlayer, credited, -1, null, credited);
            }

            // TODO: store the kill credit here
            // Main.player[credited].GetModPlayer<YourBossKillStatsPlayer>().RegisterBossKill(npc.type);
        }

        private int GetTopDamager(NPC npc) {
            int bestPlayer = -1;
            int bestDamage = 0;

            for (int i = 0; i < Main.maxPlayers; i++) {
                if (!Main.player[i].active) continue;

                int dmg = _damageByPlayer[i];
                if (dmg > bestDamage) {
                    bestDamage = dmg;
                    bestPlayer = i;
                }
            }

            // If nobody recorded (weird edge case), fallback:
            if (bestPlayer == -1) {
                if (npc.lastInteraction >= 0 && npc.lastInteraction < Main.maxPlayers &&
                    Main.player[npc.lastInteraction].active)
                    return npc.lastInteraction;

                if (_lastDamager >= 0 && _lastDamager < Main.maxPlayers && Main.player[_lastDamager].active)
                    return _lastDamager;
            }

            return bestPlayer;
        }
    }
}
