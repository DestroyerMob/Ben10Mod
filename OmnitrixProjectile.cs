using System;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Ben10Mod;

public class OmnitrixProjectile : GlobalProjectile {
    private const float TemporalFreezeRampFrames = 45f;

    public override bool InstancePerEntity => true;

    public  int     itemUsed         = 0;
    private int     framesAlive      = 0;
    public  bool    projectileSlowed = false;
    public  Vector2 initialVelocity  = Vector2.Zero;
    private bool    syncScaleHitbox  = false;
    private int     baseWidth        = 0;
    private int     baseHeight       = 0;
    private float   temporalFreezeProgress = 0f;
    
    public override void OnSpawn(Projectile projectile, IEntitySource source) {
        if (source is IEntitySource_WithStatsFromItem itemSource) {
            itemUsed        = itemSource.Item.type;
            initialVelocity = projectile.velocity;
        }
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
        // if (itemUsed == ModContent.ItemType<PlumberMagisterBadge>())
        //     if (target.life / (float)target.lifeMax >= 0.9f) {
        //         modifiers.FinalDamage *= 1.5f;
        //     }
    }

    public override void AI(Projectile projectile) {
        framesAlive++;
        if (syncScaleHitbox)
            ApplyScaledHitbox(projectile);
        HandleTemporalFreeze(projectile);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
        // if (itemUsed == ModContent.ItemType<PlumberMagisterBadge>())
        //     target.AddBuff(BuffID.OnFire, 120);
        if (itemUsed == ModContent.ItemType<HeavenlyCrystallineBadge>()) {
            if (!Main.rand.NextBool(3)) return;
            for (int i = 0; i < 3; i++) {
                Vector2 spawnPos = target.Center + new Vector2(Main.rand.NextFloat(-200f, 201f), -620f);
                Vector2 vel      = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * 17.5f;
                int projNum = Projectile.NewProjectile(projectile.GetSource_FromThis(),
                    spawnPos,
                    vel, ProjectileID.QueenSlimeGelAttack,
                    damageDone / 3, 0);
                Main.projectile[projNum].hostile  = false;
                Main.projectile[projNum].friendly = true;
            }
        }
    }

    public void EnableScaleHitboxSync(Projectile projectile) {
        if (!syncScaleHitbox) {
            syncScaleHitbox = true;
            baseWidth = projectile.width;
            baseHeight = projectile.height;
        }

        ApplyScaledHitbox(projectile);
    }

    private void ApplyScaledHitbox(Projectile projectile) {
        float effectiveScale = Math.Max(projectile.scale, 0.01f);
        int scaledWidth = Math.Max(1, (int)Math.Round(baseWidth * effectiveScale));
        int scaledHeight = Math.Max(1, (int)Math.Round(baseHeight * effectiveScale));

        if (projectile.width == scaledWidth && projectile.height == scaledHeight)
            return;

        Vector2 center = projectile.Center;
        projectile.width = scaledWidth;
        projectile.height = scaledHeight;
        projectile.Center = center;
    }

    private void HandleTemporalFreeze(Projectile projectile) {
        bool shouldFreeze = ShouldFreezeProjectile(projectile);
        if (shouldFreeze) {
            if (!projectileSlowed) {
                projectileSlowed = true;
                temporalFreezeProgress = 0f;
                initialVelocity = projectile.velocity;
            }

            if (initialVelocity == Vector2.Zero && projectile.velocity != Vector2.Zero)
                initialVelocity = projectile.velocity;

            temporalFreezeProgress = Math.Min(1f, temporalFreezeProgress + 1f / TemporalFreezeRampFrames);
            Vector2 startVelocity = initialVelocity == Vector2.Zero ? projectile.velocity : initialVelocity;
            projectile.velocity = Vector2.Lerp(startVelocity, Vector2.Zero, temporalFreezeProgress);

            if (temporalFreezeProgress >= 1f || projectile.velocity.LengthSquared() < 0.0025f)
                projectile.velocity = Vector2.Zero;

            return;
        }

        if (!projectileSlowed)
            return;

        if (initialVelocity != Vector2.Zero)
            projectile.velocity = initialVelocity * 2f;

        projectileSlowed = false;
        temporalFreezeProgress = 0f;
        initialVelocity = Vector2.Zero;
    }

    private static bool ShouldFreezeProjectile(Projectile projectile) {
        if (!IsXLR8UltimateActive())
            return false;

        if (!projectile.active || projectile.type == ProjectileID.None)
            return false;

        if (projectile.minion || projectile.sentry)
            return false;

        return projectile.velocity.LengthSquared() > 0.0025f || projectile.GetGlobalProjectile<OmnitrixProjectile>().projectileSlowed;
    }

    private static bool IsXLR8UltimateActive() {
        Player player = Main.LocalPlayer;
        if (player == null || !player.active)
            return false;

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        return omp.IsUltimateAbilityActive && omp.currentTransformationId == "Ben10Mod:XLR8";
    }
}
