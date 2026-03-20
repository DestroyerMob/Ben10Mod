using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastAuraRodProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private const float AuraRadius = 7f * 16f;
    private const float AuraHalfThickness = 16f;
    private const int DustPoints = 48;
    private const int BurnDuration = 10 * 60;
    private const int DamageInterval = 20;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 22;
        Projectile.height = 42;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Projectile.SentryLifeTime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.hide = true;
        Projectile.sentry = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = 0f;

        EmitDust();
        Lighting.AddLight(Projectile.Center, new Vector3(0.95f, 0.42f, 0.08f) * 0.9f);

        UpdateLocalHitCooldowns();
        TryDamageAuraNPCs();
    }

    private void EmitDust() {
        if (Main.rand.NextBool(2)) {
            Vector2 centerOffset = Main.rand.NextVector2Circular(10f, 14f);
            Dust centerDust = Dust.NewDustPerfect(Projectile.Center + centerOffset, DustID.Flare,
                new Vector2(Main.rand.NextFloat(-0.35f, 0.35f), Main.rand.NextFloat(-2.3f, -0.75f)), 110,
                new Color(255, 170, 90), Main.rand.NextFloat(1.1f, 1.55f));
            centerDust.noGravity = true;
        }

        if (Projectile.localAI[0]++ % 4f != 0f)
            return;

        for (int i = 0; i < DustPoints; i++) {
            float angle = MathHelper.TwoPi * i / DustPoints;
            Vector2 unit = angle.ToRotationVector2();
            Vector2 dustPosition = Projectile.Center + unit * AuraRadius;
            Vector2 dustVelocity = unit.RotatedBy(MathHelper.PiOver2) * 0.45f;
            Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Torch, dustVelocity, 90,
                new Color(255, 120, 30), Main.rand.NextFloat(1.05f, 1.3f));
            dust.noGravity = true;
        }
    }

    private void UpdateLocalHitCooldowns() {
        for (int i = 0; i < Projectile.localNPCImmunity.Length; i++) {
            if (Projectile.localNPCImmunity[i] > 0)
                Projectile.localNPCImmunity[i]--;
        }
    }

    private void TryDamageAuraNPCs() {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Player owner = Main.player[Projectile.owner];
        int auraDamage = Math.Max(1, Projectile.damage);

        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            if (Projectile.localNPCImmunity[npc.whoAmI] > 0)
                continue;

            if (!IsTouchingAuraRing(npc))
                continue;

            npc.AddBuff(BuffID.OnFire3, BurnDuration);
            npc.SimpleStrikeNPC(auraDamage, owner.direction, false, 0f, ModContent.GetInstance<HeroDamage>());
            Projectile.localNPCImmunity[npc.whoAmI] = DamageInterval;
        }
    }

    private bool IsTouchingAuraRing(NPC npc) {
        float npcRadius = Math.Max(npc.width, npc.height) * 0.5f;
        float distance = Vector2.Distance(npc.Center, Projectile.Center);
        float innerRadius = AuraRadius - AuraHalfThickness - npcRadius;
        float outerRadius = AuraRadius + AuraHalfThickness + npcRadius;
        return distance >= innerRadius && distance <= outerRadius;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 basePosition = Projectile.Bottom - Main.screenPosition;

        Main.EntitySpriteDraw(pixel, basePosition + new Vector2(0f, -21f), null, new Color(95, 35, 20, 255), 0f,
            new Vector2(0.5f, 0.5f), new Vector2(8f, 42f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, basePosition + new Vector2(0f, -21f), null, new Color(255, 145, 45, 220), 0f,
            new Vector2(0.5f, 0.5f), new Vector2(4f, 34f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, basePosition + new Vector2(0f, -38f), null, new Color(255, 220, 155, 235), 0f,
            new Vector2(0.5f, 0.5f), new Vector2(12f, 9f), SpriteEffects.None, 0);
        return false;
    }
}
