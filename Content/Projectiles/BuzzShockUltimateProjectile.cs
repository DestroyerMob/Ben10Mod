using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BuzzShockUltimateProjectile : ModProjectile {
    public const float ArcVolleyMode = -1f;
    public const float UltimateMode = 1f;
    public const float TaggedStormMode = 2f;

    private const int MaxArcForkDepth = 2;
    private const int MaxUltimateForkDepth = 3;
    private const float ArcTargetRange = 640f;
    private const float UltimateTargetRange = 920f;
    private const float ForkTargetRange = 700f;

    private int target = -1;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
        AIType = ProjectileID.Bullet;
        Projectile.friendly = true;
        Projectile.penetrate = 10;
        Projectile.timeLeft = 96;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (target == -1 && Projectile.ai[2] >= 0f && Projectile.ai[2] < Main.maxNPCs) {
            NPC initialTarget = Main.npc[(int)Projectile.ai[2]];
            if (initialTarget.CanBeChasedBy(this))
                target = initialTarget.whoAmI;
        }

        if (target == -1 || !Main.npc[target].active || !Main.npc[target].CanBeChasedBy(this))
            FindTarget();

        if (target != -1) {
            NPC npc = Main.npc[target];
            bool tagged = BuzzShockTargeting.IsTagged(npc);
            float speed = TaggedStorm ? 34f : UltimateBolt ? 30f : 25f;
            if (tagged)
                speed += TaggedStorm ? 7f : 5f;

            float inertia = tagged ? 0.34f : 0.22f;
            Vector2 desiredVelocity = Projectile.DirectionTo(npc.Center) * speed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, inertia);
        }

        Lighting.AddLight(Projectile.Center, 0.16f, 0.46f, 0.68f);
        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                TaggedStorm ? DustID.GemDiamond : DustID.UltraBrightTorch,
                -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.4f, 1.8f),
                90, Color.White, TaggedStorm ? 1.2f : 1f);
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC targetNpc, NPC.HitInfo hit, int damageDone) {
        bool wasTagged = BuzzShockTargeting.IsTagged(targetNpc);
        targetNpc.AddBuff(BuzzShockTargeting.TagBuffType, UltimateBolt ? 360 : 300);
        targetNpc.AddBuff(BuffID.Electrified, TaggedStorm ? 180 : wasTagged ? 120 : 70);
        SpawnImpactDust(targetNpc, TaggedStorm ? 18 : wasTagged ? 14 : 8);

        if (TaggedStorm)
            return;

        if (ArcVolley && Projectile.ai[1] < MaxArcForkDepth)
            SpawnForks(targetNpc, wasTagged ? 2 : 1, wasTagged ? 0.66f : 0.48f, ArcVolleyMode);
        else if (UltimateBolt && wasTagged && Projectile.ai[1] < MaxUltimateForkDepth)
            SpawnForks(targetNpc, 1, 0.58f, UltimateMode);

        FindTarget();
    }

    private bool ArcVolley => Projectile.ai[0] < 0f;

    private bool UltimateBolt => Projectile.ai[0] > 0f;

    private bool TaggedStorm => Projectile.ai[0] >= TaggedStormMode;

    private void FindTarget() {
        float range = UltimateBolt ? UltimateTargetRange : ArcTargetRange;
        NPC selectedTarget = BuzzShockTargeting.FindTarget(Projectile.Center, range, preferTagged: true);
        target = selectedTarget?.whoAmI ?? -1;
    }

    private void SpawnForks(NPC sourceTarget, int forkCount, float damageScale, float mode) {
        int firstTarget = -1;
        for (int i = 0; i < forkCount; i++) {
            NPC forkTarget = BuzzShockTargeting.FindTarget(sourceTarget.Center, ForkTargetRange, preferTagged: true,
                excludedWhoAmI: sourceTarget.whoAmI, secondExcludedWhoAmI: firstTarget);
            if (forkTarget == null)
                return;

            if (i == 0)
                firstTarget = forkTarget.whoAmI;

            Vector2 direction = sourceTarget.Center.DirectionTo(forkTarget.Center);
            if (direction == Vector2.Zero)
                direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), sourceTarget.Center, direction * 22f,
                Type, Math.Max(1, (int)(Projectile.damage * damageScale)), Projectile.knockBack * 0.72f,
                Projectile.owner, mode, Projectile.ai[1] + 1f, forkTarget.whoAmI);
        }
    }

    private static void SpawnImpactDust(NPC targetNpc, int count) {
        for (int i = 0; i < count; i++) {
            Dust dust = Dust.NewDustPerfect(targetNpc.Center + Main.rand.NextVector2Circular(12f, 12f),
                i % 3 == 0 ? DustID.GemSapphire : DustID.UltraBrightTorch,
                Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.8f, 4.5f),
                90, Color.White, Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }
}
