using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GhostFreakFearWaveProjectile : ModProjectile {
    private const int LifetimeTicks = 26;
    private const float StartRadius = 26f;
    private const float BaseMaxRadius = 156f;
    private const float PhasedMaxRadius = 190f;

    private bool Phased => Projectile.ai[0] >= 0.5f;

    private float CurrentRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = 1f - MathF.Pow(1f - progress, 2.45f);
        float maxRadius = Phased ? PhasedMaxRadius : BaseMaxRadius;
        CurrentRadius = MathHelper.Lerp(StartRadius, maxRadius, easedProgress);
        Projectile.Center = owner.Center;
        Projectile.rotation += 0.12f;

        Lighting.AddLight(Projectile.Center, new Vector3(0.18f, 0.12f, 0.32f));
        SpawnWaveDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int existingFear = state.GetGhostFreakFearStacks(Projectile.owner);
        state.ApplyGhostFreakFear(Projectile.owner, existingFear > 0 ? 2 : 1, Phased ? 300 : 240);

        Vector2 away = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
        target.velocity = Vector2.Lerp(target.velocity, away * (target.boss ? 2.2f : 5.2f), target.boss ? 0.08f : 0.22f);
        if (!target.boss)
            target.AddBuff(BuffID.Confused, existingFear > 0 ? 210 : 135);

        if (existingFear > 0)
            SpreadFear(target);
    }

    private void SpreadFear(NPC source) {
        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.whoAmI == source.whoAmI || !npc.CanBeChasedBy(Projectile))
                continue;

            if (npc.Center.Distance(source.Center) > (Phased ? 184f : 144f))
                continue;

            npc.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyGhostFreakFear(Projectile.owner, 1, Phased ? 220 : 180);
            if (!npc.boss)
                npc.AddBuff(BuffID.Confused, Phased ? 105 : 75);
            npc.netUpdate = true;
        }
    }

    private void SpawnWaveDust() {
        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        int dustCount = Math.Max(6, (int)Math.Round(CurrentRadius / 18f));
        for (int i = 0; i < dustCount; i++) {
            float angle = Projectile.rotation + MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.08f, 0.08f);
            Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(CurrentRadius * 0.72f, CurrentRadius);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool() ? DustID.Shadowflame : DustID.WhiteTorch,
                offset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.55f, 0.55f),
                110, Main.rand.NextBool() ? new Color(150, 105, 220) : new Color(235, 235, 255),
                Main.rand.NextFloat(0.72f, 1.05f));
            dust.noGravity = true;
        }
    }
}
