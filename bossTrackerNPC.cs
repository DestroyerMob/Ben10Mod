using Ben10Mod.Content;
using Ben10Mod.Content.Buffs.Abilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
    public class bossTrackerNPC : GlobalNPC {
        public override bool InstancePerEntity => true;

        // Tracks per-player contribution on each boss instance so unlock rewards only go to participants.
        private readonly int[] _damageByPlayer = new int[Main.maxPlayers];

        private static bool CountsAsBoss(NPC npc) {
            return npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
        }

        private static bool CountsAsTrackedEncounter(NPC npc) {
            if (CountsAsBoss(npc))
                return true;

            return !string.IsNullOrEmpty(GetTransformationIdForBoss(npc.type));
        }

        private void RecordDamage(int playerIndex, int damage) {
            if (damage <= 0) return;
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers) return;

            Player p = Main.player[playerIndex];
            if (!p.active) return;

            _damageByPlayer[playerIndex] += damage;
        }



        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            if (damageDone > 0)
                player.GetModPlayer<OmnitrixPlayer>().RecordEventParticipation(npc);

            if (!CountsAsTrackedEncounter(npc)) return;

            RecordDamage(player.whoAmI, damageDone);
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (damageDone <= 0) return;

            int owner = projectile.owner;
            if (owner >= 0 && owner < Main.maxPlayers && projectile.friendly && !projectile.hostile) {
                Main.player[owner].GetModPlayer<OmnitrixPlayer>().RecordEventParticipation(npc);

                if (!CountsAsTrackedEncounter(npc)) return;

                RecordDamage(owner, damageDone);
            }
        }

        public override void OnKill(NPC npc) {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (!CountsAsTrackedEncounter(npc)) return;

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

            string transformationId = GetTransformationIdForBoss(npc.type);

            for (int i = 0; i < Main.maxPlayers; i++) {
                if (_damageByPlayer[i] <= 0) continue;

                Player player = Main.player[i];
                if (!player.active) continue;

                var omp = player.GetModPlayer<OmnitrixPlayer>();

                if (!string.IsNullOrEmpty(transformationId))
                    TransformationHandler.AddTransformation(player, transformationId);

                if (omp.equippedOmnitrix?.ShouldStartEvolution(player, omp, npc.type) == true)
                    omp.equippedOmnitrix.StartEvolution(player, omp);
            }
        }

        private static string GetTransformationIdForBoss(int npcType) {
            switch (npcType) {
                // Pre-hardmode bosses
                case NPCID.KingSlime:
                    return "Ben10Mod:DiamondHead";
                case NPCID.EyeofCthulhu:
                    return "Ben10Mod:XLR8";
                case NPCID.BrainofCthulhu:
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsTail:
                case NPCID.EaterofWorldsBody:
                    return "Ben10Mod:FourArms";
                case NPCID.QueenBee:
                    return "Ben10Mod:StinkFly";
                case NPCID.SkeletronHead:
                    return "Ben10Mod:BuzzShock";
                case NPCID.Deerclops:
                    return "Ben10Mod:WildVine";
                case NPCID.WallofFlesh:
                    return "Ben10Mod:Rath";

                // Early hardmode bosses
                case NPCID.QueenSlimeBoss:
                    return "Ben10Mod:ChromaStone";
                case NPCID.TheDestroyer:
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                    return "Ben10Mod:Humungousaur";
                case NPCID.Retinazer:
                case NPCID.Spazmatism:
                    return "Ben10Mod:EyeGuy";
                case NPCID.SkeletronPrime:
                    return "Ben10Mod:EchoEcho";

                // Mid / late hardmode bosses
                case NPCID.Plantera:
                    return "Ben10Mod:Swampfire";
                case NPCID.Golem:
                case NPCID.GolemHead:
                case NPCID.GolemFistLeft:
                case NPCID.GolemFistRight:
                    return string.Empty;
                case NPCID.DukeFishron:
                    return string.Empty;
                case NPCID.HallowBoss:
                    return string.Empty;
                case NPCID.CultistBoss:
                    return string.Empty;
                case NPCID.MoonLordCore:
                case NPCID.MoonLordHead:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordFreeEye:
                    return string.Empty;

                // Old One's Army bosses / minibosses
                case NPCID.DD2DarkMageT1:
                case NPCID.DD2DarkMageT3:
                    return string.Empty;
                case NPCID.DD2OgreT2:
                case NPCID.DD2OgreT3:
                    return string.Empty;
                case NPCID.DD2Betsy:
                    return string.Empty;

                // Pumpkin Moon minibosses
                case NPCID.MourningWood:
                    return string.Empty;
                case NPCID.Pumpking:
                    return string.Empty;

                // Frost Moon minibosses
                case NPCID.Everscream:
                    return string.Empty;
                case NPCID.SantaNK1:
                    return string.Empty;
                case NPCID.IceQueen:
                    return "Ben10Mod:BigChill";

                default:
                    return string.Empty;
            }
        }
    }
}
