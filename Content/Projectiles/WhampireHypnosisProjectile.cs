using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WhampireHypnosisProjectile : ModProjectile {
    private const int LifetimeTicks = 22;
    private const float BaseLength = 126f;
    private const float CloakedLength = 174f;
    private const float BaseWidth = 18f;
    private const float CloakedWidth = 26f;

    private bool Cloaked => Projectile.ai[0] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 88;
        Projectile.height = 88;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead ||
            owner.GetModPlayer<OmnitrixPlayer>().currentTransformationId != "Ben10Mod:Whampire") {
            Projectile.Kill();
            return;
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Projectile.rotation = direction.ToRotation();
        Projectile.Center = GetBeamStart(owner, direction) + direction * GetBeamLength() * 0.5f;

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.25f, Volume = 0.6f }, owner.Center);
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.82f, 0.1f, 0.14f) * 0.52f);

        if (Main.rand.NextBool(2)) {
            Vector2 start = GetBeamStart(owner, direction);
            float distance = Main.rand.NextFloat(0.15f, 1f) * GetBeamLength();
            Vector2 position = start + direction * distance +
                direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-GetBeamWidth() * 0.3f, GetBeamWidth() * 0.3f);
            Dust dust = Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.Blood : DustID.Shadowflame,
                direction * Main.rand.NextFloat(0.25f, 0.8f), 120, new Color(165, 35, 50), Main.rand.NextFloat(0.75f, 1f));
            dust.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 start = GetBeamStart(owner, direction);
        Vector2 end = start + direction * GetBeamLength();
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, GetBeamWidth(),
            ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Player owner = Main.player[Projectile.owner];
        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        Vector2 start = GetBeamStart(owner, direction) - Main.screenPosition;
        float length = GetBeamLength();
        float width = GetBeamWidth();
        Vector2 center = start + direction * (length * 0.5f);
        float rotation = direction.ToRotation();

        Main.EntitySpriteDraw(pixel, center, null, new Color(35, 8, 14, 220), rotation, Vector2.One * 0.5f,
            new Vector2(length, width), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(140, 20, 32, 190), rotation, Vector2.One * 0.5f,
            new Vector2(length * 0.92f, width * 0.55f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(235, 115, 128, 150), rotation, Vector2.One * 0.5f,
            new Vector2(length * 0.72f, width * 0.22f), SpriteEffects.None, 0);

        Vector2 endpoint = start + direction * length;
        Main.EntitySpriteDraw(pixel, endpoint, null, new Color(255, 205, 215, 150), 0f, Vector2.One * 0.5f,
            new Vector2(width * 0.55f, width * 0.55f), SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Confused, Cloaked ? 360 : 300);
        target.AddBuff(ModContent.BuffType<EnemySlow>(), Cloaked ? 240 : 180);
        target.AddBuff(BuffID.Weak, Cloaked ? 300 : 240);
        target.netUpdate = true;
    }

    private float GetBeamLength() => Cloaked ? CloakedLength : BaseLength;

    private float GetBeamWidth() => Cloaked ? CloakedWidth : BaseWidth;

    private static Vector2 GetBeamStart(Player owner, Vector2 direction) {
        return owner.MountedCenter + new Vector2(owner.direction * 8f, -10f) + direction * 12f;
    }
}
