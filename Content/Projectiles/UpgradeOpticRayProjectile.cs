using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Upgrade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UpgradeOpticRayProjectile : ModProjectile {
    public override string Texture => "Ben10Mod/Content/Projectiles/EyeGuyLaserbeam";

    private UpgradeTechProfile Profile => (UpgradeTechProfile)Utils.Clamp((int)Math.Round(Projectile.ai[0]), 0, 4);
    private int FlagMask => (int)Math.Round(Projectile.ai[1]);
    private bool Overclocked => (FlagMask & 1) != 0;
    private bool FullyIntegrated => (FlagMask & 2) != 0;
    private UpgradeAttackVariant Variant => (UpgradeAttackVariant)((FlagMask >> 2) & 0x3);

    public override void SetDefaults() {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 82;
        Projectile.extraUpdates = 1;
        Projectile.alpha = 10;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 9;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            ApplyProfileDefaults();
        }

        switch (Profile) {
            case UpgradeTechProfile.Melee:
                ApplyMeleeMotion();
                break;
            case UpgradeTechProfile.Magic:
                ApplyHoming(FullyIntegrated ? 0.16f : 0.11f, FullyIntegrated ? 560f : 460f);
                break;
            case UpgradeTechProfile.Summon:
                ApplyHoming(FullyIntegrated ? 0.12f : 0.08f, FullyIntegrated ? 520f : 420f);
                Projectile.velocity = Projectile.velocity.RotatedBy(Math.Sin((Main.GameUpdateCount + Projectile.identity) * 0.05f) * 0.01f);
                break;
            default:
                if (Projectile.velocity.LengthSquared() < 1225f)
                    Projectile.velocity *= 1.0125f;
                break;
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
        Color profileColor = UpgradeTransformation.GetTechColor(Profile);
        Lighting.AddLight(Projectile.Center, profileColor.ToVector3() * 0.0038f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center,
                Main.rand.NextBool() ? DustID.Electric : DustID.GreenTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 95,
                profileColor,
                Overclocked ? Main.rand.NextFloat(0.95f, 1.18f) : Main.rand.NextFloat(0.82f, 1.02f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = texture.Size() * 0.5f;
        float rotation = Projectile.rotation + MathHelper.PiOver2;

        Color outerColor = UpgradeTransformation.GetTechColor(Profile);
        outerColor.A = 220;
        Color innerColor = FullyIntegrated
            ? new Color(235, 255, 245, 230)
            : Color.Lerp(new Color(195, 255, 215, 220), Color.White, 0.28f);

        float outerScale = Projectile.scale * (FullyIntegrated ? 1.16f : 1.02f);
        float innerScale = Projectile.scale * 0.7f;
        if (Profile == UpgradeTechProfile.Melee) {
            outerScale *= 1.18f;
            innerScale *= 0.92f;
        }

        Main.EntitySpriteDraw(texture, drawPosition, null, outerColor * Projectile.Opacity, rotation,
            origin, outerScale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(texture, drawPosition, null, innerColor * Projectile.Opacity, rotation,
            origin, innerScale, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        switch (Profile) {
            case UpgradeTechProfile.Melee:
                target.AddBuff(BuffID.BrokenArmor, FullyIntegrated ? 180 : 120);
                if (Variant != UpgradeAttackVariant.Primary)
                    target.AddBuff(ModContent.BuffType<EnemySlow>(), FullyIntegrated ? 150 : 105);
                break;
            case UpgradeTechProfile.Ranged:
                if (Overclocked || FullyIntegrated)
                    target.AddBuff(BuffID.BrokenArmor, FullyIntegrated ? 150 : 90);
                break;
            case UpgradeTechProfile.Magic:
                target.AddBuff(BuffID.Electrified, FullyIntegrated ? 210 : 150);
                if (Variant != UpgradeAttackVariant.Primary)
                    target.AddBuff(BuffID.Confused, FullyIntegrated ? 120 : 75);
                break;
            case UpgradeTechProfile.Summon:
                target.AddBuff(ModContent.BuffType<EnemySlow>(), FullyIntegrated ? 165 : 120);
                if (Overclocked)
                    target.AddBuff(BuffID.Electrified, 90);
                break;
            default:
                if (FullyIntegrated)
                    target.AddBuff(BuffID.Electrified, 90);
                break;
        }
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        Color profileColor = UpgradeTransformation.GetTechColor(Profile);
        for (int i = 0; i < 7; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.GreenTorch,
                Main.rand.NextVector2Circular(1.9f, 1.9f), 95,
                profileColor, Main.rand.NextFloat(0.9f, 1.16f));
            dust.noGravity = true;
        }
    }

    private void ApplyProfileDefaults() {
        switch (Profile) {
            case UpgradeTechProfile.Melee:
                Projectile.scale = Variant == UpgradeAttackVariant.Primary ? 1.2f : 1.42f;
                Projectile.penetrate = Variant == UpgradeAttackVariant.Primary ? 1 : 2;
                Projectile.extraUpdates = 0;
                Projectile.timeLeft = Variant == UpgradeAttackVariant.Construct ? 20 : 24;
                Projectile.tileCollide = false;
                Projectile.localNPCHitCooldown = 12;
                break;
            case UpgradeTechProfile.Magic:
                Projectile.scale = Variant == UpgradeAttackVariant.Primary ? 1.08f : 1.28f;
                Projectile.penetrate = FullyIntegrated ? 4 : 3;
                Projectile.extraUpdates = 1;
                Projectile.timeLeft = Variant == UpgradeAttackVariant.Construct ? 86 : 98;
                Projectile.localNPCHitCooldown = 10;
                break;
            case UpgradeTechProfile.Summon:
                Projectile.scale = Variant == UpgradeAttackVariant.Primary ? 0.96f : 1.08f;
                Projectile.penetrate = Variant == UpgradeAttackVariant.Construct ? 2 : 3;
                Projectile.extraUpdates = 1;
                Projectile.timeLeft = 94;
                break;
            case UpgradeTechProfile.Ranged:
                Projectile.scale = Variant == UpgradeAttackVariant.Construct ? 0.94f : 1f;
                Projectile.penetrate = Variant == UpgradeAttackVariant.Primary ? 2 : 3;
                Projectile.extraUpdates = 2;
                Projectile.timeLeft = Variant == UpgradeAttackVariant.Construct ? 74 : 84;
                break;
            default:
                Projectile.scale = 1f;
                Projectile.penetrate = FullyIntegrated ? 3 : 2;
                Projectile.extraUpdates = 2;
                break;
        }

        if (Overclocked)
            Projectile.scale *= 1.05f;
    }

    private void ApplyMeleeMotion() {
        Projectile.velocity *= Variant == UpgradeAttackVariant.Construct ? 0.94f : 0.91f;
        if (Projectile.velocity.LengthSquared() < 10f)
            Projectile.velocity *= 1.08f;
    }

    private void ApplyHoming(float homingStrength, float maxDistance) {
        NPC target = FindTarget(maxDistance);
        if (target == null)
            return;

        float speed = Projectile.velocity.Length();
        if (speed <= 0.01f)
            speed = FullyIntegrated ? 17f : 15f;

        Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity) * speed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
    }

    private NPC FindTarget(float maxDistance) {
        NPC bestTarget = null;
        float bestDistanceSquared = maxDistance * maxDistance;
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy())
                continue;

            float distanceSquared = Vector2.DistanceSquared(Projectile.Center, npc.Center);
            if (distanceSquared >= bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            bestTarget = npc;
        }

        return bestTarget;
    }
}
