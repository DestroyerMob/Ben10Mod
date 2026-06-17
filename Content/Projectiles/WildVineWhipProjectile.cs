using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WildVineWhipProjectile : ModProjectile {
    private static readonly Rectangle HandleFrame = new(0, 0, 22, 24);
    private static readonly Rectangle[] BodyFrames = {
        new(0, 34, 22, 18),
        new(0, 62, 22, 18),
        new(0, 90, 22, 18)
    };
    private static readonly Rectangle TipFrame = new(0, 112, 22, 28);

    public override string Texture => "Ben10Mod/Content/Projectiles/WildVineWhipProjectile";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.CloneDefaults(ProjectileID.ThornWhip);
        AIType = ProjectileID.ThornWhip;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.WhipSettings.Segments = 24;
        Projectile.WhipSettings.RangeMultiplier = 1.4f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        Vector2 destination = WildVineAnchorProjectile.ResolveControlPoint(owner, target.Center, 430f);
        Vector2 pullDirection = target.Center.DirectionTo(destination).SafeNormalize(Vector2.Zero);
        bool lightEnemy = !target.boss && target.knockBackResist >= 0.45f && target.lifeMax < 5000;

        target.AddBuff(ModContent.BuffType<WildVineTethered>(), lightEnemy ? 75 : 120);
        if (lightEnemy) {
            float pullSpeed = MathHelper.Lerp(5.5f, 9.5f, MathHelper.Clamp(target.knockBackResist, 0f, 1f));
            target.velocity = Vector2.Lerp(target.velocity, pullDirection * pullSpeed, 0.55f);
        }
        else {
            target.velocity *= target.boss ? 0.86f : 0.72f;
            target.velocity += pullDirection * (target.boss ? 0.45f : 1.5f);
        }

        target.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        List<Vector2> controlPoints = new();
        Projectile.FillWhipControlPoints(Projectile, controlPoints);
        DrawCustomWhip(controlPoints);
        return false;
    }

    private void DrawCustomWhip(List<Vector2> controlPoints) {
        if (controlPoints.Count < 2)
            return;

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        for (int i = 0; i < controlPoints.Count - 1; i++) {
            Vector2 start = controlPoints[i];
            Vector2 end = controlPoints[i + 1];
            Vector2 segmentVector = end - start;
            float segmentLength = segmentVector.Length();
            if (segmentLength <= 0.001f)
                continue;

            Rectangle sourceRectangle = GetSourceRectangle(i, controlPoints.Count - 2);
            float rotation = segmentVector.ToRotation() - MathHelper.PiOver2;
            Color color = Projectile.GetAlpha(Lighting.GetColor((int)(start.X / 16f), (int)(start.Y / 16f)));
            Vector2 origin = new(sourceRectangle.Width * 0.5f, 0f);
            Vector2 scale = new(1f, segmentLength / sourceRectangle.Height);

            Main.EntitySpriteDraw(texture, start - Main.screenPosition, sourceRectangle, color, rotation, origin, scale,
                SpriteEffects.None, 0);
        }
    }

    private static Rectangle GetSourceRectangle(int segmentIndex, int lastSegmentIndex) {
        if (segmentIndex <= 0)
            return HandleFrame;

        if (segmentIndex >= lastSegmentIndex)
            return TipFrame;

        return BodyFrames[(segmentIndex - 1) % BodyFrames.Length];
    }
}
