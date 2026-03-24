using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class DiamondHeadSpireProjectile : ModProjectile {
    private const int LifetimeTicks = 28;
    private const float StartScale = 0.25f;
    private const float MaxScale = 1.1f;

    public override string Texture => "Ben10Mod/Content/Projectiles/GiantDiamondProjectile";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 50;
        Projectile.height = 118;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.scale = StartScale;
        Projectile.localAI[0] = Projectile.Bottom.Y;
    }

    public override void AI() {
        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float easedProgress = progress * progress * (3f - 2f * progress);
        Projectile.scale = MathHelper.Lerp(StartScale, MaxScale, easedProgress);
        UpdateAnchoredPosition();

        Lighting.AddLight(Projectile.Center, 0.2f, 0.34f, 0.48f);
        SpawnSpireDust();
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        int width = (int)(Projectile.width * Projectile.scale);
        int height = (int)(Projectile.height * Projectile.scale);
        hitbox = new Rectangle(
            (int)(Projectile.Center.X - width * 0.5f),
            (int)(Projectile.localAI[0] - height),
            width,
            height
        );
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 12; i++) {
            Dust dust = Dust.NewDustPerfect(new Vector2(Projectile.Center.X, Projectile.localAI[0]) +
                Main.rand.NextVector2Circular(16f, 8f), DustID.GemDiamond,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 95, new Color(220, 255, 255), Main.rand.NextFloat(1f, 1.45f));
            dust.noGravity = true;
        }
    }

    private void UpdateAnchoredPosition() {
        float scaledHeight = Projectile.height * Projectile.scale;
        Projectile.Center = new Vector2(Projectile.Center.X, Projectile.localAI[0] - scaledHeight * 0.5f);
    }

    private void SpawnSpireDust() {
        if (Main.dedServ || !Main.rand.NextBool(2))
            return;

        float scaledHeight = Projectile.height * Projectile.scale;
        Vector2 dustPosition = new(
            Projectile.Center.X + Main.rand.NextFloat(-Projectile.width * Projectile.scale * 0.32f, Projectile.width * Projectile.scale * 0.32f),
            Projectile.localAI[0] - Main.rand.NextFloat(8f, scaledHeight)
        );

        Dust dust = Dust.NewDustPerfect(dustPosition, DustID.GemDiamond,
            new Vector2(Main.rand.NextFloat(-0.45f, 0.45f), Main.rand.NextFloat(-2.4f, -0.8f)), 100,
            new Color(200, 255, 255), Main.rand.NextFloat(0.95f, 1.25f));
        dust.noGravity = true;
    }
}
