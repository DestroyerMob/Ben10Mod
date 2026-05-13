using Ben10Mod.Content.Players;
using Ben10Mod.Content.Transformations.Humungousaur;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UltimateHumungousaurMeteorStompProjectile : ModProjectile {
    private const int WindupFrames = 8;
    private const int LaunchFrames = 10;
    private const float BaseLaunchVelocity = -9.8f;
    private const float CataclysmLaunchVelocity = -11.6f;
    private const float BaseSlamVelocity = 17f;
    private const float CataclysmSlamVelocity = 20f;
    private const float MaxSlamVelocity = 34f;
    private const float CataclysmMaxSlamVelocity = 39f;
    private const float SlamAcceleration = 2.8f;

    private ref float Phase => ref Projectile.localAI[0];
    private ref float PhaseTimer => ref Projectile.localAI[1];
    private bool Cataclysm => Projectile.ai[0] >= 0.5f;
    private bool StartedGrounded => Projectile.ai[1] >= 0.5f;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 60;
        Projectile.height = 84;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.timeLeft = 150;
    }

    public override bool? CanDamage() => false;

    public override bool PreDraw(ref Color lightColor) => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;
        owner.noFallDmg = true;
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = System.Math.Max(owner.immuneTime, Cataclysm ? 10 : 8);
        owner.armorEffectDrawShadow = true;
        owner.GetModPlayer<HumungousaurCombatPlayer>().RegisterAttackGuard(3, Cataclysm ? 0.28f : 0.21f);

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
        owner.velocity.X *= StartedGrounded ? 0.42f : 0.72f;
        owner.velocity.Y = StartedGrounded ? 0f : MathHelper.Lerp(owner.velocity.Y, -1.6f, 0.24f);

        if (!Main.dedServ && Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(20f, 8f),
                Main.rand.NextBool(3) ? DustID.Torch : DustID.Smoke,
                new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.3f, -0.2f)), 110,
                new Color(244, 184, 122), Main.rand.NextFloat(0.95f, Cataclysm ? 1.42f : 1.24f));
            dust.noGravity = true;
        }

        if (PhaseTimer < WindupFrames)
            return;

        Phase = StartedGrounded ? 1f : 2f;
        PhaseTimer = 0f;
        if (StartedGrounded)
            owner.velocity.Y = Cataclysm ? CataclysmLaunchVelocity : BaseLaunchVelocity;
    }

    private void UpdateLaunch(Player owner) {
        PhaseTimer++;
        owner.velocity.X *= 0.86f;

        if (!Main.dedServ && Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(18f, 10f), DustID.Stone,
                new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(0.6f, 3.2f)), 95,
                Color.White, Main.rand.NextFloat(0.92f, Cataclysm ? 1.3f : 1.14f));
            dust.noGravity = true;
        }

        if (PhaseTimer < LaunchFrames && owner.velocity.Y < -2.2f)
            return;

        Phase = 2f;
        PhaseTimer = 0f;
        owner.velocity.Y = System.Math.Max(owner.velocity.Y, Cataclysm ? CataclysmSlamVelocity : BaseSlamVelocity);
    }

    private void UpdateSlam(Player owner) {
        PhaseTimer++;
        owner.velocity.X *= 0.72f;
        owner.velocity.Y = MathHelper.Clamp(owner.velocity.Y + SlamAcceleration,
            Cataclysm ? CataclysmSlamVelocity : BaseSlamVelocity,
            Cataclysm ? CataclysmMaxSlamVelocity : MaxSlamVelocity);

        if (!Main.dedServ) {
            Dust dust = Dust.NewDustPerfect(owner.Bottom + Main.rand.NextVector2Circular(18f, 10f),
                Main.rand.NextBool(Cataclysm ? 2 : 4) ? DustID.Torch : DustID.Smoke,
                new Vector2(Main.rand.NextFloat(-0.45f, 0.45f), Main.rand.NextFloat(1.2f, 3.2f)), 110,
                new Color(242, 184, 126), Main.rand.NextFloat(1f, Cataclysm ? 1.48f : 1.28f));
            dust.noGravity = true;
        }

        if (PhaseTimer < 3f || !AlienIdentityPlayer.IsGrounded(owner))
            return;

        owner.velocity.Y = 0f;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.immuneTime = System.Math.Max(owner.immuneTime, Cataclysm ? 22 : 16);
        UltimateHumungousaurTransformation.SpawnMeteorStompImpact(owner, Projectile.GetSource_FromThis(),
            Projectile.damage, Projectile.knockBack, Cataclysm);
        Projectile.Kill();
    }
}
