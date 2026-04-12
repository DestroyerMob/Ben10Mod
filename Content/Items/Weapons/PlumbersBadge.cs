using System;
using System.Collections.Generic;
using Ben10Mod.Common.Systems;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Prefixes;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Utilities;

namespace Ben10Mod.Content.Items.Weapons {
    public abstract class PlumbersBadge : ModItem {
        public virtual int   BaseDamage                 => 15;
        public virtual float DamageMultiplier           => 1f;
        public virtual float AttackSpeedMultiplier      => 1f;
        public virtual float AdditionalProjectileChance => 0;
        public virtual int   UntransformedBoltDamage    => 6 + BadgeRankValue * 2;
        public virtual int   UntransformedUseTime       => 30;
        public virtual int   BadgeItemValue => BadgeRankValue switch {
            <= 0 => Item.buyPrice(silver: 20),
            1 => Item.buyPrice(silver: 35),
            2 => Item.buyPrice(silver: 75),
            3 => Item.buyPrice(gold: 1, silver: 25),
            4 => Item.buyPrice(gold: 2),
            5 => Item.buyPrice(gold: 3, silver: 50),
            6 => Item.buyPrice(gold: 5),
            7 => Item.buyPrice(gold: 7, silver: 50),
            8 => Item.buyPrice(gold: 10),
            _ => Item.buyPrice(gold: 15)
        };

        public virtual string BadgeRankName  => "Helper";
        public virtual int    BadgeRankValue => 0;

        private int  lastNormalizedPrefix = int.MinValue;
        private bool wasHeldLastFrame;

        private static bool HasActiveOwnedProjectile(Player player, int projType) {
            if (projType <= 0) return false;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == projType)
                    return true;
            }

