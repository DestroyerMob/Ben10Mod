using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Accessories;
using Terraria.Audio;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Content.Projectiles;

namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI != Main.myPlayer)
                return true;

            var trans = CurrentTransformation;
            return trans?.CanBeHitByNPC(Player, this, npc, ref cooldownSlot) ?? true;
        }

        public override bool CanBeHitByProjectile(Projectile proj) {
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI != Main.myPlayer)
                return true;

            var trans = CurrentTransformation;
            return trans?.CanBeHitByProjectile(Player, this, proj) ?? true;
        }

        public override bool FreeDodge(Player.HurtInfo info) {
            var trans = CurrentTransformation;
            return trans?.FreeDodge(Player, this, info) ?? false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
            var trans = CurrentTransformation;
            trans?.ModifyHurt(Player, this, ref modifiers);
        }

        public override void OnHurt(Player.HurtInfo info) {
            CurrentTransformation?.OnHurt(Player, this, info);
            base.OnHurt(info);
        }

        public override void PostHurt(Player.HurtInfo info) {
            CurrentTransformation?.PostHurt(Player, this, info);
            base.PostHurt(info);
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust,
            ref PlayerDeathReason damageSource) {
            if (TryTriggerCompletedOmnitrixRevival()) {
                playSound = false;
                genDust = false;
                return false;
            }

            if (!HasTransformationFailsafeEquipped() || !IsTransformed)
                return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);

            TriggerTransformationFailsafe();
            playSound = false;
            genDust = false;
            return false;
        }

        public override void OnHitAnything(float x, float y, Entity victim) {
            CurrentTransformation?.OnHitAnything(Player, this, victim, x, y);
        }

        public override bool? CanHitNPCWithItem(Item item, NPC target) {
            var trans = CurrentTransformation;
            return trans?.CanHitNPCWithItem(Player, this, item, target);
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
            var trans = CurrentTransformation;
            trans?.ModifyHitNPCWithItem(Player, this, item, target, ref modifiers);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
            var trans = CurrentTransformation;
            trans?.OnHitNPCWithItem(Player, this, item, target, hit, damageDone);
            ApplyAbsorptionHitEffects(target);
            base.OnHitNPCWithItem(item, target, hit, damageDone);
            TryAccumulateOmniCoreReactorChargeFromHit(damageDone, item: item);
            TryGrantOmnitrixEnergyFromDamage(damageDone);
            TryTriggerHeroConvergence(target, damageDone, IsHeroConvergenceItem(item));
        }

        public override bool? CanHitNPCWithProj(Projectile proj, NPC target) {
            var trans = CurrentTransformation;
            return trans?.CanHitNPCWithProjectile(Player, this, proj, target);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
            var trans = CurrentTransformation;
            trans?.ModifyHitNPCWithProjectile(Player, this, proj, target, ref modifiers);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
            var trans = CurrentTransformation;
            trans?.OnHitNPCWithProjectile(Player, this, proj, target, hit, damageDone);
            ApplyAbsorptionHitEffects(target);
            base.OnHitNPCWithProj(proj, target, hit, damageDone);
            TryAccumulateOmniCoreReactorChargeFromHit(damageDone, projectile: proj);
            TryGrantOmnitrixEnergyFromDamage(damageDone, proj);
            TryTriggerHeroConvergence(target, damageDone, IsHeroConvergenceProjectile(proj));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            var trans = CurrentTransformation;
            trans?.OnHitNPC(Player, this, target, hit, damageDone);
            ApplyAbsorptionHitEffects(target);

            base.OnHitNPC(target, hit, damageDone);
        }

        private bool ShouldBlockOmnitrixEnergyGain(Projectile projectile = null) {
            if (projectile != null && projectile.GetGlobalProjectile<OmnitrixProjectile>().BlocksOmnitrixEnergyGain)
                return true;

            if (AttackSelectionState.BlocksOmnitrixEnergyGain())
                return true;

            Transformation transformation = CurrentTransformation;
            if (Player.HeldItem?.ModItem is not PlumbersBadge || transformation == null)
                return false;

            if (transformation.GetEnergyCost(this) > 0)
                return true;

            AttackSelection selection = transformation.ResolveAttackSelection(setAttack, this);
            return transformation.GetAttackSustainEnergyCost(selection, this) > 0;
        }

        private void TryGrantOmnitrixEnergyFromDamage(int damageDone, Projectile projectile = null) {
            Omnitrix activeOmnitrix = GetActiveOmnitrix();
            if (!isTransformed || ultimateAttack || IsUltimateAbilityActive || activeOmnitrix == null || damageDone <= 0)
                return;

            if (IsAccessoryProcProjectile(projectile))
                return;

            if (ShouldBlockOmnitrixEnergyGain(projectile))
                return;

            RestoreOmnitrixEnergy(activeOmnitrix.GetEnergyGainFromDamage(damageDone));
        }

        private void UpdateAccessoryProcStates() {
            if (chronoAcceleratorProcCooldown > 0)
                chronoAcceleratorProcCooldown--;

            if (heroConvergenceProcCooldown > 0)
                heroConvergenceProcCooldown--;

            if (omniCoreReactorPulseCooldown > 0)
                omniCoreReactorPulseCooldown--;

            if (!heroConvergenceEmblemEquipped)
                heroConvergenceHitCount = 0;

            if (!omniCoreReactorEquipped) {
                omniCoreReactorCharge = 0f;
                return;
            }

            if (!IsTransformed)
                return;

            if (!Main.dedServ && Main.rand.NextBool(4) &&
                omniCoreReactorCharge >= OmniCoreReactorChargeThreshold * 0.65f) {
                Vector2 orbit = Main.rand.NextVector2Circular(26f, 26f);
                Dust dust = Dust.NewDustPerfect(Player.Center + orbit, DustID.AncientLight,
                    orbit.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * 1.1f, 110,
                    new Color(110, 255, 230), Main.rand.NextFloat(0.85f, 1.2f));
                dust.noGravity = true;
                Lighting.AddLight(Player.Center, new Vector3(0.1f, 0.42f, 0.36f));
            }

            if (Player.whoAmI != Main.myPlayer || omniCoreReactorPulseCooldown > 0 ||
                omniCoreReactorCharge < OmniCoreReactorChargeThreshold)
                return;

            float chargePower = MathHelper.Clamp(1f + (omniCoreReactorCharge - OmniCoreReactorChargeThreshold) / 90f, 1f, 1.45f);
            int pulseDamage = Math.Max(1, (int)Math.Round(Player.GetDamage<HeroDamage>().ApplyTo(54f * chargePower)));
            int projectileIndex = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
                ModContent.ProjectileType<OmniCorePulseProjectile>(), pulseDamage, 2.2f, Player.whoAmI, chargePower);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                Main.projectile[projectileIndex].netUpdate = true;

            omniCoreReactorCharge -= OmniCoreReactorChargeThreshold;
            omniCoreReactorPulseCooldown = OmniCoreReactorPulseCooldownMax;
            SoundEngine.PlaySound(SoundID.Item92 with { Pitch = 0.1f, Volume = 0.7f }, Player.Center);
        }

        private void TryTriggerChronoAccelerator(int energyCost, int sustainEnergyCost) {
            if (!chronoAcceleratorEquipped || !IsTransformed || chronoAcceleratorProcCooldown > 0 ||
                Player.whoAmI != Main.myPlayer)
                return;

            Transformation transformation = CurrentTransformation;
            if (transformation == null)
                return;

            AttackSelection selection = transformation.ResolveAttackSelection(setAttack, this);
            if (selection is not AttackSelection.PrimaryAbility and not AttackSelection.SecondaryAbility and
                not AttackSelection.TertiaryAbility and not AttackSelection.Ultimate)
                return;

            int effectiveEnergy = energyCost + sustainEnergyCost * 2;
            float powerScale = MathHelper.Clamp(1f + effectiveEnergy / 95f, 1f, 1.65f);
            int damage = Math.Max(1, (int)Math.Round(Player.GetDamage<HeroDamage>().ApplyTo(34f + effectiveEnergy * 0.45f)));

            Vector2 origin = Player.MountedCenter;
            Vector2 aimTarget = Main.MouseWorld;
            Vector2 aimDirection = origin.DirectionTo(aimTarget);
            if (aimDirection == Vector2.Zero)
                aimDirection = new Vector2(Player.direction == 0 ? 1f : Player.direction, 0f);

            float aimDistance = Vector2.Distance(origin, aimTarget);
            Vector2 spawnPosition = origin + aimDirection * MathHelper.Clamp(aimDistance, 96f, 360f);
            int projectileIndex = Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPosition, Vector2.Zero,
                ModContent.ProjectileType<ChronoAcceleratorFieldProjectile>(), damage, 0.6f, Player.whoAmI, powerScale);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                Main.projectile[projectileIndex].netUpdate = true;

            chronoAcceleratorProcCooldown = ChronoAcceleratorProcCooldownMax;
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.2f, Volume = 0.55f }, spawnPosition);
        }

        private void AccumulateOmniCoreReactorCharge(int energyCost, int sustainEnergyCost) {
            if (!omniCoreReactorEquipped)
                return;

            int effectiveEnergy = energyCost + sustainEnergyCost * 2;
            if (effectiveEnergy <= 0)
                return;

            omniCoreReactorCharge = Math.Min(OmniCoreReactorChargeThreshold * 2.4f, omniCoreReactorCharge + effectiveEnergy);
        }

        private void TryAccumulateOmniCoreReactorChargeFromHit(int damageDone, Item item = null, Projectile projectile = null) {
            if (!omniCoreReactorEquipped || damageDone <= 0)
                return;

            bool countsAsHeroHit = item != null
                ? !item.IsAir && item.CountsAsClass(ModContent.GetInstance<HeroDamage>())
                : projectile != null && projectile.active && projectile.CountsAsClass(ModContent.GetInstance<HeroDamage>());
            if (!countsAsHeroHit)
                return;

            if (IsAccessoryProcProjectile(projectile))
                return;

            float chargeGain = MathHelper.Clamp(damageDone * OmniCoreReactorHitChargeMultiplier,
                OmniCoreReactorMinHitCharge, OmniCoreReactorMaxHitCharge);
            omniCoreReactorCharge = Math.Min(OmniCoreReactorChargeThreshold * 2.4f, omniCoreReactorCharge + chargeGain);
        }

        private void TryTriggerHeroConvergence(NPC target, int damageDone, bool shouldCountHit) {
            if (!heroConvergenceEmblemEquipped || !shouldCountHit || target == null || !target.active || damageDone <= 0)
                return;

            if (heroConvergenceProcCooldown > 0)
                return;

            heroConvergenceHitCount = Math.Min(heroConvergenceHitCount + 1, HeroConvergenceHitsRequired);
            if (heroConvergenceHitCount < HeroConvergenceHitsRequired)
                return;

            heroConvergenceHitCount = 0;
            heroConvergenceProcCooldown = HeroConvergenceProcCooldownMax;

            if (Player.whoAmI != Main.myPlayer)
                return;

            int boltDamage = Math.Max(1, (int)Math.Round(damageDone * 0.68f));
            for (int i = 0; i < 3; i++) {
                float angle = -MathHelper.PiOver2 + (i - 1) * 0.42f;
                Vector2 spawnOffset = angle.ToRotationVector2() * Main.rand.NextFloat(180f, 230f);
                spawnOffset.Y -= 120f;
                Vector2 spawnPosition = target.Center + spawnOffset;
                Vector2 velocity = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(14.5f, 17.5f);
                int projectileIndex = Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPosition, velocity,
                    ModContent.ProjectileType<HeroConvergenceBoltProjectile>(), boltDamage, 1.4f, Player.whoAmI,
                    target.whoAmI + 1f, i);
                if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                    Main.projectile[projectileIndex].netUpdate = true;
            }

            SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.2f, Volume = 0.65f }, target.Center);
        }

        private static bool IsHeroConvergenceItem(Item item) {
            if (item == null || item.IsAir || !item.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
                return false;

            return item.ModItem is not PlumbersBadge;
        }

        private static bool IsHeroConvergenceProjectile(Projectile projectile) {
            if (projectile == null || !projectile.active || !projectile.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
                return false;

            return projectile.GetGlobalProjectile<OmnitrixProjectile>().itemUsed != ModContent.ItemType<PlumbersBadge>() &&
                   !IsAccessoryProcProjectile(projectile);
        }

        private static bool IsAccessoryProcProjectile(Projectile projectile) {
            if (projectile == null)
                return false;

            return projectile.type == ModContent.ProjectileType<ChronoAcceleratorFieldProjectile>() ||
                   projectile.type == ModContent.ProjectileType<HeroConvergenceBoltProjectile>() ||
                   projectile.type == ModContent.ProjectileType<OmniCorePulseProjectile>() ||
                   projectile.type == ModContent.ProjectileType<ConquestDroneBoltProjectile>();
        }
    }
}
