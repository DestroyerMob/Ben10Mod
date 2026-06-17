using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AmpFibianArcProjectile : ModProjectile {
    public const float BarrierArcMode = 0f;

    private int TargetIndex => (int)Projectile.ai[0];
    private float ChargeRatio => MathHelper.Clamp(Projectile.ai[2], 0f, 1f);

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 1;
        Projectile.timeLeft = 54;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI() {
        NPC target = ResolveTarget();
        if (target != null) {
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * (18f + ChargeRatio * 4f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.18f + ChargeRatio * 0.08f);
        }

        if (Projectile.velocity.LengthSquared() <= 0.01f)
            Projectile.velocity = Vector2.UnitX * 12f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.22f + ChargeRatio * 0.12f, 0.44f, 0.86f));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                Main.rand.NextBool() ? DustID.Electric : DustID.BlueTorch,
                -Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.4f, 1.5f),
                100, Color.Lerp(new Color(115, 205, 255), new Color(230, 252, 255), ChargeRatio),
                Main.rand.NextFloat(0.9f, 1.22f + ChargeRatio * 0.22f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float width = 4.8f + ChargeRatio * 2.8f;
        float length = 32f + ChargeRatio * 20f;
        Color arcColor = Color.Lerp(new Color(105, 205, 255, 185), new Color(230, 252, 255, 220), ChargeRatio);

        Main.EntitySpriteDraw(pixel, center, null, arcColor, Projectile.rotation, Vector2.One * 0.5f,
            new Vector2(length, width), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(245, 255, 255, 230), Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(length * 0.52f, width * 0.42f), SpriteEffects.None, 0);
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.ArmorPenetration += 6 + (int)System.Math.Round(ChargeRatio * 8f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Electrified, 210 + (int)System.Math.Round(ChargeRatio * 90f));
        SpawnImpactDust(target.Center);
    }

    public override void OnKill(int timeLeft) {
        SpawnImpactDust(Projectile.Center);
    }

    private NPC ResolveTarget() {
        if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs) {
            NPC target = Main.npc[TargetIndex];
            if (target.active && target.CanBeChasedBy(Projectile))
                return target;
        }

        NPC bestTarget = null;
        float bestDistanceSquared = 620f * 620f;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distanceSquared = Vector2.DistanceSquared(Projectile.Center, npc.Center);
            if (distanceSquared >= bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            bestTarget = npc;
        }

        if (bestTarget != null)
            Projectile.ai[0] = bestTarget.whoAmI;

        return bestTarget;
    }

    private static void SpawnImpactDust(Vector2 center) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 100, new Color(150, 230, 255),
                Main.rand.NextFloat(0.9f, 1.28f));
            dust.noGravity = true;
        }
    }
}
