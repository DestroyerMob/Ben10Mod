using System;
using Ben10Mod.Content.Transformations.Upgrade;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UpgradeConstructProjectile : ModProjectile {
    public const int BaseLifetimeTicks = 14 * 60;

    public override string Texture => $"Terraria/Images/NPC_{NPCID.Probe}";

    private int TechLevel => Utils.Clamp((int)Math.Round(Projectile.ai[0]), 0, 4);
    private UpgradeTechProfile Profile => (UpgradeTechProfile)Utils.Clamp((int)Math.Round(Projectile.ai[1]), 0, 4);

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = BaseLifetimeTicks;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        bool overclocked = omp.PrimaryAbilityEnabled;
        bool fullyIntegrated = omp.IsUltimateAbilityActive;

        float bob = (float)Math.Sin((Main.GameUpdateCount + Projectile.whoAmI * 11) * 0.08f) * 6f;
        Vector2 desiredPosition = Profile switch {
            UpgradeTechProfile.Melee => owner.MountedCenter + new Vector2(owner.direction * 20f, -18f + bob * 0.55f),
            UpgradeTechProfile.Ranged => owner.MountedCenter + new Vector2(owner.direction * 56f, -40f + bob),
            UpgradeTechProfile.Magic => owner.MountedCenter + new Vector2(owner.direction * 42f, -58f + bob * 1.2f),
            UpgradeTechProfile.Summon => owner.MountedCenter + new Vector2(owner.direction * 64f, -52f + bob * 1.1f),
            _ => owner.MountedCenter + new Vector2(owner.direction * 46f, -42f + bob)
        };
        Projectile.Center = Vector2.Lerp(Projectile.Center, desiredPosition, 0.16f);
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = (owner.Center - Projectile.Center).X * 0.012f;
        Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f + TechLevel * 0.05f + (fullyIntegrated ? 0.12f : 0f), 0.14f);

        int baseFireDelay = Profile switch {
            UpgradeTechProfile.Melee => 34,
            UpgradeTechProfile.Ranged => 38,
            UpgradeTechProfile.Magic => 32,
            UpgradeTechProfile.Summon => 26,
            _ => 40
        };
        int fireDelay = Math.Max(10, baseFireDelay - TechLevel * 3 - (overclocked ? 4 : 0) - (fullyIntegrated ? 6 : 0));
        if (Projectile.localAI[0] > 0f)
            Projectile.localAI[0]--;

        NPC target = FindTarget(Projectile.Center, Profile switch {
            UpgradeTechProfile.Melee => 300f,
            UpgradeTechProfile.Summon => fullyIntegrated ? 720f : 620f,
            UpgradeTechProfile.Magic => fullyIntegrated ? 660f : 560f,
            _ => fullyIntegrated ? 620f : 500f
        });
        if (target != null && Projectile.owner == Main.myPlayer && Projectile.localAI[0] <= 0f) {
            Vector2 fireDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX * owner.direction);

            if (Profile == UpgradeTechProfile.Summon && fullyIntegrated) {
                for (int i = 0; i < 2; i++) {
                    float offset = i == 0 ? -0.08f : 0.08f;
                    UpgradeTransformation.FireAdaptiveShot(owner, Projectile.GetSource_FromThis(), fireDirection.RotatedBy(offset),
                        Projectile.damage, Projectile.knockBack, Profile, UpgradeAttackVariant.Construct, overclocked, fullyIntegrated,
                        Projectile.Center);
                }
            }
            else {
                UpgradeTransformation.FireAdaptiveShot(owner, Projectile.GetSource_FromThis(), fireDirection,
                    Projectile.damage, Projectile.knockBack, Profile, UpgradeAttackVariant.Construct, overclocked, fullyIntegrated,
                    Projectile.Center);
            }

            Projectile.localAI[0] = fireDelay;
            Projectile.netUpdate = true;
        }

        Color profileColor = UpgradeTransformation.GetTechColor(Profile);
        Lighting.AddLight(Projectile.Center, profileColor.ToVector3() * (fullyIntegrated ? 0.0042f : 0.003f));
        if (!Main.dedServ && Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                Main.rand.NextBool() ? DustID.Electric : DustID.GreenTorch,
                Main.rand.NextVector2Circular(0.7f, 0.7f), 95, profileColor, Main.rand.NextFloat(0.85f, 1.12f));
            dust.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        UpgradeTransformation.SpawnAssimilationDust(Projectile.Center, 10, UpgradeTransformation.GetTechColor(Profile));
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
}
