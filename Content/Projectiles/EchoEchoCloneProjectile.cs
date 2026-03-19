using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Ben10Mod.Content.Buffs.Summons;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class EchoEchoCloneProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private ref float AttackTimer => ref Projectile.ai[0];
    private ref float VisualSeed => ref Projectile.ai[1];

    public override void SetStaticDefaults() {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = false;
        Projectile.minion = true;
        Projectile.minionSlots = 1f;
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
            owner.ClearBuff(ModContent.BuffType<EchoEchoCloneBuff>());
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != "Ben10Mod:EchoEcho") {
            Projectile.Kill();
            return;
        }

        if (owner.HasBuff(ModContent.BuffType<EchoEchoCloneBuff>()))
            Projectile.timeLeft = 2;

        int cloneIndex = GetCloneIndex();
        float angle = MathHelper.TwoPi * cloneIndex / Math.Max(1f, owner.ownedProjectileCounts[Type]);
        float bob = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4.2f + cloneIndex * 0.65f) * 7f;
        Vector2 idleOffset = new Vector2(54f + 10f * cloneIndex, -42f + bob).RotatedBy(angle * 0.32f);
        NPC target = FindTarget(owner, 620f);
        Vector2 targetCenter = owner.Center + idleOffset;

        if (target != null) {
            Vector2 attackOffset = target.DirectionFrom(owner.Center) * 76f;
            targetCenter = target.Center - attackOffset + new Vector2(0f, -18f + bob);
        }

        Projectile.Center = Vector2.Lerp(Projectile.Center, targetCenter, 0.22f);
        Projectile.rotation = Projectile.velocity.X * 0.02f;

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Firework_Red,
                Main.rand.NextVector2Circular(1f, 1f), 120, new Color(255, 140, 140), 0.9f);
            dust.noGravity = true;
        }

        int attackRate = omp.PrimaryAbilityEnabled ? 22 : 34;
        AttackTimer++;

        if (target != null && AttackTimer >= attackRate && Main.myPlayer == Projectile.owner) {
            AttackTimer = Main.rand.Next(4);
            Vector2 direction = Projectile.Center.DirectionTo(target.Center);
            if (direction != Vector2.Zero) {
                Vector2 velocity = direction * 11f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                    ModContent.ProjectileType<EchoEchoSonicBlastProjectile>(), Projectile.damage, 0f, Projectile.owner);
            }
        }
    }

    private int GetCloneIndex() {
        int index = 0;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile other = Main.projectile[i];
            if (!other.active || other.owner != Projectile.owner || other.type != Projectile.type || other.whoAmI == Projectile.whoAmI)
                continue;

            if (other.whoAmI < Projectile.whoAmI)
                index++;
        }

        return index;
    }

    private NPC FindTarget(Player owner, float maxDistance) {
        NPC closestTarget = null;
        float closestDistance = maxDistance;

        if (owner.HasMinionAttackTargetNPC) {
            NPC forcedTarget = Main.npc[owner.MinionAttackTargetNPC];
            if (forcedTarget.CanBeChasedBy(Projectile) && Projectile.Center.Distance(forcedTarget.Center) < closestDistance)
                return forcedTarget;
        }

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
        SpriteEffects effects = Projectile.velocity.X < 0f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Main.EntitySpriteDraw(pixel, center + new Vector2(0f, -10f), null, new Color(255, 120, 120, 110), 0f,
            Vector2.One * 0.5f, new Vector2(7f, 7f), effects, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(0f, 3f), null, new Color(255, 90, 90, 95), Projectile.rotation,
            Vector2.One * 0.5f, new Vector2(11f, 14f), effects, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(-4f, 15f), null, new Color(255, 90, 90, 90), 0f,
            Vector2.One * 0.5f, new Vector2(3f, 10f), effects, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(4f, 15f), null, new Color(255, 90, 90, 90), 0f,
            Vector2.One * 0.5f, new Vector2(3f, 10f), effects, 0);
        return false;
    }
}
