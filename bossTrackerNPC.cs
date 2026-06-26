using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Consumable;
using Ben10Mod.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
    public class bossTrackerNPC : GlobalNPC {
        public override bool InstancePerEntity => true;

        // Tracks per-player contribution on each boss instance so evolution rewards only go to participants.
        private readonly int[] _damageByPlayer = new int[Main.maxPlayers];
        private static readonly Dictionary<string, int[]> EncounterDamageByPlayer = new();
        private static readonly Dictionary<string, bool[]> EncounterParticipantsByPlayer = new();

        private const float EventSoulDropChance = 0.1f;
        private const float PillarSoulDropChance = 0.1f;
        private const int CachedPumpkinMoonInvasionGroup = -4;
        private const int CachedFrostMoonInvasionGroup = -5;
        private const int CachedSolarEclipseInvasionGroup = -6;
        private const int CachedBloodMoonInvasionGroup = -10;

        private static bool CountsAsBoss(NPC npc) {
            return npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
        }

        private static bool CountsAsTrackedEncounter(NPC npc) {
            if (CountsAsBoss(npc))
                return true;

            return !string.IsNullOrEmpty(GetTransformationIdForBoss(npc.type)) ||
                   !string.IsNullOrEmpty(GetEncounterContributionKey(npc));
        }

        private void RecordDamage(NPC npc, int playerIndex, int damage) {
            if (damage <= 0) return;
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers) return;

            Player p = Main.player[playerIndex];
            if (!p.active) return;

            _damageByPlayer[playerIndex] += damage;

            string encounterKey = GetEncounterContributionKey(npc);
            if (string.IsNullOrEmpty(encounterKey))
                return;

            if (!EncounterDamageByPlayer.TryGetValue(encounterKey, out int[] encounterDamage)) {
                encounterDamage = new int[Main.maxPlayers];
                EncounterDamageByPlayer[encounterKey] = encounterDamage;
            }

            encounterDamage[playerIndex] += damage;

            if (!EncounterParticipantsByPlayer.TryGetValue(encounterKey, out bool[] encounterParticipants)) {
                encounterParticipants = new bool[Main.maxPlayers];
                EncounterParticipantsByPlayer[encounterKey] = encounterParticipants;
            }

            encounterParticipants[playerIndex] = true;
        }



        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) {
            if (damageDone > 0)
                player.GetModPlayer<OmnitrixPlayer>().RecordEventParticipation(npc);

            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            if (!CountsAsTrackedEncounter(npc)) return;

            RecordDamage(npc, player.whoAmI, damageDone);
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
            if (damageDone <= 0) return;

            int owner = projectile.owner;
            if (owner >= 0 && owner < Main.maxPlayers && projectile.friendly && !projectile.hostile) {
                Main.player[owner].GetModPlayer<OmnitrixPlayer>().RecordEventParticipation(npc);

                if (Main.netMode == NetmodeID.MultiplayerClient) return;

                if (!CountsAsTrackedEncounter(npc)) return;

                RecordDamage(npc, owner, damageDone);
            }
        }

        public override void OnKill(NPC npc) {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            TryDropPillarSoul(npc);
            TryDropEventSoul(npc);

            if (!CountsAsTrackedEncounter(npc)) return;

            CaptureEncounterParticipants(npc);

            if (!IsEncounterComplete(npc))
                return;

            string transformationId = GetTransformationIdForBoss(npc.type);
            bool[] participatingPlayers = GetParticipatingPlayers(npc);

            if (!string.IsNullOrEmpty(transformationId))
                SoulOfTransformation.Spawn(npc.GetSource_Death(), npc.Center, transformationId,
                    SoulOfTransformationSource.Boss);

            for (int i = 0; i < Main.maxPlayers; i++) {
                if (!participatingPlayers[i]) continue;

                Player player = Main.player[i];
                if (!player.active) continue;

                var omp = player.GetModPlayer<OmnitrixPlayer>();

                if (!string.IsNullOrEmpty(transformationId) &&
                    ModContent.GetInstance<Ben10ServerConfig>().AllowDirectProgressionUnlocks)
                    omp.UnlockTransformation(transformationId);

                Omnitrix activeOmnitrix = omp.GetActiveOmnitrix();
                if (activeOmnitrix?.ShouldStartEvolution(player, omp, npc.type) == true)
                    activeOmnitrix.StartEvolution(player, omp);
            }

            ClearEncounterContribution(npc);
        }

        private static void TryDropPillarSoul(NPC npc) {
            if (!IsLunarPillar(npc.type))
                return;

            if (Main.rand.NextFloat() >= PillarSoulDropChance)
                return;

            SoulOfTransformation.Spawn(npc.GetSource_Death(), npc.Center, "Ben10Mod:WayBig",
                SoulOfTransformationSource.Boss);
        }

        private static void TryDropEventSoul(NPC npc) {
            if (!TryGetEventSoulTransformationId(npc, out string transformationId))
                return;

            if (Main.rand.NextFloat() >= EventSoulDropChance)
                return;

            SoulOfTransformation.Spawn(npc.GetSource_Death(), npc.Center, transformationId,
                SoulOfTransformationSource.Event);
        }

        private static bool TryGetEventSoulTransformationId(NPC npc, out string transformationId) {
            transformationId = string.Empty;
            if (npc == null || npc.friendly || npc.townNPC || npc.CountsAsACritter)
                return false;

            if (Main.slimeRain && npc.type != NPCID.KingSlime && npc.aiStyle == NPCAIStyleID.Slime) {
                transformationId = "Ben10Mod:Goop";
                return true;
            }

            int invasionGroup = NPC.GetNPCInvasionGroup(npc.type);
            switch (invasionGroup) {
                case InvasionID.GoblinArmy when Main.invasionType == InvasionID.GoblinArmy:
                    transformationId = "Ben10Mod:RipJaws";
                    return true;
                case InvasionID.SnowLegion when Main.invasionType == InvasionID.SnowLegion:
                    transformationId = "Ben10Mod:Fasttrack";
                    return true;
                case InvasionID.PirateInvasion when Main.invasionType == InvasionID.PirateInvasion:
                    transformationId = "Ben10Mod:WaterHazard";
                    return true;
                case InvasionID.MartianMadness when Main.invasionType == InvasionID.MartianMadness:
                    transformationId = "Ben10Mod:Astrodactyl";
                    return true;
                case CachedBloodMoonInvasionGroup when Main.bloodMoon:
                    transformationId = "Ben10Mod:GhostFreak";
                    return true;
                case CachedSolarEclipseInvasionGroup when Main.eclipse:
                    transformationId = "Ben10Mod:Frankenstrike";
                    return true;
                case CachedPumpkinMoonInvasionGroup when Main.pumpkinMoon:
                    transformationId = "Ben10Mod:Whampire";
                    return true;
                case CachedFrostMoonInvasionGroup when Main.snowMoon:
                    transformationId = "Ben10Mod:Lodestar";
                    return true;
            }

            if (Main.bloodMoon && IsBloodMoonEventNpc(npc.type)) {
                transformationId = "Ben10Mod:GhostFreak";
                return true;
            }

            if (Main.eclipse && IsSolarEclipseNpc(npc.type)) {
                transformationId = "Ben10Mod:Frankenstrike";
                return true;
            }

            if (Main.pumpkinMoon && IsPumpkinMoonNpc(npc.type)) {
                transformationId = "Ben10Mod:Whampire";
                return true;
            }

            if (Main.snowMoon && IsFrostMoonNpc(npc.type)) {
                transformationId = "Ben10Mod:Lodestar";
                return true;
            }

            return false;
        }

        private static bool IsLunarPillar(int npcType) {
            return npcType is NPCID.LunarTowerSolar
                or NPCID.LunarTowerVortex
                or NPCID.LunarTowerNebula
                or NPCID.LunarTowerStardust;
        }

        private static bool IsBloodMoonEventNpc(int npcType) {
            return npcType is NPCID.BloodZombie
                or NPCID.Drippler
                or NPCID.ZombieMerman
                or NPCID.BloodEelHead
                or NPCID.BloodEelBody
                or NPCID.BloodEelTail
                or NPCID.BloodNautilus;
        }

        private static bool IsSolarEclipseNpc(int npcType) {
            return npcType is NPCID.Frankenstein
                or NPCID.SwampThing
                or NPCID.Vampire
                or NPCID.VampireBat
                or NPCID.Reaper
                or NPCID.Eyezor
                or NPCID.Mothron
                or NPCID.MothronEgg
                or NPCID.MothronSpawn
                or NPCID.Butcher
                or NPCID.CreatureFromTheDeep
                or NPCID.Fritz
                or NPCID.Nailhead
                or NPCID.Psycho
                or NPCID.DeadlySphere
                or NPCID.DrManFly
                or NPCID.ThePossessed;
        }

        private static bool IsPumpkinMoonNpc(int npcType) {
            return npcType is NPCID.Scarecrow1
                or NPCID.Scarecrow2
                or NPCID.Scarecrow3
                or NPCID.Scarecrow4
                or NPCID.Scarecrow5
                or NPCID.Scarecrow6
                or NPCID.Scarecrow7
                or NPCID.Scarecrow8
                or NPCID.Scarecrow9
                or NPCID.Scarecrow10
                or NPCID.Splinterling
                or NPCID.Hellhound
                or NPCID.Poltergeist
                or NPCID.HeadlessHorseman
                or NPCID.MourningWood
                or NPCID.Pumpking;
        }

        private static bool IsFrostMoonNpc(int npcType) {
            return npcType is NPCID.ZombieXmas
                or NPCID.ZombieSweater
                or NPCID.ZombieElf
                or NPCID.ZombieElfBeard
                or NPCID.ZombieElfGirl
                or NPCID.GingerbreadMan
                or NPCID.Yeti
                or NPCID.Nutcracker
                or NPCID.NutcrackerSpinning
                or NPCID.ElfArcher
                or NPCID.Krampus
                or NPCID.Flocko
                or NPCID.ElfCopter
                or NPCID.PresentMimic
                or NPCID.Everscream
                or NPCID.SantaNK1
                or NPCID.IceQueen;
        }

        private static string GetEncounterContributionKey(NPC npc) {
            return npc.type switch {
                NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail => "EaterOfWorlds",
                NPCID.Retinazer or NPCID.Spazmatism => "Twins",
                NPCID.TheDestroyer or NPCID.TheDestroyerBody or NPCID.TheDestroyerTail => "Destroyer",
                NPCID.SkeletronHead or NPCID.SkeletronHand => "Skeletron",
                NPCID.WallofFlesh or NPCID.WallofFleshEye => "WallOfFlesh",
                NPCID.Golem or NPCID.GolemHead or NPCID.GolemHeadFree or NPCID.GolemFistLeft or NPCID.GolemFistRight => "Golem",
                NPCID.MoonLordCore or NPCID.MoonLordHead or NPCID.MoonLordHand or NPCID.MoonLordFreeEye => "MoonLord",
                _ => string.Empty
            };
        }

        private static bool IsEncounterComplete(NPC npc) {
            string encounterKey = GetEncounterContributionKey(npc);
            if (string.IsNullOrEmpty(encounterKey))
                return true;

            for (int i = 0; i < Main.npc.Length; i++) {
                NPC candidate = Main.npc[i];
                if (!candidate.active || candidate.whoAmI == npc.whoAmI)
                    continue;

                if (GetEncounterContributionKey(candidate) != encounterKey)
                    continue;

                if (IsDyingSharedLifePart(encounterKey, npc, candidate))
                    continue;

                return false;
            }

            return true;
        }

        private static bool IsDyingSharedLifePart(string encounterKey, NPC dyingNpc, NPC candidate) {
            // Destroyer segments can still be active while the shared-life head is dying.
            if (encounterKey != "Destroyer")
                return false;

            int dyingRootIndex = GetSharedLifeRootIndex(dyingNpc);
            int candidateRootIndex = GetSharedLifeRootIndex(candidate);
            if (dyingRootIndex < 0 || dyingRootIndex != candidateRootIndex)
                return false;

            if (dyingRootIndex == dyingNpc.whoAmI)
                return true;

            if (dyingRootIndex >= Main.npc.Length)
                return false;

            NPC root = Main.npc[dyingRootIndex];
            return !root.active || root.life <= 0;
        }

        private static int GetSharedLifeRootIndex(NPC npc) {
            if (npc.realLife >= 0 && npc.realLife < Main.npc.Length)
                return npc.realLife;

            return npc.whoAmI;
        }

        private static void CaptureEncounterParticipants(NPC npc) {
            string encounterKey = GetEncounterContributionKey(npc);
            if (string.IsNullOrEmpty(encounterKey))
                return;

            if (!EncounterParticipantsByPlayer.TryGetValue(encounterKey, out bool[] encounterParticipants)) {
                encounterParticipants = new bool[Main.maxPlayers];
                EncounterParticipantsByPlayer[encounterKey] = encounterParticipants;
            }

            for (int i = 0; i < Main.maxPlayers; i++) {
                if (npc.playerInteraction[i])
                    encounterParticipants[i] = true;
            }

            if (npc.lastInteraction >= 0 && npc.lastInteraction < Main.maxPlayers)
                encounterParticipants[npc.lastInteraction] = true;
        }

        private bool[] GetParticipatingPlayers(NPC npc) {
            bool[] participatingPlayers = new bool[Main.maxPlayers];
            string encounterKey = GetEncounterContributionKey(npc);
            if (!string.IsNullOrEmpty(encounterKey)) {
                if (EncounterParticipantsByPlayer.TryGetValue(encounterKey, out bool[] encounterParticipants)) {
                    for (int i = 0; i < Main.maxPlayers; i++)
                        participatingPlayers[i] = encounterParticipants[i];
                }

                if (EncounterDamageByPlayer.TryGetValue(encounterKey, out int[] encounterDamage)) {
                    for (int i = 0; i < Main.maxPlayers; i++) {
                        if (encounterDamage[i] > 0)
                            participatingPlayers[i] = true;
                    }
                }

                return participatingPlayers;
            }

            for (int i = 0; i < Main.maxPlayers; i++) {
                if (npc.playerInteraction[i] || _damageByPlayer[i] > 0)
                    participatingPlayers[i] = true;
            }

            if (npc.lastInteraction >= 0 && npc.lastInteraction < Main.maxPlayers)
                participatingPlayers[npc.lastInteraction] = true;

            return participatingPlayers;
        }

        private static void ClearEncounterContribution(NPC npc) {
            string encounterKey = GetEncounterContributionKey(npc);
            if (!string.IsNullOrEmpty(encounterKey)) {
                EncounterDamageByPlayer.Remove(encounterKey);
                EncounterParticipantsByPlayer.Remove(encounterKey);
            }
        }

        private static string GetTransformationIdForBoss(int npcType) {
            if (npcType == ModContent.NPCType<AlbedoBoss>())
                return "Ben10Mod:GrayMatter";

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
                case NPCID.GolemHeadFree:
                case NPCID.GolemFistLeft:
                case NPCID.GolemFistRight:
                    return "Ben10Mod:Armodrillo";
                case NPCID.DukeFishron:
                    return "Ben10Mod:Jetray";
                case NPCID.HallowBoss:
                    return "Ben10Mod:AmpFibian";
                case NPCID.CultistBoss:
                    return "Ben10Mod:Terraspin";
                case NPCID.MoonLordCore:
                case NPCID.MoonLordHead:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordFreeEye:
                    return "Ben10Mod:AlienX";

                // Old One's Army bosses / minibosses
                case NPCID.DD2DarkMageT1:
                case NPCID.DD2DarkMageT3:
                    return "Ben10Mod:PeskyDust";
                case NPCID.DD2OgreT2:
                case NPCID.DD2OgreT3:
                    return "Ben10Mod:Cannonbolt";
                case NPCID.DD2Betsy:
                    return "Ben10Mod:Clockwork";

                // Pumpkin Moon minibosses
                case NPCID.MourningWood:
                    return "Ben10Mod:SnareOh";
                case NPCID.Pumpking:
                    return "Ben10Mod:Blitzwolfer";

                // Frost Moon minibosses
                case NPCID.Everscream:
                    return "Ben10Mod:Arctiguana";
                case NPCID.SantaNK1:
                    return "Ben10Mod:NRG";
                case NPCID.IceQueen:
                    return "Ben10Mod:BigChill";

                default:
                    return string.Empty;
            }
        }
    }
}
