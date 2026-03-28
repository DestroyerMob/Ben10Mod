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

public class WhampireNightSwarmProjectile : ModProjectile {
    private bool Cloaked => Projectile.ai[0] >= 0.5f;
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
        Projectile.timeLeft = 210;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 24;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead ||
            owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != "Ben10Mod:Whampire") {
            Projectile.Kill();
            return;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.4f, Volume = 0.7f }, Projectile.Center);
        }

        Projectile.velocity = Vector2.Zero;
        Projectile.rotation += 0.02f;
        Projectile.localAI[1]++;

        float pulse = 0.5f + 0.5f * MathF.Sin(Main.GameUpdateCount * 0.075f + Projectile.identity * 0.05f);
        CurrentRadius = MathHelper.Lerp(Cloaked ? 110f : 96f, Cloaked ? 172f : 154f, pulse);

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            HarassNPCs();

            if ((int)Projectile.localAI[1] % 24 == 0) {
                int childDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.4f));
                for (int i = 0; i < 2; i++) {
                    Vector2 direction = Main.rand.NextVector2Unit();
                    Vector2 spawnPosition = Projectile.Center + direction * Main.rand.NextFloat(CurrentRadius * 0.4f, CurrentRadius * 0.85f);
                    Vector2 velocity = -direction.RotatedByRandom(0.35f) * Main.rand.NextFloat(7f, 10f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity,
                        ModContent.ProjectileType<WhampireCorrupturaBoltProjectile>(), childDamage, Projectile.knockBack * 0.6f,
                        Projectile.owner, Cloaked ? 1f : 0f);
                }
            }
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.65f, 0.08f, 0.1f) * 0.58f);

        if (Main.rand.NextBool()) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(16f, CurrentRadius);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                Main.rand.NextBool() ? DustID.Shadowflame : DustID.Smoke,
                offset.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.45f, 0.45f),
                120, new Color(120, 18, 28), Main.rand.NextFloat(0.85f, 1.1f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        DrawRing(pixel, center, CurrentRadius, 4.8f, new Color(28, 6, 12, 90), Projectile.rotation * 0.7f);
        DrawRing(pixel, center, CurrentRadius * 0.72f, 4.2f, new Color(120, 18, 28, 98), -Projectile.rotation);
        DrawRing(pixel, center, CurrentRadius * 0.46f, 3.4f, new Color(235, 90, 105, 82), Projectile.rotation * 1.3f);

        const int BatStreaks = 10;
        for (int i = 0; i < BatStreaks; i++) {
            float angle = Projectile.rotation * 0.55f + MathHelper.TwoPi * i / BatStreaks;
            Vector2 position = center + angle.ToRotationVector2() * (CurrentRadius * 0.58f);
            Main.EntitySpriteDraw(pixel, position, null, new Color(180, 40, 54, 110), angle, Vector2.One * 0.5f,
                new Vector2(18f, 3f), SpriteEffects.None, 0);
        }

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Bleeding, Cloaked ? 300 : 240);
        target.AddBuff(BuffID.Confused, 150);
        target.AddBuff(BuffID.Weak, Cloaked ? 240 : 180);
        target.netUpdate = true;
    }

    private void HarassNPCs() {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(npc.Center, Projectile.Center);
            if (distance > CurrentRadius || distance <= 8f)
                continue;

            float pullStrength = MathHelper.Lerp(1.2f, Cloaked ? 5.5f : 4.5f, 1f - distance / CurrentRadius);
            Vector2 desiredVelocity = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero) * pullStrength;
            desiredVelocity.Y -= 0.25f;
            npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, npc.boss ? 0.05f : 0.16f);
            npc.netUpdate = true;
        }
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 24;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.1f), SpriteEffects.None, 0);
        }
    }
}
