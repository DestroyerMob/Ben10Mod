using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class StinkFlyPoisonProjectile : ModProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 96;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI() {
        if (Projectile.velocity.LengthSquared() < 484f)
            Projectile.velocity *= 1.01f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.24f, 0.06f));

        if (Main.rand.NextBool(2)) {
            Dust acidDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                Main.rand.NextBool() ? DustID.GreenBlood : DustID.Poisoned,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.1f), 100, new Color(210, 235, 90),
                Main.rand.NextFloat(0.9f, 1.18f));
            acidDust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle source = new(0, 0, 1, 1);
        Vector2 origin = new(0.5f, 0.5f);
        float rotation = Projectile.rotation;

        Main.EntitySpriteDraw(pixel, drawPosition, source, new Color(202, 210, 66, 220), rotation, origin,
            new Vector2(15f, 5f) * Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, drawPosition + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 1.5f, source,
            new Color(249, 255, 160, 205), rotation, origin, new Vector2(7f, 2.5f) * Projectile.scale, SpriteEffects.None, 0);
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        bool slimed = target.HasBuff(ModContent.BuffType<EnemySlow>());
        target.AddBuff(BuffID.Poisoned, 5 * 60);

        if (!slimed)
            return;

        target.AddBuff(BuffID.Venom, 2 * 60);
        TryChainFromSlimedTarget(target);
    }

    private void TryChainFromSlimedTarget(NPC sourceTarget) {
        if (Projectile.ai[0] >= 1f)
            return;

        if (Projectile.owner != Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
            return;

        NPC chainTarget = FindChainTarget(sourceTarget);
        if (chainTarget == null)
            return;

        Vector2 chainDirection = sourceTarget.Center.DirectionTo(chainTarget.Center)
            .SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
        int chainDamage = Math.Max(1, (int)Math.Round(Projectile.damage * 0.62f));
        int projectileIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), sourceTarget.Center, chainDirection * 17f,
            Type, chainDamage, Projectile.knockBack * 0.7f, Projectile.owner, 1f);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;
    }

    private NPC FindChainTarget(NPC sourceTarget) {
        const float maxChainRange = 360f;
        NPC bestTarget = null;
        float bestScore = maxChainRange;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC candidate = Main.npc[i];
            if (candidate == null || candidate.whoAmI == sourceTarget.whoAmI || !candidate.active ||
                !candidate.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(sourceTarget.Center, candidate.Center);
            if (distance > maxChainRange)
                continue;

            if (!Collision.CanHitLine(sourceTarget.position, sourceTarget.width, sourceTarget.height, candidate.position,
                    candidate.width, candidate.height))
                continue;

            float score = candidate.HasBuff(ModContent.BuffType<EnemySlow>()) ? distance * 0.72f : distance;
            if (score >= bestScore)
                continue;

            bestScore = score;
            bestTarget = candidate;
        }

        return bestTarget;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 9; i++) {
            Dust burst = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenBlood : DustID.Poisoned,
                Main.rand.NextVector2Circular(2.3f, 2.3f), 96, new Color(220, 245, 110), Main.rand.NextFloat(0.95f, 1.25f));
            burst.noGravity = true;
        }
    }
}
