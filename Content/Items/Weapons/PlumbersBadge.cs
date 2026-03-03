using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ben10Mod.Content.Buffs.Summons;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Enums;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;

namespace Ben10Mod.Content.Items.Weapons {
    public abstract class PlumbersBadge : ModItem {

        // Override these in subclasses for tier-specific values
        public virtual int   BaseDamage                 => 15;
        public virtual float DamageMultiplier           => 1f; // For universal scaling if needed
        public virtual float AttackSpeedMultiplier      => 1f;
        public virtual float AdditionalProjectileChance => 0;

        public virtual string BadgeRankName  => "Helper";
        public virtual int    BadgeRankValue => 0;
        public         int    OmnitrixEnergyUse  = 0;


        private int GetUltimateProjectileType(OmnitrixPlayer omp) {
            // Add more mappings here as you implement other ultimate attacks.
            return omp.currTransformation switch {
                TransformationEnum.EyeGuy => ModContent.ProjectileType<EyeGuyUltimateBeam>(),
                TransformationEnum.GhostFreak => ModContent.ProjectileType<GhostFreakPossesionProjectile>(),
                TransformationEnum.DiamondHead => ModContent.ProjectileType<GiantDiamondProjectile>(),
                TransformationEnum.HeatBlast => ModContent.ProjectileType<HeatBlastUltimateProjectile>(),
                TransformationEnum.BuzzShock => ModContent.ProjectileType<BuzzShockUltimateProjectile>(),
                _ => 0
            };
        }

        private static bool HasActiveOwnedProjectile(Player player, int projType) {
            if (projType <= 0) return false;

            int owner = player.whoAmI;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner && p.type == projType)
                    return true;
            }

