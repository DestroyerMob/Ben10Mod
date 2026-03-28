using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BlitzwolferPounceProjectile : ModProjectile {
    public const int MinPounceFrames = 6;
    public const int MaxPounceFrames = 18;
    private const float BasePounceSpeed = 24f;
    private const float HeightenedPounceSpeed = 29f;

    private bool Heightened => Projectile.ai[0] >= 0.5f;

    public static float GetPounceSpeed(bool heightened) => heightened ? HeightenedPounceSpeed : BasePounceSpeed;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 42;
        Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MinPounceFrames;
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
        float pounceSpeed = GetPounceSpeed(Heightened);
        Projectile.velocity = direction * pounceSpeed;
        Projectile.rotation = direction.ToRotation();
        Projectile.Center = owner.Center + direction * (Heightened ? 20f : 16f);

        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, Heightened ? 14 : 10);
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.armorEffectDrawShadow = true;

        if (Main.rand.NextBool(Heightened ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                Main.rand.NextBool() ? DustID.Smoke : DustID.GemDiamond,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 110, new Color(230, 240, 255),
                Main.rand.NextFloat(0.9f, Heightened ? 1.2f : 1.05f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int resonance = identity.GetBlitzwolferResonanceStacks(Projectile.owner);
        if (resonance > 0)
            modifiers.SourceDamage *= 1f + resonance * (Heightened ? 0.14f : 0.1f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int resonance = identity.ConsumeBlitzwolferResonance(Projectile.owner);
        identity.ApplyBlitzwolferResonance(Projectile.owner, Heightened ? 3 : 2, 220);
        if (resonance > 0)
            target.velocity = Vector2.Lerp(target.velocity, Projectile.velocity.SafeNormalize(Vector2.UnitX) * (8f + resonance), 0.55f);
        target.netUpdate = true;
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.velocity *= 0.45f;

        if (Main.dedServ)
            return;

        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GemDiamond : DustID.Smoke,
                Main.rand.NextVector2Circular(3.2f, 3.2f), 110, new Color(235, 245, 255), Main.rand.NextFloat(0.9f, 1.18f));
            dust.noGravity = true;
        }

        if (Projectile.owner != Main.myPlayer)
            return;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        int shockwaveDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.55f));
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + direction * 14f, direction * 6f,
            ModContent.ProjectileType<BlitzwolferHowlProjectile>(), shockwaveDamage, Projectile.knockBack, Projectile.owner);
    }
}
