using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Transformations.PeskyDust;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class PeskyDustSandmanStormProjectile : ModProjectile {
    private bool Drifting => Projectile.ai[0] >= 0.5f;
    private float CurrentRadius {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 34;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 22;
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
            SoundEngine.PlaySound(SoundID.Item105 with { Pitch = 0.2f, Volume = 0.65f }, Projectile.Center);
        }

        Projectile.velocity = Vector2.Zero;
        Projectile.rotation += 0.014f;

        float pulse = 0.5f + 0.5f * MathF.Sin(Main.GameUpdateCount * 0.06f + Projectile.identity * 0.09f);
        CurrentRadius = MathHelper.Lerp(Drifting ? 118f : 104f, Drifting ? 176f : 160f, pulse);

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            DrowseNPCs();

            if ((int)Projectile.localAI[1] % 20 == 0) {
                int childDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.45f));
                for (int i = 0; i < 3; i++) {
                    Vector2 direction = Main.rand.NextVector2Unit();
                    Vector2 spawnPosition = Projectile.Center + direction * Main.rand.NextFloat(12f, CurrentRadius * 0.45f);
                    Vector2 velocity = direction.RotatedByRandom(0.45f) * Main.rand.NextFloat(5f, 8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity,
                        ModContent.ProjectileType<PeskyDustSleepDustProjectile>(), childDamage, Projectile.knockBack * 0.65f,
                        Projectile.owner, Drifting ? 1f : 0f);
                }
            }
        }

        Projectile.localAI[1]++;
        Lighting.AddLight(Projectile.Center, new Vector3(0.98f, 0.82f, 0.78f) * 0.55f);

        if (Main.rand.NextBool()) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(18f, CurrentRadius);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool() ? DustID.GoldFlame : DustID.PinkFairy,
                offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.4f, 0.4f),
                110, new Color(255, 220, 170), Main.rand.NextFloat(0.85f, 1.15f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        DrawRing(pixel, center, CurrentRadius, 5f, new Color(255, 210, 140, 52), Projectile.rotation * 0.6f);
        DrawRing(pixel, center, CurrentRadius * 0.72f, 4.6f, new Color(255, 165, 225, 68), -Projectile.rotation);
        DrawRing(pixel, center, CurrentRadius * 0.44f, 4f, new Color(210, 245, 255, 82), Projectile.rotation * 1.45f);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 245, 210, 140), 0f, Vector2.One * 0.5f,
            new Vector2(20f, 20f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.velocity *= 0.7f;
        target.AddBuff(BuffID.Confused, 210);
        target.netUpdate = true;
    }

    private void DrowseNPCs() {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(npc.Center, Projectile.Center);
            if (distance > CurrentRadius || distance <= 8f)
                continue;

            float pullStrength = MathHelper.Lerp(0.9f, Drifting ? 4.8f : 4f, 1f - distance / CurrentRadius);
            Vector2 desiredVelocity = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero) * pullStrength;
            desiredVelocity.Y -= 0.3f;
            npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, npc.boss ? 0.05f : 0.15f);
            npc.GetGlobalNPC<AlienIdentityGlobalNPC>().AddPeskyDrowsy(Projectile.owner, Drifting ? 5 : 4, 100,
                PeskyDustTransformation.DreamThreshold, Drifting ? 300 : 240);
            npc.netUpdate = true;
        }
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 26;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.2f), SpriteEffects.None, 0);
        }
    }
}
