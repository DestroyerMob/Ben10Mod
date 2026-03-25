using System;
using Ben10Mod.Content.Transformations.Upgrade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UpgradeIntegrationMatrixProjectile : ModProjectile {
    private const int NodeCount = 3;

    public override string Texture => "Terraria/Images/Projectile_0";

    private UpgradeTechProfile Profile => (UpgradeTechProfile)Utils.Clamp((int)Math.Round(Projectile.ai[0]), 0, 4);

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.hide = true;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (!omp.IsUltimateAbilityActive || !omp.IsTransformed || omp.currentTransformationId != "Ben10Mod:Upgrade") {
            Projectile.Kill();
            return;
        }

        UpgradeTechPlayer techPlayer = owner.GetModPlayer<UpgradeTechPlayer>();
        Projectile.ai[0] = (float)techPlayer.ActiveTechProfile;
        Projectile.Center = owner.MountedCenter;
        Projectile.timeLeft = 2;
        Projectile.localAI[0]++;

        bool overclocked = omp.PrimaryAbilityEnabled;
        if (Projectile.localAI[1] > 0f)
            Projectile.localAI[1]--;

        if (Projectile.owner == Main.myPlayer && Projectile.localAI[1] <= 0f) {
            FireMatrixVolley(owner, overclocked);
            Projectile.localAI[1] = ResolveFireDelay(overclocked);
            Projectile.netUpdate = true;
        }

        Color profileColor = UpgradeTransformation.GetTechColor(Profile);
        Lighting.AddLight(owner.Center, profileColor.ToVector3() * 0.0062f);

        if (!Main.dedServ && Main.rand.NextBool(3)) {
            Vector2 nodePosition = GetNodePosition((int)(Main.GameUpdateCount % NodeCount), owner);
            Dust dust = Dust.NewDustPerfect(nodePosition, Main.rand.NextBool() ? DustID.Electric : DustID.GreenTorch,
                Main.rand.NextVector2Circular(0.5f, 0.5f), 95, profileColor, Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return false;

        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Color profileColor = UpgradeTransformation.GetTechColor(Profile);
        Vector2 center = owner.MountedCenter - Main.screenPosition;

        DrawRing(pixel, center, ResolveNodeRadius() * 0.78f, 3.4f, profileColor * 0.55f, -Projectile.localAI[0] * 0.03f);
        DrawRing(pixel, center, ResolveNodeRadius() * 1.04f, 4.2f, Color.Lerp(profileColor, Color.White, 0.2f) * 0.72f,
            Projectile.localAI[0] * 0.035f);

        for (int i = 0; i < NodeCount; i++) {
            Vector2 nodePosition = GetNodePosition(i, owner) - Main.screenPosition;
            Main.EntitySpriteDraw(pixel, nodePosition, null, profileColor * 0.95f, 0f, Vector2.One * 0.5f,
                new Vector2(12f, 12f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(pixel, nodePosition, null, Color.White * 0.8f, 0f, Vector2.One * 0.5f,
                new Vector2(5f, 5f), SpriteEffects.None, 0);
        }

        return false;
    }

    private void FireMatrixVolley(Player owner, bool overclocked) {
        switch (Profile) {
            case UpgradeTechProfile.Melee:
                FireSharedTargetVolley(owner, 260f, overclocked);
                break;
            case UpgradeTechProfile.Ranged:
                FireSharedTargetVolley(owner, 760f, overclocked);
                break;
            case UpgradeTechProfile.Magic:
                FireSharedTargetVolley(owner, 700f, overclocked);
                break;
            case UpgradeTechProfile.Summon:
                FireDistributedSummonVolley(owner, overclocked);
                break;
            default:
                FireSharedTargetVolley(owner, 700f, overclocked);
                break;
        }
    }

    private void FireSharedTargetVolley(Player owner, float maxDistance, bool overclocked) {
        NPC target = FindTarget(owner.Center, maxDistance);
        if (target == null)
            return;

        for (int i = 0; i < NodeCount; i++) {
            Vector2 nodePosition = GetNodePosition(i, owner);
            Vector2 direction = (target.Center - nodePosition).SafeNormalize(Vector2.UnitX * owner.direction);
            UpgradeTransformation.FireAdaptiveShot(owner, Projectile.GetSource_FromThis(), direction, Projectile.damage, 2.5f,
                Profile, UpgradeAttackVariant.Special, overclocked, true, nodePosition);
        }
    }

    private void FireDistributedSummonVolley(Player owner, bool overclocked) {
        bool firedAny = false;
        for (int i = 0; i < NodeCount; i++) {
            Vector2 nodePosition = GetNodePosition(i, owner);
            NPC target = FindTarget(nodePosition, 820f);
            if (target == null)
                continue;

            Vector2 direction = (target.Center - nodePosition).SafeNormalize(Vector2.UnitX * owner.direction);
            UpgradeTransformation.FireAdaptiveShot(owner, Projectile.GetSource_FromThis(), direction, Projectile.damage, 2f,
                Profile, UpgradeAttackVariant.Special, overclocked, true, nodePosition);
            firedAny = true;
        }

        if (!firedAny) {
            NPC fallbackTarget = FindTarget(owner.Center, 820f);
            if (fallbackTarget == null)
                return;

            for (int i = 0; i < NodeCount; i++) {
                Vector2 nodePosition = GetNodePosition(i, owner);
                Vector2 direction = (fallbackTarget.Center - nodePosition).SafeNormalize(Vector2.UnitX * owner.direction);
                UpgradeTransformation.FireAdaptiveShot(owner, Projectile.GetSource_FromThis(), direction, Projectile.damage, 2f,
                    Profile, UpgradeAttackVariant.Special, overclocked, true, nodePosition);
            }
        }
    }

    private int ResolveFireDelay(bool overclocked) {
        int delay = Profile switch {
            UpgradeTechProfile.Melee => 18,
            UpgradeTechProfile.Ranged => 16,
            UpgradeTechProfile.Magic => 20,
            UpgradeTechProfile.Summon => 14,
            _ => 18
        };

        if (overclocked)
            delay -= 3;

        return Math.Max(8, delay);
    }

    private float ResolveNodeRadius() {
        return Profile switch {
            UpgradeTechProfile.Melee => 40f,
            UpgradeTechProfile.Ranged => 48f,
            UpgradeTechProfile.Magic => 54f,
            UpgradeTechProfile.Summon => 58f,
            _ => 46f
        };
    }

    private Vector2 GetNodePosition(int index, Player owner) {
        float baseAngle = Projectile.localAI[0] * 0.045f + MathHelper.TwoPi * index / NodeCount;
        float radius = ResolveNodeRadius() + (float)Math.Sin((Projectile.localAI[0] + index * 7f) * 0.07f) * 4f;
        return owner.MountedCenter + baseAngle.ToRotationVector2() * radius;
    }

    private static NPC FindTarget(Vector2 origin, float maxDistance) {
        NPC bestTarget = null;
        float bestDistanceSquared = maxDistance * maxDistance;
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy())
                continue;

            float distanceSquared = Vector2.DistanceSquared(origin, npc.Center);
            if (distanceSquared >= bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            bestTarget = npc;
        }

        return bestTarget;
    }

    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color,
        float rotationOffset) {
        const int Segments = 28;
        for (int i = 0; i < Segments; i++) {
            float angle = rotationOffset + MathHelper.TwoPi * i / Segments;
            Vector2 position = center + angle.ToRotationVector2() * radius;
            Main.EntitySpriteDraw(pixel, position, null, color, angle, Vector2.One * 0.5f,
                new Vector2(thickness, thickness * 1.85f), SpriteEffects.None, 0);
        }
    }
}