            return false;
        }

        private void FinalizeUltimateIfEnded(Player player, OmnitrixPlayer omp) {
            if (!omp.ultimateAttack) return;

            var state = player.GetModPlayer<BadgeUltimateState>();

            // If the ultimate hasn't actually started yet (no ultimate projectile spawned),
            // don't auto-cancel it. This prevents "arming" the ultimate from immediately ending it.
            if (!state.ultimateStarted)
                return;

            // If player is still holding the channel, ultimate is still in progress.
            if (player.channel) return;

            // If the ultimate projectile is still alive, ultimate is still in progress.
            int ultimateProjType = GetUltimateProjectileType(omp);
            if (HasActiveOwnedProjectile(player, ultimateProjType) &&
                omp.currTransformation == TransformationEnum.EyeGuy) return;

            // If we're here, the player has released channel and the ultimate projectile is gone.
            if (!player.HasBuff<UltimateAbility_Cooldown>())
                player.AddBuff(ModContent.BuffType<UltimateAbility_Cooldown>(), 60 * 60);

            omp.ultimateAttack    = false;
            state.ultimateStarted = false;
        }

        public override void SetDefaults() {
            Item.width        = 32;
            Item.height       = 32;
            Item.noUseGraphic = true;
            Item.useTurn      = false;
            Item.autoReuse    = true;
            Item.noMelee      = true;

            Item.shoot = 1;

            Item.DamageType = ModContent.GetInstance<HeroDamage>();
            Item.damage     = BaseDamage;
            Item.knockBack  = 4f;

            Item.useStyle   = ItemUseStyleID.Swing;
            Item.useTime    = Item.useAnimation = 25;
            Item.shootSpeed = 10f;

            Item.UseSound = null;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "badgeHelperLine",
                "Right click while holding to alternate between primary and secondary attacks"));
        }

        public override bool CanUseItem(Player player) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            return player.GetModPlayer<OmnitrixPlayer>().isTransformed &&
                   !(omp.omnitrixEnergy < OmnitrixEnergyUse && omp.ultimateAttack);
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            FinalizeUltimateIfEnded(player, omp);

            // Safety defaults
            Item.useTime          = Item.useAnimation = 25;
            Item.shootSpeed       = 10f;
            Item.useStyle         = ItemUseStyleID.Swing;
            Item.ArmorPenetration = 0;
            Item.UseSound         = null;
            Item.channel          = false;
            OmnitrixEnergyUse     = 0;


            if (!omp.isTransformed)
                return;

            // Fixed per-alien useTime/shootSpeed (balanced to feel good - no alt-dependent for now to avoid timing issues)
            switch (omp.currTransformation) {
                case TransformationEnum.HeatBlast:
                    Item.useTime    = Item.useAnimation = omp.altAttack ? 50 : 6;
                    Item.shootSpeed = omp.ultimateAttack ? 0 : omp.altAttack ? 10f : 3f;
                    Item.useStyle = omp.ultimateAttack ? ItemUseStyleID.HoldUp :
                        omp.altAttack ? ItemUseStyleID.Swing : ItemUseStyleID.Shoot;
                    OmnitrixEnergyUse = omp.ultimateAttack ? 10 : 0;
                    Item.channel      = omp.ultimateAttack;
                    break;
                case TransformationEnum.XLR8:
                    Item.useTime    = Item.useAnimation = 10; // Fast punches
                    Item.shootSpeed = 25f;
                    break;
                case TransformationEnum.FourArms:
                    Item.useTime    = Item.useAnimation = 18; // Fast punches
                    Item.shootSpeed = 25f;
                    break;

                case TransformationEnum.DiamondHead:
                    Item.useStyle         = ItemUseStyleID.Shoot;
                    Item.useTime          = Item.useAnimation = 8;
                    Item.shootSpeed       = 35f;
                    Item.ArmorPenetration = 25;
                    OmnitrixEnergyUse     = omp.ultimateAttack ? 25 : 0;
                    break;

                case TransformationEnum.RipJaws:
                    Item.useTime    = Item.useAnimation = 28;
                    Item.shootSpeed = 6f;
                    break;

                case TransformationEnum.ChromaStone:
                    Item.useTime    = Item.useAnimation = 20;
                    Item.shootSpeed = 25f;
                    break;

                case TransformationEnum.BuzzShock:
                    Item.useTime      = Item.useAnimation = 20;
                    Item.shootSpeed   = 25f;
                    OmnitrixEnergyUse = omp.ultimateAttack ? 25 : 0;
                    break;

                case TransformationEnum.StinkFly:
                    Item.useTime    = Item.useAnimation = 30;
                    Item.shootSpeed = 25f;
                    break;

                case TransformationEnum.GhostFreak:
                    Item.useTime      = Item.useAnimation = 14;
                    Item.shootSpeed   = 12f;
                    OmnitrixEnergyUse = omp.ultimateAttack ? 50 : 0;
                    break;

                case TransformationEnum.WildVine:
                    Item.useTime    = Item.useAnimation = 32;
                    Item.shootSpeed = 10f;
                    break;
                case TransformationEnum.EyeGuy:
                    Item.useStyle     = ItemUseStyleID.Shoot;
                    Item.useTime      = Item.useAnimation = 12;
                    Item.shootSpeed   = omp.ultimateAttack ? 0f : 35f;
                    Item.UseSound     = omp.ultimateAttack ? null : SoundID.Item12;
                    Item.channel      = omp.ultimateAttack;
                    OmnitrixEnergyUse = omp.ultimateAttack ? 10 : 0;
                    break;
                default:
                    Item.useTime    = Item.useAnimation = 25;
                    Item.shootSpeed = 10f;
                    Item.useStyle   = ItemUseStyleID.Swing;
                    break;
            }

            Item.useTime = Item.useAnimation = (int)(Item.useTime / AttackSpeedMultiplier);
        }

        public override bool? UseItem(Player player) {
            if (player.altFunctionUse == 2)
                player.GetModPlayer<OmnitrixPlayer>().altAttack = !player.GetModPlayer<OmnitrixPlayer>().altAttack;
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            if (omp.omnitrixEnergy >= OmnitrixEnergyUse) {
                omp.omnitrixEnergy -= OmnitrixEnergyUse;
            }
            else {
                return false;
            }

            return base.UseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            if (!omp.isTransformed || player.altFunctionUse == 2) return false;

            int activeUltimateType = GetUltimateProjectileType(omp);
            bool ultimateInProgress = player.channel ||
                                      player.GetModPlayer<BadgeUltimateState>().ultimateStarted;

            if (ultimateInProgress && !omp.ultimateAttack)
                return false;

            if (omp.ultimateAttack && player.HasBuff<UltimateAbility_Cooldown>())
                return false;

            int projType    = ProjectileID.ImpFireball;
            int finalDamage = damage;

            switch (omp.currTransformation) {
                case TransformationEnum.HeatBlast:
                    projType = omp.ultimateAttack ? ModContent.ProjectileType<HeatBlastUltimateProjectile>() :
                        omp.altAttack ? ModContent.ProjectileType<HeatBlastBomb>() : ProjectileID.Flames;
                    finalDamage = omp.ultimateAttack ? (int)(damage * 3f) :
                        omp.altAttack ? (int)(damage * 1.5f) : (int)(damage * 0.3f);
                    if (omp.ultimateAttack) {
                        velocity = Vector2.Zero;
                    }

                    break;

                case TransformationEnum.XLR8:
                    projType    = ModContent.ProjectileType<FistProjectile>();
                    finalDamage = (int)(damage * 0.25f);
                    break;

                case TransformationEnum.FourArms:
                    projType = omp.altAttack
                        ? ModContent.ProjectileType<FourArmsClap>()
                        : ModContent.ProjectileType<FistProjectile>();
                    break;

                case TransformationEnum.DiamondHead:
                    projType = omp.ultimateAttack
                        ? ModContent.ProjectileType<GiantDiamondProjectile>()
                        : ModContent.ProjectileType<DiamondHeadProjectile>();
                    finalDamage = omp.ultimateAttack ? damage * 5 : (int)(damage * 0.5f);
                    if (omp.ultimateAttack) {
                        velocity = Vector2.Zero;
                        position = Main.MouseWorld;
                    }

                    break;

                case TransformationEnum.RipJaws:
                    projType = ModContent.ProjectileType<RipJawsProjectile>();
                    break;

                case TransformationEnum.ChromaStone:
                    projType    = ModContent.ProjectileType<ChromaStoneProjectile>();
                    finalDamage += omp.ChromaStoneAbsorbtion;
                    break;

                case TransformationEnum.BuzzShock:
                    
                    if (omp.altAttack) {
                        SoundEngine.PlaySound(SoundID.AbigailSummon, player.position);
                        int buffType   = ModContent.BuffType<BuzzShockMinionBuff>();
                        int minionType = ModContent.ProjectileType<BuzzShockMinionProjectile>();
                        player.AddBuff(buffType, 2);
                        player.SpawnMinionOnCursor(
                            source,
                            player.whoAmI,
                            minionType,
                            (int)(finalDamage * DamageMultiplier),
                            knockback
                        );

                        return false;
                    }
                    SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, player.position);
                    if (omp.ultimateAttack) finalDamage = (int)(2.5f * finalDamage);
                    projType = omp.ultimateAttack ? ModContent.ProjectileType<BuzzShockUltimateProjectile>() : ModContent.ProjectileType<BuzzShockProjectile>();

                    break;

                case TransformationEnum.StinkFly:
                    projType = omp.altAttack
                        ? ModContent.ProjectileType<StinkFlyPoisonProjectile>()
                        : ModContent.ProjectileType<StinkFlySlowProjectile>();
                    break;

                case TransformationEnum.GhostFreak:
                    projType = omp.ultimateAttack
                        ? ModContent.ProjectileType<GhostFreakPossesionProjectile>()
                        : omp.altAttack
                            ? ProjectileID.CursedFlameFriendly
                            : ModContent.ProjectileType<GhostFreakProjectile>();
                    break;

                case TransformationEnum.WildVine:
                    projType = omp.altAttack
                        ? ModContent.ProjectileType<WildVineGrapple>()
                        : ModContent.ProjectileType<WildVineProjectile>();
                    break;

                case TransformationEnum.EyeGuy:
                    projType = omp.ultimateAttack
                        ? ModContent.ProjectileType<EyeGuyUltimateBeam>()
                        : ModContent.ProjectileType<EyeGuyLaserbeam>();
                    finalDamage = omp.ultimateAttack ? (int)(damage * 2f) : damage;
                    break;
            }

            if (projType == 0) return false;

            if (omp.ultimateAttack && HasActiveOwnedProjectile(player, projType))
                return false;

            if (omp.ultimateAttack && projType == activeUltimateType)
                player.GetModPlayer<BadgeUltimateState>().ultimateStarted = true;

            if (omp.currTransformation == TransformationEnum.BuzzShock && omp.ultimateAttack) {
                for (int i = 0; i < 5; i++) {
                    Projectile.NewProjectile(source, position, velocity.RotatedBy(i * 2.5), projType, (int)(finalDamage * DamageMultiplier),
                        knockback, player.whoAmI);
                }
            
                return false;
            }
            
            Projectile.NewProjectile(source, position, velocity, projType, (int)(finalDamage * DamageMultiplier),
                knockback, player.whoAmI);

            if (!omp.ultimateAttack) {
                for (int i = (int)Math.Floor(AdditionalProjectileChance); i >= 1; i--) {
                    Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.25f), projType,
                        (int)(finalDamage * DamageMultiplier),
                        knockback, player.whoAmI);
                }

                if (Main.rand.Next(100) <=
                    100 * (int)(AdditionalProjectileChance - Math.Floor(AdditionalProjectileChance))) {
                    Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.25f), projType,
                        (int)(finalDamage * DamageMultiplier),
                        knockback, player.whoAmI);
                }
            }

            return false;
        }
    }


    public class BadgeUltimateState : ModPlayer {
        public bool ultimateStarted;

        public override void ResetEffects() {
            // no per-tick reset; state persists until ultimate finishes
        }
    }
}