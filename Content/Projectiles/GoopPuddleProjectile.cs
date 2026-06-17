using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GoopPuddleProjectile : ModProjectile {
    public const int BaseLifetime = 5 * 60;
    public const int MaxLifetime = 8 * 60;
    public const int MaxOwnedPuddles = 8;
    public const float PuddleWidth = 54f;
    public const float PuddleHeight = 18f;
    public const float MaxPuddleScale = 1.8f;
    private const float MergeRange = 48f;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

    public override void SetDefaults() {
        Projectile.width = (int)PuddleWidth;
        Projectile.height = (int)PuddleHeight;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = BaseLifetime;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.hide = true;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 20;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        Projectile.velocity = Vector2.Zero;
        if (Projectile.ai[0] <= 0f)
            Projectile.ai[0] = 1f;

        float puddleScale = GetPuddleScale(Projectile);
        Projectile.scale = puddleScale;
        int dissolveRadius = (int)(PuddleWidth * 0.48f * puddleScale);
        if (Projectile.owner == Main.myPlayer || Main.netMode != NetmodeID.MultiplayerClient)
            ApplyDissolveAura(dissolveRadius);

        float fade = Utils.GetLerpValue(0f, 24f, Projectile.timeLeft, true);
        if (Main.rand.NextBool(2)) {
            Vector2 position = Projectile.Bottom + new Vector2(Main.rand.NextFloat(-PuddleWidth * 0.45f, PuddleWidth * 0.45f) * puddleScale,
                Main.rand.NextFloat(-PuddleHeight, 0f) * puddleScale);
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.45f, 0.45f), Main.rand.NextFloat(-1.6f, -0.35f));
            Dust drip = Dust.NewDustPerfect(position, Main.rand.NextBool(3) ? DustID.GreenTorch : DustID.GreenMoss, velocity,
                95, new Color(125, 245, 145), Main.rand.NextFloat(0.95f, 1.2f) * fade);
            drip.noGravity = true;
        }

        if (Projectile.timeLeft == BaseLifetime - 1) {
            for (int i = 0; i < 14; i++) {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 offset = new Vector2((float)Math.Cos(angle) * PuddleWidth * 0.42f * puddleScale,
                    (float)Math.Sin(angle) * PuddleHeight * 0.32f * puddleScale);
                Dust ring = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GreenTorch, Vector2.Zero, 80,
                    new Color(115, 235, 130), 1.05f);
                ring.noGravity = true;
            }
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Rectangle puddleHitbox = GetPuddleHitbox(Projectile);
        return puddleHitbox.Intersects(targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<GoopDissolved>(), 4 * 60);
        target.AddBuff(BuffID.Venom, 2 * 60);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 10; i++) {
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-1.8f, 1.8f), Main.rand.NextFloat(-2.2f, -0.4f));
            Dust splash = Dust.NewDustPerfect(Projectile.Bottom + new Vector2(Main.rand.NextFloat(-PuddleWidth * 0.4f, PuddleWidth * 0.4f) * Projectile.scale, -4f),
                i % 2 == 0 ? DustID.GreenMoss : DustID.GreenTorch, velocity, 95, new Color(115, 240, 135), Main.rand.NextFloat(0.9f, 1.15f));
            splash.noGravity = false;
        }
    }

    public static int CreateOrGrow(IEntitySource source, Vector2 center, int damage, int owner, float startingScale = 1f,
        float growth = 0.2f, int refreshTime = BaseLifetime) {
        Projectile existing = FindClosestOwnedPuddle(owner, center, MergeRange);
        if (existing != null) {
            existing.ai[0] = MathHelper.Clamp(GetPuddleScale(existing) + growth, 1f, MaxPuddleScale);
            existing.damage = Math.Max(existing.damage, damage);
            existing.timeLeft = Math.Min(MaxLifetime, Math.Max(existing.timeLeft, refreshTime));
            existing.netUpdate = true;
            EmitMergeDust(existing.Center, GetPuddleScale(existing));
            return existing.whoAmI;
        }

        EnforcePuddleLimit(owner);
        int projectileIndex = Projectile.NewProjectile(source, center, Vector2.Zero,
            ModContent.ProjectileType<GoopPuddleProjectile>(), damage, 0f, owner,
            MathHelper.Clamp(startingScale, 1f, MaxPuddleScale));

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;

        return projectileIndex;
    }

    public static int DetonateOwnedPuddles(int owner, IEntitySource source, float damageMultiplier, int minimumDamage) {
        int detonated = 0;
        int detonationType = ModContent.ProjectileType<GoopPuddleDetonationProjectile>();

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile puddle = Main.projectile[i];
            if (!IsOwnedActivePuddle(puddle, owner))
                continue;

            float puddleScale = GetPuddleScale(puddle);
            int damage = Math.Max(minimumDamage,
                (int)Math.Round(puddle.damage * damageMultiplier * MathHelper.Lerp(0.95f, 1.35f, (puddleScale - 1f) / (MaxPuddleScale - 1f))));
            int projectileIndex = Projectile.NewProjectile(source, puddle.Center, Vector2.Zero, detonationType, damage, 0f,
                owner, puddleScale);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                Main.projectile[projectileIndex].netUpdate = true;

            puddle.Kill();
            detonated++;
        }

        return detonated;
    }

    public static int CountOwnedPuddles(int owner) {
        int count = 0;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            if (IsOwnedActivePuddle(Main.projectile[i], owner))
                count++;
        }

        return count;
    }

    public static Projectile FindOwnedPuddleAtPoint(int owner, Vector2 point, float padding = 0f) {
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile puddle = Main.projectile[i];
            if (IsOwnedActivePuddle(puddle, owner) && ContainsPoint(puddle, point, padding))
                return puddle;
        }

        return null;
    }

    public static bool IsOwnedActivePuddle(Projectile projectile, int owner) {
        return projectile != null && projectile.active &&
               projectile.type == ModContent.ProjectileType<GoopPuddleProjectile>() &&
               projectile.owner == owner;
    }

    public static float GetPuddleScale(Projectile puddle) {
        return MathHelper.Clamp(puddle.ai[0] <= 0f ? 1f : puddle.ai[0], 1f, MaxPuddleScale);
    }

    private static Projectile FindClosestOwnedPuddle(int owner, Vector2 point, float maxDistance) {
        Projectile bestPuddle = null;
        float bestDistance = maxDistance;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile puddle = Main.projectile[i];
            if (!IsOwnedActivePuddle(puddle, owner))
                continue;

            float distance = Vector2.Distance(puddle.Center, point);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestPuddle = puddle;
        }

        return bestPuddle;
    }

    private static Rectangle GetPuddleHitbox(Projectile puddle, float padding = 0f) {
        float puddleScale = GetPuddleScale(puddle);
        float width = PuddleWidth * puddleScale + padding * 2f;
        float height = PuddleHeight * puddleScale + padding * 2f;
        float bottom = puddle.Center.Y + PuddleHeight * 0.5f;
        return new Rectangle(
            (int)(puddle.Center.X - width * 0.5f),
            (int)(bottom - height),
            (int)width,
            (int)height
        );
    }

    private static bool ContainsPoint(Projectile puddle, Vector2 point, float padding = 0f) {
        Rectangle hitbox = GetPuddleHitbox(puddle, padding);
        return point.X >= hitbox.Left && point.X <= hitbox.Right && point.Y >= hitbox.Top && point.Y <= hitbox.Bottom;
    }

    private static void EnforcePuddleLimit(int owner) {
        if (CountOwnedPuddles(owner) < MaxOwnedPuddles)
            return;

        Projectile oldest = null;
        int lowestTimeLeft = int.MaxValue;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile puddle = Main.projectile[i];
            if (!IsOwnedActivePuddle(puddle, owner) || puddle.timeLeft >= lowestTimeLeft)
                continue;

            lowestTimeLeft = puddle.timeLeft;
            oldest = puddle;
        }

        oldest?.Kill();
    }

    private void ApplyDissolveAura(int radius) {
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc == null || !npc.active || npc.friendly || npc.dontTakeDamage)
                continue;

            if (Vector2.DistanceSquared(npc.Center, Projectile.Center) > radius * radius)
                continue;

            if (GetPuddleHitbox(Projectile, 6f).Intersects(npc.Hitbox))
                npc.AddBuff(ModContent.BuffType<GoopDissolved>(), 45);
        }
    }

    private static void EmitMergeDust(Vector2 center, float puddleScale) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Dust pulse = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(PuddleWidth * 0.35f * puddleScale, PuddleHeight * 0.45f),
                DustID.GreenTorch, Main.rand.NextVector2Circular(1.4f, 0.8f), 90, new Color(145, 255, 150),
                Main.rand.NextFloat(1f, 1.35f));
            pulse.noGravity = true;
        }
    }
}
