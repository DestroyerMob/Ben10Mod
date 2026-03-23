using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.UltimateAttacks;

public abstract class ChannelBeamUltimateProjectile : ModProjectile
{
    private SlotId _loopSlot;
    private bool _loopStarted;
    private int _sustainTimer;

    protected float BeamHitLength {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    protected float BeamDrawLength {
        get => Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    protected virtual float MaxLength => 2600f;
    protected virtual float BeamThickness => 28f;
    protected virtual float StartOffset => 52f;
    protected virtual float EndCapPadding => 6f;
    protected virtual int MinEnergyToSustain => 10;
    protected virtual int ProjectileAlpha => 255;
    protected virtual int BeamFrameCount => 3;
    protected virtual Vector2 StartScale => new(1.55f, 1f);
    protected virtual Vector2 OuterScale => new(2.55f, 1f);
    protected virtual Vector2 MidScale => new(1.85f, 1f);
    protected virtual Vector2 InnerScale => new(1.25f, 1f);
    protected virtual Color BeamColor => new(60, 255, 140);
    protected virtual Color BeamHighlightColor => Color.White;
    protected virtual int EndDustType => DustID.GreenTorch;
    protected virtual int EndDustCount => 5;
    protected virtual SoundStyle? LoopSound => SoundID.Item15;
    protected virtual float LightR => 0.2f;
    protected virtual float LightG => 1.6f;
    protected virtual float LightB => 0.6f;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = BeamFrameCount;
    }

    public override void SetDefaults() {
        Projectile.width = 34;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = false;
        Projectile.alpha = ProjectileAlpha;
        Projectile.timeLeft = 2;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();

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

        Vector2 dir = GetAimDirection(owner);
        Projectile.velocity = dir;
        Projectile.rotation = dir.ToRotation();
        Projectile.Center = owner.Center + dir * StartOffset;

        Vector2 start = owner.Center + dir * StartOffset;
        BeamHitLength = GetBeamLength(start, dir);
        BeamDrawLength = MathHelper.Clamp(BeamHitLength - EndCapPadding, 16f, BeamHitLength);

        UpdateLoopSound();
        Lighting.AddLight(Projectile.Center, LightR, LightG, LightB);
        OnBeamUpdated(owner, omp, start, dir);
    }

    protected virtual bool ShouldStayAlive(Player owner, OmnitrixPlayer omp) {
        return owner.active &&
               !owner.dead &&
               omp.ultimateAttack &&
               owner.channel &&
               !owner.noItems &&
               !owner.CCed &&
               omp.omnitrixEnergy >= MinEnergyToSustain;
    }

    protected virtual Vector2 GetAimDirection(Player owner) {
        Vector2 dir = Main.MouseWorld - owner.Center;
        if (dir.LengthSquared() < 0.0001f)
            dir = new Vector2(owner.direction, 0f);

        dir.Normalize();
        return dir;
    }

    protected virtual void OnBeamUpdated(Player owner, OmnitrixPlayer omp, Vector2 start, Vector2 direction) { }

    private bool TryConsumeSustainCost(Player owner, OmnitrixPlayer omp) {
        var trans = omp.CurrentTransformation;
        if (trans == null)
            return true;

        int sustainInterval = trans.GetAttackSustainInterval(OmnitrixPlayer.AttackSelection.Ultimate, omp);
        int sustainCost = trans.GetAttackSustainEnergyCost(OmnitrixPlayer.AttackSelection.Ultimate, omp);
        if (sustainInterval <= 0 || sustainCost <= 0 || owner.whoAmI != Main.myPlayer)
            return true;

        _sustainTimer++;
        if (_sustainTimer < sustainInterval)
            return true;

        _sustainTimer = 0;
        return trans.TryConsumeAttackSustainCost(OmnitrixPlayer.AttackSelection.Ultimate, omp);
    }

    private void UpdateLoopSound() {
        if (Projectile.owner != Main.myPlayer)
            return;

        SoundStyle? loopSound = LoopSound;
        if (loopSound is null)
            return;

        if (!_loopStarted) {
            SoundStyle style = loopSound.Value;
            style.IsLooped = true;
            style.MaxInstances = 1;
            style.SoundLimitBehavior = SoundLimitBehavior.IgnoreNew;

            _loopSlot = SoundEngine.PlaySound(style, Projectile.Center);
            _loopStarted = true;
        }

        if (SoundEngine.TryGetActiveSound(_loopSlot, out ActiveSound active))
            active.Position = Projectile.Center;
    }

    protected virtual float GetBeamLength(Vector2 start, Vector2 dir) {
        float[] samples = new float[3];
        Collision.LaserScan(start, dir, BeamThickness, MaxLength, samples);

        float tileLength = 0f;
        for (int i = 0; i < samples.Length; i++)
            tileLength += samples[i];
        tileLength /= samples.Length;

        tileLength = MathHelper.Clamp(tileLength, 16f, MaxLength);

        float best = tileLength;

        for (int n = 0; n < Main.maxNPCs; n++) {
            NPC npc = Main.npc[n];
            if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                continue;

            float collisionPoint = 0f;
            bool hit = Collision.CheckAABBvLineCollision(
                npc.Hitbox.TopLeft(),
                npc.Hitbox.Size(),
                start,
                start + dir * tileLength,
                BeamThickness,
                ref collisionPoint
            );

            if (hit && collisionPoint > 0f && collisionPoint < best)
                best = collisionPoint;
        }

        return best;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];

        Vector2 dir = Projectile.velocity;
        if (dir.LengthSquared() < 0.0001f)
            return false;

        Vector2 start = owner.Center + dir * StartOffset;
        Vector2 end = start + dir * BeamHitLength;
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            start,
            end,
            BeamThickness,
            ref collisionPoint
        );
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer &&
            SoundEngine.TryGetActiveSound(_loopSlot, out ActiveSound active)) {
            active.Stop();
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];

        Vector2 dir = Projectile.velocity;
        if (dir.LengthSquared() < 0.0001f)
            return false;

        dir.Normalize();

        Vector2 start = owner.Center + dir * StartOffset;
        float length = BeamDrawLength;

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        int frameHeight = texture.Height / BeamFrameCount;
        int frameWidth = texture.Width;

        Rectangle startFrame = new(0, 0, frameWidth, frameHeight);
        Rectangle midFrame = new(0, frameHeight, frameWidth, frameHeight);
        Rectangle endFrame = new(0, frameHeight * 2, frameWidth, frameHeight);

        float rotation = dir.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;
        Vector2 origin = new(frameWidth * 0.5f, frameHeight * 0.5f);

        float t = Main.GlobalTimeWrappedHourly;
        float pulse = 0.88f + 0.12f * (float)System.Math.Sin(t * 10f);
        float shimmer = 0.82f + 0.18f * (float)System.Math.Sin(t * 6.5f);
        Color baseColor = BeamColor * (shimmer * 1.25f);

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

        Main.EntitySpriteDraw(
            texture,
            start - Main.screenPosition,
            startFrame,
            baseColor,
            rotation,
            origin,
            StartScale * new Vector2(pulse, 1f),
            SpriteEffects.None,
            0
        );

        float step = frameHeight * 0.60f;
        float distance = step * 0.50f;

        while (distance < length - step * 0.50f) {
            float along = distance / length;
            float fadeOut = along > 0.90f
                ? MathHelper.SmoothStep(1f, 0f, (along - 0.90f) / 0.10f)
                : 1f;

            Vector2 pos = start + dir * distance;

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, midFrame, baseColor * (0.18f * fadeOut), rotation,
                origin, OuterScale * new Vector2(pulse, 1f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, midFrame, baseColor * (0.32f * fadeOut), rotation,
                origin, MidScale * new Vector2(pulse, 1f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, midFrame, BeamHighlightColor * (0.58f * fadeOut), rotation,
                origin, InnerScale * new Vector2(pulse, 1f), SpriteEffects.None, 0);

            distance += step;
        }

        Vector2 endPos = start + dir * length;
        for (int i = 0; i < EndDustCount; i++) {
            int dustNum = Dust.NewDust(endPos, endFrame.Width, endFrame.Height, EndDustType, 0, 0, 0, Color.White, 3f);
            Main.dust[dustNum].noGravity = true;
        }

        Main.EntitySpriteDraw(
            texture,
            endPos - Main.screenPosition,
            endFrame,
            baseColor * 1.15f,
            rotation,
            origin,
            StartScale * new Vector2(pulse, 1f),
            SpriteEffects.None,
            0
        );

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
}
