using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GhostFreakPossesionProjectile : ModProjectile {
    
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";
    
    public override void SetDefaults() {
        Projectile.width   = 4;
        Projectile.height  = 4;
        Projectile.aiStyle = ProjAIStyleID.Arrow;

        AIType                 = ProjectileID.Bullet;
        Projectile.friendly    = true;
        Projectile.timeLeft    = 64;
        Projectile.tileCollide = false;
            
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
    }


    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<GhostFreakPossesion>(), 360);

        if (Main.myPlayer == Projectile.owner)
        {
            Player player = Main.player[Projectile.owner];
            var    omp    = player.GetModPlayer<OmnitrixPlayer>(); // Or CameraPlayer if separate

            // Only start possession if not already in mode (prevent stacking)
            if (!omp.inPossessionMode)
            {
                omp.prePossessionPosition = player.position; // Save current pos
                omp.possessedTarget       = target;
                omp.possessionTimer       = 360;
                omp.inPossessionMode      = true;

                // Instant teleport player to NPC center
                player.Center = target.Center;

                // Initial effects
                SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.5f, Volume = 0.8f }, player.Center);
                for (int i = 0; i < 40; i++)
                {
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(8f, 8f), Scale: 2f);
                    d.noGravity = true;
                }
            }
        }
    }
    
    public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) {
        Random random = new Random();
        for (int i = 0; i < 5; i++) {
            int dustNum = Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.WhiteTorch, 0, 0, 1, Color.White, 1);
            Main.dust[dustNum].noGravity = true;
        }
    }
}