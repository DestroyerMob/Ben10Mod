using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Armour;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class PlumberSiegeBoomerangProjectile : ModProjectile {
    private static DamageClass HeroClass => ModContent.GetInstance<HeroDamage>();

    private const int StateHover = 0;
    private const int StateAttack = 1;
    private const int StateReturn = 2;

    private const float HoverSpeed = 12f;
    private const float HoverInertia = 14f;
    private const float AttackSpeed = 22f;
    private const float AttackInertia = 6f;
    private const float ReturnSpeed = 20f;
    private const float ReturnInertia = 8f;
    private const float HoverSnapDistance = 14f;
    private const float HomeOffsetY = 56f;
    private const float MaxTargetRange = 560f;
    private const float LostTargetRange = 760f;
    private const float TeleportDistance = 1200f;
    private const int MaxAttackTime = 28;

    private ref float State => ref Projectile.ai[0];
    private ref float Timer => ref Projectile.ai[1];

    public override void SetStaticDefaults() {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.minion = true;
        Projectile.minionSlots = 0f;
        Projectile.netImportant = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18000;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = HeroClass;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 14;
    }

    public override bool MinionContactDamage() => true;

    public override bool? CanDamage() => State == StateAttack ? null : false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || !ShouldExist(owner)) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        Projectile.originalDamage = HeroPlumberArmorPlayer.SiegeBoomerangBaseDamage;
        Projectile.damage = Math.Max(1, (int)Math.Round(owner.GetDamage<HeroDamage>().ApplyTo(HeroPlumberArmorPlayer.SiegeBoomerangBaseDamage)));

        Vector2 homePosition = GetHomePosition(owner);
        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;
            Projectile.Center = homePosition;
        }

        if (Projectile.Center.Distance(homePosition) > TeleportDistance) {
            Projectile.Center = homePosition;
            Projectile.velocity = Vector2.Zero;
            State = StateHover;
            Timer = 0f;
            Projectile.netUpdate = true;
        }

        NPC target = FindTarget(owner, State == StateAttack ? LostTargetRange : MaxTargetRange);
        switch ((int)State) {
            case StateAttack:
                DoAttack(owner, homePosition, target);
                break;
            case StateReturn:
                DoReturn(homePosition);
                break;
            default:
                DoHover(owner, homePosition, target);
                break;
        }

        if (Projectile.velocity.X != 0f)
            Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;
        Lighting.AddLight(Projectile.Center, new Vector3(0.06f, 0.1f, 0.18f) * 0.7f);

        if (Main.rand.NextBool(6)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.BlueTorch,
                Projectile.velocity * 0.08f, 120, new Color(120, 170, 255), 0.95f);
            dust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        BeginReturn();

        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(target.Center, DustID.BlueTorch,
                Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.8f, 4f), 110,
                new Color(130, 185, 255), 1.05f);
            dust.noGravity = true;
        }

        SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
    }

    private void DoHover(Player owner, Vector2 homePosition, NPC target) {
        MoveTowards(homePosition, HoverSpeed, HoverInertia);

        if (Projectile.Center.Distance(homePosition) <= HoverSnapDistance) {
            Projectile.Center = homePosition;
            if (Projectile.velocity.LengthSquared() < 0.08f)
                Projectile.velocity = Vector2.Zero;
            else
                Projectile.velocity *= 0.82f;
        }

        if (target == null || Projectile.Center.Distance(homePosition) > HoverSnapDistance + 4f)
            return;

        Vector2 attackDirection = Projectile.DirectionTo(target.Center);
        if (attackDirection == Vector2.Zero)
            attackDirection = Vector2.UnitX * owner.direction;

        Projectile.velocity = attackDirection * AttackSpeed;
        State = StateAttack;
        Timer = 0f;
        Projectile.netUpdate = true;

        SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
    }

    private void DoAttack(Player owner, Vector2 homePosition, NPC target) {
        Timer++;

        if (target == null || !target.CanBeChasedBy(Projectile) || Projectile.Center.Distance(target.Center) > LostTargetRange) {
            BeginReturn();
            return;
        }

        MoveTowards(target.Center, AttackSpeed, AttackInertia);

        if (Timer >= MaxAttackTime || Projectile.Center.Distance(homePosition) > LostTargetRange)
            BeginReturn();
    }

    private void DoReturn(Vector2 homePosition) {
        MoveTowards(homePosition, ReturnSpeed, ReturnInertia);

        if (Projectile.Center.Distance(homePosition) > HoverSnapDistance)
            return;

        Projectile.Center = homePosition;
        Projectile.velocity = Vector2.Zero;
        State = StateHover;
        Timer = 0f;
        Projectile.netUpdate = true;
    }

    private void BeginReturn() {
        if (State == StateReturn)
            return;

        State = StateReturn;
        Timer = 0f;
        Projectile.netUpdate = true;
    }

    private void MoveTowards(Vector2 destination, float speed, float inertia) {
        Vector2 toDestination = destination - Projectile.Center;
        if (toDestination == Vector2.Zero)
            return;

        Vector2 desiredVelocity = toDestination.SafeNormalize(Vector2.Zero) * Math.Min(speed, toDestination.Length());
        Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desiredVelocity) / inertia;
    }

    private static Vector2 GetHomePosition(Player owner) {
        return owner.MountedCenter + new Vector2(0f, -HomeOffsetY);
    }

    private NPC FindTarget(Player owner, float maxDistance) {
        if (owner.HasMinionAttackTargetNPC) {
            NPC forcedTarget = Main.npc[owner.MinionAttackTargetNPC];
            if (forcedTarget.CanBeChasedBy(Projectile) && Projectile.Center.Distance(forcedTarget.Center) < maxDistance)
                return forcedTarget;
        }

        NPC closestTarget = null;
        float closestDistance = maxDistance;

        foreach (NPC npc in Main.ActiveNPCs) {
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

    private static bool ShouldExist(Player owner) {
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        bool transformed = omp.isTransformed || omp.IsTransformed;
        if (!transformed)
            return false;

        return owner.armor[0].type == ModContent.ItemType<PlumberSiegeMask>()
            && owner.armor[1].type == ModContent.ItemType<PlumberSiegeCuirass>()
            && owner.armor[2].type == ModContent.ItemType<PlumberSiegeBoots>();
    }
}
