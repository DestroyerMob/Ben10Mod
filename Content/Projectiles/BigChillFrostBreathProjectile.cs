using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.BigChill;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BigChillFrostBreathProjectile : ModProjectile {
    private const int LifetimeTicks = 14;
    private const float MinLength = 38f;
    private const float BaseMaxLength = 106f;
    private const float PhaseDriftBonusLength = 34f;
    private const float AbsoluteZeroBonusLength = 30f;
    private const float MinWidth = 13f;
    private const float BaseMaxWidth = 28f;

    private bool AbsoluteZero => Projectile.ai[0] >= 0.5f;
    private bool PhaseDriftEmpowered => Projectile.ai[1] >= 0.5f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 84;
        Projectile.height = 84;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || !BigChillStatePlayer.IsBigChillTransformationId(owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId)) {
            Projectile.Kill();
            return;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.rotation = direction.ToRotation();
        Projectile.scale = MathHelper.Lerp(0.88f, AbsoluteZero ? 1.26f : 1.08f, GetProgress());
        Projectile.Center = GetBreathStart(owner, direction) + direction * GetBreathLength() * 0.52f;
        Projectile.localNPCHitCooldown = AbsoluteZero ? 8 : 10;

        Lighting.AddLight(Projectile.Center,
            AbsoluteZero ? new Vector3(0.24f, 0.48f, 0.74f) : new Vector3(0.16f, 0.34f, 0.58f));
        SpawnBreathDust(owner, direction);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 start = GetBreathStart(owner, direction);
        Vector2 end = start + direction * GetBreathLength();
        float collisionPoint = 0f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, GetBreathWidth(),
            ref collisionPoint);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        BigChillTransformation.ResolveBreathHit(Projectile, target, damageDone);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 start = GetBreathStart(owner, direction) - Main.screenPosition;
        float length = GetBreathLength();
        float width = GetBreathWidth();
        Vector2 center = start + direction * (length * 0.5f);
        float rotation = direction.ToRotation();

        Main.EntitySpriteDraw(pixel, center, null, new Color(110, 200, 255, 172), rotation,
            Vector2.One * 0.5f, new Vector2(length, width), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(210, 245, 255, 158), rotation,
            Vector2.One * 0.5f, new Vector2(length * 0.88f, width * 0.44f), SpriteEffects.None, 0);
        if (PhaseDriftEmpowered) {
            Main.EntitySpriteDraw(pixel, center, null, new Color(255, 250, 255, 102), rotation,
                Vector2.One * 0.5f, new Vector2(length * 0.78f, width * 0.18f), SpriteEffects.None, 0);
        }
        return false;
    }

    private float GetProgress() {
        return 1f - Projectile.timeLeft / (float)LifetimeTicks;
    }

    private float GetBreathLength() {
        float maxLength = BaseMaxLength +
                          (PhaseDriftEmpowered ? PhaseDriftBonusLength : 0f) +
                          (AbsoluteZero ? AbsoluteZeroBonusLength : 0f);
        return MathHelper.Lerp(MinLength, maxLength, MathHelper.Clamp(GetProgress() * 1.25f, 0f, 1f));
    }

    private float GetBreathWidth() {
        float maxWidth = BaseMaxWidth + (AbsoluteZero ? 12f : 0f);
        return MathHelper.Lerp(MinWidth, maxWidth, MathHelper.Clamp(GetProgress() * 1.2f, 0f, 1f)) * Projectile.scale;
    }

    private static Vector2 GetBreathStart(Player owner, Vector2 direction) {
        return owner.MountedCenter + new Vector2(owner.direction * 8f, -10f) + direction * 12f;
    }

    private void SpawnBreathDust(Player owner, Vector2 direction) {
        if (Main.dedServ)
            return;

        Vector2 start = GetBreathStart(owner, direction);
        Vector2 normal = new(-direction.Y, direction.X);
        float length = GetBreathLength();
        float width = GetBreathWidth();
        int dustCount = AbsoluteZero ? 3 : 2;
        for (int i = 0; i < dustCount; i++) {
            if (!Main.rand.NextBool(2))
                continue;

            float distance = Main.rand.NextFloat(0.12f, 0.98f) * length;
            Vector2 position = start + direction * distance +
                               normal * Main.rand.NextFloat(-width * 0.32f, width * 0.32f);
            Dust dust = Dust.NewDustPerfect(position, i % 2 == 0 ? DustID.IceTorch : DustID.Frost,
                direction * Main.rand.NextFloat(0.35f, 1.2f) + normal * Main.rand.NextFloat(-0.25f, 0.25f),
                105, AbsoluteZero ? new Color(198, 242, 255) : new Color(172, 225, 255),
                Main.rand.NextFloat(0.92f, AbsoluteZero ? 1.24f : 1.1f));
            dust.noGravity = true;
        }
    }
}
