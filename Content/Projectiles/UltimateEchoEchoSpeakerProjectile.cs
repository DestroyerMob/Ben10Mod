using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Summons;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Transformations.EchoEcho;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class UltimateEchoEchoSpeakerProjectile : ModProjectile {
    private const float RepositionReuseDistance = 56f;
    private const float RelayCursorPreferenceDistance = 220f;
    private const float MaxTargetDistance = 520f;

    private ref float FireTimer => ref Projectile.localAI[0];

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
        if (omp.currentTransformationId != UltimateEchoEchoStatePlayer.TransformationId) {
            Projectile.Kill();
            return;
        }

        if (owner.HasBuff(ModContent.BuffType<UltimateEchoEchoSpeakerBuff>()))
            Projectile.timeLeft = 2;

        UltimateEchoEchoStatePlayer state = owner.GetModPlayer<UltimateEchoEchoStatePlayer>();
        Vector2 anchor = new Vector2(Projectile.ai[0], Projectile.ai[1]);
        if (anchor == Vector2.Zero)
            anchor = Projectile.Center;

        Projectile.Center = Vector2.Lerp(Projectile.Center, anchor, 0.22f);
        if (Projectile.Center.Distance(anchor) < 10f) {
            Projectile.Center = anchor;
            Projectile.velocity = Vector2.Zero;
        }

        NPC target = FindPreferredTarget(owner, MaxTargetDistance);
        FireTimer++;
        Projectile.rotation = target != null
            ? Projectile.DirectionTo(target.Center).ToRotation()
            : Projectile.rotation;

        if (Main.rand.NextBool(2)) {
            Vector2 dustVelocity = Main.rand.NextVector2Circular(0.8f, 0.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueCrystalShard, dustVelocity, 130,
                state.CataclysmActive ? new Color(150, 225, 255) : new Color(90, 190, 255),
                state.CataclysmActive ? 1.22f : 1.08f);
            dust.noGravity = true;
        }

        int fireRate = state.CataclysmActive ? 18 : state.EffectiveOverclockActive ? 22 : 34;
        if (target != null &&
            FireTimer >= fireRate &&
            (Main.netMode != NetmodeID.MultiplayerClient || Projectile.owner == Main.myPlayer)) {
            FireTimer = Main.rand.Next(4);
            FireAtTarget(owner, target, state);
        }
    }

    public static List<Projectile> GetOwnedSpeakers(Player owner) {
        List<Projectile> speakers = new();
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (IsOwnedSpeaker(projectile, owner))
                speakers.Add(projectile);
        }

        speakers.Sort(static (left, right) => left.localAI[1].CompareTo(right.localAI[1]));
        return speakers;
    }

    public static Projectile FindNearestSpeakerToPoint(Player owner, Vector2 point, float maxDistance) {
        Projectile bestSpeaker = null;
        float bestDistance = maxDistance;

        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile projectile = Main.projectile[i];
            if (!IsOwnedSpeaker(projectile, owner))
                continue;

            float distance = projectile.Center.Distance(point);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestSpeaker = projectile;
        }

        return bestSpeaker;
    }

    public static Projectile FindRelaySpeaker(Player owner, Vector2 cursorWorld) {
        Projectile cursorSpeaker = FindNearestSpeakerToPoint(owner, cursorWorld, RelayCursorPreferenceDistance);
        return cursorSpeaker ?? FindNearestSpeakerToPoint(owner, owner.Center, 640f);
    }

    public static bool TryRepositionSpeakerNearAnchor(Player owner, Vector2 anchor) {
        Projectile speaker = FindNearestSpeakerToPoint(owner, anchor, RepositionReuseDistance);
        if (speaker == null)
            return false;

        ReanchorSpeaker(speaker, anchor, snapToAnchor: true);
        if (!Main.dedServ)
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.18f, Volume = 0.32f }, anchor);
        return true;
    }

    public static void ReanchorSpeaker(Projectile speaker, Vector2 anchor, bool snapToAnchor) {
        if (speaker == null || !speaker.active)
            return;

        speaker.ai[0] = anchor.X;
        speaker.ai[1] = anchor.Y;
        if (snapToAnchor) {
            speaker.Center = anchor;
            speaker.velocity = Vector2.Zero;
        }

        speaker.localAI[0] = 0f;
        speaker.netUpdate = true;
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

    private static bool IsOwnedSpeaker(Projectile projectile, Player owner) {
        return projectile.active &&
               projectile.owner == owner.whoAmI &&
               projectile.type == ModContent.ProjectileType<UltimateEchoEchoSpeakerProjectile>();
    }

    private NPC FindPreferredTarget(Player owner, float maxDistance) {
        if (owner.HasMinionAttackTargetNPC) {
            NPC forcedTarget = Main.npc[owner.MinionAttackTargetNPC];
            if (forcedTarget.CanBeChasedBy() && Projectile.Center.Distance(forcedTarget.Center) <= maxDistance + 100f)
                return forcedTarget;
        }

        NPC focusedTarget = UltimateEchoEchoTransformation.FindFocusedTarget(owner, Projectile.Center, maxDistance + 140f);
        if (focusedTarget != null)
            return focusedTarget;

        NPC bestTarget = null;
        float bestScore = float.MaxValue;
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Projectile.Center.Distance(npc.Center);
            AlienIdentityGlobalNPC identity = npc.GetGlobalNPC<AlienIdentityGlobalNPC>();
            float score = distance - (identity.IsEchoEchoFracturedFor(Projectile.owner) ? 36f : 0f);
            if (distance > maxDistance || score >= bestScore)
                continue;

            bestScore = score;
            bestTarget = npc;
        }

        return bestTarget;
    }

    private void FireAtTarget(Player owner, NPC target, UltimateEchoEchoStatePlayer state) {
        for (int i = 0; i < 8; i++) {
            Vector2 burstVelocity = Main.rand.NextVector2CircularEdge(2.2f, 2.2f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueCrystalShard, burstVelocity, 100,
                new Color(120, 210, 255), 1.25f);
            dust.noGravity = true;
        }

        bool overclocked = state.EffectiveOverclockActive;
        float shotSpeed = overclocked ? 15.5f : 12f;
        Vector2 velocity = Projectile.DirectionTo(target.Center) * shotSpeed;
        int attackDamage = UltimateEchoEchoTransformation.ResolveSpeakerShotDamage(Projectile, owner, overclocked);
        UltimateEchoEchoTransformation.SpawnSonicShot(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
            attackDamage, 0.8f, Projectile.owner,
            overclocked ? UltimateEchoEchoShotKind.OverclockSpeaker : UltimateEchoEchoShotKind.Speaker,
            Projectile.whoAmI);
    }
}
