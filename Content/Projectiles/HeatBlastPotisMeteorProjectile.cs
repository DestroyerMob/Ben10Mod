using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastPotisMeteorProjectile : ModProjectile {
    private bool Snowflake => Projectile.ai[1] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 6;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 120;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.velocity.X *= 0.996f;
        Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y + 0.38f, -18f, 19f);
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, Snowflake ? new Vector3(0.38f, 0.72f, 1.06f) : new Vector3(1.15f, 0.48f, 0.08f));

        if (Main.rand.NextBool(2)) {
            int dustType = Snowflake ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce) :
                (Main.rand.NextBool(3) ? DustID.InfernoFork : DustID.Flare);
            Color dustColor = Snowflake ? new Color(185, 235, 255) : new Color(255, 172, 88);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), dustType,
                -Projectile.velocity * Main.rand.NextFloat(0.03f, 0.1f), 96, dustColor, Main.rand.NextFloat(1f, 1.26f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Color trailColor = Snowflake ? new Color(110, 205, 255, 92) : new Color(255, 115, 35, 92);
        Color coreColor = Snowflake ? new Color(232, 245, 255, 215) : new Color(255, 235, 185, 220);
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
        float rotation = direction.ToRotation();

        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float trailProgress = 1f - i / (float)Projectile.oldPos.Length;
            Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            DrawPixel(pixel, drawPosition, rotation, new Vector2(44f, 8f) * trailProgress, trailColor * (trailProgress * 0.65f));
        }

        Vector2 center = Projectile.Center - Main.screenPosition;
        DrawPixel(pixel, center - direction * 10f, rotation, new Vector2(58f, 11f), trailColor * 0.92f);
        DrawPixel(pixel, center, rotation, new Vector2(20f, 20f), coreColor);
        DrawPixel(pixel, center, 0f, new Vector2(10f, 10f), Color.White * 0.86f);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(Snowflake ? BuffID.Frostburn2 : BuffID.OnFire3, 300);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            int burstDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.72f));
            int projectileIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<HeatBlastPotisSolarBurstProjectile>(), burstDamage, Projectile.knockBack + 1f,
                Projectile.owner, 88f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Main.projectile[projectileIndex].timeLeft = 18;
                Main.projectile[projectileIndex].netUpdate = true;
            }
        }

        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.18f, Volume = 0.55f }, Projectile.Center);

        int primaryDust = Snowflake ? DustID.IceTorch : DustID.InfernoFork;
        int secondaryDust = Snowflake ? DustID.SnowflakeIce : DustID.Smoke;
        Color dustColor = Snowflake ? new Color(190, 238, 255) : new Color(255, 178, 92);

        for (int i = 0; i < 16; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 4 == 0 ? secondaryDust : primaryDust,
                Main.rand.NextVector2Circular(3.4f, 3.4f), 100, dustColor, Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private static void DrawPixel(Texture2D pixel, Vector2 position, float rotation, Vector2 scale, Color color) {
        Main.EntitySpriteDraw(pixel, position, null, color, rotation, Vector2.One * 0.5f, scale, SpriteEffects.None, 0);
    }
}
