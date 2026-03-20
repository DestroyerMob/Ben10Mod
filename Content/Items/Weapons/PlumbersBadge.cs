using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace Ben10Mod.Content.Items.Weapons {
    public abstract class PlumbersBadge : ModItem {
        public virtual int   BaseDamage                 => 15;
        public virtual float DamageMultiplier           => 1f;
        public virtual float AttackSpeedMultiplier      => 1f;
        public virtual float AdditionalProjectileChance => 0;

        public virtual string BadgeRankName  => "Helper";
        public virtual int    BadgeRankValue => 0;

        private int OmnitrixEnergyUse = 0;

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
                omp.ResetAttackToBaseSelection();

                if (!player.HasBuff<UltimateAbilityCooldown>())
                    player.AddBuff(ModContent.BuffType<UltimateAbilityCooldown>(), 60 * 60);
            }

            int ultimateType = omp.CurrentTransformation?.GetUltimateAttackProjectileType(omp) ?? 0;
            if (HasActiveOwnedProjectile(player, ultimateType))
                return;

            state.ultimateStarted = false;
        }

        public override void SetDefaults() {
            Item.width        = Item.height = 32;
            Item.noUseGraphic = true;
            Item.useTurn      = false;
            Item.autoReuse    = true;
            Item.noMelee      = true;
            Item.shoot        = 1;

            Item.DamageType = ModContent.GetInstance<HeroDamage>();
            Item.damage     = BaseDamage;
            Item.knockBack  = 4f;

            Item.useStyle   = ItemUseStyleID.Swing;
            Item.useTime    = Item.useAnimation = 25;
            Item.shootSpeed = 10f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "badgeHelperLine",
                "Right click while holding to alternate between primary and secondary attacks"));
        }

        public override bool CanUseItem(Player player) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            return omp.IsTransformed &&
                   !(omp.omnitrixEnergy < OmnitrixEnergyUse && omp.HasLoadedBadgeAttack);
        }

        public override void HoldItem(Player player) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            var state = player.GetModPlayer<BadgeUltimateState>();
            FinalizeUltimateIfEnded(player, omp);

            // Start from a neutral badge state before the active transformation applies its attack profile.
            Item.useTime          = Item.useAnimation = 25;
            Item.shootSpeed       = 10f;
            Item.useStyle         = ItemUseStyleID.Swing;
            Item.channel          = false;
            Item.noMelee          = true;
            Item.ArmorPenetration = 0;
            Item.UseSound         = null;
            OmnitrixEnergyUse     = 0;

            if (!omp.IsTransformed) {
                state.ultimateStarted = false;
                return;
            }

            var trans = omp.CurrentTransformation;
            if (trans != null) {
                trans.ModifyPlumbersBadgeStats(Item, omp);
                OmnitrixEnergyUse = trans.GetEnergyCost(omp);
            }

            Item.useTime = Item.useAnimation = (int)(Item.useTime / AttackSpeedMultiplier);
        }

        public override bool? UseItem(Player player) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            if (omp.omnitrixEnergy >= OmnitrixEnergyUse) {
                omp.omnitrixEnergy -= OmnitrixEnergyUse;
                return base.UseItem(player);
            }

            return false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            var state = player.GetModPlayer<BadgeUltimateState>();
            if (!omp.IsTransformed || player.altFunctionUse == 2) return false;

            var trans = omp.CurrentTransformation;
            if (trans == null) return false;

            bool firingUltimate = omp.ultimateAttack;
            bool firingLoadedAbilityAttack = omp.HasLoadedAbilityAttack;

            if (firingUltimate && player.HasBuff<UltimateAbilityCooldown>()) return false;
            if (firingUltimate && state.ultimateStarted) return false;

            if (firingUltimate)
                state.ultimateStarted = true;

            trans.Shoot(player, omp, source, position, velocity, damage, knockback);

            if (firingLoadedAbilityAttack)
                omp.NotifyLoadedAbilityAttackFired();

            return false;
        }
    }

    public class BadgeUltimateState : ModPlayer {
        public bool ultimateStarted;

        public override void Initialize() {
            ultimateStarted = false;
        }
    }
}
