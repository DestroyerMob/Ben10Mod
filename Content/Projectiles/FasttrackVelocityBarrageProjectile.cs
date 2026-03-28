using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FasttrackVelocityBarrageProjectile : ModProjectile {
    private const int BarrageLifetime = 30;
    private const int StrikeInterval = 4;

    private float MomentumRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private bool Overdrive => Projectile.ai[1] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = BarrageLifetime;
        Projectile.hide = true;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead ||
            owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != "Ben10Mod:Fasttrack") {
            Projectile.Kill();
            return;
        }

        Vector2 direction = ResolveAimDirection(owner, Projectile.velocity);
        Projectile.velocity = direction;
        Projectile.Center = owner.MountedCenter + direction * 12f;
        Projectile.rotation = direction.ToRotation();

        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = Math.Max(owner.itemTime, 2);
        owner.itemAnimation = Math.Max(owner.itemAnimation, 2);
        owner.itemRotation = direction.ToRotation() * owner.direction;
        owner.armorEffectDrawShadow = true;

        int interval = Math.Max(2, (int)Math.Round(MathHelper.Lerp(StrikeInterval, 2f, MomentumRatio)));
        if ((int)Projectile.localAI[0] % interval == 0) {
            SpawnStrike(owner, direction);
        }

        Projectile.localAI[0]++;
        Lighting.AddLight(Projectile.Center, Vector3.Lerp(new Vector3(0.1f, 0.45f, 0.3f), new Vector3(0.15f, 0.74f, 0.46f), MomentumRatio));

        if (Main.rand.NextBool(Overdrive ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(16f, 20f),
                Main.rand.NextBool() ? DustID.GemEmerald : DustID.GreenFairy,
                -direction * Main.rand.NextFloat(0.8f, 2f) + Main.rand.NextVector2Circular(0.8f, 0.8f), 100,
                new Color(160, 255, 225), Main.rand.NextFloat(0.85f, 1.08f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Player owner = Main.player[Projectile.owner];
        Vector2 center = owner.Center - Main.screenPosition;
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        Color outer = Color.Lerp(new Color(10, 16, 20, 70), new Color(20, 26, 30, 80), MomentumRatio);
        Color inner = Color.Lerp(new Color(90, 225, 190, 60), new Color(140, 255, 220, 70), MomentumRatio);

        for (int i = -1; i <= 1; i++) {
            Vector2 offset = direction * (i * 10f) + normal * (i * 8f);
            Main.EntitySpriteDraw(pixel, center + offset, null, outer, Projectile.rotation, Vector2.One * 0.5f,
                new Vector2(26f, 8f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(pixel, center + offset, null, inner, Projectile.rotation, Vector2.One * 0.5f,
                new Vector2(16f, 4f), SpriteEffects.None, 0);
        }

        return false;
    }

    private void SpawnStrike(Player owner, Vector2 direction) {
        if (Main.netMode == NetmodeID.MultiplayerClient && owner.whoAmI != Main.myPlayer)
            return;

        if (Main.netMode == NetmodeID.Server)
            return;

        int strikeIndex = (int)Projectile.localAI[1];
        float spread = strikeIndex % 3 switch {
            1 => -0.12f,
            2 => 0.12f,
            _ => 0f
        };

        Vector2 strikeDirection = direction.RotatedBy(spread);
        float scale = MathHelper.Lerp(1.08f, Overdrive ? 1.26f : 1.22f, MomentumRatio);
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.MountedCenter + strikeDirection * 14f, strikeDirection,
            ModContent.ProjectileType<FasttrackPunchProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner,
            scale, MomentumRatio);

        SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.36f, Volume = 0.55f }, owner.Center);
        Projectile.localAI[1]++;
    }

    private static Vector2 ResolveAimDirection(Player player, Vector2 fallbackVelocity) {
        Vector2 direction = fallbackVelocity.SafeNormalize(new Vector2(player.direction, 0f));

        if (Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer) {
            Vector2 mouseDirection = player.DirectionTo(Main.MouseWorld);
            if (mouseDirection != Vector2.Zero)
                direction = mouseDirection;
        }

        return direction;
    }
}
