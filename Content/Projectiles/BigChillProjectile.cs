using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.BigChill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BigChillProjectile : ModProjectile {
    private bool AbsoluteZero => Projectile.ai[0] >= 0.5f;
    private bool PhaseDriftEmpowered => Projectile.ai[1] >= 0.5f;
    private bool UltimateForm =>
        Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && BigChillTransformation.IsUltimateBigChill(Main.player[Projectile.owner]);

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 72;
        Projectile.extraUpdates = 2;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, GetLightColor());

        if (Projectile.velocity.LengthSquared() < (UltimateForm ? 784f : 676f))
            Projectile.velocity *= AbsoluteZero ? 1.02f : UltimateForm ? 1.016f : 1.012f;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                GetDustType(),
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.11f), 100,
                GetDustColor(),
                Main.rand.NextFloat(0.9f, AbsoluteZero ? 1.18f : 1.06f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float rotation = direction.ToRotation();
        float drawScale = (UltimateForm ? 1.12f : 1f) * (PhaseDriftEmpowered ? 1.08f : 1f);
        Color outer = GetOuterColor();
        Color inner = GetInnerColor();

        Main.EntitySpriteDraw(pixel, center, null, outer, rotation, Vector2.One * 0.5f,
            new Vector2(18f, 6f) * Projectile.scale * drawScale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center - direction * 2f, null, inner, rotation, Vector2.One * 0.5f,
            new Vector2(10f, 3f) * Projectile.scale * drawScale, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        BigChillTransformation.ResolveCryoLanceHit(Projectile, target, damageDone);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 8; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, GetDustType(i),
                Main.rand.NextVector2Circular(2.2f, 2.2f), 105,
                GetDustColor(),
                Main.rand.NextFloat(0.9f, 1.15f));
            dust.noGravity = true;
        }
    }

    private Vector3 GetLightColor() {
        if (UltimateForm)
            return AbsoluteZero ? new Vector3(0.74f, 0.14f, 0.12f) : new Vector3(0.56f, 0.1f, 0.16f);

        return AbsoluteZero ? new Vector3(0.22f, 0.46f, 0.74f) : new Vector3(0.14f, 0.32f, 0.56f);
    }

    private Color GetOuterColor() {
        if (UltimateForm)
            return AbsoluteZero ? new Color(255, 118, 98, 220) : new Color(228, 84, 112, 214);

        return AbsoluteZero ? new Color(126, 216, 255, 218) : new Color(88, 178, 228, 208);
    }

    private Color GetInnerColor() {
        if (UltimateForm)
            return PhaseDriftEmpowered
                ? new Color(255, 244, 246, 236)
                : AbsoluteZero
                    ? new Color(255, 226, 214, 228)
                    : new Color(255, 208, 218, 226);

        return PhaseDriftEmpowered ? new Color(255, 250, 255, 232) : new Color(220, 242, 255, 224);
    }

    private Color GetDustColor() {
        if (UltimateForm)
            return AbsoluteZero ? new Color(255, 190, 172) : new Color(255, 160, 184);

        return AbsoluteZero ? new Color(200, 245, 255) : new Color(188, 232, 255);
    }

    private int GetDustType(int index = 0) {
        if (UltimateForm)
            return index % 2 == 0 ? DustID.Torch : DustID.Flare;

        return index % 2 == 0 ? DustID.IceTorch : DustID.Frost;
    }
}