            return false;
        }

        private void FinalizeUltimateIfEnded(Player player, OmnitrixPlayer omp) {
            var state = player.GetModPlayer<BadgeUltimateState>();
            if (!state.ultimateStarted) return;
            if (player.channel) return;

            if (omp.ultimateAttack) {
                int ultimateCooldown = omp.CurrentTransformation?.GetUltimateAbilityCooldown(omp) ?? 0;
                omp.ResetAttackToBaseSelection();

                if (ultimateCooldown > 0 && !player.HasBuff<UltimateAbilityCooldown>())
                    player.AddBuff(ModContent.BuffType<UltimateAbilityCooldown>(), ultimateCooldown);
            }

            int ultimateType = omp.CurrentTransformation?.GetUltimateAttackProjectileType(omp) ?? 0;
            if (HasActiveOwnedProjectile(player, ultimateType))
                return;

            state.ultimateStarted = false;
        }

        protected virtual void ConfigureUntransformedBadgeStats(Player player, OmnitrixPlayer omp) {
            Item.noUseGraphic = false;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = Item.useAnimation = UntransformedUseTime;
            Item.shoot = ModContent.ProjectileType<PlumberBlasterBoltProjectile>();
            Item.shootSpeed = 11.5f;
            Item.knockBack = 1.75f;
            Item.UseSound = SoundID.Item91 with { Pitch = -0.14f, Volume = 0.58f };
        }

        protected virtual bool ShootUntransformedBadge(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int damage, float knockback) {
            Vector2 shotVelocity = velocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f)) *
                Item.shootSpeed;
            int projectileIndex = Projectile.NewProjectile(source, position, shotVelocity,
                ModContent.ProjectileType<PlumberBlasterBoltProjectile>(), damage, knockback, player.whoAmI, 0f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                Main.projectile[projectileIndex].netUpdate = true;
            return false;
        }

        protected virtual void OnTransformationAttackFired(Player player, OmnitrixPlayer omp,
            EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int damage, float knockback,
            bool firingUltimate, bool firingLoadedAbilityAttack) {
        }

        public override void SetDefaults() {
            Item.width        = Item.height = 32;
            Item.noUseGraphic = true;
            Item.useTurn      = false;
            Item.autoReuse    = true;
            Item.noMelee      = true;
            Item.shoot        = ProjectileID.WoodenArrowFriendly;
            Item.value        = BadgeItemValue;

            Item.DamageType = ModContent.GetInstance<HeroDamage>();
            Item.damage     = BaseDamage;
            Item.knockBack  = 4f;

            Item.useStyle   = ItemUseStyleID.Swing;
            Item.useTime    = Item.useAnimation = 25;
            Item.shootSpeed = 10f;
        }

        public override bool WeaponPrefix() {
            return false;
        }

        public override bool RangedPrefix() {
            return true;
        }

        public override bool AllowPrefix(int pre) {
            return pre <= 0 || BadgePrefix.IsBadgePrefixType(pre);
        }

        public override bool? PrefixChance(int pre, UnifiedRandom rand) {
            if (pre > 0 && !AllowPrefix(pre))
                return false;

            return base.PrefixChance(pre, rand);
        }

        public override void ApplyPrefix(int pre) {
            RefreshStoredPrefixStats();
        }

        public override void PreReforge() {
            RefreshStoredPrefixStats();
        }

        public override void PostReforge() {
            RefreshStoredPrefixStats();
        }

        public override bool CanReforge() {
            return !IsBlacklisted();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            if (IsBlacklisted()) {
                tooltips.Add(new TooltipLine(Mod, "Disabled",
                    "Disabled by the Ben10Mod feature blacklist."));
            }

            tooltips.Add(new TooltipLine(Mod, "badgeHelperLine",
                "Right click while holding to alternate between primary and secondary attacks"));
        }

        public override bool CanUseItem(Player player) {
            if (IsBlacklisted())
                return false;

            var omp = player.GetModPlayer<OmnitrixPlayer>();
            if (!omp.IsTransformed)
                return player.altFunctionUse != 2;

            var trans = omp.CurrentTransformation;
            return (trans?.CanAffordCurrentAttack(omp) ?? false) &&
                   (trans?.CanStartCurrentAttack(player, omp) ?? false);
        }

        public override void HoldItem(Player player) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            var state = player.GetModPlayer<BadgeUltimateState>();
            FinalizeUltimateIfEnded(player, omp);
            wasHeldLastFrame = true;

            // Start from a neutral badge state before the active transformation applies its attack profile.
            Item.useTime          = Item.useAnimation = 25;
            Item.shootSpeed       = 10f;
            Item.useStyle         = ItemUseStyleID.Swing;
            Item.channel          = false;
            Item.noMelee          = true;
            Item.noUseGraphic     = true;
            ResetPrefixSensitiveStats();
            Item.shoot            = ProjectileID.WoodenArrowFriendly;
            Item.UseSound         = null;

            if (IsBlacklisted()) {
                state.ultimateStarted = false;
                return;
            }

            if (!omp.IsTransformed) {
                state.ultimateStarted = false;
                ConfigureUntransformedBadgeStats(player, omp);
                ApplyBadgePrefixStats();
                return;
            }

            var trans = omp.CurrentTransformation;
            if (trans != null)
                trans.ModifyPlumbersBadgeStats(Item, omp);

            ApplyBadgePrefixStats();
            Item.useTime = Item.useAnimation = (int)(Item.useTime / AttackSpeedMultiplier);
        }

        public override void UpdateInventory(Player player) {
            base.UpdateInventory(player);

            if (ReferenceEquals(player.HeldItem, Item))
                return;

            if (!wasHeldLastFrame && Item.prefix == lastNormalizedPrefix)
                return;

            wasHeldLastFrame = false;
            RefreshStoredPrefixStats();
        }

        public override bool? UseItem(Player player) {
            return base.UseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            if (IsBlacklisted())
                return false;

            var omp = player.GetModPlayer<OmnitrixPlayer>();
            var state = player.GetModPlayer<BadgeUltimateState>();
            if (!omp.IsTransformed) {
                if (player.altFunctionUse == 2)
                    return false;

                return ShootUntransformedBadge(player, source, position, velocity, damage, knockback);
            }

            if (player.altFunctionUse == 2) return false;

            var trans = omp.CurrentTransformation;
            if (trans == null) return false;

            bool firingUltimate = omp.ultimateAttack;
            bool firingLoadedAbilityAttack = omp.HasLoadedAbilityAttack;

            if (firingUltimate && player.HasBuff<UltimateAbilityCooldown>()) return false;
            if (firingUltimate && state.ultimateStarted) return false;

            if (!trans.CanStartCurrentAttack(player, omp))
                return false;

            int energyCost = trans.GetEnergyCost(omp);
            int sustainEnergyCost = trans.GetAttackSustainEnergyCost(omp.setAttack, omp);
            if (!trans.TryConsumeCurrentAttackCost(omp))
                return false;

            if (!trans.BeginCurrentAttack(player, omp)) {
                if (energyCost > 0)
                    omp.RestoreOmnitrixEnergy(energyCost);
                return false;
            }

            omp.NotifyCurrentAttackSpentEnergy(energyCost, sustainEnergyCost,
                Math.Max(player.itemAnimationMax, player.itemTimeMax));

            if (firingUltimate)
                state.ultimateStarted = true;

            trans.Shoot(player, omp, source, position, velocity, damage, knockback);
            OnTransformationAttackFired(player, omp, source, position, velocity, damage, knockback, firingUltimate,
                firingLoadedAbilityAttack);

            if (firingLoadedAbilityAttack)
                omp.NotifyLoadedAbilityAttackFired();

            return false;
        }

        private bool IsBlacklisted() {
            return Ben10FeatureBlacklistRegistry.IsFeatureBlacklisted(Ben10FeatureType.PlumbersBadge, Mod);
        }

        private void RefreshStoredPrefixStats() {
            if (Item.prefix > 0 && !AllowPrefix(Item.prefix))
                Item.prefix = 0;

            Item.Refresh(false);
            ResetPrefixSensitiveStats();
            ApplyBadgePrefixStats();
            lastNormalizedPrefix = Item.prefix;
        }

        private void ResetPrefixSensitiveStats() {
            Item.damage = BaseDamage;
            Item.knockBack = 4f;
            Item.ArmorPenetration = 0;
            Item.crit = 0;
        }

        private void ApplyBadgePrefixStats() {
            if (PrefixLoader.GetPrefix(Item.prefix) is not BadgePrefix prefix)
                return;

            Item.damage = Math.Max(1, (int)Math.Round(Item.damage * prefix.BadgeDamageMultiplier));
            Item.crit += prefix.BadgeCritBonus;
            Item.ArmorPenetration = Math.Max(0, Item.ArmorPenetration + prefix.BadgeArmorPenetrationBonus);
            Item.knockBack = Math.Max(0f, Item.knockBack * prefix.BadgeKnockbackMultiplier);
        }
    }

    public class BadgeUltimateState : ModPlayer {
        public bool ultimateStarted;

        public override void Initialize() {
            ultimateStarted = false;
        }
    }
}
