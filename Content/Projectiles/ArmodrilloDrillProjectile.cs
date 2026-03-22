using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArmodrilloDrillProjectile : ModProjectile {
    private const int Lifetime = 16;
    private const float MinReach = 12f;
    private const float MaxReach = 56f;
    private const float HitboxLead = 16f;
    private const float HandleOffsetX = 10f;
    private const float HandleOffsetY = -4f;

    public override string Texture => $"Terraria/Images/Item_{ItemID.ChlorophyteJackhammer}";

    public override void SetDefaults() {
        Projectile.width = 36;
        Projectile.height = 36;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 3;
        Projectile.timeLeft = Lifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float progress = 1f - Projectile.timeLeft / (float)Lifetime;
        float thrust = progress < 0.38f
            ? Utils.GetLerpValue(0f, 0.38f, progress, true)
            : Utils.GetLerpValue(1f, 0.38f, progress, true);
        float reach = MathHelper.Lerp(MinReach, MaxReach, thrust);
        owner.direction = direction.X >= 0f ? 1 : -1;
        Vector2 handPosition = GetHandPosition(owner);
        Vector2 drillHeadPosition = handPosition + direction * (reach + HitboxLead);

        Projectile.Center = drillHeadPosition;
        Projectile.rotation = direction.ToRotation() - MathHelper.PiOver4;
        owner.itemRotation = direction.ToRotation() * owner.direction;
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, direction.ToRotation() - MathHelper.PiOver2);
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(drillHeadPosition + Main.rand.NextVector2Circular(8f, 8f), DustID.Smoke,
                direction * Main.rand.NextFloat(1.1f, 2.2f), 110, new Color(170, 130, 70), 1.1f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return false;

        Texture2D texture = TextureAssets.Item[ItemID.ChlorophyteJackhammer].Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 drawPosition = GetHandPosition(owner) - Main.screenPosition;
        Vector2 origin = new(8f, texture.Height - 8f);
        SpriteEffects effects = direction.X < 0f ? SpriteEffects.FlipVertically : SpriteEffects.None;
        float rotation = direction.ToRotation() - MathHelper.PiOver4 + (direction.X < 0f ? MathHelper.Pi : 0f);

        Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale,
            effects, 0);
        return false;
    }

    private static Vector2 GetHandPosition(Player owner) {
        return owner.MountedCenter + new Vector2(owner.direction * HandleOffsetX, HandleOffsetY);
    }
}
