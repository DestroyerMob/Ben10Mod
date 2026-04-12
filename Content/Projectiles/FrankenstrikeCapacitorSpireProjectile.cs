using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Transformations.Frankenstrike;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FrankenstrikeCapacitorSpireProjectile : ModProjectile {
    private const float RepositionReuseDistance = 64f;
    private const float MaxTargetDistance = 520f;
    private const float TetherHalfWidth = 20f;
    private const int MaxSpires = 2;

    private ref float FireTimer => ref Projectile.localAI[0];
    private ref float SpawnOrder => ref Projectile.localAI[1];

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 54;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.hide = true;
        Projectile.timeLeft = 18000;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (!string.Equals(omp.currentTransformationId, FrankenstrikeStatePlayer.TransformationId, System.StringComparison.Ordinal)) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        if (SpawnOrder <= 0f)
            SpawnOrder = Main.GameUpdateCount;

        Vector2 anchor = new(Projectile.ai[0], Projectile.ai[1]);
        if (anchor == Vector2.Zero)
            anchor = Projectile.Center;

        Projectile.Center = Vector2.Lerp(Projectile.Center, anchor, 0.24f);
        if (Projectile.Center.Distance(anchor) < 8f)
            Projectile.Center = anchor;

        UpdateLocalHitCooldowns();

        FrankenstrikeStatePlayer state = owner.GetModPlayer<FrankenstrikeStatePlayer>();
        NPC target = FindPreferredTarget(owner, MaxTargetDistance);
        if (target != null)
            Projectile.rotation = Projectile.DirectionTo(target.Center).ToRotation();

        FireTimer++;
        int fireRate = state.StormheartActive ? 18 : state.GalvanizedActive ? 24 : 34;
        if (target != null &&
            FireTimer >= fireRate &&
            (Main.netMode != NetmodeID.MultiplayerClient || Projectile.owner == Main.myPlayer)) {
            FireTimer = Main.rand.Next(4);
            FireAtTarget(owner, target, state);
        }

        Projectile partner = FindTetherPartner(owner);
        if (partner != null &&
            Projectile.whoAmI < partner.whoAmI &&
            (Main.GameUpdateCount + Projectile.whoAmI) % (state.StormheartActive ? 10 : state.GalvanizedActive ? 13 : 16) == 0 &&
            Main.netMode != NetmodeID.MultiplayerClient) {
            PulseTether(owner, partner, state);
        }

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(9f, 18f),
                Main.rand.NextBool(3) ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(0.35f, 1.1f), 110, new Color(165, 220, 255),
                Main.rand.NextFloat(0.95f, state.StormheartActive ? 1.28f : 1.12f));
            dust.noGravity = true;
        }

        Lighting.AddLight(Projectile.Center, state.StormheartActive
            ? new Vector3(0.34f, 0.5f, 0.92f)
            : new Vector3(0.24f, 0.42f, 0.84f));
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 center = Projectile.Center - Main.screenPosition;
        bool stormheart = Main.player[Projectile.owner].GetModPlayer<FrankenstrikeStatePlayer>().StormheartActive;

        Main.EntitySpriteDraw(pixel, center + new Vector2(0f, 10f), null, new Color(54, 68, 92, 255), 0f,
            new Vector2(0.5f, 0.5f), new Vector2(9f, 48f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(0f, -6f), null, stormheart ? new Color(125, 182, 255, 220) : new Color(105, 158, 255, 205),
            0f, new Vector2(0.5f, 0.5f), new Vector2(18f, 12f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center + new Vector2(0f, -6f), null, new Color(240, 250, 255, 200), 0f,
            new Vector2(0.5f, 0.5f), new Vector2(8f, 6f), SpriteEffects.None, 0);

        Projectile partner = FindTetherPartner(Main.player[Projectile.owner]);
        if (partner != null && Projectile.whoAmI < partner.whoAmI) {
            DrawTether(pixel, center, partner.Center - Main.screenPosition,
                stormheart ? new Color(150, 205, 255, 155) : new Color(110, 170, 255, 120),
                stormheart ? 5.2f : 4.1f);
            DrawTether(pixel, center, partner.Center - Main.screenPosition, new Color(240, 248, 255, 175),
                stormheart ? 2.4f : 1.9f);
        }

        return false;
    }

    public static List<Projectile> GetOwnedSpires(Player owner) {
        List<Projectile> spires = new();
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (IsOwnedSpire(projectile, owner))
                spires.Add(projectile);
        }

        spires.Sort(static (left, right) => left.localAI[1].CompareTo(right.localAI[1]));
        return spires;
    }

    public static bool TryRepositionSpireNearAnchor(Player owner, Vector2 anchor) {
        Projectile spire = FindNearestSpireToPoint(owner, anchor, RepositionReuseDistance);
        if (spire == null)
            return false;

        ReanchorSpire(spire, anchor);
        if (!Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.12f, Volume = 0.34f }, anchor);
        return true;
    }

    public static Projectile FindOldestSpire(Player owner) {
        List<Projectile> spires = GetOwnedSpires(owner);
        return spires.Count > 0 ? spires[0] : null;
    }

    public static void ReanchorSpire(Projectile spire, Vector2 anchor) {
        if (spire == null || !spire.active)
            return;

        spire.ai[0] = anchor.X;
        spire.ai[1] = anchor.Y;
        spire.Center = anchor;
        spire.velocity = Vector2.Zero;
        spire.localAI[0] = 0f;
        spire.netUpdate = true;
    }

    private static bool IsOwnedSpire(Projectile projectile, Player owner) {
        return projectile.active &&
               projectile.owner == owner.whoAmI &&
               projectile.type == ModContent.ProjectileType<FrankenstrikeCapacitorSpireProjectile>();
    }

    private static Projectile FindNearestSpireToPoint(Player owner, Vector2 point, float maxDistance) {
        Projectile best = null;
        float bestDistance = maxDistance;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!IsOwnedSpire(projectile, owner))
                continue;

            float distance = projectile.Center.Distance(point);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = projectile;
        }

        return best;
    }

    private NPC FindPreferredTarget(Player owner, float maxDistance) {
        if (owner.HasMinionAttackTargetNPC) {
            NPC forcedTarget = Main.npc[owner.MinionAttackTargetNPC];
            if (forcedTarget.CanBeChasedBy() && Projectile.Center.Distance(forcedTarget.Center) <= maxDistance + 120f)
                return forcedTarget;
        }

        NPC bestTarget = null;
        float bestScore = float.MaxValue;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Projectile.Center.Distance(npc.Center);
            if (distance > maxDistance)
                continue;

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            float score = distance;
            if (identity.IsFrankenstrikeOverchargedFor(Projectile.owner))
                score -= 150f;
            else if (identity.IsFrankenstrikeConductiveFor(Projectile.owner))
                score -= 70f;

            if (score >= bestScore)
                continue;

            bestScore = score;
            bestTarget = npc;
        }

        return bestTarget;
    }

    private Projectile FindTetherPartner(Player owner) {
        List<Projectile> spires = GetOwnedSpires(owner);
        if (spires.Count < MaxSpires)
            return null;

        for (int i = 0; i < spires.Count; i++) {
            Projectile spire = spires[i];
            if (spire.whoAmI != Projectile.whoAmI)
                return spire;
        }

        return null;
    }

    private void FireAtTarget(Player owner, NPC target, FrankenstrikeStatePlayer state) {
        Vector2 velocity = Projectile.DirectionTo(target.Center) * (state.StormheartActive ? 15.8f : 13.4f);
        int shotDamage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * (state.StormheartActive ? 0.92f : 0.8f)));
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
            ModContent.ProjectileType<FrankenstrikeTeslaProjectile>(), shotDamage, 0.7f, Projectile.owner,
            (float)FrankenstrikeTeslaProjectile.ShotVariant.Spire, state.StormheartActive ? 1f : 0f);
    }

    private void PulseTether(Player owner, Projectile partner, FrankenstrikeStatePlayer state) {
        Vector2 start = Projectile.Center;
        Vector2 end = partner.Center;
        int damage = System.Math.Max(1, (int)System.Math.Round(Projectile.damage * (state.StormheartActive ? 0.6f : 0.46f)));

        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile) || Projectile.localNPCImmunity[npc.whoAmI] > 0)
                continue;

            float collisionPoint = 0f;
            if (!Collision.CheckAABBvLineCollision(npc.Hitbox.TopLeft(), npc.Hitbox.Size(), start, end,
                    TetherHalfWidth + (state.StormheartActive ? 5f : 0f), ref collisionPoint)) {
                continue;
            }

            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            int tetherDamage = identity.IsFrankenstrikeOverchargedFor(owner.whoAmI)
                ? System.Math.Max(1, (int)System.Math.Round(damage * 1.35f))
                : damage;
            npc.SimpleStrikeNPC(tetherDamage, owner.direction, false, 0f, ModContent.GetInstance<HeroDamage>());
            FrankenstrikeTransformation.ApplyConductiveHit(owner, npc, 1, 180);
            Projectile.localNPCImmunity[npc.whoAmI] = state.StormheartActive ? 8 : 12;
        }

        if (Main.dedServ)
            return;

        for (int i = 0; i < 9; i++) {
            float progress = Main.rand.NextFloat();
            Vector2 dustPosition = Vector2.Lerp(start, end, progress);
            Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Electric,
                (end - start).SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.8f, 0.8f),
                110, new Color(180, 228, 255), Main.rand.NextFloat(0.9f, state.StormheartActive ? 1.25f : 1.05f));
            dust.noGravity = true;
        }
    }

    private void UpdateLocalHitCooldowns() {
        for (int i = 0; i < Projectile.localNPCImmunity.Length; i++) {
            if (Projectile.localNPCImmunity[i] > 0)
                Projectile.localNPCImmunity[i]--;
        }
    }

    private static void DrawTether(Texture2D pixel, Vector2 start, Vector2 end, Color color, float width) {
        Vector2 delta = end - start;
        float length = delta.Length();
        if (length <= 0.5f)
            return;

        Main.EntitySpriteDraw(pixel, start, null, color, delta.ToRotation(), new Vector2(0f, 0.5f),
            new Vector2(length, width), SpriteEffects.None, 0);
    }
}
