using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ArctiguanaBreathProjectile : ModProjectile {
    private const int LifetimeTicks = 24;
    private const float MinLength = 34f;
    private const float MaxLength = 118f;
    private const float MinWidth = 18f;
    private const float MaxWidth = 48f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 86;
        Projectile.height = 86;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != "Ben10Mod:Arctiguana") {
            Projectile.Kill();
            return;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.rotation = direction.ToRotation();

        float progress = GetProgress();
        float length = GetBreathLength(progress);
        Projectile.scale = MathHelper.Lerp(0.75f, 1.15f, progress);
        Projectile.Center = GetBreathStart(owner, direction) + direction * length * 0.52f;

        Lighting.AddLight(Projectile.Center, new Vector3(0.18f, 0.55f, 0.78f) * Projectile.scale);
        SpawnBreathDust(owner, direction, progress, length);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float progress = GetProgress();
        Vector2 start = GetBreathStart(owner, direction);
        Vector2 end = start + direction * GetBreathLength(progress);
        float collisionPoint = 0f;
        float width = MathHelper.Lerp(MinWidth, MaxWidth, progress) * Projectile.scale;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width,
            ref collisionPoint);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool alreadyChilled = target.HasBuff(BuffID.Frostburn2) || target.HasBuff(ModContent.BuffType<EnemySlow>());
        target.AddBuff(BuffID.Frostburn2, 180);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), 150);
        if (alreadyChilled) {
            target.AddBuff(ModContent.BuffType<EnemyFrozen>(), 24);
            target.netUpdate = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float progress = GetProgress();
        Vector2 start = GetBreathStart(owner, direction) - Main.screenPosition;
        float length = GetBreathLength(progress);
        float width = MathHelper.Lerp(MinWidth, MaxWidth, progress) * Projectile.scale;
        Vector2 center = start + direction * (length * 0.5f);
        float rotation = direction.ToRotation();

        Main.EntitySpriteDraw(pixel, center, null, new Color(95, 190, 255, 170), rotation,
            Vector2.One * 0.5f, new Vector2(length, width), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(210, 245, 255, 150), rotation,
            Vector2.One * 0.5f, new Vector2(length * 0.88f, width * 0.48f), SpriteEffects.None, 0);
        return false;
    }

    private float GetProgress() {
        return 1f - Projectile.timeLeft / (float)LifetimeTicks;
    }

    private static Vector2 GetBreathStart(Player owner, Vector2 direction) {
        return owner.MountedCenter + new Vector2(owner.direction * 8f, -10f) + direction * 12f;
    }

    private static float GetBreathLength(float progress) {
        return MathHelper.Lerp(MinLength, MaxLength, MathHelper.Clamp(progress * 1.2f, 0f, 1f));
    }

    private void SpawnBreathDust(Player owner, Vector2 direction, float progress, float length) {
        if (Main.dedServ)
            return;

        Vector2 start = GetBreathStart(owner, direction);
        Vector2 normal = new(-direction.Y, direction.X);
        int dustCount = Main.rand.NextBool(2) ? 2 : 1;
        float width = MathHelper.Lerp(MinWidth, MaxWidth, progress) * Projectile.scale;

        for (int i = 0; i < dustCount; i++) {
            float distance = Main.rand.NextFloat(0.15f, 0.98f) * length;
            Vector2 position = start + direction * distance + normal * Main.rand.NextFloat(-width * 0.35f, width * 0.35f);
            Dust dust = Dust.NewDustPerfect(position, i % 2 == 0 ? DustID.IceTorch : DustID.Frost,
                direction * Main.rand.NextFloat(0.3f, 1.1f) + normal * Main.rand.NextFloat(-0.35f, 0.35f),
                105, new Color(175, 235, 255), Main.rand.NextFloat(0.95f, 1.18f));
            dust.noGravity = true;
        }
    }
}
