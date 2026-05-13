using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations.Humungousaur;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UltimateHumungousaurRocketPlayerProjectile : ModProjectile {
    private bool Charged => Projectile.ai[0] >= 1f;
    private bool Cataclysm => Projectile.ai[0] >= 2f;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.RocketI}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.penetrate = 1;
        Projectile.timeLeft = 150;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        Projectile.scale = Cataclysm ? 1.22f : Charged ? 1.12f : 1f;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, Cataclysm ? 1.15f : 0.95f, 0.25f, 0.15f);

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                Cataclysm ? DustID.Torch : DustID.Smoke, 0f, 0f, 105, new Color(255, 175, 115),
                Main.rand.NextFloat(0.95f, 1.25f));
            dust.velocity *= 0.35f;
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Explode(target.Center, target.whoAmI);
    }

    public override void OnKill(int timeLeft) {
        Explode(Projectile.Center);
    }

    private void Explode(Vector2 center, int ignoredTarget = -1) {
        if (Projectile.localAI[0] > 0f)
            return;

        Projectile.localAI[0] = 1f;
        float radius = Cataclysm ? 132f : Charged ? 112f : 96f;
        int splashDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * (Cataclysm ? 0.68f : Charged ? 0.56f : 0.46f)));

        Player owner = Main.player[Projectile.owner];
        if (Main.netMode != NetmodeID.MultiplayerClient && owner.active && !owner.dead) {
            foreach (NPC npc in Main.ActiveNPCs) {
                if (npc.whoAmI == ignoredTarget || !npc.CanBeChasedBy(Projectile))
                    continue;

                float distance = npc.Center.Distance(center);
                if (distance > radius)
                    continue;

                float falloff = MathHelper.Lerp(1f, 0.58f, distance / radius);
                int areaDamage = System.Math.Max(1, (int)System.Math.Round(splashDamage * falloff));
                int hitDirection = npc.Center.X >= owner.Center.X ? 1 : -1;
                npc.SimpleStrikeNPC(areaDamage, hitDirection, false, Projectile.knockBack * 0.45f,
                    ModContent.GetInstance<HeroDamage>());
                UltimateHumungousaurTransformation.ApplyBreachHit(owner, npc, Cataclysm ? 2 : 1,
                    UltimateHumungousaurTransformation.BreachDurationTicks);
                UltimateHumungousaurTransformation.TryConsumeShattered(owner, npc, Projectile.GetSource_FromThis(),
                    System.Math.Max(1, (int)System.Math.Round(Projectile.damage * (Cataclysm ? 0.84f : 0.62f))),
                    Projectile.knockBack + 0.7f, Cataclysm);
            }
        }

        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Item14 with { Pitch = Cataclysm ? 0.02f : 0.08f, Volume = Cataclysm ? 0.62f : 0.5f },
            center);
        int dustCount = Cataclysm ? 34 : Charged ? 26 : 20;
        for (int i = 0; i < dustCount; i++) {
            Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(radius * 0.18f, radius * 0.18f),
                i % 3 == 0 ? DustID.Firework_Red : i % 3 == 1 ? DustID.Torch : DustID.Smoke,
                Main.rand.NextVector2Circular(Cataclysm ? 5.4f : 4.2f, Cataclysm ? 5.4f : 4.2f), 105,
                new Color(255, 172, 112), Main.rand.NextFloat(1f, Cataclysm ? 1.72f : 1.4f));
            dust.noGravity = true;
        }
    }
}
