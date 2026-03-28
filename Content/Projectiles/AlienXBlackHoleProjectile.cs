using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AlienXBlackHoleProjectile : ModProjectile {
    private const float PullRadius = 184f;
    private const float DamageRadius = 34f;
    private const float StrongPullRadius = 96f;
    private bool Deliberation => Projectile.ai[0] >= 0.5f;
    private float CurrentPullRadius => Deliberation ? PullRadius + 42f : PullRadius;
    private float CurrentDamageRadius => Deliberation ? DamageRadius + 8f : DamageRadius;
    private float CurrentStrongPullRadius => Deliberation ? StrongPullRadius + 18f : StrongPullRadius;

    private float VisualRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float VisualTimer {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 110;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 26;
    }

    public override void AI() {
        Projectile.rotation += 0.08f;
        VisualTimer++;
        Projectile.velocity *= 0.965f;
        if (Projectile.velocity.LengthSquared() < 0.16f)
            Projectile.velocity = Vector2.Zero;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            PullNPCs();

        float fadeIn = Utils.GetLerpValue(0f, 12f, VisualTimer, true);
        float fadeOut = Utils.GetLerpValue(0f, 18f, Projectile.timeLeft, true);
        float pulse = 0.5f + 0.5f * MathF.Sin(VisualTimer * 0.12f);
        float maxVisualRadius = Deliberation ? 132f : 104f;
        VisualRadius = MathHelper.Lerp(maxVisualRadius * 0.58f, maxVisualRadius, pulse) * fadeIn * fadeOut;

        Lighting.AddLight(Projectile.Center, new Vector3(0.56f, 0.6f, 0.95f));
        SpawnSingularityDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentDamageRadius;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        int judgement = target.GetGlobalNPC<AlienIdentityGlobalNPC>().GetAlienXJudgementStacks(Projectile.owner);
        if (judgement > 0)
            modifiers.SourceDamage *= 1f + judgement * 0.12f;
    }

    private void PullNPCs() {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.active || !npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(npc.Center, Projectile.Center);
            if (distance > CurrentPullRadius || distance <= 4f)
                continue;

            Vector2 pullDirection = (Projectile.Center - npc.Center) / distance;
            float distanceFactor = 1f - distance / CurrentPullRadius;
            float pullStrength = MathHelper.Lerp(1.4f, 12.5f, distanceFactor);
            if (distance <= CurrentStrongPullRadius)
                pullStrength *= MathHelper.Lerp(1.15f, 1.55f, 1f - distance / CurrentStrongPullRadius);

            if (npc.boss)
                pullStrength *= 0.55f;
            else if (npc.knockBackResist > 0f)
                pullStrength *= MathHelper.Lerp(0.7f, 1.15f, npc.knockBackResist);

            Vector2 targetVelocity = pullDirection * pullStrength;
            npc.velocity = Vector2.Lerp(npc.velocity, targetVelocity, npc.boss ? 0.12f : 0.3f);
            npc.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplyAlienXJudgement(Projectile.owner, 1, 45);
            npc.netUpdate = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        identity.ApplyAlienXJudgement(Projectile.owner, Deliberation ? 3 : 2, Deliberation ? 320 : 260);
        if (target.Center.Distance(Projectile.Center) <= CurrentStrongPullRadius)
            identity.ApplyAlienXStasis(Projectile.owner, Deliberation ? 38 : 24);
        target.netUpdate = true;
    }

    private void SpawnSingularityDust() {
        if (Main.dedServ)
            return;

        int points = Deliberation ? 8 : 6;
        float rotation = VisualTimer * 0.035f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            Vector2 ringPosition = Projectile.Center + direction * VisualRadius;

            Dust ringDust = Dust.NewDustPerfect(ringPosition, i % 2 == 0 ? DustID.GemDiamond : DustID.BlueTorch,
                -direction * Main.rand.NextFloat(0.2f, 1.05f), 115, new Color(215, 228, 255),
                Main.rand.NextFloat(1f, 1.3f));
            ringDust.noGravity = true;

            if (Main.rand.NextBool(2)) {
                Dust interiorDust = Dust.NewDustPerfect(
                    Projectile.Center + direction * Main.rand.NextFloat(VisualRadius * 0.2f, VisualRadius * 0.75f),
                    DustID.ShadowbeamStaff,
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    120,
                    new Color(150, 175, 255),
                    Main.rand.NextFloat(0.75f, 1.05f));
                interiorDust.noGravity = true;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }
}
