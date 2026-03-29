using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastPotisSolarBurstProjectile : ModProjectile {
    private float MaxRadius => Projectile.ai[0] > 0f ? Projectile.ai[0] : 96f;

    private float CurrentRadius {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float PreviousRadius {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 20;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 14;
    }

    public override void AI() {
        float progress = 1f - Projectile.timeLeft / 20f;
        float easedProgress = 1f - System.MathF.Pow(1f - progress, 2.4f);

        PreviousRadius = CurrentRadius;
        CurrentRadius = MathHelper.Lerp(14f, MaxRadius, easedProgress);
        EmitDust();
        Lighting.AddLight(Projectile.Center, Snowflake ? new Vector3(0.44f, 0.76f, 1.04f) : new Vector3(1.28f, 0.5f, 0.08f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outer = Snowflake ? new Color(110, 205, 255, 105) : new Color(255, 115, 32, 105);
        Color inner = Snowflake ? new Color(235, 245, 255, 205) : new Color(255, 228, 175, 210);
        int segments = 18;
        float segmentLength = MathHelper.TwoPi * CurrentRadius / segments * 0.85f;

        for (int i = 0; i < segments; i++) {
            float angle = MathHelper.TwoPi * i / segments + Main.GlobalTimeWrappedHourly * 0.55f;
            Vector2 offset = angle.ToRotationVector2() * CurrentRadius;
            Main.EntitySpriteDraw(pixel, center + offset, null, outer, angle, Vector2.One * 0.5f,
                new Vector2(segmentLength, 4.8f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(pixel, center + offset, null, inner * 0.75f, angle, Vector2.One * 0.5f,
                new Vector2(segmentLength * 0.52f, 2.5f), SpriteEffects.None, 0);
        }

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(Snowflake ? BuffID.Frostburn2 : BuffID.OnFire3, 240);
    }

    private void EmitDust() {
        if (Main.dedServ)
            return;

        int ringPoints = 10;
        int dustType = Snowflake ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce) :
            (Main.rand.NextBool(3) ? DustID.InfernoFork : DustID.Flare);
        Color startColor = Snowflake ? new Color(185, 235, 255) : new Color(255, 118, 52);
        Color endColor = Snowflake ? new Color(235, 248, 255) : new Color(255, 218, 150);

        for (int i = 0; i < ringPoints; i++) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 direction = angle.ToRotationVector2();
            float distance = MathHelper.Lerp(PreviousRadius, CurrentRadius, Main.rand.NextFloat());
            Dust dust = Dust.NewDustPerfect(Projectile.Center + direction * distance, dustType,
                direction * Main.rand.NextFloat(0.4f, 2.2f), 100,
                Color.Lerp(startColor, endColor, Main.rand.NextFloat()), Main.rand.NextFloat(0.92f, 1.3f));
            dust.noGravity = true;
        }
    }

    private bool Snowflake {
        get {
            Player owner = Main.player[Projectile.owner];
            return owner.active && owner.GetModPlayer<OmnitrixPlayer>().snowflake;
        }
    }
}
