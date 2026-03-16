using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Ben10Mod.Content.Items.Consumable;
using Ben10Mod.Content.Projectiles;

namespace Ben10Mod.Content.NPCs.Bosses {
    public class AlbedoBoss : ModNPC {
        private enum AlbedoPhase {
            IntroHuman,
            BaseHumungousaur,
            UltimateHumungousaur,
            UltimateEchoEcho,
            UltimatrixSwap
        }

        private enum SwapForm {
            Humungousaur,
            EchoEcho
        }

        private ref float PhaseTimer => ref NPC.ai[0];
        private ref float AttackTimer => ref NPC.ai[1];
        private ref float AuxTimer => ref NPC.ai[2];
        private ref float CurrentSwapForm => ref NPC.ai[3];
        private ref float DialogueShown => ref NPC.localAI[1];

        public override string Texture => "Ben10Mod/Content/Items/Accessories/UltimatrixAlt";

        public override void SetStaticDefaults() {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
        }

        public override void SetDefaults() {
            NPC.width = 72;
            NPC.height = 120;
            NPC.damage = 80;
            NPC.defense = 24;
            NPC.lifeMax = 42000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = Item.buyPrice(gold: 15);
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
            Music = MusicID.Boss3;
        }

        public override void OnSpawn(IEntitySource source) {
            NPC.localAI[0] = (float)AlbedoPhase.IntroHuman;
            PhaseTimer = 0f;
            AttackTimer = 0f;
            AuxTimer = 0f;
            CurrentSwapForm = (float)SwapForm.Humungousaur;
            DialogueShown = 0f;
        }

        public override void AI() {
            if (!TryGetTarget(out Player target)) {
                Despawn();
                return;
            }

            NPC.TargetClosest();
            NPC.direction = NPC.spriteDirection = target.Center.X >= NPC.Center.X ? 1 : -1;

            AlbedoPhase phase = GetCurrentPhase();
            UpdatePhaseThresholds();

            switch (phase) {
                case AlbedoPhase.IntroHuman:
                    RunIntroPhase(target);
                    break;
                case AlbedoPhase.BaseHumungousaur:
                    RunBaseHumungousaurPhase(target);
                    break;
                case AlbedoPhase.UltimateHumungousaur:
                    RunUltimateHumungousaurPhase(target);
                    break;
                case AlbedoPhase.UltimateEchoEcho:
                    RunUltimateEchoEchoPhase(target);
                    break;
                case AlbedoPhase.UltimatrixSwap:
                    RunUltimatrixSwapPhase(target);
                    break;
            }
        }

        private void RunIntroPhase(Player target) {
            NPC.damage = 0;
            NPC.defense = 24;
            MoveTowards(target.Center + new Vector2(0f, -140f), 10f, 0.08f);
            PhaseTimer++;

            if (DialogueShown == 0f) {
                BroadcastDialogue("You think you deserve the Ultimatrix? Then prove it.");
                DialogueShown = 1f;
                NPC.netUpdate = true;
            }

            if (PhaseTimer >= 120f)
                TransitionToPhase(AlbedoPhase.BaseHumungousaur);
        }

        private void RunBaseHumungousaurPhase(Player target) {
            NPC.damage = 90;
            NPC.defense = 26;
            AttackTimer++;

            Vector2 desiredPosition = target.Center + new Vector2(target.direction * -140f, -40f);
            MoveTowards(desiredPosition, 11f, 0.11f);

            if (AttackTimer % 120f == 0f)
                PerformSlam(target, projectileDamage: 32, shockwaveCount: 8, speed: 8f);
        }

        private void RunUltimateHumungousaurPhase(Player target) {
            NPC.damage = 100;
            NPC.defense = 30;
            AttackTimer++;

            Vector2 desiredPosition = target.Center + new Vector2(target.direction * -180f, -70f);
            MoveTowards(desiredPosition, 12.5f, 0.1f);

            if (AttackTimer % 90f == 0f)
                FireRocketVolley(target, 3, 10f, 28);

            if (AttackTimer % 150f == 75f)
                PerformSlam(target, projectileDamage: 36, shockwaveCount: 10, speed: 9f);
        }

        private void RunUltimateEchoEchoPhase(Player target) {
            NPC.damage = 75;
            NPC.defense = 22;
            AttackTimer++;

            Vector2 desiredPosition = target.Center + new Vector2(0f, -220f);
            MoveTowards(desiredPosition, 10f, 0.08f);

            if (AttackTimer % 75f == 0f)
                SpawnSpeaker(target, 24);

            if (AttackTimer % 150f == 30f)
                SpawnSpeaker(target, 24);
        }

