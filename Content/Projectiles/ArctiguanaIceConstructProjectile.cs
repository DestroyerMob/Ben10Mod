using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArctiguanaIceConstructProjectile : ModProjectile {
    private const int LifetimeTicks = 210;
    private const float StartScale = 0.22f;
    private const float MaxScale = 1f;
    private const float ConstructWidth = 34f;
    private const float ConstructHeight = 118f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 44;
        Projectile.height = 126;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 18;
    }

    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) {
        Projectile.scale = StartScale;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != "Ben10Mod:Arctiguana") {
            Projectile.Kill();
            return;
        }

        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float growth = MathHelper.Clamp(progress * 3f, 0f, 1f);
        Projectile.scale = MathHelper.Lerp(StartScale, MaxScale, growth);

        Lighting.AddLight(Projectile.Center, new Vector3(0.16f, 0.5f, 0.76f) * Projectile.scale);
        SpawnConstructDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Rectangle hitbox = GetConstructHitbox();
        return hitbox.Intersects(targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Frostburn2, 180);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 180);
        target.AddBuff(ModContent.BuffType<EnemyFrozen>(), 30);
        target.netUpdate = true;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 18; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 38f),
                i % 3 == 0 ? DustID.IceTorch : DustID.Frost,
                Main.rand.NextVector2Circular(2.4f, 3.6f), 95, new Color(175, 240, 255), Main.rand.NextFloat(0.95f, 1.3f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 drawCenter = Projectile.Center - Main.screenPosition;
        Vector2 outerScale = new(ConstructWidth * Projectile.scale, ConstructHeight * Projectile.scale);
        Vector2 midScale = new(outerScale.X * 0.6f, outerScale.Y * 0.92f);
        Vector2 innerScale = new(outerScale.X * 0.28f, outerScale.Y * 0.78f);

        Main.EntitySpriteDraw(pixel, drawCenter, null, new Color(90, 175, 255, 185), 0f,
            Vector2.One * 0.5f, outerScale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, drawCenter, null, new Color(150, 225, 255, 190), 0f,
            Vector2.One * 0.5f, midScale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, drawCenter + new Vector2(0f, -outerScale.Y * 0.48f), null,
            new Color(235, 250, 255, 210), 0f, Vector2.One * 0.5f, new Vector2(outerScale.X * 1.15f, 8f),
            SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, drawCenter, null, new Color(235, 250, 255, 185), 0f,
            Vector2.One * 0.5f, innerScale, SpriteEffects.None, 0);
        return false;
    }

    private Rectangle GetConstructHitbox() {
        int width = Math.Max(12, (int)(ConstructWidth * Projectile.scale));
        int height = Math.Max(38, (int)(ConstructHeight * Projectile.scale));
        return new Rectangle((int)(Projectile.Center.X - width * 0.5f), (int)(Projectile.Center.Y - height * 0.5f),
            width, height);
    }

    private void SpawnConstructDust() {
        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        Rectangle hitbox = GetConstructHitbox();
        Vector2 dustPosition = new(
            Main.rand.NextFloat(hitbox.Left, hitbox.Right),
            Main.rand.NextFloat(hitbox.Top, hitbox.Bottom)
        );

        Dust dust = Dust.NewDustPerfect(dustPosition, Main.rand.NextBool(3) ? DustID.IceTorch : DustID.Frost,
            Main.rand.NextVector2Circular(0.35f, 1.2f), 95, new Color(180, 235, 255), Main.rand.NextFloat(0.9f, 1.16f));
        dust.noGravity = true;
    }
}
