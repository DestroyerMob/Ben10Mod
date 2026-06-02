using Ben10Mod.Content.Transformations.FourArms;
using Ben10Mod.Content.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsGroundSlamSequenceProjectile : ModProjectile {
    private const int WindupFrames = 10;
    private const int LaunchFrames = 12;
    private const float LaunchVelocity = -10.5f;
    private const float SlamStartVelocity = 15f;
    private const float SlamVelocity = 30f;
    private const float SlamAcceleration = 2.6f;
    private const float HorizontalDamp = 0.82f;

    private ref float Phase => ref Projectile.localAI[0];
    private ref float PhaseTimer => ref Projectile.localAI[1];
    private bool Berserked => Projectile.ai[0] >= 0.5f;
    private bool StartedGrounded => Projectile.ai[1] >= 0.5f;
    private bool PotisInfused => Projectile.ai[2] >= 0.5f;
    private int WindupDuration => PotisInfused ? 8 : WindupFrames;
    private int LaunchDuration => PotisInfused ? 10 : LaunchFrames;
    private float CurrentLaunchVelocity => PotisInfused ? -11.8f : LaunchVelocity;
    private float CurrentSlamVelocity => PotisInfused ? 34f : SlamVelocity;
    private float CurrentSlamAcceleration => PotisInfused ? 3.2f : SlamAcceleration;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 80;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.timeLeft = 120;
    }

    public override bool? CanDamage() => false;

    public override bool PreDraw(ref Color lightColor) => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        owner.GetModPlayer<FourArmsGroundSlamPlayer>().RegisterGroundSlamState();
        Projectile.Center = owner.Center;

        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;
        owner.noFallDmg = true;
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = System.Math.Max(owner.immuneTime, 6);

        switch ((int)Phase) {
            case 0:
                UpdateWindup(owner);
                break;
            case 1:
                UpdateLaunch(owner);
                break;
            default:
                UpdateSlam(owner);
                break;
        }
    }

    private void UpdateWindup(Player owner) {
        PhaseTimer++;
        owner.velocity.X *= StartedGrounded ? 0.45f : 0.8f;

        if (StartedGrounded) {
            owner.velocity.Y = 0f;
        }
        else {
            owner.velocity.Y = MathHelper.Lerp(owner.velocity.Y, -1.5f, 0.22f);
        }

        if (!Main.dedServ && Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(18f, 8f), DustID.Smoke,
                new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.2f, -0.2f)), 105,
                new Color(220, 185, 165), Main.rand.NextFloat(0.95f, 1.3f));
            dust.noGravity = true;
        }

        if (PhaseTimer < WindupDuration)
            return;

        Phase = StartedGrounded ? 1f : 2f;
        PhaseTimer = 0f;
        if (StartedGrounded)
            owner.velocity.Y = CurrentLaunchVelocity;
    }

    private void UpdateLaunch(Player owner) {
        PhaseTimer++;
        owner.velocity.X *= 0.9f;

        if (PhaseTimer >= LaunchDuration || owner.velocity.Y >= -2.5f) {
            Phase = 2f;
            PhaseTimer = 0f;
            owner.velocity.Y = System.Math.Max(owner.velocity.Y, SlamStartVelocity);
            return;
        }

        if (!Main.dedServ && Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(18f, 10f), DustID.Stone,
                new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 2.8f)), 95,
                Color.White, Main.rand.NextFloat(0.9f, 1.2f));
            dust.noGravity = true;
        }
    }

    private void UpdateSlam(Player owner) {
        PhaseTimer++;
        owner.velocity.X *= HorizontalDamp;
        owner.velocity.Y = MathHelper.Clamp(owner.velocity.Y + CurrentSlamAcceleration, SlamStartVelocity, CurrentSlamVelocity);

        if (!Main.dedServ && Main.rand.NextBool()) {
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(18f, 12f),
                PotisInfused && Main.rand.NextBool(3) ? DustID.Torch : DustID.Smoke,
                new Vector2(Main.rand.NextFloat(-0.45f, 0.45f), Main.rand.NextFloat(0.8f, PotisInfused ? 3.4f : 2.8f)), 105,
                PotisInfused ? new Color(255, 190, 120) : new Color(215, 180, 160),
                Main.rand.NextFloat(PotisInfused ? 1.12f : 1.02f, PotisInfused ? 1.55f : 1.35f));
            dust.noGravity = true;
        }

        if (!AlienIdentityPlayer.IsGrounded(owner))
            return;

        owner.velocity.Y = 0f;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.immuneTime = System.Math.Max(owner.immuneTime, 16);
        SoundEngine.PlaySound(SoundID.Item14 with { Pitch = PotisInfused ? -0.28f : -0.18f, Volume = PotisInfused ? 0.96f : 0.85f },
            owner.Center);
        EmitImpactDust(owner);
        SpawnImpactProjectiles(owner);
        Projectile.Kill();
    }

    private void SpawnImpactProjectiles(Player owner) {
        Vector2 spawnPosition = owner.Bottom + new Vector2(0f, -10f);
        var source = Projectile.GetSource_FromThis();

        Projectile.NewProjectile(source, spawnPosition, Vector2.Zero,
            ModContent.ProjectileType<FourArmsLandingShockwaveProjectile>(), Projectile.damage, Projectile.knockBack,
            owner.whoAmI, PotisInfused ? Berserked ? 1.78f : 1.48f : Berserked ? 1.35f : 1.12f);

        if (!Berserked && !PotisInfused)
            return;

        int fissureDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * (PotisInfused ? 0.84f : 0.72f)));
        Projectile.NewProjectile(source, spawnPosition + new Vector2(10f, 0f), Vector2.Zero,
            ModContent.ProjectileType<FourArmsFissureProjectile>(), fissureDamage, Projectile.knockBack + 1f,
            owner.whoAmI, 1f, PotisInfused ? 1f : 0f);
        Projectile.NewProjectile(source, spawnPosition + new Vector2(-10f, 0f), Vector2.Zero,
            ModContent.ProjectileType<FourArmsFissureProjectile>(), fissureDamage, Projectile.knockBack + 1f,
            owner.whoAmI, -1f, PotisInfused ? 1f : 0f);
    }

    private void EmitImpactDust(Player owner) {
        int dustCount = PotisInfused ? 42 : 28;
        for (int i = 0; i < dustCount; i++) {
            Vector2 velocity = new Vector2(Main.rand.NextFloat(PotisInfused ? -7.2f : -5.8f, PotisInfused ? 7.2f : 5.8f),
                Main.rand.NextFloat(PotisInfused ? -4.2f : -3.4f, 0.25f));
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(24f, 10f),
                PotisInfused && i % 5 == 0 ? DustID.WhiteTorch : i % 4 == 0 ? DustID.Torch : DustID.Smoke,
                velocity, 95,
                i % 4 == 0 || PotisInfused ? new Color(255, 190, 115) : new Color(230, 215, 205),
                Main.rand.NextFloat(PotisInfused ? 1.18f : 1.05f, PotisInfused ? 1.9f : 1.65f));
            dust.noGravity = true;
        }
    }
}
