using System;
using Ben10Mod.Content.Items.Armour;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Ben10Mod;

public readonly record struct MagistrataOutlineDrawData(
    Texture2D Texture,
    Vector2 DrawPosition,
    Rectangle? SourceRectangle,
    Vector2 Origin,
    float Rotation,
    float Scale,
    SpriteEffects Effects
);

public interface IMagistrataOutlineProvider {
    bool TryGetMagistrataOutlineDrawData(out MagistrataOutlineDrawData drawData);
}

public class OmnitrixProjectile : GlobalProjectile {
    private const float TemporalFreezeRampFrames = 45f;
    private static readonly string[] TexturelessProjectilePaths = {
        "Terraria/Images/Projectile_0",
        "Terraria/Images/Projectile_-1"
    };

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

        if (ShouldApplyMagistrataVisuals(projectile) && IsDustOnlyProjectile(projectile))
            SpawnMagistrataDustRing(projectile);
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

    public override void PostDraw(Projectile projectile, Color lightColor) {
        if (!ShouldDrawMagistrataOutline(projectile))
            return;

        if (!TryGetMagistrataOutlineDrawData(projectile, out MagistrataOutlineDrawData drawData))
            return;

        Color outerColor = new Color(70, 255, 110, 220) * 0.9f;
        Color innerColor = new Color(150, 255, 175, 180) * 0.75f;
        float outerOffset = Math.Max(3.5f, projectile.scale * 4f);
        float innerOffset = Math.Max(1.8f, projectile.scale * 2.2f);

        for (int i = 0; i < 12; i++) {
            Vector2 offset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * outerOffset;
            Main.EntitySpriteDraw(drawData.Texture, drawData.DrawPosition + offset, drawData.SourceRectangle, outerColor,
                drawData.Rotation, drawData.Origin, drawData.Scale, drawData.Effects, 0);
        }

        for (int i = 0; i < 8; i++) {
            Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * innerOffset;
            Main.EntitySpriteDraw(drawData.Texture, drawData.DrawPosition + offset, drawData.SourceRectangle, innerColor,
                drawData.Rotation, drawData.Origin, drawData.Scale, drawData.Effects, 0);
        }

        Main.EntitySpriteDraw(drawData.Texture, drawData.DrawPosition, drawData.SourceRectangle, new Color(95, 255, 120, 55),
            drawData.Rotation, drawData.Origin, drawData.Scale * 1.01f, drawData.Effects, 0);
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

    private static bool ShouldDrawMagistrataOutline(Projectile projectile) {
        if (!ShouldApplyMagistrataVisuals(projectile) || IsDustOnlyProjectile(projectile))
            return false;

        return true;
    }

    private static bool ShouldApplyMagistrataVisuals(Projectile projectile) {
        if (!projectile.active || !projectile.friendly || projectile.hostile || projectile.Opacity <= 0f)
            return false;

        if (!projectile.CountsAsClass(ModContent.GetInstance<HeroDamage>()))
            return false;

        if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            return false;

        Player owner = Main.player[projectile.owner];
        if (!owner.active)
            return false;

        return owner.GetModPlayer<HeroPlumberArmorPlayer>().IsMagistrataEffectActive();
    }

    private static bool IsDustOnlyProjectile(Projectile projectile) {
        string texturePath = GetProjectileTexturePath(projectile);
        if (string.IsNullOrEmpty(texturePath))
            return true;

        foreach (string texturelessPath in TexturelessProjectilePaths) {
            if (string.Equals(texturePath, texturelessPath, StringComparison.Ordinal))
                return true;
        }

        return projectile.hide && string.Equals(texturePath, $"Terraria/Images/Projectile_{ProjectileID.None}", StringComparison.Ordinal);
    }

    private static string GetProjectileTexturePath(Projectile projectile) {
        if (!string.IsNullOrEmpty(projectile.ModProjectile?.Texture))
            return projectile.ModProjectile.Texture;

        if (projectile.type >= 0)
            return $"Terraria/Images/Projectile_{projectile.type}";

        return string.Empty;
    }

    private static bool TryGetMagistrataOutlineDrawData(Projectile projectile, out MagistrataOutlineDrawData drawData) {
        if (projectile.ModProjectile is IMagistrataOutlineProvider provider &&
            provider.TryGetMagistrataOutlineDrawData(out drawData))
            return true;

        Texture2D texture = TextureAssets.Projectile[projectile.type].Value;
        if (texture is null)
        {
            drawData = default;
            return false;
        }

        int frameCount = Main.projFrames[projectile.type] <= 0 ? 1 : Main.projFrames[projectile.type];
        Rectangle frame = texture.Frame(1, frameCount, 0, projectile.frame);
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 drawPosition = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
        SpriteEffects effects = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        drawData = new MagistrataOutlineDrawData(texture, drawPosition, frame, origin, projectile.rotation, projectile.scale, effects);
        return true;
    }

    private void SpawnMagistrataDustRing(Projectile projectile) {
        if (Main.dedServ || framesAlive % 4 != 0)
            return;

        float radiusX = Math.Max(6f, projectile.width * projectile.scale * 0.5f);
        float radiusY = Math.Max(6f, projectile.height * projectile.scale * 0.5f);
        int points = Math.Clamp((int)Math.Round((radiusX + radiusY) / 10f), 8, 18);
        float rotation = Main.GlobalTimeWrappedHourly * 1.8f + projectile.identity * 0.13f;

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 unit = angle.ToRotationVector2();
            Vector2 offset = new Vector2(unit.X * radiusX, unit.Y * radiusY);
            Vector2 velocity = unit * 0.18f + projectile.velocity * 0.03f;
            Dust dust = Dust.NewDustPerfect(projectile.Center + offset, DustID.GreenTorch, velocity, 90,
                new Color(110, 255, 135), 1.12f);
            dust.noGravity = true;
        }
    }
}
