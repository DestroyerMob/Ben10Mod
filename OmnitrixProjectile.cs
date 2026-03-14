using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Ben10Mod;

public class OmnitrixProjectile : GlobalProjectile {
    public override bool InstancePerEntity => true;

    public  int     itemUsed         = 0;
    private int     framesAlive      = 0;
    public  bool    projectileSlowed = false;
    public  Vector2 initialVelocity  = Vector2.Zero;
    
    public override void OnSpawn(Projectile projectile, IEntitySource source) {
        if (source is IEntitySource_WithStatsFromItem itemSource) {
            itemUsed        = itemSource.Item.type;
            initialVelocity = projectile.velocity;
        }
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
        // if (itemUsed == ModContent.ItemType<PlumberMagisterBadge>())
        //     if (target.life / (float)target.lifeMax >= 0.9f) {
        //         modifiers.FinalDamage *= 1.5f;
        //     }
    }

    public override void AI(Projectile projectile) {
        framesAlive++;
        if (projectile.owner == Main.LocalPlayer.whoAmI) {
            var omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
            // if (omp.UltimateAbilityEnabled && omp.currTransformation == TransformationEnum.XLR8) {
            //     projectile.velocity = initialVelocity * (1 - framesAlive / 60f);
            //     if (framesAlive >= 60) projectile.velocity = initialVelocity.SafeNormalize(Vector2.Zero);
            //     projectileSlowed    = true;
            // }
            // else if (projectileSlowed) projectile.velocity = initialVelocity * 2f;
        }
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
        // if (itemUsed == ModContent.ItemType<PlumberMagisterBadge>())
        //     target.AddBuff(BuffID.OnFire, 120);
        if (itemUsed == ModContent.ItemType<HeavenlyCrystallineBadge>()) {
            if (!Main.rand.NextBool(3)) return;
            for (int i = 0; i < 3; i++) {
                Vector2 spawnPos = target.Center + new Vector2(Main.rand.NextFloat(-200f, 201f), -620f);
                Vector2 vel      = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * 17.5f;
                int projNum = Projectile.NewProjectile(projectile.GetSource_FromThis(),
                    spawnPos,
                    vel, ProjectileID.QueenSlimeGelAttack,
                    damageDone / 3, 0);
                Main.projectile[projNum].hostile  = false;
                Main.projectile[projNum].friendly = true;
            }
        }
    }
}