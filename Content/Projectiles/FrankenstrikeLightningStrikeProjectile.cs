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

public class FrankenstrikeLightningStrikeProjectile : ModProjectile {
    private bool Overcharged => Projectile.ai[1] >= 0.5f;
    private bool Activated {
        get => Projectile.localAI[0] >= 0.5f;
        set => Projectile.localAI[0] = value ? 1f : 0f;
    }

    private float ActiveTime {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    private float CurrentRadius => Overcharged ? 72f : 58f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 40;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override bool? CanDamage() => Activated && ActiveTime > 0f;

    public override void AI() {
        if (!Activated) {
            if (Projectile.ai[0] > 0f) {
                Projectile.ai[0]--;
            }
            else {
                Activated = true;
                ActiveTime = Overcharged ? 7f : 6f;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.38f, Volume = 0.82f }, Projectile.Center);

                if (!Main.dedServ) {
                    for (int i = 0; i < 24; i++) {
                        Vector2 velocity = Main.rand.NextVector2Circular(4.6f, 4.6f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                            velocity, 105, new Color(190, 235, 255), Main.rand.NextFloat(1f, 1.35f));
                        dust.noGravity = true;
                    }
                }
            }
        }
        else {
            ActiveTime--;
            if (ActiveTime <= 0f)
                Projectile.Kill();
        }

        Lighting.AddLight(Projectile.Center, Activated ? new Vector3(0.35f, 0.58f, 1f) : new Vector3(0.12f, 0.2f, 0.48f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (!Activated || ActiveTime <= 0f)
            return false;

        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;

        if (!Activated) {
            float radius = Overcharged ? 28f : 22f;
            DrawRing(pixel, center, radius, 3f, new Color(165, 205, 255, 65), Projectile.ai[0] * 0.08f);
            DrawRing(pixel, center, radius * 0.62f, 2.2f, new Color(235, 245, 255, 90), -Projectile.ai[0] * 0.1f);
            return false;
        }

        DrawBeam(pixel, center + new Vector2(0f, -440f), center, Overcharged ? 12f : 9f, new Color(105, 155, 255, 145));
        DrawBeam(pixel, center + new Vector2(0f, -440f), center, Overcharged ? 6f : 4f, new Color(235, 245, 255, 220));
        DrawRing(pixel, center, CurrentRadius, 4f, new Color(105, 155, 255, 88), ActiveTime * 0.1f);
        DrawRing(pixel, center, CurrentRadius * 0.64f, 3.2f, new Color(235, 245, 255, 138), -ActiveTime * 0.14f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.velocity = Vector2.Lerp(target.velocity, new Vector2(target.velocity.X * 0.35f, -2.4f), 0.5f);
        target.AddBuff(BuffID.Electrified, Overcharged ? 300 : 240);
        target.AddBuff(BuffID.BrokenArmor, Overcharged ? 240 : 180);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), Overcharged ? 150 : 105);
        target.netUpdate = true;
    }

    private static void DrawBeam(Texture2D pixel, Vector2 start, Vector2 end, float width, Color color) {
        Vector2 delta = end - start;
        float length = delta.Length();
        if (length <= 0.5f)
            return;

        float rotation = delta.ToRotation();
        Main.EntitySpriteDraw(pixel, start, null, color, rotation, new Vector2(0f, 0.5f),
            new Vector2(length, width), SpriteEffects.None, 0);
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotation) {
        const int Segments = 20;
        for (int i = 0; i < Segments; i++) {
            float angle = rotation + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 2.4f), SpriteEffects.None, 0);
        }
    }
}
