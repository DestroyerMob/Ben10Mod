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
        public override string FullID             => "Ben10Mod:HeatBlast";
        public override string TransformationName => "Heatblast";

        public override string Description =>
            "A fiery Pyronite from the blazing star Pyros. A living inferno of plasma wrapped in molten rock.";

        public override string IconPath             => "Ben10Mod/Content/Interface/HeatBlastSelect";
        public override int    TransformationBuffId => ModContent.BuffType<HeatBlast_Buff>();
        public override int PrimaryAbilityDuration => 60 * 120;
        public override int PrimaryAbilityCooldown => 60 * 60;

        public override List<string> Abilities => new List<string> {
            "Flamethrower blast",
            "Flame bombs",
            "Flame-boosted jump",
            "Fire & lava immunity",
            "Flame aura (inflicts hellflame debuff on nearby enemies)",
            "Large fireball attack - ultimate charged attack"
        };
        
        public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
            player.fireWalk   = true;
            player.lavaImmune = true;

            // Ability slot extra jump
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();
            abilitySlot.FunctionalItem = new Item(ModContent.ItemType<HeatBlastExtraJumpAccessory>());

            // Passive dust aura (normal form)
            var rand = Main.rand; // better than new Random()
            int dustNum = Dust.NewDust(player.position, player.width, player.height,
                omp.snowflake ? DustID.IceTorch : DustID.Flare,
                0, rand.Next(-1, 2), rand.Next(-1, 2), Color.White, rand.Next(3));
            Main.dust[dustNum].noGravity = true;

            // Primary Ability Aura (circle of fire/ice + DoT on nearby enemies)
            if (omp.PrimaryAbilityEnabled) {
                Vector2[] points = GenerateCirclePoints(250, 7 * 16);
                for (int i = 0; i < points.Length; i++) {
                    dustNum = Dust.NewDust(points[i] + player.Center, 1, 1,
                        omp.snowflake ? DustID.IceTorch : DustID.Torch,
                        rand.Next(-1, 2), rand.Next(-1, 2));
                    Main.dust[dustNum].noGravity = true;
                }

                foreach (NPC npc in Main.npc) {
                    if (npc.active && !npc.friendly && player.Distance(npc.Center) <= 10 * 16) {
                        if (omp.snowflake && !npc.HasBuff(BuffID.Frostburn2))
                            npc.AddBuff(BuffID.Frostburn2, 10 * 60);
                        else if (!omp.snowflake && !npc.HasBuff(BuffID.OnFire3))
                            npc.AddBuff(BuffID.OnFire3, 10 * 60);
                    }
                }
            }
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

        private static Vector2[] GenerateCirclePoints(int numberOfPoints, float radius) {
            Vector2[] circlePoints   = new Vector2[numberOfPoints];
            float     angleIncrement = 360f / numberOfPoints;

            for (int i = 0; i < numberOfPoints; i++) {
                float angle   = i * angleIncrement;
                float radians = MathF.PI / 180f * angle;

                float x = radius * MathF.Cos(radians);
                float y = radius * MathF.Sin(radians);

                circlePoints[i] = new Vector2(x, y);
            }

            return circlePoints;
        }

        public override void FrameEffects(Player player, OmnitrixPlayer omp) {
            var costume = ModContent.GetInstance<HeatBlast>();
            player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
            player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
            player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        }
    }
}
