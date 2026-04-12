using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.EyeGuy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EyeGuySpectrumBurstProjectile : ModProjectile {
    public const int ModeCompoundVision = 0;
    public const int ModeSpectrumBreak = 1;
    public const int ModeAllEyesOpen = 2;

    private const int LifetimeTicks = 16;

    private int Mode => Utils.Clamp((int)Math.Round(Projectile.ai[0]), ModeCompoundVision, ModeAllEyesOpen);
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
            if (!Main.dedServ) {
                SoundEngine.PlaySound((Mode == ModeCompoundVision ? SoundID.Item27 : SoundID.Item62) with {
                    Pitch = Mode == ModeAllEyesOpen ? 0.18f : 0.34f,
                    Volume = Mode == ModeCompoundVision ? 0.26f : 0.34f
                }, Projectile.Center);
            }

            MaybeSpawnAllEyesOpenLances();
        }

        Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.42f, 0.64f) *
            (Mode == ModeCompoundVision ? 0.42f : 0.58f));
        SpawnPopDust();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float radius = Mode switch {
            ModeCompoundVision => MathHelper.Lerp(10f, 28f, (float)System.Math.Sqrt(Progress)),
            ModeSpectrumBreak => MathHelper.Lerp(14f, 40f, (float)System.Math.Sqrt(Progress)),
            _ => MathHelper.Lerp(18f, 48f, (float)System.Math.Sqrt(Progress))
        };

        return targetHitbox.Distance(Projectile.Center) <= radius * ScaleMultiplier;
    }

    public override bool PreDraw(ref Color lightColor) => false;

    private void MaybeSpawnAllEyesOpenLances() {
        if (Mode != ModeAllEyesOpen)
            return;

        Player owner = Main.player[Projectile.owner];
        if (!owner.active)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient && owner.whoAmI != Main.myPlayer)
            return;

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f));
        EyeGuyElement[] elements = { EyeGuyElement.Fire, EyeGuyElement.Shock };
        float[] offsets = { -0.4f, 0.4f };

        for (int i = 0; i < offsets.Length; i++) {
            Vector2 boltDirection = direction.RotatedBy(offsets[i]);
            int flags = EyeGuyLaserbeam.FlagOverload | EyeGuyLaserbeam.FlagDisableShockChain;
            int damage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.58f));
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + boltDirection * 10f,
                boltDirection * 22f, ModContent.ProjectileType<EyeGuyLaserbeam>(), damage, Projectile.knockBack * 0.7f,
                Projectile.owner, (float)elements[i], flags);
        }
    }

    private void SpawnPopDust() {
        if (Main.dedServ)
            return;

        float radius = MathHelper.Lerp(8f, Mode == ModeCompoundVision ? 24f : 36f, Progress) * ScaleMultiplier;
        Color[] colors = {
            new(255, 135, 90),
            new(145, 235, 255),
            new(165, 205, 255)
        };

        for (int i = 0; i < 4; i++) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * radius;
            Color dustColor = colors[(i + Projectile.identity) % colors.Length];
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset,
                i % 3 == 0 ? DustID.Electric : (i % 2 == 0 ? DustID.GoldFlame : DustID.GemDiamond),
                offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.4f, 1.2f), 95,
                dustColor, Main.rand.NextFloat(0.78f, 1.2f) * ScaleMultiplier);
            dust.noGravity = true;
        }
    }
}
