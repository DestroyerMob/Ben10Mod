using System;
using System.IO;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.ChromaStone;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneSupernovaProjectile : ModProjectile {
    private Vector2 syncedAimDirection = Vector2.UnitX;
    private bool hasSyncedAimDirection;
    private int aimSyncTimer;
    private int sustainTimer;

    private int FacetPower => Math.Clamp((int)Math.Round(Projectile.ai[0]), 0, 3);
    private float StoredPowerRatio => MathHelper.Clamp(Projectile.ai[1], 0f, 1f);

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
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.timeLeft = 2;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
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

        if (!ShouldStayAlive(owner, omp)) {
            Projectile.Kill();
            return;
        }

        if (!TryConsumeSustainCost(owner, omp)) {
            owner.channel = false;
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        Projectile.localNPCHitCooldown = Math.Max(4, 8 - FacetPower);

        Vector2 direction = GetAimDirection(owner);
        Projectile.velocity = direction;
        Projectile.rotation = direction.ToRotation();

        Vector2 start = GetBeamStart(owner, direction);
        Projectile.Center = start;
        BeamHitLength = GetBeamLength(start, direction);
        BeamDrawLength = Math.Max(22f, BeamHitLength - 10f);

        owner.ChangeDir(direction.X >= 0f ? 1 : -1);
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;
        owner.itemRotation = (float)Math.Atan2(direction.Y * owner.direction, direction.X * owner.direction);
        owner.noKnockback = true;
        owner.armorEffectDrawShadow = true;
        owner.velocity.X *= 0.9f;
        if (owner.velocity.Y > 0.35f)
            owner.velocity.Y = 0.35f;
        else
            owner.velocity.Y = Math.Max(owner.velocity.Y - 0.06f, -1.35f);

        AbsorbHostileProjectiles(owner, state, start, direction);

        Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(StoredPowerRatio * 2.4f + 0.3f, 1.14f);
        Lighting.AddLight(start + direction * Math.Min(BeamDrawLength * 0.42f, 240f), prismColor.ToVector3() * 0.84f);

        if (!Main.dedServ && Main.rand.NextBool(1)) {
            Vector2 dustPosition = start + direction * Main.rand.NextFloat(18f, Math.Max(24f, BeamDrawLength * 0.9f));
            Dust dust = Dust.NewDustPerfect(dustPosition + Main.rand.NextVector2Circular(12f, 12f), DustID.WhiteTorch,
                direction.RotatedByRandom(0.32f) * Main.rand.NextFloat(0.5f, 2.6f), 95, prismColor,
                Main.rand.NextFloat(1f, 1.35f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f));
        Vector2 start = GetBeamStart(owner, direction);
        float collisionPoint = 0f;

        if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start,
                start + direction * BeamHitLength, GetMainBeamThickness(), ref collisionPoint)) {
            return true;
        }

        int sideBeamCount = GetSideBeamCount();
        for (int i = 0; i < sideBeamCount; i++) {
            Vector2 refDirection = direction.RotatedBy(GetSideBeamAngles(sideBeamCount)[i]).SafeNormalize(direction);
            Vector2 refStart = owner.Center + refDirection * 20f;
            float refractionLength = BeamHitLength * 0.84f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), refStart,
                    refStart + refDirection * refractionLength, GetSideBeamThickness(), ref collisionPoint)) {
                return true;
            }
        }

        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.SourceDamage *= 1.22f + FacetPower * 0.16f + StoredPowerRatio * 0.1f;
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return false;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f));
        Vector2 start = GetBeamStart(owner, direction);
        Color outer = ChromaStonePrismHelper.GetSpectrumColor(0.12f + StoredPowerRatio * 1.9f, 1.08f) * 0.58f;
        Color inner = ChromaStonePrismHelper.GetSpectrumColor(0.74f + StoredPowerRatio * 1.4f, 1.12f) * 0.94f;
        Color core = new Color(246, 250, 255, 235);
        Color beamColor = Color.Lerp(outer, inner, 0.4f);
        float mainScale = GetMainBeamThickness() / 28f;

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
            new Vector2(1.58f * mainScale, 1f),
            new Vector2(2.58f * mainScale, 1.02f),
            new Vector2(1.92f * mainScale, 1f),
            new Vector2(1.28f * mainScale, 0.96f),
            0.18f,
            0.34f,
            0.60f,
            1.22f);

        int sideBeamCount = GetSideBeamCount();
        float[] angles = GetSideBeamAngles(sideBeamCount);
        for (int i = 0; i < sideBeamCount; i++) {
            Vector2 refDirection = direction.RotatedBy(angles[i]).SafeNormalize(direction);
            Vector2 refStart = owner.Center + refDirection * 20f;
            float refScale = GetSideBeamThickness() / 20f;
            Color refBeamColor = Color.Lerp(outer * 0.88f, inner, 0.3f);

            VanillaBeamDrawHelper.DrawLastPrismBeam(refStart, refDirection, BeamDrawLength * 0.84f, refBeamColor, core * 0.84f,
                new Vector2(1.22f * refScale, 1f),
                new Vector2(1.96f * refScale, 1f),
                new Vector2(1.44f * refScale, 1f),
                new Vector2(0.92f * refScale, 0.98f),
                0.14f,
                0.28f,
                0.48f,
                1.08f);
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

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            Vector2 endPoint = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * BeamDrawLength;
            int burstDamage = Math.Max(1, (int)Math.Round(Projectile.damage * (0.42f + FacetPower * 0.08f)));
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), endPoint, Vector2.Zero,
                ModContent.ProjectileType<ChromaStoneRadianceBurstProjectile>(), burstDamage, 4.4f, Projectile.owner,
                MathHelper.Clamp(0.35f + FacetPower * 0.18f, 0f, 1f), 1f);
        }

        if (Main.dedServ)
            return;

        for (int i = 0; i < 30; i++) {
            Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(i * 0.15f + StoredPowerRatio);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), DustID.WhiteTorch,
                Main.rand.NextVector2Circular(5.5f, 5.5f), 90, prismColor, Main.rand.NextFloat(1.05f, 1.6f));
            dust.noGravity = true;
        }
    }

    private static bool ShouldStayAlive(Player owner, OmnitrixPlayer omp) {
        return owner.active &&
               !owner.dead &&
               omp.currentTransformationId == ChromaStoneStatePlayer.TransformationId &&
               omp.ultimateAttack &&
               owner.channel &&
               !owner.noItems &&
               !owner.CCed;
    }

    private static Vector2 GetBeamStart(Player owner, Vector2 direction) {
        return owner.Center + direction * 26f;
    }

    private float GetBeamLength(Vector2 start, Vector2 direction) {
        float[] samples = new float[3];
        float thickness = GetMainBeamThickness();
        float maxLength = 1050f + FacetPower * 110f;
        Collision.LaserScan(start, direction, thickness, maxLength, samples);

        float tileLength = 0f;
        for (int i = 0; i < samples.Length; i++)
            tileLength += samples[i];
        tileLength /= samples.Length;

        return MathHelper.Clamp(tileLength, 40f, maxLength);
    }

    private float GetMainBeamThickness() {
        return 28f + FacetPower * 4f;
    }

    private float GetSideBeamThickness() {
        return 16f + FacetPower * 2f;
    }

    private int GetSideBeamCount() {
        return FacetPower;
    }

    private static float[] GetSideBeamAngles(int count) {
        return count switch {
            3 => new[] { -0.55f, 0f, 0.55f },
            2 => new[] { -0.44f, 0.44f },
            1 => new[] { 0f },
            _ => Array.Empty<float>()
        };
    }

    private void AbsorbHostileProjectiles(Player owner, ChromaStoneStatePlayer state, Vector2 beamStart, Vector2 direction) {
        if (owner.whoAmI != Main.myPlayer)
            return;

        float collisionPoint = 0f;
        Rectangle ownerRect = owner.Hitbox;
        Vector2 beamEnd = beamStart + direction * BeamHitLength;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile hostile = Main.projectile[i];
            if (!hostile.active || !hostile.hostile || hostile.damage <= 0 || hostile.friendly || !IsWeakProjectile(hostile))
                continue;

            bool hitOwner = hostile.Hitbox.Intersects(ownerRect);
            bool hitBeam = Collision.CheckAABBvLineCollision(hostile.Hitbox.TopLeft(), hostile.Hitbox.Size(), beamStart, beamEnd,
                GetMainBeamThickness(), ref collisionPoint);

            if (!hitOwner && !hitBeam)
                continue;

            state.RegisterDischargeAbsorption(hostile.Center, direction, hostile.damage);
            hostile.Kill();
        }
    }

    private bool TryConsumeSustainCost(Player owner, OmnitrixPlayer omp) {
        var transformation = omp.CurrentTransformation;
        if (transformation == null)
            return true;

        int sustainInterval = transformation.GetAttackSustainInterval(OmnitrixPlayer.AttackSelection.Ultimate, omp);
        int sustainCost = transformation.GetAttackSustainEnergyCost(OmnitrixPlayer.AttackSelection.Ultimate, omp);
        if (sustainInterval <= 0 || sustainCost <= 0 || owner.whoAmI != Main.myPlayer)
            return true;

        sustainTimer++;
        if (sustainTimer < sustainInterval)
            return true;

        sustainTimer = 0;
        return transformation.TryConsumeAttackSustainCost(OmnitrixPlayer.AttackSelection.Ultimate, omp);
    }

    private static bool IsWeakProjectile(Projectile projectile) {
        return projectile.damage <= 70 &&
               projectile.width <= 42 &&
               projectile.height <= 42;
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
        if (!changed && aimSyncTimer < 5)
            return;

        syncedAimDirection = direction;
        hasSyncedAimDirection = true;
        aimSyncTimer = 0;
        if (Main.netMode != NetmodeID.SinglePlayer)
            Projectile.netUpdate = true;
    }
}
