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
    private const float MinArmReach = 6f;
    private const float MaxArmReach = 26f;
    private const float DrillTipLead = 26f;
    private const float HandNormalOffset = 5f;

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
        float thrust = GetThrustAmount();
        owner.direction = direction.X >= 0f ? 1 : -1;
        Vector2 handPosition = GetHandPosition(owner, direction, thrust);
        Vector2 drillHeadPosition = handPosition + direction * DrillTipLead;

        Projectile.Center = drillHeadPosition;
        Projectile.rotation = direction.ToRotation();
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
        Vector2 drawPosition = GetHandPosition(owner, direction, GetThrustAmount()) - Main.screenPosition;
        Vector2 origin = new(8f, texture.Height - 8f);
        float rotation = direction.ToRotation();

        Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale,
            SpriteEffects.None, 0);
        return false;
    }

    private float GetThrustAmount() {
        float progress = 1f - Projectile.timeLeft / (float)Lifetime;
        return progress < 0.38f
            ? Utils.GetLerpValue(0f, 0.38f, progress, true)
            : Utils.GetLerpValue(1f, 0.38f, progress, true);
    }

    private static Vector2 GetHandPosition(Player owner, Vector2 direction, float thrust) {
        Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
        float armReach = MathHelper.Lerp(MinArmReach, MaxArmReach, thrust);
        return owner.MountedCenter + direction * armReach - normal * HandNormalOffset;
    }
}
