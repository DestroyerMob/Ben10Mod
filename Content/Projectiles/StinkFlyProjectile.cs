using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class StinkFlyProjectile : ModProjectile {
    private const float ChildDamageMultiplier = 0.32f;
    private const int MinDroplets = 4;
    private const int MaxDroplets = 9;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
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
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        if (Projectile.velocity.LengthSquared() < 256f)
            Projectile.velocity *= 1.008f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.26f, 0.08f));

        if (Main.rand.NextBool()) {
            Dust acidMist = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                Main.rand.NextBool() ? DustID.GreenBlood : DustID.Poisoned,
                -Projectile.velocity * Main.rand.NextFloat(0.03f, 0.09f), 100, new Color(235, 255, 145),
                Main.rand.NextFloat(1f, 1.28f));
            acidMist.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle source = new(0, 0, 1, 1);
        Vector2 origin = new(0.5f, 0.5f);
        float rotation = Projectile.rotation;

        Main.EntitySpriteDraw(pixel, drawPosition, source, new Color(152, 200, 70, 225), rotation, origin,
            new Vector2(20f, 12f) * Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, drawPosition, source, new Color(239, 255, 162, 205), rotation, origin,
            new Vector2(10f, 6f) * Projectile.scale, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 3 * 60);
        target.AddBuff(BuffID.Poisoned, 4 * 60);

        if (Projectile.ai[0] >= 0.55f)
            target.AddBuff(BuffID.Venom, 2 * 60);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        Vector2 baseVelocity = Projectile.velocity.LengthSquared() > 0.01f ? Projectile.velocity : Projectile.oldVelocity;
        Vector2 baseDirection = baseVelocity.SafeNormalize(new Vector2(Main.player[Projectile.owner].direction, 0f));
        float speedPressure = MathHelper.Clamp(Projectile.ai[0], 0f, 1f);

        if (Projectile.owner == Main.myPlayer || Main.netMode != NetmodeID.MultiplayerClient) {
            int childDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * ChildDamageMultiplier));
            int dropletCount = MinDroplets + (int)System.Math.Round((MaxDroplets - MinDroplets) * speedPressure);
            float rainWidth = MathHelper.Lerp(56f, 164f, speedPressure);

            for (int i = 0; i < dropletCount; i++) {
                float progress = dropletCount <= 1 ? 0.5f : i / (float)(dropletCount - 1);
                int projectileType = i % 3 == 0
                    ? ModContent.ProjectileType<StinkFlySlowProjectile>()
                    : ModContent.ProjectileType<StinkFlyPoisonProjectile>();
                float lateralOffset = (progress - 0.5f) * rainWidth + Main.rand.NextFloat(-10f, 10f);
                Vector2 childPosition = Projectile.Center + new Vector2(lateralOffset, Main.rand.NextFloat(-54f, -18f));
                float lateralVelocity = MathHelper.Lerp(-2.4f, 2.4f, progress) +
                                        baseDirection.X * MathHelper.Lerp(0.8f, 2.2f, speedPressure);
                Vector2 childVelocity = new(lateralVelocity, Main.rand.NextFloat(8.5f, 12.5f) + speedPressure * 3.5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), childPosition, childVelocity, projectileType,
                    childDamage, Projectile.knockBack * 0.75f, Projectile.owner);
            }
        }

        if (Main.dedServ)
            return;

        for (int i = 0; i < 16; i++) {
            Dust splash = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenBlood : DustID.Poisoned,
                Main.rand.NextVector2Circular(3.4f, 3.4f), 95, new Color(230, 255, 145), Main.rand.NextFloat(1.05f, 1.35f));
            splash.noGravity = true;
        }
    }
}
