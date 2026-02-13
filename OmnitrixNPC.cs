using System.Runtime.CompilerServices;
using Ben10Mod.Content;
using Ben10Mod.Enums;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
    public class OmnitrixNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // total damage dealt to THIS npc instance by each player
        private readonly int[] _damageByPlayer = new int[Main.maxPlayers];

        // optional: track who last damaged it as a tie-breaker
        private int _lastDamager = -1;

        private static bool CountsAsBoss(NPC npc)
        {
            // npc.boss is true for most bosses, but this catches extra boss-like NPCs too
            return npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
        }

        private void RecordDamage(int playerIndex, int damage)
        {
            if (damage <= 0) return;
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers) return;

            Player p = Main.player[playerIndex];
            if (!p.active) return;

            _damageByPlayer[playerIndex] += damage;
            _lastDamager = playerIndex;
        }

        

        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (!CountsAsBoss(npc)) return;

            // IMPORTANT: use damageDone (actual applied damage)
            RecordDamage(player.whoAmI, damageDone);
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (!CountsAsBoss(npc)) return;
            if (damageDone <= 0) return;

            // Credit only player-owned friendly projectiles (weapons/minions/whips/etc.)
            int owner = projectile.owner;
            if (owner >= 0 && owner < Main.maxPlayers && projectile.friendly && !projectile.hostile) {
                RecordDamage(owner, damageDone);
            }
        }

        public override void OnKill(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (!CountsAsBoss(npc)) return;

            int credited = GetTopDamager(npc);
            if (credited == -1) return;

            string msg = $"{Main.player[credited].name} dealt the most damage!";

            // Show message in both SP and MP
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(msg, Color.Cyan);
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(
                    Terraria.Localization.NetworkText.FromLiteral(msg),
                    Color.Cyan
                );
            }

            switch (npc.type) {
                case NPCID.KingSlime: {
                    Main.player[credited].GetModPlayer<OmnitrixPlayer>()
                        .addTransformation(TransformationEnum.DiamondHead);
                    break;
                }
                case NPCID.EyeofCthulhu: {
                    Main.player[credited].GetModPlayer<OmnitrixPlayer>()
                        .addTransformation(TransformationEnum.XLR8);
                    break;
                }
                case NPCID.BrainofCthulhu: {
                    Main.player[credited].GetModPlayer<OmnitrixPlayer>()
                        .addTransformation(TransformationEnum.FourArms);
                    break;
                }
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsTail:
                case NPCID.EaterofWorldsBody: {
                    Main.player[credited].GetModPlayer<OmnitrixPlayer>()
                        .addTransformation(TransformationEnum.FourArms);
                    break;
                }
                case NPCID.QueenBee: {
                    Main.player[credited].GetModPlayer<OmnitrixPlayer>()
                        .addTransformation(TransformationEnum.StinkFly);
                    break;
                }
                case NPCID.SkeletronHead: {
                    Main.player[credited].GetModPlayer<OmnitrixPlayer>()
                        .addTransformation(TransformationEnum.BuzzShock);
                    break;
                }
                case NPCID.Deerclops: {
                    Main.player[credited].GetModPlayer<OmnitrixPlayer>()
                        .addTransformation(TransformationEnum.WildVine);
                    break;
                }
                case NPCID.WallofFlesh: {
                    Main.player[credited].GetModPlayer<OmnitrixPlayer>()
                        .addTransformation(TransformationEnum.ChromaStone);
                    break;
                }
                default: break;
            }

            // TODO: store the kill credit here
            // Main.player[credited].GetModPlayer<YourBossKillStatsPlayer>().RegisterBossKill(npc.type);
        }

        private int GetTopDamager(NPC npc)
        {
            int bestPlayer = -1;
            int bestDamage = 0;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (!Main.player[i].active) continue;

                int dmg = _damageByPlayer[i];
                if (dmg > bestDamage)
                {
                    bestDamage = dmg;
                    bestPlayer = i;
                }
            }

            // If nobody recorded (weird edge case), fallback:
            if (bestPlayer == -1)
            {
                if (npc.lastInteraction >= 0 && npc.lastInteraction < Main.maxPlayers && Main.player[npc.lastInteraction].active)
                    return npc.lastInteraction;

                if (_lastDamager >= 0 && _lastDamager < Main.maxPlayers && Main.player[_lastDamager].active)
                    return _lastDamager;
            }

            return bestPlayer;
        }
    }
}
