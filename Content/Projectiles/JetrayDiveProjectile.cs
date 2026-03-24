using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class JetrayDiveProjectile : ModProjectile {
    public const float VariantAbility = 0f;
    public const float VariantUltimate = 1f;

    private bool IsUltimate => Projectile.ai[0] >= VariantUltimate;
    private float DashSpeed => IsUltimate ? 36f : 28f;
    private int LifetimeTicks => IsUltimate ? 40 : 28;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PhantasmalBolt}";

    public override void SetDefaults() {
        Projectile.width = 34;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.timeLeft = LifetimeTicks;
            Projectile.velocity = direction * DashSpeed;
            SpawnLaunchBurst(direction);
        }
        else {
            float minimumSpeed = DashSpeed * (IsUltimate ? 0.86f : 0.8f);
            float currentSpeed = Math.Max(minimumSpeed, Projectile.velocity.Length() * (IsUltimate ? 0.992f : 0.988f));
            Projectile.velocity = direction * currentSpeed;
        }

        direction = Projectile.velocity.SafeNormalize(direction);
        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneTime = IsUltimate ? 20 : 14;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.noFallDmg = true;
        owner.armorEffectDrawShadow = true;

        Projectile.Center = owner.Center + direction * (IsUltimate ? 24f : 18f);
        Projectile.rotation = direction.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, IsUltimate ? new Vector3(0.2f, 1f, 0.88f) : new Vector3(0.1f, 0.78f, 0.68f));

        if (Main.rand.NextBool(IsUltimate ? 1 : 2)) {
            int dustType = IsUltimate && Main.rand.NextBool(3) ? DustID.Electric : DustID.BlueCrystalShard;
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType,
                -direction.X * Main.rand.NextFloat(1.2f, 2.4f), -direction.Y * Main.rand.NextFloat(1.2f, 2.4f), 100,
                IsUltimate ? new Color(160, 255, 240) : Color.SpringGreen, Main.rand.NextFloat(1f, 1.28f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Electrified, IsUltimate ? 240 : 150);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.velocity *= IsUltimate ? 0.62f : 0.45f;

        SpawnImpactBurst();
        if (IsUltimate && Projectile.owner == Main.myPlayer)
            SpawnUltimateVolley();
    }

    private void SpawnLaunchBurst(Vector2 direction) {
        if (Main.dedServ)
            return;

        int dustCount = IsUltimate ? 16 : 10;
        for (int i = 0; i < dustCount; i++) {
            Vector2 velocity = -direction.RotatedByRandom(0.42f) * Main.rand.NextFloat(1.8f, IsUltimate ? 5.2f : 3.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.Electric : DustID.BlueCrystalShard,
                velocity, 100, IsUltimate ? new Color(155, 255, 235) : new Color(120, 255, 200),
                Main.rand.NextFloat(1f, IsUltimate ? 1.45f : 1.25f));
            dust.noGravity = true;
        }
    }

    private void SpawnImpactBurst() {
        if (Main.dedServ)
            return;

        int dustCount = IsUltimate ? 22 : 14;
        for (int i = 0; i < dustCount; i++) {
            int dustType = IsUltimate && i % 3 == 0 ? DustID.Electric : DustID.BlueCrystalShard;
            Vector2 velocity = Main.rand.NextVector2Circular(IsUltimate ? 4.6f : 3f, IsUltimate ? 4.6f : 3f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, velocity, 100,
                IsUltimate ? new Color(170, 255, 245) : new Color(135, 255, 215),
                Main.rand.NextFloat(1f, IsUltimate ? 1.55f : 1.3f));
            dust.noGravity = true;
        }
    }

    private void SpawnUltimateVolley() {
        int boltDamage = Math.Max(1, (int)(Projectile.damage * 0.55f));
        const int boltCount = 6;

        for (int i = 0; i < boltCount; i++) {
            Vector2 velocity = (MathHelper.TwoPi * i / boltCount).ToRotationVector2() * 15f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                ModContent.ProjectileType<JetrayBoltProjectile>(), boltDamage, Projectile.knockBack, Projectile.owner);
        }
    }
}
