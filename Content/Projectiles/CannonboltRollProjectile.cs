using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Players;
using Ben10Mod.Content.Transformations.Cannonbolt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class CannonboltRollProjectile : ModProjectile {
    private const int AirStateGround = 0;
    private const int AirStateVaultRise = 1;
    private const int AirStateSlamDive = 2;
    private const float BaseRollSpeed = 8.5f;
    private const float MaxNormalSpeed = 13.5f;
    private const float MaxRicochetSpeed = 18.5f;
    private const float MaxSiegeSpeed = 22f;
    private const float BaseVaultLaunchSpeed = 11.5f;
    private const float BaseGroundDeceleration = 0.08f;
    private const float SiegeAcceleration = 0.12f;
    private const float RicochetAcceleration = 0.1f;
    private const float NormalAcceleration = 0.085f;
    private const float BounceChargeGain = 0.16f;
    private const float SiegeBounceChargeGain = 0.22f;
    private const float SlamTerminalVelocity = 19f;

    private float ImpactCharge {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = MathHelper.Clamp(value, 0f, 1f);
    }

    private int BounceCount {
        get => Math.Max(0, (int)Math.Round(Projectile.ai[1]));
        set => Projectile.ai[1] = MathHelper.Clamp(value, 0f, CannonboltStatePlayer.MaxBounceCount);
    }

    private int AirState {
        get => (int)Math.Round(Projectile.ai[2]);
        set => Projectile.ai[2] = value;
    }

    private float CurrentSpeed {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = Math.Max(BaseRollSpeed * 0.7f, value);
    }

    private bool Initialized {
        get => Projectile.localAI[1] > 0f;
        set => Projectile.localAI[1] = value ? 1f : 0f;
    }

    private bool RicochetActive => Owner.GetModPlayer<CannonboltStatePlayer>().RicochetActive || SiegeActive;
    private bool SiegeActive => Owner.GetModPlayer<CannonboltStatePlayer>().SiegeActive;
    private Player Owner => Main.player[Projectile.owner];
    private int BaseDamage => Projectile.originalDamage > 0 ? Projectile.originalDamage : Projectile.damage;

    public bool IsVaultingVisible => AirState != AirStateGround;
    public float VisibleSpeedRatio => MathHelper.Clamp((CurrentSpeed - BaseRollSpeed) / (MaxSiegeSpeed - BaseRollSpeed), 0f, 1f);
    public float VisibleImpactChargeRatio => ImpactCharge;
    public int VisibleBounceCount => BounceCount;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 9;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 52;
        Projectile.height = 52;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.ownerHitCheck = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public bool TryTriggerVaultLaunch() {
        if (!Projectile.active || AirState != AirStateGround)
            return false;

        float launchSpeed = BaseVaultLaunchSpeed + ImpactCharge * 2.8f + (SiegeActive ? 1.4f : 0f);
        AirState = AirStateVaultRise;
        Projectile.velocity = new Vector2(ResolveTravelDirection() * Math.Max(CurrentSpeed, BaseRollSpeed + 2.2f), -launchSpeed);
        Projectile.netUpdate = true;
        return true;
    }

    public void RequestEndRoll() {
        if (Projectile.active)
            Projectile.Kill();
    }

    public override void AI() {
        Player owner = Owner;
        if (!owner.active || owner.dead || owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != CannonboltStatePlayer.TransformationId) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        Projectile.localNPCHitCooldown = SiegeActive ? 6 : 10;

        if (!Initialized)
            InitializeRoll(owner);

        Vector2 desiredVelocity = Projectile.velocity;
        int inputX = (owner.controlRight ? 1 : 0) - (owner.controlLeft ? 1 : 0);
        float maxSpeed = GetMaxSpeed();

        if (AirState == AirStateGround)
            UpdateGroundRoll(inputX, maxSpeed, ref desiredVelocity);
        else
            UpdateAirRoll(inputX, maxSpeed, ref desiredVelocity);

        ResolveTileInteractions(owner, ref desiredVelocity, maxSpeed);
        ApplyRollMotion(owner, desiredVelocity);
        UpdateDamage();
        SpawnRollDust();

        Lighting.AddLight(Projectile.Center, SiegeActive
            ? new Vector3(0.9f, 0.68f, 0.26f)
            : RicochetActive
                ? new Vector3(0.72f, 0.56f, 0.24f)
                : new Vector3(0.48f, 0.36f, 0.16f));
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;

        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--) {
            Vector2 oldPos = Projectile.oldPos[i];
            if (oldPos == Vector2.Zero)
                continue;

            float progress = 1f - i / (float)Projectile.oldPos.Length;
            Vector2 drawCenter = oldPos + Projectile.Size * 0.5f - Main.screenPosition;
            float trailScale = Projectile.scale * (0.78f + progress * 0.16f);
            Color trailColor = SiegeActive
                ? new Color(255, 218, 120, 90)
                : RicochetActive
                    ? new Color(240, 205, 125, 78)
                    : new Color(200, 175, 115, 58);
            DrawShell(pixel, drawCenter, Projectile.oldRot[i], trailScale, trailColor, trailColor * 0.72f, true);
        }

        Vector2 center = Projectile.Center - Main.screenPosition;
        Color outerColor = SiegeActive
            ? new Color(172, 118, 42, 235)
            : RicochetActive
                ? new Color(154, 108, 44, 230)
                : new Color(132, 92, 42, 225);
        Color innerColor = SiegeActive
            ? new Color(255, 229, 170, 215)
            : new Color(245, 214, 162, 205);
        DrawShell(pixel, center, Projectile.rotation, Projectile.scale, outerColor, innerColor, false);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        SpawnImpactDust(target.Center, 16, 1f + VisibleSpeedRatio * 0.25f);
        target.AddBuff(BuffID.BrokenArmor, SiegeActive ? 240 : 120);

        if (SiegeActive || VisibleSpeedRatio >= 0.72f)
            SpawnImpactBurst(Projectile.Center, 0.66f + ImpactCharge * 0.24f, SiegeActive ? 0.46f : 0.28f);

        if (!SiegeActive)
            CurrentSpeed = Math.Max(BaseRollSpeed, CurrentSpeed * 0.95f);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Owner;
        if (owner.active && !owner.dead)
            owner.velocity = new Vector2(owner.velocity.X * (SiegeActive ? 0.92f : 0.48f), owner.velocity.Y);

        owner.GetModPlayer<CannonboltStatePlayer>().ClearRollTelemetry();
    }

    private void InitializeRoll(Player owner) {
        Initialized = true;
        AirState = AirStateGround;
        BounceCount = 0;
        ImpactCharge = 0f;
        CurrentSpeed = Math.Max(BaseRollSpeed, Math.Abs(Projectile.velocity.X) * 0.92f + 1.6f);
        Projectile.velocity = new Vector2(ResolveTravelDirection() * CurrentSpeed, 0f);
        Projectile.scale = 1.04f;
        SpawnImpactDust(owner.Center, 14, 1f);
    }

    private void UpdateGroundRoll(int inputX, float maxSpeed, ref Vector2 desiredVelocity) {
        int travelDirection = ResolveTravelDirection();
        float targetSpeed = MathHelper.Lerp(BaseRollSpeed + 1f, maxSpeed, 0.36f + ImpactCharge * 0.64f);
        float acceleration = SiegeActive ? SiegeAcceleration : RicochetActive ? RicochetAcceleration : NormalAcceleration;

        if (inputX != 0) {
            if (inputX != travelDirection && CurrentSpeed > BaseRollSpeed + 2.4f) {
                CurrentSpeed = Math.Max(BaseRollSpeed, CurrentSpeed - 0.95f);
            }
            else {
                travelDirection = inputX;
                CurrentSpeed = MathHelper.Lerp(CurrentSpeed, targetSpeed, acceleration);
            }
        }
        else if (!SiegeActive) {
            float idleSpeed = RicochetActive
                ? MathHelper.Lerp(BaseRollSpeed, BaseRollSpeed + 1.2f, ImpactCharge * 0.35f)
                : BaseRollSpeed * 0.84f;
            CurrentSpeed = MathHelper.Lerp(CurrentSpeed, idleSpeed, BaseGroundDeceleration);
        }

        CurrentSpeed = MathHelper.Clamp(CurrentSpeed, BaseRollSpeed * 0.78f, maxSpeed);
        desiredVelocity = new Vector2(travelDirection * CurrentSpeed, Math.Max(desiredVelocity.Y, 0f));
    }

    private void UpdateAirRoll(int inputX, float maxSpeed, ref Vector2 desiredVelocity) {
        int travelDirection = ResolveTravelDirection();
        if (inputX != 0)
            travelDirection = inputX;

        if (AirState == AirStateVaultRise) {
            desiredVelocity.X = MathHelper.Lerp(desiredVelocity.X, travelDirection * CurrentSpeed, 0.08f);
            desiredVelocity.Y += 0.34f;
            if (desiredVelocity.Y >= 0f || Owner.controlDown)
                AirState = AirStateSlamDive;
        }

        if (AirState == AirStateSlamDive) {
            desiredVelocity.X = MathHelper.Lerp(desiredVelocity.X, travelDirection * CurrentSpeed, 0.05f);
            desiredVelocity.Y = Math.Min(desiredVelocity.Y + (SiegeActive ? 0.96f : 0.72f), SlamTerminalVelocity + (SiegeActive ? 4f : 0f));
        }

        CurrentSpeed = MathHelper.Clamp(Math.Max(Math.Abs(desiredVelocity.X), CurrentSpeed), BaseRollSpeed, maxSpeed);
    }

    private void ResolveTileInteractions(Player owner, ref Vector2 desiredVelocity, float maxSpeed) {
        bool grounded = AlienIdentityPlayer.IsGrounded(owner);
        Vector2 collisionVelocity = Collision.TileCollision(owner.position, desiredVelocity, owner.width, owner.height, false, false,
            (int)owner.gravDir);
        bool hitWall = Math.Abs(collisionVelocity.X - desiredVelocity.X) > 0.25f;
        bool hitCeiling = AirState == AirStateVaultRise && desiredVelocity.Y < -0.6f &&
                          Math.Abs(collisionVelocity.Y - desiredVelocity.Y) > 0.25f;
        bool hitFloor = AirState != AirStateGround &&
                        (grounded || desiredVelocity.Y > 1f && Math.Abs(collisionVelocity.Y - desiredVelocity.Y) > 0.25f);

        if (hitFloor) {
            HandleLanding(owner, ref desiredVelocity, maxSpeed);
            return;
        }

        if ((hitWall || hitCeiling) && RicochetActive) {
            HandleBounce(owner, ref desiredVelocity, hitWall, hitCeiling, maxSpeed);
            return;
        }

        if (hitWall || hitCeiling) {
            SpawnImpactDust(owner.Center + new Vector2(Math.Sign(desiredVelocity.X) * 16f, 0f), 12, 0.9f);
            Projectile.Kill();
        }
    }

    private void HandleLanding(Player owner, ref Vector2 desiredVelocity, float maxSpeed) {
        float landingScale = 0.94f + ImpactCharge * 0.5f + (SiegeActive ? 0.34f : 0f);
        float damageMultiplier = SiegeActive
            ? 0.82f + VisibleSpeedRatio * 0.45f
            : 0.58f + ImpactCharge * 0.32f;

        SpawnImpactBurst(owner.Bottom, landingScale, damageMultiplier);
        SpawnImpactDust(owner.Bottom, 22, 1.18f + ImpactCharge * 0.15f);

        ImpactCharge = MathHelper.Clamp(ImpactCharge + 0.1f, 0f, 1f);
        BounceCount = Math.Min(CannonboltStatePlayer.MaxBounceCount, BounceCount + 1);
        CurrentSpeed = MathHelper.Clamp(CurrentSpeed + 0.85f + (SiegeActive ? 0.55f : 0f), BaseRollSpeed, maxSpeed);
        AirState = AirStateGround;
        desiredVelocity = new Vector2(ResolveTravelDirection() * CurrentSpeed, 0f);
    }

    private void HandleBounce(Player owner, ref Vector2 desiredVelocity, bool hitWall, bool hitCeiling, float maxSpeed) {
        if (hitWall)
            desiredVelocity.X = -Math.Sign(desiredVelocity.X == 0f ? ResolveTravelDirection() : desiredVelocity.X) *
                                MathHelper.Clamp(Math.Abs(desiredVelocity.X) * (SiegeActive ? 1.02f : 0.96f) + 1.6f,
                                    BaseRollSpeed + 1f, maxSpeed);

        if (hitCeiling) {
            desiredVelocity.Y = Math.Abs(desiredVelocity.Y) * 0.86f + 2.4f;
            AirState = AirStateSlamDive;
        }
        else if (AirState == AirStateGround) {
            desiredVelocity.Y = Math.Min(desiredVelocity.Y, -2.4f - ImpactCharge * 1.1f);
        }

        ImpactCharge = MathHelper.Clamp(ImpactCharge + (SiegeActive ? SiegeBounceChargeGain : BounceChargeGain), 0f, 1f);
        BounceCount = Math.Min(CannonboltStatePlayer.MaxBounceCount, BounceCount + 1);
        CurrentSpeed = MathHelper.Clamp(Math.Max(Math.Abs(desiredVelocity.X), CurrentSpeed + 1.05f + (SiegeActive ? 0.6f : 0.28f)),
            BaseRollSpeed, maxSpeed);

        if (SiegeActive)
            SpawnImpactBurst(owner.Center, 0.72f + ImpactCharge * 0.22f, 0.42f);

        SpawnImpactDust(owner.Center, 18, 1.08f + ImpactCharge * 0.18f);
        owner.position += new Vector2(Math.Sign(desiredVelocity.X) * 4f, hitCeiling ? 6f : 0f);
    }

    private void ApplyRollMotion(Player owner, Vector2 desiredVelocity) {
        Projectile.velocity = desiredVelocity;
        owner.velocity = desiredVelocity;
        owner.direction = desiredVelocity.X >= 0f ? 1 : -1;
        owner.noKnockback = true;
        owner.noFallDmg = true;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = Math.Max(owner.immuneTime, SiegeActive ? 9 : 6);
        owner.armorEffectDrawShadow = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.itemTime = owner.itemAnimation = 2;
        owner.itemRotation = 0f;
        owner.heldProj = Projectile.whoAmI;

        Projectile.Center = owner.Center;
        Projectile.rotation += desiredVelocity.X * (0.13f + VisibleSpeedRatio * 0.22f + (SiegeActive ? 0.04f : 0f));
        Projectile.scale = 1.04f + VisibleSpeedRatio * 0.18f + (SiegeActive ? 0.1f : 0f);
        Projectile.GetGlobalProjectile<OmnitrixProjectile>().EnableScaleHitboxSync(Projectile);
    }

    private void UpdateDamage() {
        float speedBonus = 1f + VisibleSpeedRatio * 0.48f;
        float chargeBonus = 1f + ImpactCharge * (SiegeActive ? 0.72f : 0.44f);
        float airBonus = AirState == AirStateSlamDive ? 1.18f : AirState == AirStateVaultRise ? 1.08f : 1f;
        Projectile.damage = Math.Max(1, (int)Math.Round(BaseDamage * speedBonus * chargeBonus * airBonus));
        Projectile.knockBack = 4.8f + VisibleSpeedRatio * 2.8f + (SiegeActive ? 1.6f : 0.6f);
    }

    private void SpawnRollDust() {
        if (!Main.rand.NextBool(SiegeActive ? 1 : 2))
            return;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(ResolveTravelDirection(), 0f));
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 dustPosition = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width * 0.28f, Projectile.height * 0.22f);
        Vector2 dustVelocity = -direction * Main.rand.NextFloat(0.8f, 2.4f) + normal * Main.rand.NextFloat(-0.9f, 0.9f);
        int dustType = Main.rand.NextBool(3) ? DustID.GemTopaz : DustID.Smoke;
        Color dustColor = dustType == DustID.GemTopaz
            ? (SiegeActive ? new Color(255, 222, 148) : new Color(235, 205, 140))
            : new Color(205, 195, 175);
        Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 110, dustColor,
            Main.rand.NextFloat(1f, 1.28f) + VisibleSpeedRatio * 0.16f);
        dust.noGravity = true;
    }

    private void SpawnImpactDust(Vector2 position, int count, float scale) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < count; i++) {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.6f, 5.2f);
            int dustType = i % 4 == 0 ? DustID.GemTopaz : DustID.Smoke;
            Color dustColor = dustType == DustID.GemTopaz
                ? new Color(255, 212, 140)
                : new Color(214, 206, 194);
            Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(10f, 10f), dustType, velocity, 112, dustColor,
                Main.rand.NextFloat(0.95f, 1.42f) * scale);
            dust.noGravity = true;
        }
    }

    private void SpawnImpactBurst(Vector2 center, float scale, float damageMultiplier) {
        int burstDamage = Math.Max(1, (int)Math.Round(Projectile.damage * damageMultiplier));
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), center, Vector2.Zero,
            ModContent.ProjectileType<CannonboltImpactBurstProjectile>(), burstDamage, Projectile.knockBack + 2f,
            Projectile.owner, scale);
    }

    private int ResolveTravelDirection() {
        if (Projectile.velocity.X > 0.15f)
            return 1;
        if (Projectile.velocity.X < -0.15f)
            return -1;

        return Owner.direction == 0 ? 1 : Owner.direction;
    }

    private float GetMaxSpeed() {
        if (SiegeActive)
            return MaxSiegeSpeed;

        return RicochetActive ? MaxRicochetSpeed : MaxNormalSpeed;
    }

    private static void DrawShell(Texture2D pixel, Vector2 center, float rotation, float scale, Color outerColor,
        Color innerColor, bool trail) {
        Vector2 outerScale = trail ? new Vector2(18f, 13f) * scale : new Vector2(22f, 16f) * scale;
        Vector2 innerScale = outerScale * (trail ? 0.62f : 0.68f);

        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), outerColor, rotation,
            new Vector2(0.5f, 0.5f), outerScale, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(pixel, center, new Rectangle(0, 0, 1, 1), innerColor, rotation,
            new Vector2(0.5f, 0.5f), innerScale, SpriteEffects.None, 0f);

        if (!trail) {
            Main.spriteBatch.Draw(pixel, center + new Vector2(4f, -4f).RotatedBy(rotation), new Rectangle(0, 0, 1, 1),
                new Color(255, 244, 214, 110), 0f, new Vector2(0.5f, 0.5f), new Vector2(10f, 6f) * scale,
                SpriteEffects.None, 0f);
        }
    }
}
