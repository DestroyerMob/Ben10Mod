using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GhostFreakHauntProjectile : ModProjectile {
    private bool Phased => Projectile.ai[0] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 78;
        Projectile.extraUpdates = 1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        NPC target = FindFearedTarget();
        if (target != null) {
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) *
                                      MathHelper.Clamp(Projectile.velocity.Length(), 8f, 15f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, Phased ? 0.12f : 0.08f);
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.22f, 0.12f, 0.34f));
        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                Main.rand.NextBool() ? DustID.Shadowflame : DustID.WhiteTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.11f), 110,
                Main.rand.NextBool() ? new Color(145, 95, 205) : Color.White, Main.rand.NextFloat(0.78f, 1.08f));
            dust.noGravity = true;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int fearStacks = state.GetGhostFreakFearStacks(Projectile.owner);
        if (fearStacks > 0)
            modifiers.SourceDamage *= 1f + fearStacks * 0.08f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        state.ApplyGhostFreakHaunt(Projectile.owner, Phased ? 270 : 220);
        target.velocity *= target.boss ? 0.92f : 0.7f;
        if (!target.boss)
            target.AddBuff(BuffID.Confused, Phased ? 240 : 180);
    }

    private NPC FindFearedTarget() {
        NPC bestTarget = null;
        float bestDistance = 540f;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            AlienIdentityGlobalNPC state = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            if (!state.IsGhostFreakFearedFor(Projectile.owner))
                continue;

            float distance = npc.Center.Distance(Projectile.Center);
            if (distance >= bestDistance)
                continue;

            bestTarget = npc;
            bestDistance = distance;
        }

        return bestTarget;
    }
}
