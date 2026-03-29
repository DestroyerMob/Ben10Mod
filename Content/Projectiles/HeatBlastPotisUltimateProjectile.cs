using Ben10Mod.Content.Projectiles.UltimateAttacks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastPotisUltimateProjectile : ChargedThrownUltimateProjectile {
    protected override Vector2 ChargeOffset => new(0f, -82f);
    protected override float InitialScale => 0.36f;
    protected override float MaxChargeScale => 2.45f;
    protected override float ChargeStep => 0.045f;
    protected override float LaunchSpeed => 7.2f;
    protected override int MaxLifetime => 16 * 60;

    private bool Snowflake {
        get {
            Player owner = Main.player[Projectile.owner];
            return owner.active && owner.GetModPlayer<OmnitrixPlayer>().snowflake;
        }
    }

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 8;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    protected override void ConfigureDefaults() {
        Projectile.tileCollide = true;
        Projectile.penetrate = 1;
        Projectile.hide = true;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    protected override void UpdateCharging(Player owner) {
        float radius = 52f * Projectile.scale;
        Lighting.AddLight(Projectile.Center, Snowflake ? new Vector3(0.45f, 0.78f, 1.08f) : new Vector3(1.32f, 0.48f, 0.08f));

        for (int i = 0; i < 20; i++) {
            Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
            int dustType = Snowflake ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce) :
                (Main.rand.NextBool(3) ? DustID.InfernoFork : DustID.Flare);
            Color dustColor = Snowflake ? new Color(190, 238, 255) : new Color(255, 176, 88);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, dustType,
                Main.rand.NextVector2Circular(1.2f, 1.8f), 96, dustColor, Main.rand.NextFloat(1.1f, 1.85f));
            dust.noGravity = true;
        }
    }

    protected override void OnLaunched(Player owner) {
        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.15f, Volume = 0.72f }, Projectile.Center);
    }

    protected override void UpdateReleased(Player owner) {
        if (Projectile.velocity.LengthSquared() < 324f)
            Projectile.velocity *= 1.0125f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, Snowflake ? new Vector3(0.5f, 0.82f, 1.15f) : new Vector3(1.35f, 0.52f, 0.08f));

        if (Main.rand.NextBool(2)) {
            int dustType = Snowflake ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce) :
                (Main.rand.NextBool(3) ? DustID.InfernoFork : DustID.Flare);
            Color dustColor = Snowflake ? new Color(190, 238, 255) : new Color(255, 178, 92);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), dustType,
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.12f), 98, dustColor,
                Main.rand.NextFloat(1.05f, 1.35f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[ProjectileID.ImpFireball].Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = frame.Size() * 0.5f;
        Color outer = Snowflake ? new Color(125, 215, 255, 200) : new Color(255, 140, 52, 205);
        Color inner = Snowflake ? new Color(240, 248, 255, 220) : new Color(255, 236, 188, 220);
        float pulse = 1f + System.MathF.Sin(Main.GlobalTimeWrappedHourly * 8.2f) * 0.08f;

        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float trailProgress = 1f - i / (float)Projectile.oldPos.Length;
            Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Main.EntitySpriteDraw(texture, drawPosition, frame, outer * (trailProgress * 0.42f), Projectile.rotation,
                origin, Projectile.scale * (1.45f - 0.45f * (1f - trailProgress)), SpriteEffects.None, 0);
        }

        Vector2 center = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(texture, center, frame, outer, Projectile.rotation, origin, Projectile.scale * 1.42f * pulse,
            SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, center, frame, inner, Projectile.rotation, origin, Projectile.scale * 0.88f * pulse,
            SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(Snowflake ? BuffID.Frostburn2 : BuffID.OnFire3, 420);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            float chargeRatio = Utils.GetLerpValue(InitialScale, MaxChargeScale, Projectile.scale, true);
            float burstRadius = MathHelper.Lerp(132f, 196f, chargeRatio);
            int burstDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.86f));
            int burstIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<HeatBlastPotisSolarBurstProjectile>(), burstDamage, Projectile.knockBack + 1.4f,
                Projectile.owner, burstRadius);
            if (burstIndex >= 0 && burstIndex < Main.maxProjectiles) {
                Main.projectile[burstIndex].timeLeft = 22;
                Main.projectile[burstIndex].netUpdate = true;
            }

            int meteorDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.55f));
            for (int i = 0; i < 4; i++) {
                Vector2 impactPosition = Projectile.Center + new Vector2(MathHelper.Lerp(-120f, 120f, i / 3f), Main.rand.NextFloat(-24f, 28f));
                Vector2 spawnPosition = impactPosition + new Vector2(Main.rand.NextFloat(-36f, 36f), -420f - 38f * i);
                Vector2 launchVelocity = (impactPosition - spawnPosition).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(14f, 16.2f);
                int meteorIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, launchVelocity,
                    ModContent.ProjectileType<HeatBlastPotisMeteorProjectile>(), meteorDamage, Projectile.knockBack + 0.9f,
                    Projectile.owner, 0f, Snowflake ? 1f : 0f);
                if (meteorIndex >= 0 && meteorIndex < Main.maxProjectiles)
                    Main.projectile[meteorIndex].netUpdate = true;
            }
        }

        if (Main.dedServ)
            return;

        int primaryDust = Snowflake ? DustID.IceTorch : DustID.InfernoFork;
        int secondaryDust = Snowflake ? DustID.SnowflakeIce : DustID.Flare;
        Color dustColor = Snowflake ? new Color(192, 240, 255) : new Color(255, 182, 92);

        for (int i = 0; i < 28; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 4 == 0 ? secondaryDust : primaryDust,
                Main.rand.NextVector2Circular(4.4f, 4.4f), 98, dustColor, Main.rand.NextFloat(1.1f, 1.75f));
            dust.noGravity = true;
        }
    }
}
