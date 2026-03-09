using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GhostFreakPossesionProjectile : ModProjectile {
    public override void SetDefaults() {
        Projectile.width   = 26;
        Projectile.height  = 44;
        Projectile.aiStyle = ProjAIStyleID.Arrow;

        AIType                 = ProjectileID.Bullet;
        Projectile.friendly    = true;
        Projectile.timeLeft    = 64;
        Projectile.tileCollide = false;
            
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
    }


    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {

        if (Main.myPlayer == Projectile.owner) {
            Player player = Main.player[Projectile.owner];
            var    omp    = player.GetModPlayer<OmnitrixPlayer>(); // Or CameraPlayer if separate

            // Only start possession if not already in mode (prevent stacking)
            if (!omp.inPossessionMode) {
                omp.prePossessionPosition = player.position; // Save current pos
                omp.possessedTargetIndex  = target.whoAmI;
                omp.possessionTimer       = 360;
                omp.inPossessionMode      = true;

                // Instant teleport player to NPC center
                player.Center = target.Center;

                // Initial effects
                SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.5f, Volume = 0.8f }, player.Center);
                for (int i = 0; i < 40; i++) {
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                        Main.rand.NextVector2Circular(8f, 8f), Scale: 2f);
                    d.noGravity = true;
                }
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        lightColor.A /= 2;
        return base.PreDraw(ref lightColor);
    }

    public override void AI() {
        for (int i = 0; i < 5; i++) {
            int dustNum = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, 0, 0, 1, i % 2 == 0 ? Color.White : Color.Black, 3);
            Main.dust[dustNum].noGravity = true;
        }
    }
}