using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FrankenstrikeStormLeapProjectile : ModProjectile {
    public const int MinLeapFrames = 6;
    public const int MaxLeapFrames = 18;
    private const float BaseLeapSpeed = 22f;
    private const float OverchargedLeapSpeed = 27f;

    private float ChargeRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private bool CapacitorCore => Projectile.ai[1] >= 0.5f;
    private bool Overcharged => ChargeRatio >= 0.45f || CapacitorCore;

    public static float GetLeapSpeed(bool overcharged) => overcharged ? OverchargedLeapSpeed : BaseLeapSpeed;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 46;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MinLeapFrames;
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
        float leapSpeed = GetLeapSpeed(Overcharged);
        Projectile.velocity = direction * leapSpeed;
        Projectile.rotation = direction.ToRotation();
        Projectile.Center = owner.Center + direction * (Overcharged ? 22f : 18f);

        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, Overcharged ? 16 : 12);
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.noFallDmg = true;
        owner.armorEffectDrawShadow = true;

        if (Main.rand.NextBool(Overcharged ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                Main.rand.NextBool(3) ? DustID.Electric : DustID.BlueTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.14f), 115, new Color(170, 225, 255),
                Main.rand.NextFloat(0.95f, Overcharged ? 1.28f : 1.1f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int conductiveStacks = identity.GetFrankenstrikeConductiveStacks(Projectile.owner);
        modifiers.SourceDamage *= 1f + ChargeRatio * 0.16f + conductiveStacks * 0.08f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyFrankenstrikeConductive(Projectile.owner, Overcharged ? 3 : 2,
            Overcharged ? 260 : 220);
        target.AddBuff(BuffID.Electrified, Overcharged ? 240 : 180);
        target.netUpdate = true;
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.velocity *= 0.5f;

        if (Main.dedServ)
            return;

        for (int i = 0; i < 16; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(3.4f, 3.4f), 110, new Color(170, 225, 255), Main.rand.NextFloat(0.95f, 1.22f));
            dust.noGravity = true;
        }

        if (Projectile.owner != Main.myPlayer)
            return;

        int shockDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.52f));
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
            ModContent.ProjectileType<FrankenstrikeThunderclapProjectile>(), shockDamage, Projectile.knockBack,
            Projectile.owner, ChargeRatio);
    }
}
