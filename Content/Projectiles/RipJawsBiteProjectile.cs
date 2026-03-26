using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class RipJawsBiteProjectile : ModProjectile {
    private const float LandDashSpeed = 18f;
    private const float WaterDashSpeed = 24f;
    private const float DashDecay = 0.95f;
    private const float ForwardOffset = 30f;
    private const float DownwardPull = 0.2f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 56;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 15;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        owner.GetModPlayer<OmnitrixPlayer>().RegisterActiveLunge();

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float dashSpeed = owner.wet ? WaterDashSpeed : LandDashSpeed;

        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.velocity = direction * dashSpeed + new Vector2(0f, owner.velocity.Y > 0f ? DownwardPull : 0f);
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = 10;
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.armorEffectDrawShadow = true;
        owner.itemRotation = direction.ToRotation() * owner.direction;

        Projectile.rotation = direction.ToRotation();
        Projectile.Center = owner.MountedCenter + direction * ForwardOffset;
        Projectile.velocity *= DashDecay;

        if (Main.rand.NextBool(2)) {
            int dustType = owner.wet ? DustID.Water : DustID.Blood;
            Color dustColor = owner.wet ? new Color(110, 190, 255) : new Color(220, 75, 60);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                dustType, -direction * Main.rand.NextFloat(0.6f, 2.4f), 120, dustColor, owner.wet ? 1.1f : 0.95f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Bleeding, 240);

        for (int i = 0; i < 18; i++) {
            Vector2 burstVelocity = Projectile.rotation.ToRotationVector2().RotatedByRandom(0.65f) * Main.rand.NextFloat(1.4f, 4.2f);
            int dustType = i % 4 == 0 ? DustID.Water : DustID.Blood;
            Color dustColor = dustType == DustID.Water ? new Color(120, 210, 255) : new Color(240, 100, 85);
            Dust dust = Dust.NewDustPerfect(target.Center, dustType, burstVelocity, 90, dustColor, 1.1f);
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return;

        owner.noKnockback = false;
        owner.velocity *= 0.4f;
    }
}
