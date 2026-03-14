using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.UltimateAttacks;

public abstract class BurstFieldUltimateProjectile : ModProjectile
{
    protected virtual float RadiusGrowthPerTick => 10f;
    protected virtual float MaxRadius => 160f;
    protected virtual float FollowOffsetY => 0f;
    protected virtual int LifetimeTicks => 60;
    protected virtual int DustType => DustID.GemEmerald;
    protected virtual Color FieldColor => Color.LimeGreen;
    protected virtual float LightStrength => 0.8f;

    protected float CurrentRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center + new Vector2(0f, FollowOffsetY);
        CurrentRadius = System.Math.Min(MaxRadius, CurrentRadius + RadiusGrowthPerTick);
        SpawnFieldDust();
        Lighting.AddLight(Projectile.Center, FieldColor.ToVector3() * LightStrength);
        UpdateField(owner);
    }

    protected virtual void UpdateField(Player owner) { }

    private void SpawnFieldDust() {
        for (int i = 0; i < 10; i++) {
            Vector2 offset = Main.rand.NextVector2CircularEdge(CurrentRadius, CurrentRadius);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustType, offset * 0.02f, 100, FieldColor, 1.4f);
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }
}
