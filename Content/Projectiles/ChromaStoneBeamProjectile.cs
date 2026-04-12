using System;
using System.IO;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.ChromaStone;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneBeamProjectile : ModProjectile {
    private Vector2 syncedAimDirection = Vector2.UnitX;
    private bool hasSyncedAimDirection;
    private int aimSyncTimer;
    private int facetConsumeTimer;

    private float BeamHitLength {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    private float BeamDrawLength {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = VanillaBeamDrawHelper.LastPrismFrameCount;
    }

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.timeLeft = 2;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(syncedAimDirection.X);
        writer.Write(syncedAimDirection.Y);
        writer.Write(hasSyncedAimDirection);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        syncedAimDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        hasSyncedAimDirection = reader.ReadBoolean();
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        ChromaStoneStatePlayer state = owner.GetModPlayer<ChromaStoneStatePlayer>();

        if (!ShouldStayAlive(owner, omp, state)) {
            Projectile.Kill();
            return;
        }

        if (owner.whoAmI == Main.myPlayer)
            state.TryConsumeFacetForBeam(ref facetConsumeTimer);

        Projectile.timeLeft = 2;
        Projectile.localNPCHitCooldown = Math.Max(6, 11 - state.VisibleFacetCount * 2);

        Vector2 direction = GetAimDirection(owner);
        Projectile.velocity = direction;
        Projectile.rotation = direction.ToRotation();

        Vector2 start = GetBeamStart(owner, direction);
        Projectile.Center = start;
        BeamHitLength = GetBeamLength(start, direction, state);
        BeamDrawLength = Math.Max(18f, BeamHitLength - 8f);

        owner.ChangeDir(direction.X >= 0f ? 1 : -1);
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;
        owner.itemRotation = (float)Math.Atan2(direction.Y * owner.direction, direction.X * owner.direction);

        float powerRatio = state.FacetPowerRatio;
        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(powerRatio * 2.4f + 0.2f, 1.08f);
        Lighting.AddLight(start + direction * Math.Min(BeamDrawLength * 0.35f, 180f), prismColor.ToVector3() * 0.64f);

        if (!Main.dedServ && Main.rand.NextBool(state.VisibleFacetCount > 0 ? 1 : 2)) {
            Vector2 dustPosition = start + direction * Main.rand.NextFloat(14f, Math.Max(18f, BeamDrawLength * 0.85f));
            Dust dust = Dust.NewDustPerfect(dustPosition + Main.rand.NextVector2Circular(10f, 10f), DustID.GemDiamond,
                direction.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.4f, 2f), 95, prismColor,
                Main.rand.NextFloat(0.9f, 1.24f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        ChromaStoneStatePlayer state = owner.GetModPlayer<ChromaStoneStatePlayer>();
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f));
        Vector2 start = GetBeamStart(owner, direction);
        float collisionPoint = 0f;

        if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start,
                start + direction * BeamHitLength, GetMainBeamThickness(state), ref collisionPoint)) {
            return true;
        }

        int refractionCount = GetRefractionCount(state);
        if (refractionCount <= 0)
            return false;

        for (int i = 0; i < refractionCount; i++) {
            Vector2 refStart = Vector2.Zero;
            Vector2 refDirection = Vector2.Zero;
            GetRefractionSegment(owner, state, direction, i, ref refStart, ref refDirection);
            float refractionLength = BeamHitLength * 0.68f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), refStart,
                    refStart + refDirection * refractionLength, GetRefractionThickness(state), ref collisionPoint)) {
                return true;
            }
        }

        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        ChromaStoneStatePlayer state = Main.player[Projectile.owner].GetModPlayer<ChromaStoneStatePlayer>();
        modifiers.SourceDamage *= 1f + state.VisibleFacetCount * 0.12f + state.FacetPowerRatio * 0.08f;
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        ChromaStoneStatePlayer state = owner.GetModPlayer<ChromaStoneStatePlayer>();
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f));
        Vector2 start = GetBeamStart(owner, direction);
        float thickness = GetMainBeamThickness(state);
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(0.2f + state.FacetPowerRatio * 1.9f, 1.05f) * 0.56f;
        Color inner = ChromaStonePrismHelper.GetSpectrumColor(0.78f + state.FacetPowerRatio * 1.4f, 1.1f) * 0.92f;
        Color core = new Color(246, 250, 255, 225);
        Color beamColor = Color.Lerp(outer, inner, 0.35f);
        float mainScale = thickness / 22f;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Additive,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );

        VanillaBeamDrawHelper.DrawLastPrismBeam(start, direction, BeamDrawLength, beamColor, core,
            new Vector2(1.36f * mainScale, 1f),
            new Vector2(2.18f * mainScale, 1f),
            new Vector2(1.62f * mainScale, 1f),
            new Vector2(1.02f * mainScale, 1f),
            0.16f,
            0.30f,
            0.56f,
            1.18f);
        DrawBeamNode(pixel, start, 16f + state.VisibleFacetCount * 2f, inner, core);
        DrawBeamNode(pixel, start + direction * BeamDrawLength, 13f + state.VisibleFacetCount * 1.4f, outer, core);

        int refractionCount = GetRefractionCount(state);
        for (int i = 0; i < refractionCount; i++) {
            Vector2 refStart = Vector2.Zero;
            Vector2 refDirection = Vector2.Zero;
            GetRefractionSegment(owner, state, direction, i, ref refStart, ref refDirection);
            float refractionLength = BeamDrawLength * 0.68f;
            float refThickness = GetRefractionThickness(state);
            float refScale = refThickness / 18f;
            Color refBeamColor = Color.Lerp(outer * 0.86f, inner, 0.28f);

            VanillaBeamDrawHelper.DrawLastPrismBeam(refStart, refDirection, refractionLength, refBeamColor, core * 0.82f,
                new Vector2(1.12f * refScale, 1f),
                new Vector2(1.72f * refScale, 1f),
                new Vector2(1.28f * refScale, 1f),
                new Vector2(0.82f * refScale, 1f),
                0.12f,
                0.24f,
                0.46f,
                1.05f);
            DrawBeamNode(pixel, refStart, 10f + state.VisibleFacetCount, outer * 0.9f, core * 0.82f);
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );

        return false;
    }

    private static void DrawBeamNode(Texture2D pixel, Vector2 worldCenter, float scale, Color outer, Color inner) {
        Vector2 center = worldCenter - Main.screenPosition;
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, MathHelper.PiOver4, new Vector2(scale, scale), outer * 0.5f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, 0f, new Vector2(scale * 0.6f, scale * 0.6f), outer * 0.75f);
        ChromaStonePrismHelper.DrawRotatedRect(pixel, center, MathHelper.PiOver4, new Vector2(scale * 0.28f, scale * 0.28f), inner);
    }

    private static bool ShouldStayAlive(Player owner, OmnitrixPlayer omp, ChromaStoneStatePlayer state) {
        return owner.active &&
               !owner.dead &&
               omp.currentTransformationId == ChromaStoneStatePlayer.TransformationId &&
               omp.altAttack &&
               owner.channel &&
               !owner.noItems &&
               !owner.CCed &&
               !state.Guarding &&
               !state.DischargeActive;
    }

    private static Vector2 GetBeamStart(Player owner, Vector2 direction) {
        return owner.MountedCenter + direction * 18f;
    }

    private float GetBeamLength(Vector2 start, Vector2 direction, ChromaStoneStatePlayer state) {
        float[] samples = new float[3];
        float thickness = GetMainBeamThickness(state);
        float maxLength = 760f;
        Collision.LaserScan(start, direction, thickness, maxLength, samples);

        float tileLength = 0f;
        for (int i = 0; i < samples.Length; i++)
            tileLength += samples[i];
        tileLength /= samples.Length;

        return MathHelper.Clamp(tileLength, 28f, maxLength);
    }

    private static float GetMainBeamThickness(ChromaStoneStatePlayer state) {
        return 16f + state.VisibleFacetCount * 2.5f;
    }

    private static float GetRefractionThickness(ChromaStoneStatePlayer state) {
        return 9f + state.VisibleFacetCount * 1.2f;
    }

    private static int GetRefractionCount(ChromaStoneStatePlayer state) {
        return Math.Min(ChromaStoneStatePlayer.MaxFacets, state.VisibleFacetCount);
    }

    private static void GetRefractionSegment(Player owner, ChromaStoneStatePlayer state, Vector2 direction, int index,
        ref Vector2 start, ref Vector2 refractionDirection) {
        int refractionCount = GetRefractionCount(state);
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float[] baseAngles = GetRefractionAngles(refractionCount);
        float angleOffset = baseAngles[index];
        float lateral = refractionCount switch {
            1 => -0.55f,
            2 => index == 0 ? -1f : 1f,
            _ => MathHelper.Lerp(-1f, 1f, index / 2f)
        };
        float along = 34f + index * 18f;
        Vector2 facetOffset = state.GetFacetWorldOffset(Math.Min(index, ChromaStoneStatePlayer.MaxFacets - 1));
        start = GetBeamStart(owner, direction) + direction * along + normal * lateral * (10f + refractionCount * 2f) +
            facetOffset * 0.12f;
        refractionDirection = direction.RotatedBy(angleOffset).SafeNormalize(direction);
    }

    private static float[] GetRefractionAngles(int count) {
        return count switch {
            3 => new[] { -0.34f, 0f, 0.34f },
            2 => new[] { -0.26f, 0.26f },
            1 => new[] { -0.2f },
            _ => Array.Empty<float>()
        };
    }

    private Vector2 GetAimDirection(Player owner) {
        if (Main.netMode == NetmodeID.SinglePlayer || Projectile.owner == Main.myPlayer) {
            Vector2 localDirection = Main.MouseWorld - owner.Center;
            if (localDirection.LengthSquared() < 0.0001f)
                localDirection = new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f);

            localDirection.Normalize();
            SyncAimDirection(localDirection);
            return localDirection;
        }

        if (hasSyncedAimDirection && syncedAimDirection.LengthSquared() > 0.0001f)
            return syncedAimDirection;

        return Projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f));
    }

    private void SyncAimDirection(Vector2 direction) {
        bool changed = !hasSyncedAimDirection || Vector2.DistanceSquared(direction, syncedAimDirection) > 0.0004f;
        aimSyncTimer++;
        if (!changed && aimSyncTimer < 6)
            return;

        syncedAimDirection = direction;
        hasSyncedAimDirection = true;
        aimSyncTimer = 0;
        if (Main.netMode != NetmodeID.SinglePlayer)
            Projectile.netUpdate = true;
    }
}
