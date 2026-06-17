using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SwampfireIgnitionBurstProjectile : ModProjectile {
    private const int Lifetime = 18;
    private const float BaseRadius = 72f;

    public override string Texture => "Terraria/Images/Projectile_0";

    private bool Ultimate => Projectile.ai[1] >= 1f;
    private float RadiusScale => MathHelper.Clamp(Projectile.ai[0], 0.45f, Ultimate ? 3.1f : 2.15f);
    private float Progress => 1f - Projectile.timeLeft / (float)Lifetime;
    private float CurrentRadius => BaseRadius * RadiusScale * MathHelper.Lerp(0.55f, 1.12f, Progress);

    public override void SetDefaults() {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 closest = new(
            MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
            MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));

        return Vector2.DistanceSquared(Projectile.Center, closest) <= CurrentRadius * CurrentRadius;
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.48f, 0.12f) * (Ultimate ? 1.45f : 0.95f));

        if (Main.dedServ)
            return;

        int dustCount = Ultimate ? 11 : 7;
        for (int i = 0; i < dustCount; i++) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float distance = CurrentRadius * Main.rand.NextFloat(0.12f, 0.92f);
            Vector2 position = Projectile.Center + angle.ToRotationVector2() * distance;
            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(1.1f, Ultimate ? 4.2f : 3f);
            Dust dust = Dust.NewDustPerfect(position, i % 3 == 0 ? DustID.Grass : DustID.Torch, velocity,
                95, i % 3 == 0 ? new Color(135, 230, 80) : new Color(255, 150, 55),
                Main.rand.NextFloat(0.9f, Ultimate ? 1.55f : 1.25f));
            dust.noGravity = true;
            dust.fadeIn = 0.75f;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (target.HasBuff(ModContent.BuffType<FuelVapour>()))
            modifiers.FinalDamage *= Ultimate ? 1.36f : 1.18f;

        modifiers.ArmorPenetration += Ultimate ? 14 : 7;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, Ultimate ? 420 : 260);
        target.AddBuff(ModContent.BuffType<FuelVapour>(), Ultimate ? 90 : 60);

        if (Ultimate)
            target.AddBuff(BuffID.Poisoned, 210);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float opacity = MathF.Sin(MathHelper.Clamp(Progress, 0f, 1f) * MathHelper.Pi);
        Color flame = Ultimate ? new Color(255, 120, 35, 145) : new Color(255, 155, 55, 125);
        Color bloom = Ultimate ? new Color(135, 245, 80, 95) : new Color(110, 220, 75, 75);
        float radius = CurrentRadius;

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), flame * opacity,
            0f, new Vector2(0.5f, 0.5f), new Vector2(radius * 2f, radius * 0.72f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), bloom * opacity,
            MathHelper.PiOver4, new Vector2(0.5f, 0.5f), new Vector2(radius * 1.25f, radius * 0.42f), SpriteEffects.None, 0f);
        return false;
    }
}
