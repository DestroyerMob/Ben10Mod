using System;
using Ben10Mod.Content.DamageClasses;
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
    private Vector2 temporalFreezeResumeVelocity = Vector2.Zero;
    private float   temporalFreezeRotation = 0f;
    private int     temporalFreezeDirection = 1;
    private int     temporalFreezeSpriteDirection = 1;
    
    public override void OnSpawn(Projectile projectile, IEntitySource source) {
        if (source is IEntitySource_WithStatsFromItem itemSource) {
            itemUsed        = itemSource.Item.type;
            initialVelocity = projectile.velocity;
        }
        else if (source is EntitySource_Parent { Entity: Projectile parentProjectile }) {
            itemUsed = parentProjectile.GetGlobalProjectile<OmnitrixProjectile>().itemUsed;
            initialVelocity = projectile.velocity;
        }

        ApplyTransformationDamageType(projectile, source);
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

    public override void PostAI(Projectile projectile) {
        if (!projectileSlowed)
            return;

        if (projectile.velocity.LengthSquared() > 0.0025f) {
            CaptureFrozenFacing(projectile);
            return;
        }

        ApplyFrozenFacing(projectile);
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
                temporalFreezeResumeVelocity = projectile.velocity;
                CaptureFrozenFacing(projectile);
                projectile.netUpdate = true;
            }

            if (temporalFreezeResumeVelocity == Vector2.Zero && projectile.velocity != Vector2.Zero)
                temporalFreezeResumeVelocity = projectile.velocity;

            temporalFreezeProgress = Math.Min(1f, temporalFreezeProgress + 1f / TemporalFreezeRampFrames);
            Vector2 startVelocity = temporalFreezeResumeVelocity == Vector2.Zero
                ? projectile.velocity
                : temporalFreezeResumeVelocity;
            projectile.velocity = Vector2.Lerp(startVelocity, Vector2.Zero, temporalFreezeProgress);

            if (temporalFreezeProgress >= 1f || projectile.velocity.LengthSquared() < 0.0025f)
                projectile.velocity = Vector2.Zero;

            return;
        }

        if (!projectileSlowed)
            return;

        if (temporalFreezeResumeVelocity != Vector2.Zero)
            projectile.velocity = temporalFreezeResumeVelocity * 2f;

        projectileSlowed = false;
        temporalFreezeProgress = 0f;
        temporalFreezeResumeVelocity = Vector2.Zero;
        projectile.netUpdate = true;
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
        foreach (Player player in Main.ActivePlayers) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            if (omp.IsUltimateAbilityActive && omp.currentTransformationId == "Ben10Mod:XLR8")
                return true;
        }

        return false;
    }

    private void CaptureFrozenFacing(Projectile projectile) {
        temporalFreezeRotation = projectile.rotation;
        temporalFreezeDirection = projectile.direction == 0 ? (projectile.velocity.X >= 0f ? 1 : -1) : projectile.direction;
        temporalFreezeSpriteDirection = projectile.spriteDirection == 0
            ? temporalFreezeDirection
            : projectile.spriteDirection;
    }

    private void ApplyFrozenFacing(Projectile projectile) {
        projectile.rotation = temporalFreezeRotation;
        projectile.direction = temporalFreezeDirection;
        projectile.spriteDirection = temporalFreezeSpriteDirection;
    }

    private static void ApplyTransformationDamageType(Projectile projectile, IEntitySource source) {
        if (!projectile.friendly || projectile.hostile)
            return;

        if (!ShouldUseHeroDamage(projectile, source))
            return;

        projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    private static bool ShouldUseHeroDamage(Projectile projectile, IEntitySource source) {
        if (source is IEntitySource_WithStatsFromItem itemSource && itemSource.Item.ModItem is PlumbersBadge)
            return true;

        if (source is EntitySource_Parent parentSource) {
            if (parentSource.Entity is Projectile parentProjectile) {
                if (parentProjectile.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
                    return true;

                if (parentProjectile.GetGlobalProjectile<OmnitrixProjectile>().itemUsed != 0)
                    return true;
            }

            if (parentSource.Entity is Player sourcePlayer &&
                sourcePlayer.GetModPlayer<OmnitrixPlayer>().IsTransformed &&
                projectile.ModProjectile?.Mod?.Name == "Ben10Mod")
                return true;
        }

        if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers) {
            Player owner = Main.player[projectile.owner];
            if (owner.active &&
                owner.GetModPlayer<OmnitrixPlayer>().IsTransformed &&
                projectile.ModProjectile?.Mod?.Name == "Ben10Mod")
                return true;
        }

        return false;
    }
}
