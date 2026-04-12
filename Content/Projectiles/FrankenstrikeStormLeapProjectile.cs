using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Transformations.Frankenstrike;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FrankenstrikeStormLeapProjectile : ModProjectile {
    public const int MinLeapFrames = 7;
    public const int MaxLeapFrames = 20;
    private const float BaseLeapSpeed = 22f;
    private const float StormheartLeapSpeed = 27f;

    private bool Stormheart => Projectile.ai[0] >= 0.5f;

    public static float GetLeapSpeed(bool stormheart) => stormheart ? StormheartLeapSpeed : BaseLeapSpeed;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 48;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MinLeapFrames;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        owner.GetModPlayer<OmnitrixPlayer>().RegisterActiveLunge();

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            if (Stormheart) {
                FrankenstrikeTransformation.SpawnLightningStrike(owner, Projectile.GetSource_FromThis(), owner.Center,
                    System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.32f)), Projectile.knockBack + 0.35f, 2, 0.72f);
            }
        }

        Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
        float leapSpeed = GetLeapSpeed(Stormheart);
        Projectile.velocity = direction * leapSpeed;
        Projectile.rotation = direction.ToRotation();
        Projectile.Center = owner.Center + direction * (Stormheart ? 22f : 18f);

        owner.velocity = Projectile.velocity;
        owner.direction = direction.X >= 0f ? 1 : -1;
        owner.immune = true;
        owner.immuneNoBlink = true;
        owner.immuneTime = System.Math.Max(owner.immuneTime, Stormheart ? 16 : 12);
        owner.noKnockback = true;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.noFallDmg = true;
        owner.armorEffectDrawShadow = true;

        if (Main.rand.NextBool(Stormheart ? 1 : 2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                Main.rand.NextBool(3) ? DustID.Electric : DustID.BlueTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.14f), 115, new Color(175, 228, 255),
                Main.rand.NextFloat(0.95f, Stormheart ? 1.3f : 1.1f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) => false;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (identity.IsFrankenstrikeOverchargedFor(Projectile.owner))
            modifiers.SourceDamage *= 1.12f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return;

        FrankenstrikeTransformation.ApplyConductiveHit(owner, target, Stormheart ? 3 : 2, 250);
    }

    public override void OnKill(int timeLeft) {
        Player owner = Main.player[Projectile.owner];
        if (owner.active && !owner.dead)
            owner.velocity *= 0.45f;

        FrankenstrikeTransformation.SpawnThunderclap(owner, Projectile.GetSource_FromThis(), Projectile.Center,
            System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.5f)), Projectile.knockBack, 1f, empowered: Stormheart);

        NPC overchargedTarget = FindOverchargedTarget(owner, 112f);
        if (overchargedTarget != null) {
            FrankenstrikeTransformation.TryConsumeOvercharged(owner, overchargedTarget, Projectile.GetSource_FromThis(),
                System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.88f)), Projectile.knockBack + 1.2f,
                chainBurst: true, lightningStrike: true);
        }
        else if (Stormheart) {
            FrankenstrikeTransformation.SpawnLightningStrike(owner, Projectile.GetSource_FromThis(), Projectile.Center,
                System.Math.Max(1, (int)System.Math.Round(Projectile.damage * 0.36f)), Projectile.knockBack + 0.4f, 4, 0.82f);
        }

        if (Main.dedServ)
            return;

        for (int i = 0; i < 16; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(3.4f, 3.4f), 110, new Color(175, 228, 255), Main.rand.NextFloat(0.95f, 1.22f));
            dust.noGravity = true;
        }
    }

    private NPC FindOverchargedTarget(Player owner, float maxDistance) {
        if (owner == null || !owner.active)
            return null;

        NPC bestTarget = null;
        float bestDistance = maxDistance;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            float distance = npc.Center.Distance(Projectile.Center);
            if (!identity.IsFrankenstrikeOverchargedFor(owner.whoAmI) || distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestTarget = npc;
        }

        return bestTarget;
    }
}
