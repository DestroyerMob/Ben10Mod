using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class AstrodactylCometProjectile : ModProjectile {
    private bool Hyperflight => Projectile.ai[0] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, Hyperflight ? new Vector3(0.28f, 1f, 0.72f) : new Vector3(0.2f, 0.9f, 0.62f));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                Main.rand.NextBool(3) ? DustID.GreenTorch : DustID.GemEmerald,
                -Projectile.velocity * Main.rand.NextFloat(0.03f, 0.1f), 100, new Color(180, 255, 225),
                Main.rand.NextFloat(0.95f, Hyperflight ? 1.24f : 1.1f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = direction.ToRotation();

        Main.EntitySpriteDraw(pixel, center - direction * 14f, null, new Color(60, 225, 130, 100), rotation, Vector2.One * 0.5f,
            new Vector2(44f, 9f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(225, 255, 235, 220), rotation, Vector2.One * 0.5f,
            new Vector2(18f, 6f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire3, Hyperflight ? 300 : 240);
        target.AddBuff(BuffID.Oiled, 120);
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            int shardDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.36f));
            const int ShardCount = 3;

            for (int i = 0; i < ShardCount; i++) {
                Vector2 velocity = (-MathHelper.PiOver2 + MathHelper.Lerp(-0.55f, 0.55f, i / (float)(ShardCount - 1))).ToRotationVector2() *
                    Main.rand.NextFloat(10f, 13f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                    ModContent.ProjectileType<AstrodactylPlasmaBoltProjectile>(), shardDamage, Projectile.knockBack * 0.5f,
                    Projectile.owner, Hyperflight ? 1f : 0f);
            }
        }

        if (Main.dedServ)
            return;

        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenTorch : DustID.GemEmerald,
                Main.rand.NextVector2Circular(3.5f, 3.5f), 100, new Color(185, 255, 225), Main.rand.NextFloat(0.95f, 1.22f));
            dust.noGravity = true;
        }
    }
}
