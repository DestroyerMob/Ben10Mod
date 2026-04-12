using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.EchoEcho;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoResonancePopProjectile : ModProjectile {
    private const int LifetimeTicks = 16;

    private bool ChainSuppressed => Projectile.ai[0] >= 0.5f;
    private float ScaleMultiplier => Projectile.ai[1] <= 0f ? 1f : Projectile.ai[1];
    private float Progress => 1f - Projectile.timeLeft / (float)LifetimeTicks;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = LifetimeTicks + 4;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            if (!Main.dedServ)
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.32f, Volume = 0.36f }, Projectile.Center);

            MaybeSpawnChorusChains();
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.72f, 1f) * (0.42f + 0.15f * ScaleMultiplier));
        SpawnPopDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float radius = MathHelper.Lerp(12f, 40f, (float)System.Math.Sqrt(Progress)) * ScaleMultiplier;
        return targetHitbox.Distance(Projectile.Center) <= radius;
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    private void MaybeSpawnChorusChains() {
        if (ChainSuppressed)
            return;

        Player owner = Main.player[Projectile.owner];
        if (!owner.active || !owner.GetModPlayer<EchoEchoStatePlayer>().ChorusActive)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient && owner.whoAmI != Main.myPlayer)
            return;

        for (int i = 0; i < 2; i++) {
            Vector2 offset = new Vector2(18f * (i == 0 ? -1f : 1f), -10f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + offset, Vector2.Zero,
                Type, (int)(Projectile.damage * 0.65f), Projectile.knockBack * 0.75f, Projectile.owner, 1f,
                ScaleMultiplier * 0.72f);
        }
    }

    private void SpawnPopDust() {
        if (Main.dedServ)
            return;

        float radius = MathHelper.Lerp(8f, 34f, Progress) * ScaleMultiplier;
        for (int i = 0; i < 3; i++) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * radius;
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                i % 2 == 0 ? DustID.WhiteTorch : DustID.GemDiamond,
                offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.4f, 1.2f), 95,
                new Color(190, 235, 255), Main.rand.NextFloat(0.76f, 1.16f) * ScaleMultiplier);
            dust.noGravity = true;
        }
    }
}
