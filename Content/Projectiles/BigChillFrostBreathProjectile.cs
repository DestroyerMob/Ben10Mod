using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Ben10Mod.Content.Projectiles;

public class BigChillFrostBreathProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    private         int    _timeAlive = 1;

    public override void SetDefaults() {
        Projectile.width       = 64;
        Projectile.height      = 64;
        Projectile.friendly    = true;
        Projectile.tileCollide = false;
        Projectile.penetrate   = -1;
        Projectile.timeLeft    = 40;
    }

    public override void AI() {
        if (Main.GameUpdateCount % 10 == 0) {
            _timeAlive++;
        }

        Projectile.scale = 1f - 1f / _timeAlive;
        
        Vector2 center  = Projectile.Center;
        float   scaledW = Projectile.width  * Projectile.scale;
        float   scaledH = Projectile.height * Projectile.scale;

        for (int i = 0; i < 12; i++) {
            Vector2 spawnPos = center + new Vector2(
                Main.rand.NextFloat(-scaledW / 2f, scaledW / 2f),
                Main.rand.NextFloat(-scaledH / 2f, scaledH / 2f)
            );

            int dustNum = Dust.NewDust(spawnPos, 1, 1, DustID.Frost);
            Main.dust[dustNum].noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Frostburn2, 120);
    }
}