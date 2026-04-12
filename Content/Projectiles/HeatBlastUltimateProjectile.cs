using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Ben10Mod.Content.Projectiles.UltimateAttacks;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.HeatBlast;

namespace Ben10Mod.Content.Projectiles
{
    public class HeatBlastUltimateProjectile : ChargedThrownUltimateProjectile
    {
        protected override Vector2 ChargeOffset => new(0f, -78f);
        protected override float InitialScale => 0.3f;
        protected override float MaxChargeScale => 2.2f;
        protected override float ChargeStep => 0.038f;
        protected override float LaunchSpeed => 5f;
        protected override int MaxLifetime => 15 * 60;

        private float ChargeRatio => MathHelper.Clamp((Projectile.scale - InitialScale) / (MaxChargeScale - InitialScale), 0f, 1f);

        protected override void ConfigureDefaults() {
            Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        protected override void UpdateCharging(Player owner) {
            float radius = 58f * Projectile.scale;
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());
            for (int i = 0; i < 35; i++) {
                if (Main.rand.NextBool(2)) {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(radius, radius);
                    Dust d = Dust.NewDustPerfect(pos, DustID.InfernoFork,
                        Main.rand.NextVector2Circular(1f, 2.5f), 90,
                        new Color(255, 90, 0), Main.rand.NextFloat(2.1f, 3.4f));
                    d.noGravity = true;
                }
            }
        }

        protected override void OnLaunched(Player owner) {
            float chargeRatio = ChargeRatio;
            Projectile.tileCollide = true;
            Projectile.damage = Math.Max(1, (int)Math.Round(Projectile.damage * (1.12f + chargeRatio * 1.78f)));
            Projectile.knockBack += 1f + chargeRatio * 3.2f;
            Projectile.velocity *= MathHelper.Lerp(0.95f, 1.28f, chargeRatio);
            Projectile.localAI[0] = chargeRatio;
        }

        protected override void UpdateReleased(Player owner) {
            float radius = 58f * Projectile.scale;
            Projectile.rotation += 0.08f + Projectile.velocity.Length() * 0.014f;
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());
            for (int i = 0; i < 28; i++) {
                if (Main.rand.NextBool(2)) {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(radius * 0.8f, radius * 0.8f);
                    Dust d = Dust.NewDustPerfect(pos, DustID.InfernoFork,
                        Projectile.velocity * -0.15f, 100,
                        new Color(255, 110, 0), 2.4f);
                    d.noGravity = true;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Projectile.velocity = oldVelocity * 0.1f;
            return true;
        }

        public override void OnKill(int timeLeft) {
            if (IsCharging)
                return;

            HeatBlastTransformation.OnUltimateFireballDetonated(Projectile, ChargeRatio);
        }

        public override bool PreDraw(ref Color lightColor) {
            Texture2D tex    = TextureAssets.Projectile[Projectile.type].Value;
            Vector2   origin = tex.Size() / 2f;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor,
                Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false; // skip default draw
        }
    }
}
