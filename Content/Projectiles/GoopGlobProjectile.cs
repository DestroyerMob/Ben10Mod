using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GoopGlobProjectile : ModProjectile {
    private const float InfusedDamageMultiplier = 1.18f;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 75;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.hide = true;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        TryInfuseFromPuddle();

        Projectile.velocity.Y += 0.05f;
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (IsInfused)
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1.18f, 0.22f);

        Vector2 dustVelocity = Projectile.velocity * 0.12f;
        Dust outer = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.GreenTorch,
            dustVelocity + Main.rand.NextVector2Circular(0.4f, 0.4f), 90,
            IsInfused ? new Color(175, 255, 125) : new Color(110, 235, 130),
            Main.rand.NextFloat(1f, 1.2f) * (IsInfused ? 1.18f : 1f));
        outer.noGravity = true;

        if (Main.rand.NextBool(2)) {
            Dust inner = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), DustID.GreenTorch,
                dustVelocity * 0.5f, 110, new Color(180, 255, 145), Main.rand.NextFloat(0.8f, 1f));
            inner.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool wasDissolved = target.HasBuff(ModContent.BuffType<GoopDissolved>());
        target.AddBuff(ModContent.BuffType<GoopDissolved>(), IsInfused ? 5 * 60 : 3 * 60);

        if (IsInfused || wasDissolved) {
            target.AddBuff(BuffID.Venom, 2 * 60);
            target.velocity.X *= 0.78f;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (IsInfused && Projectile.owner == Main.myPlayer) {
            int puddleDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.35f));
            GoopPuddleProjectile.CreateOrGrow(Projectile.GetSource_FromThis(), Projectile.Bottom + new Vector2(0f, -8f),
                puddleDamage, Projectile.owner, growth: 0.12f, refreshTime: GoopPuddleProjectile.BaseLifetime / 2);
        }

        for (int i = 0; i < 10; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
            Dust splash = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.GreenTorch : DustID.GreenMoss, velocity, 90,
                new Color(120, 240, 135), Main.rand.NextFloat(0.95f, 1.25f));
            splash.noGravity = false;
        }
    }

    private bool IsInfused => Projectile.ai[0] > 0f;

    private void TryInfuseFromPuddle() {
        if (IsInfused)
            return;

        Vector2 oldCenter = Projectile.oldPosition + new Vector2(Projectile.width, Projectile.height) * 0.5f;
        Vector2 middle = Vector2.Lerp(oldCenter, Projectile.Center, 0.5f);
        Projectile puddle = GoopPuddleProjectile.FindOwnedPuddleAtPoint(Projectile.owner, Projectile.Center, 12f) ??
                            GoopPuddleProjectile.FindOwnedPuddleAtPoint(Projectile.owner, middle, 12f) ??
                            GoopPuddleProjectile.FindOwnedPuddleAtPoint(Projectile.owner, oldCenter, 12f);
        if (puddle == null)
            return;

        Projectile.ai[0] = 1f;
        Projectile.damage = Math.Max(1, (int)Math.Round(Projectile.damage * InfusedDamageMultiplier));
        Projectile.penetrate = Math.Max(Projectile.penetrate, 2);
        Projectile.timeLeft += 18;
        Projectile.netUpdate = true;

        if (Main.dedServ)
            return;

        for (int i = 0; i < 12; i++) {
            Dust charge = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.GreenTorch,
                Main.rand.NextVector2Circular(1.4f, 1.4f), 85, new Color(180, 255, 125), Main.rand.NextFloat(1f, 1.35f));
            charge.noGravity = true;
        }
    }
}
