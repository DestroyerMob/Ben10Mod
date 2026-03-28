using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SnareOhUltimateProjectile : ModProjectile {
    private const float BaseRadius = 120f;
    private const float ExposedCoreRadius = 156f;
    private const float InnerCoreRadius = 36f;

    private float CurrentRadius {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    private bool ExposedCore => Projectile.ai[1] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (!omp.IsUltimateAbilityActive || !omp.IsTransformed || omp.currentTransformationId != "Ben10Mod:SnareOh") {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;
        Projectile.velocity = Vector2.Zero;
        Projectile.timeLeft = 2;
        Projectile.localAI[0]++;

        float pulse = 0.5f + 0.5f * MathF.Sin(Projectile.localAI[0] * 0.12f);
        float targetRadius = MathHelper.Lerp(BaseRadius, ExposedCoreRadius, ExposedCore ? 1f : 0f);
        CurrentRadius = MathHelper.Lerp(targetRadius - 10f, targetRadius + 8f, pulse);

        IrradiateNearbyEnemies(owner);
        SpawnRadiationDust(owner);
        Lighting.AddLight(owner.Center, ExposedCore ? new Vector3(0.44f, 0.82f, 0.2f) : new Vector3(0.28f, 0.58f, 0.18f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int curseStacks = identity.GetSnareOhCurseStacks(Projectile.owner);
        if (curseStacks > 0)
            modifiers.SourceDamage *= 1f + curseStacks * 0.12f + (ExposedCore ? 0.15f : 0f);
    }

    public override bool PreDraw(ref Color lightColor) {
        if (Main.dedServ)
            return false;

        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outerColor = ExposedCore ? new Color(165, 255, 110) : new Color(125, 220, 95);
        Color innerColor = new Color(235, 205, 110);

        DrawRing(pixel, center, CurrentRadius, 4.4f, outerColor * 0.42f, Projectile.localAI[0] * 0.025f);
        DrawRing(pixel, center, CurrentRadius * 0.68f, 3.2f, innerColor * 0.26f, -Projectile.localAI[0] * 0.032f);
        Main.EntitySpriteDraw(pixel, center, null, innerColor * 0.78f, 0f, Vector2.One * 0.5f,
            new Vector2(InnerCoreRadius, InnerCoreRadius), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, outerColor * 0.95f, 0f, Vector2.One * 0.5f,
            new Vector2(11f, 11f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int spent = identity.ConsumeSnareOhCurse(Projectile.owner, ExposedCore ? 2 : 1);
        if (spent > 0)
            target.velocity *= ExposedCore ? 0.08f : 0.2f;
        target.netUpdate = true;
    }

    private void IrradiateNearbyEnemies(Player owner) {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(owner.Center, npc.Center);
            if (distance > CurrentRadius || distance <= 10f)
                continue;

            float distanceFactor = 1f - distance / CurrentRadius;
            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            int curseStacks = identity.GetSnareOhCurseStacks(Projectile.owner);
            float damping = MathHelper.Lerp(0.985f, ExposedCore ? 0.915f : 0.95f, distanceFactor);
            if (curseStacks > 0) {
                damping *= ExposedCore ? 0.84f : 0.9f;
                Vector2 inward = (owner.Center - npc.Center).SafeNormalize(Vector2.UnitY);
                npc.velocity = Vector2.Lerp(npc.velocity, inward * (1.4f + curseStacks * 0.4f), ExposedCore ? 0.16f : 0.08f);
            }
            else {
                npc.velocity *= damping;
            }
        }
    }

    private void SpawnRadiationDust(Player owner) {
        if (Main.dedServ)
            return;

        int points = ExposedCore ? 3 : 2;
        for (int i = 0; i < points; i++) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 direction = angle.ToRotationVector2();
            Vector2 position = owner.Center + direction * Main.rand.NextFloat(InnerCoreRadius, CurrentRadius);
            Vector2 velocity = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-2.6f, 2.6f);

            Dust dust = Dust.NewDustPerfect(position,
                Main.rand.NextBool(3) ? DustID.GoldFlame : DustID.GreenTorch,
                velocity, 110, ExposedCore ? new Color(180, 255, 120) : new Color(215, 225, 120),
                Main.rand.NextFloat(0.95f, 1.28f));
            dust.noGravity = true;
        }
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 30;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 1.85f), SpriteEffects.None, 0);
        }
    }
}
