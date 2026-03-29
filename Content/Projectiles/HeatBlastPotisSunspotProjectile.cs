using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.DamageClasses;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastPotisSunspotProjectile : ModProjectile {
    private const float TargetRange = 560f;
    private const int FireInterval = 24;
    private const int MeteorVolleyInterval = 3;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 26;
        Projectile.height = 26;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Projectile.SentryLifeTime;
        Projectile.hide = true;
        Projectile.sentry = true;
        Projectile.netImportant = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || !HasPotisAltiare(owner)) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (!omp.IsTransformed || omp.currentTransformationId != "Ben10Mod:HeatBlast") {
            Projectile.Kill();
            return;
        }

        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation + 0.05f);
        Lighting.AddLight(Projectile.Center, omp.snowflake ? new Vector3(0.4f, 0.78f, 1.06f) : new Vector3(1.18f, 0.46f, 0.08f));
        EmitDust(omp);

        NPC target = FindTarget();
        if (target == null || Projectile.owner != Main.myPlayer)
            return;

        Projectile.localAI[0]++;
        if (Projectile.localAI[0] < FireInterval)
            return;

        Projectile.localAI[0] = 0f;
        Projectile.localAI[1]++;
        FireAtTarget(owner, omp, target, (int)Projectile.localAI[1]);
        Projectile.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Player owner = Main.player[Projectile.owner];
        bool snowflake = owner.active && owner.GetModPlayer<OmnitrixPlayer>().snowflake;
        Color outer = snowflake ? new Color(105, 195, 255, 110) : new Color(255, 115, 36, 110);
        Color inner = snowflake ? new Color(235, 246, 255, 220) : new Color(255, 230, 175, 220);
        Vector2 center = Projectile.Center - Main.screenPosition;
        float time = Main.GlobalTimeWrappedHourly;

        DrawCross(pixel, center, 18f, 7f, Projectile.rotation, outer * 0.95f);
        DrawCross(pixel, center, 12f, 4.2f, -Projectile.rotation * 1.3f, inner * 0.95f);
        DrawRing(pixel, center, 18f + 2f * System.MathF.Sin(time * 4.6f), 14, outer * 0.8f, 5.2f);
        DrawRing(pixel, center, 11f + 1.5f * System.MathF.Cos(time * 5.2f), 10, inner * 0.9f, 3f);
        DrawCross(pixel, center, 8f, 8f, 0f, Color.White * 0.85f);
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        Player owner = Main.player[Projectile.owner];
        bool snowflake = owner.active && owner.GetModPlayer<OmnitrixPlayer>().snowflake;
        int dustType = snowflake ? DustID.IceTorch : DustID.InfernoFork;
        Color dustColor = snowflake ? new Color(190, 238, 255) : new Color(255, 178, 92);

        for (int i = 0; i < 12; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, Main.rand.NextVector2Circular(2.8f, 2.8f), 100,
                dustColor, Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }

    private void FireAtTarget(Player owner, OmnitrixPlayer omp, NPC target, int volleyIndex) {
        Vector2 direction = Projectile.DirectionTo(target.Center);
        int lanceDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.82f));

        for (int i = -1; i <= 1; i += 2) {
            Vector2 lanceVelocity = direction.RotatedBy(i * 0.08f) * Main.rand.NextFloat(15.5f, 17.2f);
            int lanceIndex = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + direction * 16f,
                lanceVelocity, ModContent.ProjectileType<HeatBlastPotisLanceProjectile>(), lanceDamage,
                Projectile.knockBack + 0.6f, owner.whoAmI, 1f, omp.snowflake ? 1f : 0f);
            if (lanceIndex >= 0 && lanceIndex < Main.maxProjectiles)
                Main.projectile[lanceIndex].netUpdate = true;
        }

        if (volleyIndex % MeteorVolleyInterval == 0) {
            Vector2 spawnPosition = target.Center + new Vector2(Main.rand.NextFloat(-42f, 42f), -360f);
            Vector2 launchVelocity = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(14f, 16f);
            int meteorIndex = Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPosition, launchVelocity,
                ModContent.ProjectileType<HeatBlastPotisMeteorProjectile>(), System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.72f)),
                Projectile.knockBack + 0.9f, owner.whoAmI, 0f, omp.snowflake ? 1f : 0f);
            if (meteorIndex >= 0 && meteorIndex < Main.maxProjectiles)
                Main.projectile[meteorIndex].netUpdate = true;
        }
    }

    private NPC FindTarget() {
        NPC bestTarget = null;
        float bestDistance = TargetRange;

        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestTarget = npc;
        }

        return bestTarget;
    }

    private void EmitDust(OmnitrixPlayer omp) {
        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        int dustType = omp.snowflake ? (Main.rand.NextBool() ? DustID.IceTorch : DustID.SnowflakeIce) :
            (Main.rand.NextBool(3) ? DustID.InfernoFork : DustID.Flare);
        Color dustColor = omp.snowflake ? new Color(185, 235, 255) : new Color(255, 178, 92);
        Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(14f, 14f), dustType,
            Main.rand.NextVector2Circular(0.8f, 0.8f), 95, dustColor, Main.rand.NextFloat(0.95f, 1.3f));
        dust.noGravity = true;
    }

    private static void DrawCross(Texture2D pixel, Vector2 center, float length, float thickness, float rotation, Color color) {
        Main.EntitySpriteDraw(pixel, center, null, color, rotation, Vector2.One * 0.5f, new Vector2(length, thickness),
            SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, color, rotation + MathHelper.PiOver2, Vector2.One * 0.5f,
            new Vector2(length, thickness), SpriteEffects.None, 0);
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, int segments, Color color, float thickness) {
        float segmentLength = MathHelper.TwoPi * radius / segments * 0.78f;
        for (int i = 0; i < segments; i++) {
            float angle = MathHelper.TwoPi * i / segments;
            Vector2 offset = angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, center + offset, null, color, angle, Vector2.One * 0.5f,
                new Vector2(segmentLength, thickness), SpriteEffects.None, 0);
        }
    }

    private static bool HasPotisAltiare(Player player) {
        return player.GetModPlayer<PotisAltiarePlayer>().potisAltiareEquipped;
    }
}
