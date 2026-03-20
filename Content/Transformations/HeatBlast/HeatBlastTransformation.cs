using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Content.Transformations;
using Terraria.DataStructures;

namespace Ben10Mod.Content.Transformations.HeatBlast {
    public class HeatBlastTransformation : Transformation {
        private const float AuraRodDamageMultiplier = 0.1f;
        private const int AuraRodEnergyCost = 100;

        public override string FullID             => "Ben10Mod:HeatBlast";
        public override string TransformationName => "Heatblast";

        public override string Description =>
            "A fiery Pyronite from the blazing star Pyros. A living inferno of plasma wrapped in molten rock.";

        public override string IconPath             => "Ben10Mod/Content/Interface/HeatBlastSelect";
        public override int    TransformationBuffId => ModContent.BuffType<HeatBlast_Buff>();
        public override bool HasPrimaryAbility => false;
        public override int PrimaryAbilityCooldown => 0;
        public override int PrimaryAbilityAttack => ModContent.ProjectileType<HeatBlastAuraRodProjectile>();
        public override int PrimaryAbilityAttackSpeed => 24;
        public override int PrimaryAbilityAttackShootSpeed => 0;
        public override int PrimaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
        public override float PrimaryAbilityAttackModifier => AuraRodDamageMultiplier;
        public override int PrimaryAbilityAttackEnergyCost => AuraRodEnergyCost;
        public override bool PrimaryAbilityAttackSingleUse => false;

        public override List<string> Abilities => new List<string> {
            "Flamethrower blast",
            "Flame bombs",
            "Flame-boosted jump",
            "Fire & lava immunity",
            "Flame aura rod sentry",
            "Large fireball attack - ultimate charged attack"
        };
        
        public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
            player.fireWalk   = true;
            player.lavaImmune = true;

            var abilitySlot = ModContent.GetInstance<AbilitySlot>();
            abilitySlot.FunctionalItem = new Item(ModContent.ItemType<HeatBlastExtraJumpAccessory>());

            var rand = Main.rand;
            int dustNum = Dust.NewDust(player.position, player.width, player.height,
                omp.snowflake ? DustID.IceTorch : DustID.Flare,
                0, rand.Next(-1, 2), rand.Next(-1, 2), Color.White, rand.Next(3));
            Main.dust[dustNum].noGravity = true;
        }
        
        public override void OnHitNPC(Player player, OmnitrixPlayer omp, NPC target, NPC.HitInfo hit, int damageDone) {
            if (target.life <= 0) return;

            if (omp.snowflake && !target.HasBuff(BuffID.Frostburn2))
                target.AddBuff(BuffID.Frostburn2, 10 * 60);
            else if (!omp.snowflake && !target.HasBuff(BuffID.OnFire3))
                target.AddBuff(BuffID.OnFire3, 10 * 60);
        }
        
        public override int PrimaryAttack => ProjectileID.Flames;
        public override int PrimaryAttackSpeed => 6;
        public override int PrimaryShootSpeed => 3;
        public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
        public override float PrimaryAttackModifier => 0.3f;

        public override int SecondaryAttack => ModContent.ProjectileType<HeatBlastBomb>();
        public override int SecondaryAttackSpeed => 50;
        public override int SecondaryShootSpeed => 10;
        public override float SecondaryAttackModifier => 1.5f;
        public override int SecondaryUseStyle => ItemUseStyleID.Swing;

        public override int UltimateAttack =>
            ModContent.ProjectileType<HeatBlastUltimateProjectile>();
        public override int UltimateAttackSpeed => 6;
        public override int UltimateShootSpeed => 0;
        public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
        public override bool UltimateChannel => true;
        public override int UltimateEnergyCost => 10;

        public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int damage, float knockback) {
            if (!omp.IsPrimaryAbilityAttackLoaded)
                return base.Shoot(player, omp, source, position, velocity, damage, knockback);

            int rodType = ModContent.ProjectileType<HeatBlastAuraRodProjectile>();
            int maxRods = Math.Max(1, player.maxTurrets);
            int activeRodCount = 0;
            int oldestRodIndex = -1;
            float oldestSpawnOrder = float.MaxValue;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || projectile.owner != player.whoAmI || projectile.type != rodType)
                    continue;

                activeRodCount++;
                float spawnOrder = projectile.localAI[1] <= 0f ? projectile.identity : projectile.localAI[1];
                if (spawnOrder < oldestSpawnOrder) {
                    oldestSpawnOrder = spawnOrder;
                    oldestRodIndex = i;
                }
            }

            if (activeRodCount >= maxRods && oldestRodIndex != -1)
                Main.projectile[oldestRodIndex].Kill();

            Vector2 spawnPosition = Main.MouseWorld;
            int projectileIndex = Projectile.NewProjectile(source, spawnPosition, Vector2.Zero, rodType,
                Math.Max(1, (int)Math.Round(damage * AuraRodDamageMultiplier)), 0f, player.whoAmI);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                omp.transformationAttackSerial++;
                Main.projectile[projectileIndex].originalDamage = Math.Max(1, (int)Math.Round(damage * AuraRodDamageMultiplier));
                Main.projectile[projectileIndex].localAI[1] = omp.transformationAttackSerial;
                Main.projectile[projectileIndex].netUpdate = true;
            }

            for (int i = 0; i < 18; i++) {
                Dust dust = Dust.NewDustPerfect(spawnPosition + Main.rand.NextVector2Circular(18f, 18f), DustID.Flare,
                    Main.rand.NextVector2Circular(2f, 2f), 100, new Color(255, 145, 50), Main.rand.NextFloat(1.2f, 1.7f));
                dust.noGravity = true;
            }

            return false;
        }

        public override void FrameEffects(Player player, OmnitrixPlayer omp) {
            var costume = ModContent.GetInstance<HeatBlast>();
            player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
            player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
            player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        }
    }
}
