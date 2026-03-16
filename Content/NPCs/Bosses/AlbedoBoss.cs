using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.Items.Consumable;
using Ben10Mod.Content.Projectiles;

namespace Ben10Mod.Content.NPCs.Bosses {
    public class AlbedoBoss : ModNPC {
        private enum AlbedoPhase {
            IntroHuman,
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
        private ref float IntroStage => ref NPC.localAI[2];
        private ref float RocketBurstShotsRemaining => ref NPC.localAI[3];

        public override string Texture => "Ben10Mod/Content/Items/Vanity/Ben10Shirt";

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
            IntroStage = 0f;
            RocketBurstShotsRemaining = 0f;
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
            SetMovementProfile(canFly: false);
            NPC.damage = 0;
            NPC.defense = 24;
            MoveGroundedTowards(target, -120f * NPC.direction, 5.5f, 0.08f);
            PhaseTimer++;

            if (IntroStage == 0f) {
                ShowActionText("Behold the Ultimatrix!");
                IntroStage = 1f;
                NPC.netUpdate = true;
            }

            if (IntroStage == 1f && PhaseTimer >= 75f) {
                ShowActionText("Humungousaur!");
                PlayTransformationEffect();
                IntroStage = 2f;
                NPC.netUpdate = true;
            }

            if (IntroStage == 2f && PhaseTimer >= 145f) {
                ShowActionText("I can go ultimate!");
                IntroStage = 3f;
                NPC.netUpdate = true;
            }

            if (IntroStage == 3f && PhaseTimer >= 215f) {
                ShowActionText("Ultimate Humungousaur!");
                PlayTransformationEffect();
                IntroStage = 4f;
                NPC.netUpdate = true;
            }

            if (PhaseTimer >= 260f)
                TransitionToPhase(AlbedoPhase.UltimateHumungousaur);
        }

        private void RunUltimateHumungousaurPhase(Player target) {
            SetMovementProfile(canFly: false);
            NPC.damage = 100;
            NPC.defense = 30;
            AttackTimer++;

            MoveGroundedTowards(target, -220f * NPC.direction, 7.5f, 0.08f);

            if (RocketBurstShotsRemaining > 0f && AttackTimer % 10f == 0f) {
                FireSingleRocket(target, 9.5f, 28);
                RocketBurstShotsRemaining--;
            }
            else if (RocketBurstShotsRemaining <= 0f && AttackTimer % 120f == 0f) {
                RocketBurstShotsRemaining = 6f;
            }

            if (AttackTimer % 150f == 75f)
                PerformSlam(target, projectileDamage: 36, shockwaveCount: 4, speed: 8f);
        }

        private void RunUltimateEchoEchoPhase(Player target) {
            SetMovementProfile(canFly: true);
            NPC.damage = 75;
            NPC.defense = 22;
            AttackTimer++;

            Vector2 desiredPosition = target.Center + new Vector2(0f, -360f);
            MoveTowards(desiredPosition, 8.5f, 0.07f);

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
                ShowActionText((SwapForm)CurrentSwapForm == SwapForm.Humungousaur
                    ? "Ultimate Humungousaur!"
                    : "Ultimate Echo Echo!");
                PlayTransformationEffect();
            }

            if ((SwapForm)CurrentSwapForm == SwapForm.Humungousaur) {
                SetMovementProfile(canFly: false);
                NPC.damage = 110;
                MoveGroundedTowards(target, -240f * NPC.direction, 8f, 0.08f);

                if (RocketBurstShotsRemaining > 0f && AttackTimer % 9f == 0f) {
                    FireSingleRocket(target, 10f, 30);
                    RocketBurstShotsRemaining--;
                }
                else if (RocketBurstShotsRemaining <= 0f && AttackTimer % 110f == 0f) {
                    RocketBurstShotsRemaining = 5f;
                }

                if (AttackTimer % 140f == 35f)
                    PerformSlam(target, projectileDamage: 38, shockwaveCount: 4, speed: 8f);
            }
            else {
                SetMovementProfile(canFly: true);
                NPC.damage = 80;
                MoveTowards(target.Center + new Vector2(0f, -360f), 8.75f, 0.07f);

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
                ShowActionText("Ultimatrix swap!");
                TransitionToPhase(AlbedoPhase.UltimatrixSwap);
            }
            else if (lifeRatio <= 0.45f && currentPhase == AlbedoPhase.UltimateHumungousaur) {
                ShowActionText("Ultimate Echo Echo!");
                TransitionToPhase(AlbedoPhase.UltimateEchoEcho);
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
            IntroStage = 4f;
            RocketBurstShotsRemaining = 0f;
            NPC.netUpdate = true;
            PlayTransformationEffect();
        }

