using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Enums;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace Ben10Mod.Content.Items.Weapons
{
    public abstract class PlumbersBadge : ModItem
    {
        
        // Override these in subclasses for tier-specific values
        public virtual int    BaseDamage               => 15;
        public virtual float  DamageMultiplier         => 1f; // For universal scaling if needed
        public virtual string BadgeRankName            => "Helper";
        
        public override void SetDefaults()
        {
            Item.width        = 32;
            Item.height       = 32;
            Item.useStyle     = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;
            Item.useTurn      = false;
            Item.autoReuse    = true;
            Item.noMelee      = true;
            
            Item.shoot = 1;

            Item.DamageType = ModContent.GetInstance<HeroDamage>();
            Item.damage = BaseDamage;
            Item.knockBack = 4f;

            // Base defaults - overridden per alien in HoldItem
            Item.useTime = Item.useAnimation = 25;
            Item.shootSpeed = 10f;
        }

        public override bool CanUseItem(Player player)
        {
            // Only usable when transformed - prevents any use/animation when human
            return player.GetModPlayer<OmnitrixPlayer>().isTransformed;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            // Safety defaults
            Item.useTime = Item.useAnimation = 25;
            Item.shootSpeed = 10f;

            if (!omp.isTransformed)
                return;

            // Fixed per-alien useTime/shootSpeed (balanced to feel good - no alt-dependent for now to avoid timing issues)
            switch (omp.currTransformation)
            {
                case TransformationEnum.HeatBlast:

                    if (player.altFunctionUse == 2) {
                        Item.useStyle   = ItemUseStyleID.Swing;
                        Item.useTime    = Item.useAnimation = 50;
                        Item.shootSpeed = 10f;
                    }
                    else {
                        Item.useStyle = ItemUseStyleID.Shoot;
                        Item.useTime = Item.useAnimation = 6; // Fast enough for fireballs, bombs will feel strong but same rate
                        Item.shootSpeed = 3f;
                    }
                    break;
                case TransformationEnum.XLR8:
                    Item.useTime    = Item.useAnimation = 10; // Fast punches
                    Item.shootSpeed = 25f;
                    break;
                case TransformationEnum.FourArms:
                    Item.useTime = Item.useAnimation = 18; // Fast punches
                    Item.shootSpeed = 25f;
                    break;

                case TransformationEnum.DiamondHead:
                    Item.useTime = Item.useAnimation = 30;
                    Item.shootSpeed = 35f;
                    break;

                case TransformationEnum.RipJaws:
                    Item.useTime = Item.useAnimation = 28;
                    Item.shootSpeed = 6f;
                    break;

                case TransformationEnum.ChromaStone:
                    Item.useTime = Item.useAnimation = 20;
                    Item.shootSpeed = 25f;
                    break;

                case TransformationEnum.BuzzShock:
                    Item.useTime = Item.useAnimation = 20;
                    Item.shootSpeed = 25f;
                    break;

                case TransformationEnum.StinkFly:
                    Item.useTime = Item.useAnimation = 30;
                    Item.shootSpeed = 25f;
                    break;

                case TransformationEnum.GhostFreak:
                    Item.useTime = Item.useAnimation = 14;
                    Item.shootSpeed = 12f;
                    break;

                case TransformationEnum.WildVine:
                    Item.useTime = Item.useAnimation = 32;
                    Item.shootSpeed = 10f;
                    break;
                default:
                    Item.useTime    = Item.useAnimation = 25;
                    Item.shootSpeed = 10f;
                    Item.useStyle   = ItemUseStyleID.Swing;
                    break;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var omp = player.GetModPlayer<OmnitrixPlayer>();

            if (!omp.isTransformed)
                return false;

            int projType = ProjectileID.ImpFireball; // Bright vanilla fallback - you SHOULD see this if alien not matched
            int finalDamage = damage;

            switch (omp.currTransformation)
            {
                case TransformationEnum.HeatBlast:
                    if (player.altFunctionUse == 2) {                        
                        projType = ModContent.ProjectileType<HeatBlastBomb>();
                        // projType    = ProjectileID.Flamelash;
                        finalDamage = (int)(damage * 2.5f);
                    }
                    else {
                        projType = ProjectileID.Flames;
                        finalDamage = (int)(damage * 0.2f);
                    }
                    break;

                case TransformationEnum.XLR8:
                    projType = player.altFunctionUse == 2
                        ? 0
                        : ModContent.ProjectileType<FistProjectile>();
                    break;
                case TransformationEnum.FourArms:
                    projType = player.altFunctionUse == 2
                        ? ModContent.ProjectileType<FourArmsClap>()
                        : ModContent.ProjectileType<FistProjectile>();
                    break;

                case TransformationEnum.DiamondHead:
                    projType = player.altFunctionUse == 2
                        ? 0
                        : ModContent.ProjectileType<DiamondHeadProjectile>();
                    break;

                case TransformationEnum.RipJaws:
                    projType = player.altFunctionUse == 2
                        ? 0
                        : ModContent.ProjectileType<RipJawsProjectile>();
                    break;

                case TransformationEnum.ChromaStone:
                    projType = player.altFunctionUse == 2
                        ? 0
                        : ModContent.ProjectileType<ChromaStoneProjectile>();
                    finalDamage += omp.ChromaStoneAbsorbtion;
                    break;

                case TransformationEnum.BuzzShock:
                    projType = player.altFunctionUse == 2
                        ? ModContent.ProjectileType<BuzzShockMinionProjectile>()
                        : ModContent.ProjectileType<BuzzShockProjectile>();
                    SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, player.position);
                    break;

                case TransformationEnum.StinkFly:
                    projType = player.altFunctionUse == 2
                        ? ModContent.ProjectileType<StinkFlyPoisonProjectile>()
                        : ModContent.ProjectileType<StinkFlySlowProjectile>();
                    break;

                case TransformationEnum.GhostFreak:
                    projType = player.altFunctionUse == 2
                        ? ModContent.ProjectileType<GhostFreakPossesionProjectile>()
                        : ModContent.ProjectileType<GhostFreakProjectile>();
                    break;

                case TransformationEnum.WildVine:
                    projType = player.altFunctionUse == 2
                        ? ModContent.ProjectileType<WildVineGrapple>()
                        : ModContent.ProjectileType<WildVineProjectile>();
                    break;
            }

            if (projType == 0) return false;
            
            Projectile.NewProjectile(source, position, velocity, projType, finalDamage, knockback, player.whoAmI);

            return false;
        }
    }
}