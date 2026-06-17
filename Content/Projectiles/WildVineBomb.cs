using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WildVineBomb : ModProjectile {
    public const float Gravity = 0.24f;
    public const float VariantRegular = 0f;
    public const float VariantBloom = 1f;

    private bool IsBloomVariant => Projectile.ai[0] >= VariantBloom;
    private bool LandedOnTiles {
        get => Projectile.localAI[0] > 0f;
        set => Projectile.localAI[0] = value ? 1f : 0f;
    }

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 105;
        Projectile.penetrate = 1;
        Projectile.aiStyle = -1;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + Gravity, IsBloomVariant ? 12.5f : 10.5f);
        Projectile.velocity.X *= Projectile.velocity.Y > 0f ? 0.994f : 0.997f;

        if (Projectile.velocity.X != 0f) {
            Projectile.direction = Projectile.velocity.X > 0f ? 1 : -1;
            Projectile.spriteDirection = Projectile.direction;
        }

        Projectile.rotation += (Math.Abs(Projectile.velocity.X) + 0.15f) * 0.12f * Projectile.direction;
        Lighting.AddLight(Projectile.Center, IsBloomVariant
            ? new Vector3(0.14f, 0.24f, 0.09f)
            : new Vector3(0.08f, 0.16f, 0.05f));

        int dustChance = IsBloomVariant ? 1 : 2;
        if (Main.rand.NextBool(dustChance)) {
            Dust seedDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                Main.rand.NextBool(3) ? DustID.JunglePlants : DustID.Poisoned,
                -Projectile.velocity * Main.rand.NextFloat(0.035f, 0.07f),
                100, IsBloomVariant ? new Color(160, 225, 115) : new Color(140, 205, 95),
                Main.rand.NextFloat(0.95f, 1.18f));
            seedDust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Poisoned, IsBloomVariant ? 4 * 60 : 2 * 60);
        if (IsBloomVariant)
            target.AddBuff(ModContent.BuffType<WildVineTethered>(), 2 * 60);

        Projectile.Kill();
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        LandedOnTiles = true;
        Projectile.velocity = oldVelocity * 0.25f;
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);

        if (Projectile.owner == Main.myPlayer || Main.netMode != NetmodeID.MultiplayerClient) {
            float cloudMultiplier = IsBloomVariant ? 0.82f : 0.6f;
            int cloudDamage = Math.Max(1, (int)Math.Round(Projectile.damage * cloudMultiplier));
            if (LandedOnTiles) {
                int anchorDamage = Math.Max(1, (int)Math.Round(Projectile.damage * (IsBloomVariant ? 0.55f : 0.38f)));
                WildVineAnchorProjectile.CreateOrRefresh(Projectile.GetSource_FromThis(), Projectile.Bottom + new Vector2(0f, -8f),
                    anchorDamage, Projectile.owner, IsBloomVariant ? WildVineAnchorProjectile.ModeBloom : WildVineAnchorProjectile.ModeSeed,
                    IsBloomVariant ? 7 * 60 : WildVineAnchorProjectile.BaseLifetime, IsBloomVariant ? 1.18f : 1f);
            }

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<WildVineGasCloudProjectile>(), cloudDamage, IsBloomVariant ? 0.5f : 0f,
                Projectile.owner, Projectile.ai[0], LandedOnTiles ? 1f : 0f);
        }

        int burstCount = IsBloomVariant ? 28 : 18;
        for (int i = 0; i < burstCount; i++) {
            Vector2 velocity = Main.rand.NextVector2Circular(IsBloomVariant ? 3.5f : 2.6f, IsBloomVariant ? 3.5f : 2.6f);
            Dust burst = Dust.NewDustPerfect(Projectile.Center,
                i % 3 == 0 ? DustID.Poisoned : DustID.Grass,
                velocity, 95, IsBloomVariant ? new Color(170, 235, 120) : new Color(155, 225, 115),
                Main.rand.NextFloat(0.95f, 1.25f));
            burst.noGravity = true;
        }

        int sporeCount = IsBloomVariant ? 16 : 10;
        for (int i = 0; i < sporeCount; i++) {
            Dust spores = Dust.NewDustPerfect(Projectile.Center, DustID.JunglePlants,
                Main.rand.NextVector2Circular(IsBloomVariant ? 2.4f : 1.8f, IsBloomVariant ? 2.4f : 1.8f),
                110, new Color(110, 180, 80), Main.rand.NextFloat(0.85f, 1.05f));
            spores.noGravity = true;
        }
    }
}
