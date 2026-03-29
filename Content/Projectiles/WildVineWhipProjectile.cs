using Ben10Mod.Content.DamageClasses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WildVineWhipProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.ThornWhip}";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.CloneDefaults(ProjectileID.ThornWhip);
        AIType = ProjectileID.ThornWhip;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.WhipSettings.Segments = 20;
        Projectile.WhipSettings.RangeMultiplier = 1.15f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Poisoned, 4 * 60);
    }
}