        private void RunUltimatrixSwapPhase(Player target) {
            NPC.defense = 28;
            AttackTimer++;
            AuxTimer++;

            if (AuxTimer == 1f || AuxTimer >= 120f) {
                AuxTimer = 1f;
                CurrentSwapForm = CurrentSwapForm == (float)SwapForm.Humungousaur
                    ? (float)SwapForm.EchoEcho
                    : (float)SwapForm.Humungousaur;
                PlayTransformationEffect();
            }

            if ((SwapForm)CurrentSwapForm == SwapForm.Humungousaur) {
                NPC.damage = 110;
                MoveTowards(target.Center + new Vector2(target.direction * -160f, -50f), 13f, 0.11f);

                if (AttackTimer % 70f == 0f)
                    FireRocketVolley(target, 2, 10f, 30);

                if (AttackTimer % 140f == 35f)
                    PerformSlam(target, projectileDamage: 38, shockwaveCount: 10, speed: 9f);
            }
            else {
                NPC.damage = 80;
                MoveTowards(target.Center + new Vector2(0f, -240f), 10.5f, 0.08f);

                if (AttackTimer % 60f == 0f)
                    SpawnSpeaker(target, 26);
            }
        }

        private void UpdatePhaseThresholds() {
            float lifeRatio = NPC.life / (float)NPC.lifeMax;
            AlbedoPhase currentPhase = GetCurrentPhase();

            if (currentPhase == AlbedoPhase.IntroHuman)
                return;

            if (lifeRatio <= 0.20f && currentPhase != AlbedoPhase.UltimatrixSwap) {
                TransitionToPhase(AlbedoPhase.UltimatrixSwap);
            }
            else if (lifeRatio <= 0.45f && currentPhase == AlbedoPhase.UltimateHumungousaur) {
                TransitionToPhase(AlbedoPhase.UltimateEchoEcho);
            }
            else if (lifeRatio <= 0.72f && currentPhase == AlbedoPhase.BaseHumungousaur) {
                TransitionToPhase(AlbedoPhase.UltimateHumungousaur);
            }
        }

        private AlbedoPhase GetCurrentPhase() {
            return (AlbedoPhase)NPC.localAI[0];
        }

        private void TransitionToPhase(AlbedoPhase nextPhase) {
            NPC.localAI[0] = (float)nextPhase;
            PhaseTimer = 0f;
            AttackTimer = 0f;
            AuxTimer = 0f;
            DialogueShown = 1f;
            NPC.netUpdate = true;
            PlayTransformationEffect();
        }

        private void MoveTowards(Vector2 targetPosition, float maxSpeed, float inertia) {
            Vector2 desiredVelocity = NPC.DirectionTo(targetPosition) * maxSpeed;
            NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, inertia);
        }

        private void PerformSlam(Player target, int projectileDamage, int shockwaveCount, float speed) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 targetCenter = target.Center;
            NPC.velocity = NPC.DirectionTo(targetCenter) * 18f;

            for (int i = 0; i < shockwaveCount; i++) {
                Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / shockwaveCount) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                    ModContent.ProjectileType<AlbedoShockwaveProjectile>(), projectileDamage, 0f, Main.myPlayer);
            }

            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
        }

        private void FireRocketVolley(Player target, int rocketCount, float speed, int damage) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < rocketCount; i++) {
                Vector2 velocity = NPC.DirectionTo(target.Center).RotatedBy(MathHelper.ToRadians(10f * (i - (rocketCount - 1) / 2f))) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                    ModContent.ProjectileType<AlbedoRocketProjectile>(), damage, 0f, Main.myPlayer);
            }

            SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
        }

        private void SpawnSpeaker(Player target, int damage) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center,
                NPC.DirectionTo(target.Center) * 3f, ModContent.ProjectileType<AlbedoSpeakerProjectile>(),
                damage, 0f, Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item43, NPC.Center);
        }

        private void PlayTransformationEffect() {
            for (int i = 0; i < 24; i++) {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Firework_Red, Scale: 1.5f);
                dust.velocity *= 1.8f;
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(new SoundStyle("Ben10Mod/Content/Sounds/OmnitrixTransformation"), NPC.Center);
        }

        private void BroadcastDialogue(string text) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (Main.netMode == NetmodeID.SinglePlayer) {
                Main.NewText(text, 255, 80, 80);
            }
            else if (Main.netMode == NetmodeID.Server) {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), new Color(255, 80, 80));
            }
        }

        private bool TryGetTarget(out Player target) {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            return target.active && !target.dead;
        }

        private void Despawn() {
            NPC.velocity.Y -= 0.2f;
            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<UltimatrixCore>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Materials.HeroFragment>(), 1, 10, 16));
        }
    }
}
