using Microsoft.Xna.Framework;
using Ben10Mod.Content.DamageClasses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsRushProjectile : ModProjectile {
    public const int MinRushFrames = 7;
    public const int MaxRushFrames = 18;
    private const float BaseRushSpeed = 23f;
    private const float EmpoweredRushSpeed = 28f;

    private bool Empowered => Projectile.ai[0] >= 0.5f;

    public static float GetRushSpeed(bool empowered) => empowered ? EmpoweredRushSpeed : BaseRushSpeed;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 50;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MinRushFrames;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        owner.GetModPlayer<OmnitrixPlayer>().RegisterActiveLunge();

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float rushSpeed = GetRushSpeed(Empowered);
        Projectile.velocity = direction * rushSpeed;
        Projectile.rotation = direction.ToRotation();
        Projectile.Center = owner.Center + direction * (Empowered ? 24f : 20f);

        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = System.Math.Max(owner.immuneTime, Empowered ? 16 : 12);
        owner.noKnockback = true;
        owner.noFallDmg = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.armorEffectDrawShadow = true;

        if (Main.rand.NextBool(Empowered ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                Main.rand.NextBool() ? DustID.Smoke : DustID.Stone,
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.13f), 100, new Color(255, 185, 145),
                Main.rand.NextFloat(0.95f, Empowered ? 1.28f : 1.12f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.BrokenArmor, Empowered ? 240 : 180);
        target.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitX) * (Empowered ? 4.5f : 3f);
        target.netUpdate = true;
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.velocity *= 0.45f;

        if (Main.dedServ)
            return;

        for (int i = 0; i < 18; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Smoke : DustID.Stone,
                Main.rand.NextVector2Circular(3.8f, 3.8f), 105, new Color(255, 190, 155),
                Main.rand.NextFloat(0.95f, 1.24f));
            dust.noGravity = true;
        }

        if (Projectile.owner != Main.myPlayer)
            return;

        int shockDamage = System.Math.Max(1,
            (int)System.Math.Round(Projectile.damage * (Empowered ? 0.78f : 0.62f)));
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Bottom + new Vector2(0f, -10f), Vector2.Zero,
            ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>(), shockDamage, Projectile.knockBack,
            Projectile.owner, Empowered ? 1.22f : 1.05f);
    }
}
