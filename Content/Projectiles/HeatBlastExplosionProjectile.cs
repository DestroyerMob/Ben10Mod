using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastExplosionProjectile : ModProjectile {
    public const float ModeBomb = 0f;
    public const float ModeFireball = 1f;
    public const float ModeFlarePop = 2f;
    public const float ModeRodPulse = 3f;

    private float Radius => Projectile.ai[0] > 0f ? Projectile.ai[0] : 48f;
    private float Mode => Projectile.ai[1];

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        int diameter = (int)(Radius * 2f);
        hitbox = new Rectangle(
            (int)(Projectile.Center.X - Radius),
            (int)(Projectile.Center.Y - Radius),
            diameter,
            diameter);
    }

    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) {
        if (Main.dedServ)
            return;

        float dustScale = Mode == ModeFireball ? 1.5f : Mode == ModeBomb ? 1.25f : 0.95f;
        int dustCount = Mode == ModeFireball ? 32 : Mode == ModeBomb ? 24 : 16;
        Color dustColor = Mode == ModeRodPulse
            ? new Color(255, 188, 120)
            : Mode == ModeFlarePop
                ? new Color(255, 232, 182)
                : new Color(255, 145, 68);

        SoundEngine.PlaySound((Mode == ModeFireball ? SoundID.Item62 : SoundID.Item14) with {
            Pitch = Mode == ModeFlarePop ? 0.15f : -0.1f,
            Volume = Mode == ModeFireball ? 0.75f : 0.55f
        }, Projectile.Center);

        for (int i = 0; i < dustCount; i++) {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.8f, 5.8f) * dustScale;
            int dustType = Main.rand.NextBool(3) ? DustID.InfernoFork : Main.rand.NextBool() ? DustID.Flare : DustID.Torch;
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Radius * 0.18f, Radius * 0.18f),
                dustType, velocity, 100, dustColor, Main.rand.NextFloat(1f, 1.55f) * dustScale);
            dust.noGravity = true;
        }
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.34f, 0.08f) * (Mode == ModeFireball ? 1.3f : 1f));
    }
}
