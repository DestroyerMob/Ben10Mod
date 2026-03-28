using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class PeskyDustDreamSnareProjectile : ModProjectile {
    private bool Drifting => Projectile.ai[0] >= 0.5f;
    private float CurrentRadius {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 210;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead ||
            owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != "Ben10Mod:PeskyDust") {
            Projectile.Kill();
            return;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.35f, Volume = 0.55f }, Projectile.Center);
        }

        Projectile.velocity = Vector2.Zero;
        Projectile.rotation += 0.018f;
        float pulse = 0.5f + 0.5f * MathF.Sin(Main.GameUpdateCount * 0.08f + Projectile.identity * 0.07f);
        CurrentRadius = MathHelper.Lerp(Drifting ? 72f : 60f, Drifting ? 106f : 92f, pulse);

        if (Main.netMode != NetmodeID.MultiplayerClient)
            DrowseNPCs();

        Lighting.AddLight(Projectile.Center, new Vector3(0.94f, 0.8f, 0.96f) * 0.46f);

        if (Main.rand.NextBool(2)) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(18f, CurrentRadius * 0.82f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool() ? DustID.GoldFlame : DustID.PinkFairy,
                offset.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.25f, 0.25f),
                110, new Color(255, 225, 175), Main.rand.NextFloat(0.75f, 1.02f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        DrawRing(pixel, center, CurrentRadius, 4.2f, new Color(255, 215, 145, 56), Projectile.rotation);
        DrawRing(pixel, center, CurrentRadius * 0.72f, 3.8f, new Color(255, 165, 225, 78), -Projectile.rotation * 1.2f);
        DrawRing(pixel, center, CurrentRadius * 0.46f, 3.2f, new Color(210, 245, 255, 92), Projectile.rotation * 1.6f);

        const int CageStrands = 6;
        for (int i = 0; i < CageStrands; i++) {
            float angle = Projectile.rotation * 0.7f + MathHelper.TwoPi * i / CageStrands;
            Vector2 offset = angle.ToRotationVector2() * CurrentRadius * 0.46f;
            Main.EntitySpriteDraw(pixel, center + offset, null, new Color(255, 245, 210, 90), angle + MathHelper.PiOver2,
                Vector2.One * 0.5f, new Vector2(CurrentRadius * 0.48f, 2.4f), SpriteEffects.None, 0);
        }

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.velocity *= 0.74f;
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 180);
        target.AddBuff(BuffID.Confused, Drifting ? 210 : 180);
        target.AddBuff(BuffID.Weak, Drifting ? 210 : 180);
        target.netUpdate = true;
    }

    private void DrowseNPCs() {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(npc.Center, Projectile.Center);
            if (distance > CurrentRadius || distance <= 6f)
                continue;

            float pullStrength = MathHelper.Lerp(0.6f, Drifting ? 3.4f : 2.6f, 1f - distance / CurrentRadius);
            Vector2 desiredVelocity = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero) * pullStrength;
            npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, npc.boss ? 0.04f : 0.12f);
            npc.netUpdate = true;
        }
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 22;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2f), SpriteEffects.None, 0);
        }
    }
}
