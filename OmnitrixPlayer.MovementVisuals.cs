using Ben10Mod.Common.Absorption;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Ben10Mod.Content;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Interface;
using Terraria.Audio;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Content.Transformations.XLR8;

namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            var trans = CurrentTransformation;
            if (trans == null) return;

            trans.ModifyDrawInfo(Player, this, ref drawInfo);
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a,
            ref bool fullBright) {
            var customSlot = ModContent.GetInstance<OmnitrixSlot>();
            GetActiveOmnitrix()?.ApplyHandVisuals(Player, this, customSlot.HideVisuals);

            if (TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile absorptionProfile)) {
                Color tint = absorptionProfile.TintColor;
                Vector3 vividTint = Color.Lerp(tint, Color.White, 0.18f).ToVector3();
                r = MathHelper.Lerp(r, MathHelper.Clamp(vividTint.X * 1.8f, 0f, 1f), 0.97f);
                g = MathHelper.Lerp(g, MathHelper.Clamp(vividTint.Y * 1.8f, 0f, 1f), 0.97f);
                b = MathHelper.Lerp(b, MathHelper.Clamp(vividTint.Z * 1.8f, 0f, 1f), 0.97f);
                a = 1f;
                fullBright = true;

                if (Main.rand.NextBool(3)) {
                    Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(18f, 26f), DustID.GemDiamond,
                        Main.rand.NextVector2Circular(1.5f, 1.5f), 70, Color.Lerp(tint, Color.White, 0.22f), 1.2f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.1f;
                }
            }

            var trans = CurrentTransformation;
            if (trans != null && trans.TryGetTransformationTint(Player, this, out Color transformationTint,
                    out float tintBlendStrength, out bool forceTransformationFullBright)) {
                Vector3 vividTint = Color.Lerp(transformationTint, Color.White, 0.16f).ToVector3();
                float safeBlendStrength = MathHelper.Clamp(tintBlendStrength, 0f, 1f);
                r = MathHelper.Lerp(r, MathHelper.Clamp(vividTint.X * 1.65f, 0f, 1f), safeBlendStrength);
                g = MathHelper.Lerp(g, MathHelper.Clamp(vividTint.Y * 1.65f, 0f, 1f), safeBlendStrength);
                b = MathHelper.Lerp(b, MathHelper.Clamp(vividTint.Z * 1.65f, 0f, 1f), safeBlendStrength);
                a = 1f;
                if (forceTransformationFullBright)
                    fullBright = true;
            }

            if (trans != null)
                trans.DrawEffects(ref drawInfo);
        }

        public override void FrameEffects() {
            var customSlot = ModContent.GetInstance<OmnitrixSlot>();
            GetActiveOmnitrix()?.ApplyHandVisuals(Player, this, customSlot.HideVisuals);

            if (!customSlot.HideVisuals && isTransformed) {
                Player.wings = -1;
                Player.shoe = -1;
                Player.handoff = -1;
                Player.handon = -1;
                Player.back = -1;
                Player.waist = -1;
                Player.shield = -1;
            }

            CurrentTransformation?.FrameEffects(Player, this);
            ApplySelectedTransformationCostumeVisuals(Player, CurrentTransformation);

            if (ShouldShowXlr8DashAccessoryVisuals())
                ApplyXlr8DashAccessoryVisuals(customSlot.HideVisuals);
        }

        public override void PreUpdateMovement() {
            DashMovement();

            var trans = CurrentTransformation;
            if (trans != null) {
                float baseMoveSpeed = Player.moveSpeed;
                float baseMaxRunSpeed = Player.maxRunSpeed;
                float baseAccRunSpeed = Player.accRunSpeed;
                float baseRunAcceleration = Player.runAcceleration;
                trans.PreUpdateMovement(Player, this);
                ApplyTransformationMovementBoostScale(baseMoveSpeed, baseMaxRunSpeed, baseAccRunSpeed, baseRunAcceleration);
            }
        }

        private void DashMovement() {
            if (CanUseDash() && DashDir != -1 && DashDelay == 0) {
                var trans = CurrentTransformation;
                bool usingTransformationDash = trans?.FullID == "Ben10Mod:XLR8";
                bool usingAccessoryDash = !usingTransformationDash && xlr8DashAccessoryEquipped;
                if (usingTransformationDash || xlr8DashAccessoryEquipped) {
                    Vector2 newVelocity = Player.velocity;
                    float dashVelocity = usingAccessoryDash ? Xlr8DashAccessoryVelocity : DashVelocity;
                    int dashDuration = usingAccessoryDash ? Xlr8DashAccessoryDuration : DashDuration;

                    switch (DashDir) {
                        case DashLeft when Player.velocity.X > -dashVelocity:
                        case DashRight when Player.velocity.X < dashVelocity:
                            float dashDirection = DashDir == DashRight ? 1 : -1;
                            newVelocity.X = dashDirection * dashVelocity;
                            break;
                        default:
                            return;
                    }

                    DashDelay = usingAccessoryDash ? Math.Max(DashCooldown, dashDuration) : DashCooldown;
                    DashTimer = dashDuration;
                    Player.velocity = newVelocity;

                    if (usingAccessoryDash) {
                        xlr8DashAccessoryVisualTime = Math.Max(xlr8DashAccessoryVisualTime, Xlr8DashAccessoryVisualDuration);
                        PlayXlr8DashAccessoryStartEffects();
                    }
                }
            }

            if (DashDelay > 0) DashDelay--;
            if (DashTimer > 0) {
                Player.eocDash = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;
                Player.GiveImmuneTimeForCollisionAttack(40);
                if (xlr8DashAccessoryEquipped && CurrentTransformation?.FullID != "Ben10Mod:XLR8")
                    xlr8DashAccessoryVisualTime = Math.Max(xlr8DashAccessoryVisualTime, DashTimer);
                DashTimer--;
            }
        }

        private bool CanUseDash() {
            return Player.dashType == 0 && !Player.setSolar && !Player.mount.Active;
        }

        private bool ShouldShowXlr8DashAccessoryVisuals() {
            return xlr8DashAccessoryVisualTime > 0 && CurrentTransformation?.FullID != "Ben10Mod:XLR8";
        }

        private void PlayXlr8DashAccessoryStartEffects() {
            Transformation xlr8Transformation = TransformationLoader.Get("Ben10Mod:XLR8");
            if (xlr8Transformation == null)
                return;

            xlr8Transformation.SpawnTransformParticles(Player, this);
            SoundEngine.PlaySound(new SoundStyle("Ben10Mod/Content/Sounds/OmnitrixTransformation"), Player.position);
        }

        private void PlayXlr8DashAccessoryEndEffects() {
            Transformation currentTransformation = CurrentTransformation;
            if (currentTransformation != null) {
                currentTransformation.SpawnTransformParticles(Player, this);
                SoundEngine.PlaySound(new SoundStyle("Ben10Mod/Content/Sounds/OmnitrixTransformation"), Player.position);
                return;
            }

            TransformationHandler.PlayDetransformEffects(Player, showParticles: true, playSound: true);
        }

        private void ApplyXlr8DashAccessoryVisuals(bool hideVisuals) {
            if (!hideVisuals) {
                Player.wings = -1;
                Player.shoe = -1;
                Player.handoff = -1;
                Player.handon = -1;
                Player.back = -1;
                Player.waist = -1;
                Player.shield = -1;
            }

            var costume = ModContent.GetInstance<XLR8>();
            Player.armorEffectDrawShadow = true;
            Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
            Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
            Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);

            Transformation xlr8Transformation = TransformationLoader.Get("Ben10Mod:XLR8");
            ApplySelectedTransformationCostumeVisuals(Player, xlr8Transformation);
        }

        private bool IsTouchingLungeResetSurface() {
            return Player.velocity.Y >= 0f &&
                   Collision.SolidCollision(Player.position + new Vector2(0f, Player.height - 2f), Player.width, 8);
        }

        private bool IsRestrictedLungeProjectile(int projectileType) {
            return projectileType == ModContent.ProjectileType<RathPounceProjectile>() ||
                   projectileType == ModContent.ProjectileType<XLR8DashProjectile>() ||
                   projectileType == ModContent.ProjectileType<XLR8VectorDashProjectile>() ||
                   projectileType == ModContent.ProjectileType<RipJawsBiteProjectile>() ||
                   projectileType == ModContent.ProjectileType<JetrayDiveProjectile>() ||
                   projectileType == ModContent.ProjectileType<BigChillPhaseStrikeProjectile>() ||
                   projectileType == ModContent.ProjectileType<FourArmsRushProjectile>() ||
                   projectileType == ModContent.ProjectileType<GoopDelugeProjectile>();
        }

        private bool CanIgnoreAirborneLungeLimit(int projectileType) {
            return projectileType == ModContent.ProjectileType<RipJawsBiteProjectile>() && Player.wet;
        }

        private void ResetAirborneLungeState() {
            airborneLungeConsumed = false;
            activeLungeTime = 0;
        }

        public bool CanUseLungeAttack(int projectileType) {
            if (!IsRestrictedLungeProjectile(projectileType) || CanIgnoreAirborneLungeLimit(projectileType))
                return true;

            return !airborneLungeConsumed || (activeLungeTime <= 0 && IsTouchingLungeResetSurface());
        }

        public bool TryConsumeLungeAttack(int projectileType) {
            if (!IsRestrictedLungeProjectile(projectileType) || CanIgnoreAirborneLungeLimit(projectileType))
                return true;

            if (!CanUseLungeAttack(projectileType))
                return false;

            airborneLungeConsumed = true;
            activeLungeTime = Math.Max(activeLungeTime, 2);
            return true;
        }

        public void RegisterActiveLunge() {
            activeLungeTime = Math.Max(activeLungeTime, 2);
            Player.armorEffectDrawShadow = true;
        }
    }
}
