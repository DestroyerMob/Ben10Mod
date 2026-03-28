using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class SnareOhBindFieldProjectile : ModProjectile {
    private const int LifetimeTicks = 4 * 60;
    private const float MaxRadius = 84f;

    private float CurrentRadius {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = LifetimeTicks;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        float progress = 1f - Projectile.timeLeft / (float)LifetimeTicks;
        float fade = Utils.GetLerpValue(0f, 24f, Projectile.timeLeft, true);
        CurrentRadius = MaxRadius * (0.5f + 0.5f * fade) * (0.88f + 0.12f * (1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5.5f)) * 0.5f);

        HoldNearbyEnemies();
        Lighting.AddLight(Projectile.Center, new Vector3(0.34f, 0.28f, 0.12f));
        SpawnBindDust(progress);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return targetHitbox.Distance(Projectile.Center) <= CurrentRadius;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        identity.ApplySnareOhCurse(Projectile.owner, OwnerExposedCore() ? 3 : 2, 280);
        target.velocity *= OwnerExposedCore() ? 0.06f : 0.14f;
        if (OwnerExposedCore())
            identity.ConsumeSnareOhCurse(Projectile.owner, 1);
        target.netUpdate = true;
    }

    private void HoldNearbyEnemies() {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance > CurrentRadius || distance <= 6f)
                continue;

            Vector2 toCenter = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
            float holdStrength = MathHelper.Lerp(OwnerExposedCore() ? 0.28f : 0.22f, 0.08f, distance / CurrentRadius);
            Vector2 desiredVelocity = toCenter * MathHelper.Lerp(OwnerExposedCore() ? 1.9f : 1.2f, 0.45f, distance / CurrentRadius);
            npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, holdStrength);
            npc.GetGlobalNPC<AlienIdentityGlobalNPC>().ApplySnareOhCurse(Projectile.owner, 1, 60);
        }
    }

    private void SpawnBindDust(float progress) {
        if (Main.dedServ)
            return;

        int points = Math.Max(10, (int)Math.Round(CurrentRadius / 10f));
        float rotation = Main.GlobalTimeWrappedHourly * (2.4f + progress * 1.3f);

        for (int i = 0; i < points; i++) {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 direction = angle.ToRotationVector2();
            Vector2 position = Projectile.Center + direction * Main.rand.NextFloat(CurrentRadius * 0.45f, CurrentRadius);
            Vector2 velocity = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.4f, 1.5f);

            Dust dust = Dust.NewDustPerfect(position, i % 4 == 0 ? DustID.GoldFlame : DustID.Sand, velocity, 105,
                new Color(235, 215, 170), Main.rand.NextFloat(0.85f, 1.12f));
            dust.noGravity = true;
        }
    }

    private bool OwnerExposedCore() {
        Player owner = Main.player[Projectile.owner];
        return owner.active && !owner.dead && owner.GetModPlayer<OmnitrixPlayer>().PrimaryAbilityEnabled;
    }
}
