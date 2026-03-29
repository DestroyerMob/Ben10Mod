using System.Collections.Generic;
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
        Projectile.WhipSettings.Segments = 20;
        Projectile.WhipSettings.RangeMultiplier = 1.15f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Poisoned, 4 * 60);
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
