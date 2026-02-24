using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles
{
    public class EyeGuyUltimateBeam : ModProjectile
    {
        private const float MaxLength = 2600f;
        private const float BeamThickness = 28f;
        private const float StartOffset = 52f;

        private SlotId _loopSlot;
        private bool _loopStarted;

        // Beam lengths for this tick:
        // localAI[0] = collision length (reaches the first hit)
        // localAI[1] = draw length (slightly shorter so the end-cap doesn't clip)
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
            Main.projFrames[Type] = 3; // start, middle, end
        }

        public override void SetDefaults() {
            Projectile.width       = 34;
            Projectile.height      = 34;
            Projectile.friendly    = true;
            Projectile.penetrate   = -1;
            Projectile.tileCollide = false; // we handle tiles via LaserScan
            Projectile.ignoreWater = true;

            Projectile.hide     = false; // must be false for PreDraw to run
            Projectile.alpha    = 255; // hide the projectile sprite itself
            Projectile.timeLeft = 2;

            Projectile.DamageType           = DamageClass.Magic; // change if needed
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown  = 10;
        }

        public override void AI() {
            Player owner = Main.player[Projectile.owner];
            var    omp   = owner.GetModPlayer<OmnitrixPlayer>();

            if (!owner.active || owner.dead) {
                Projectile.Kill();
                return;
            }

            if (!owner.channel || owner.noItems || owner.CCed) {
                Projectile.Kill();
                return;
            }

            if (omp.omnitrixEnergy < 10) {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            Vector2 dir = Main.MouseWorld - owner.Center;
            if (dir.LengthSquared() < 0.0001f)
                dir = new Vector2(owner.direction, 0f);

            dir.Normalize();

            Projectile.velocity = dir;
            Projectile.rotation = dir.ToRotation();
            Projectile.Center   = owner.Center + dir * StartOffset;

            // Compute current beam length (tiles + first enemy hit).
            Vector2 start = owner.Center + dir * StartOffset;
            BeamHitLength = GetBeamLength(start, dir);
            // Pull back slightly so the end cap doesn't clip inside tiles/NPCs.
            BeamDrawLength = MathHelper.Clamp(BeamHitLength - 6f, 16f, BeamHitLength);

            if (Projectile.owner == Main.myPlayer) {
                if (!_loopStarted) {
                    SoundStyle loopStyle = SoundID.Item15;
                    loopStyle.IsLooped           = true;
                    loopStyle.MaxInstances       = 1;
                    loopStyle.SoundLimitBehavior = SoundLimitBehavior.IgnoreNew;

                    _loopSlot    = SoundEngine.PlaySound(loopStyle, Projectile.Center);
                    _loopStarted = true;
                }

                if (SoundEngine.TryGetActiveSound(_loopSlot, out ActiveSound active))
                    active.Position = Projectile.Center;
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 1.6f, 0.6f);
        }

        private float GetBeamLength(Vector2 start, Vector2 dir) {
            // Stop at tiles, like Last Prism.
            float[] samples = new float[3];
            Collision.LaserScan(start, dir, BeamThickness, MaxLength, samples);

            float tileLength = 0f;
            for (int i = 0; i < samples.Length; i++)
                tileLength += samples[i];
            tileLength /= samples.Length;

            // Safety clamp
            if (tileLength < 16f) tileLength       = 16f;
            if (tileLength > MaxLength) tileLength = MaxLength;

            // Now also stop at the first enemy we intersect (without killing the projectile).
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

            Vector2 start  = owner.Center + dir * StartOffset;
            float   length = BeamHitLength;
            Vector2 end    = start + dir * length;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                start,
                end,
                BeamThickness,
                ref _
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

            Vector2 start  = owner.Center + dir * StartOffset;
            float   length = BeamDrawLength;

            Texture2D tex = TextureAssets.Projectile[Type].Value;

            int frameHeight = tex.Height / 3;
            int frameWidth  = tex.Width;

            Rectangle startFrame = new Rectangle(0, 0 * frameHeight, frameWidth, frameHeight);
            Rectangle midFrame   = new Rectangle(0, 1 * frameHeight, frameWidth, frameHeight);
            Rectangle endFrame   = new Rectangle(0, 2 * frameHeight, frameWidth, frameHeight);

            // Last Prism laser is authored "up" in texture space and faces opposite our direction.
            float rot = dir.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;

            Vector2 origin = new Vector2(frameWidth * 0.5f, frameHeight * 0.5f);

            float t       = Main.GlobalTimeWrappedHourly;
            float pulse   = 0.88f + 0.12f * (float)System.Math.Sin(t * 10f);
            float shimmer = 0.82f + 0.18f * (float)System.Math.Sin(t * 6.5f);

            float intensity = 1.25f;
            Color baseColor = new Color(60, 255, 140) * (shimmer * intensity);

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

            // Start cap
            Main.EntitySpriteDraw(
                tex,
                start - Main.screenPosition,
                startFrame,
                baseColor,
                rot,
                origin,
                new Vector2(1.55f * pulse, 1f),
                SpriteEffects.None,
                0
            );

            // Middle segments: no fade-in (prevents gap), only fade near the end
            float step = frameHeight * 0.60f;
            float i    = step * 0.50f;

            while (i < length - step * 0.50f) {
                float along = i / length;

                float fadeOut = 1f;
                if (along > 0.90f)
                    fadeOut = MathHelper.SmoothStep(1f, 0f, (along - 0.90f) / 0.10f);

                Vector2 pos = start + dir * i;

                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, midFrame, baseColor * (0.18f * fadeOut), rot,
                    origin, new Vector2(2.55f * pulse, 1f), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, midFrame, baseColor * (0.32f * fadeOut), rot,
                    origin, new Vector2(1.85f * pulse, 1f), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, midFrame, Color.White * (0.58f * fadeOut), rot,
                    origin, new Vector2(1.25f * pulse, 1f), SpriteEffects.None, 0);

                i += step;
            }

            // End cap
            Vector2 endPos = start + dir * length;
            Main.EntitySpriteDraw(
                tex,
                endPos - Main.screenPosition,
                endFrame,
                baseColor * 1.15f,
                rot,
                origin,
                new Vector2(1.55f * pulse, 1f),
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
}
