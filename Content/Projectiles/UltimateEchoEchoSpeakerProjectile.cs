using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ben10Mod.Content.Buffs.Summons;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UltimateEchoEchoSpeakerProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 26;
        Projectile.height = 26;
        Projectile.friendly = false;
        Projectile.minion = true;
        Projectile.minionSlots = 0f;
        Projectile.netImportant = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18000;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            owner.ClearBuff(ModContent.BuffType<UltimateEchoEchoSpeakerBuff>());
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != "Ben10Mod:UltimateEchoEcho") {
            Projectile.Kill();
            return;
        }

        if (owner.HasBuff(ModContent.BuffType<UltimateEchoEchoSpeakerBuff>()))
            Projectile.timeLeft = 2;

        Vector2 sentryCenter = new Vector2(Projectile.ai[0], Projectile.ai[1]);
        if (sentryCenter == Vector2.Zero)
            sentryCenter = Projectile.Center;

        Projectile.Center = Vector2.Lerp(Projectile.Center, sentryCenter, 0.22f);
        if (Projectile.Center.Distance(sentryCenter) < 10f) {
            Projectile.Center = sentryCenter;
            Projectile.velocity = Vector2.Zero;
        }
        NPC target = FindClosestNPC(460f);
        Projectile.localAI[0]++;
        Projectile.rotation = target != null
            ? Projectile.DirectionTo(target.Center).ToRotation()
            : Projectile.rotation;

        if (Main.rand.NextBool(2)) {
            Vector2 dustVelocity = Main.rand.NextVector2Circular(0.8f, 0.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueCrystalShard, dustVelocity, 130,
                new Color(90, 190, 255), 1.1f);
            dust.noGravity = true;
        }

        int fireRate = omp.PrimaryAbilityEnabled ? 22 : 34;
        if (target != null && Projectile.localAI[0] >= fireRate && Main.myPlayer == Projectile.owner) {
            Projectile.localAI[0] = 0f;

            for (int i = 0; i < 8; i++) {
                Vector2 burstVelocity = Main.rand.NextVector2CircularEdge(2.2f, 2.2f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueCrystalShard, burstVelocity, 100,
                    new Color(120, 210, 255), 1.25f);
                dust.noGravity = true;
            }

            Vector2 velocity = Projectile.DirectionTo(target.Center) * 12f;
            int attackDamage = Projectile.damage;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(), attackDamage, 0f, Projectile.owner);
        }
    }

    private NPC FindClosestNPC(float maxDistance) {
        NPC closestTarget = null;
        float closestDistance = maxDistance;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Projectile.Center.Distance(npc.Center);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = npc;
        }

        return closestTarget;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = Projectile.rotation + MathHelper.PiOver2;

        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 80, 80, 110), rotation, Vector2.One * 0.5f,
            new Vector2(18f, 18f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(255, 220, 220, 170), rotation, Vector2.One * 0.5f,
            new Vector2(10f, 10f), SpriteEffects.None, 0);
        return false;
    }
}