        private void MoveTowards(Vector2 targetPosition, float maxSpeed, float inertia) {
            if (Vector2.Distance(NPC.Center, targetPosition) < 48f) {
                NPC.velocity *= 0.92f;
                return;
            }

            Vector2 desiredVelocity = NPC.DirectionTo(targetPosition) * maxSpeed;
            NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, inertia);
        }

        private void MoveGroundedTowards(Player target, float desiredHorizontalOffset, float maxSpeed, float acceleration) {
            float targetX = target.Center.X + desiredHorizontalOffset;
            float horizontalDistance = targetX - NPC.Center.X;
            float desiredVelocityX = MathHelper.Clamp(horizontalDistance * 0.04f, -maxSpeed, maxSpeed);

            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, desiredVelocityX, acceleration);

            if (Math.Abs(horizontalDistance) < 36f)
                NPC.velocity.X *= 0.85f;

            if ((NPC.collideX || target.Center.Y < NPC.Center.Y - 32f) && NPC.velocity.Y == 0f)
                NPC.velocity.Y = -9.5f;
        }

        private void SetMovementProfile(bool canFly) {
            NPC.noGravity = canFly;
            NPC.noTileCollide = canFly;
        }

        private void PerformSlam(Player target, int projectileDamage, int shockwaveCount, float speed) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            float direction = Math.Sign(target.Center.X - NPC.Center.X);
            if (direction == 0f)
                direction = NPC.direction == 0 ? 1f : NPC.direction;

            NPC.velocity.X = -direction * 4f;
            if (NPC.velocity.Y == 0f)
                NPC.velocity.Y = -6f;

            for (int i = 0; i < shockwaveCount; i++) {
                float projectileSpeed = speed + i * 0.8f;
                Vector2 spawnPosition = NPC.Bottom + new Vector2(direction * 12f, -10f);
                Vector2 velocity = new(direction * projectileSpeed, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, velocity,
                    ModContent.ProjectileType<AlbedoShockwaveProjectile>(), projectileDamage, 0f, Main.myPlayer);
            }

            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
        }

        private void FireSingleRocket(Player target, float speed, int damage) {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 velocity = NPC.DirectionTo(target.Center) * speed;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                ModContent.ProjectileType<AlbedoRocketProjectile>(), damage, 0f, Main.myPlayer);

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

        private void ShowActionText(string text) {
            if (Main.netMode == NetmodeID.Server)
                return;

            CombatText.NewText(NPC.Hitbox, new Color(255, 80, 80), text, dramatic: true);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            int npcId = GetCurrentDisplayNpcId();
            Texture2D texture = Main.Assets.Request<Texture2D>($"Images/NPC_{npcId}").Value;
            int frameCount = Main.npcFrameCount[npcId];
            if (frameCount <= 0)
                frameCount = 1;

            Rectangle frame = new(0, 0, texture.Width, texture.Height / frameCount);
            Vector2 drawPosition = NPC.Center - screenPos;
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(texture, drawPosition, frame, drawColor, 0f, frame.Size() * 0.5f, 1f, effects, 0f);
            return false;
        }

        private int GetCurrentDisplayNpcId() {
            AlbedoPhase phase = GetCurrentPhase();
            if (phase == AlbedoPhase.IntroHuman)
                return NPCID.Guide;

            if (phase == AlbedoPhase.UltimateEchoEcho)
                return NPCID.Probe;

            if (phase == AlbedoPhase.UltimatrixSwap && (SwapForm)CurrentSwapForm == SwapForm.EchoEcho)
                return NPCID.Probe;

            return NPCID.Golem;
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
