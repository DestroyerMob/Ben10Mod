using System;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ConquestDroneProjectile : ModProjectile {
    private const float HoverSpeed = 12f;
    private const float HoverInertia = 18f;
    private const float TeleportDistance = 1400f;
    private const float TargetRange = 440f;
    private const float FireSpeed = 13.5f;
    private const int FireCooldownMax = 32;

    private ref float FireCooldown => ref Projectile.ai[0];

    public override string Texture => $"Terraria/Images/NPC_{NPCID.Probe}";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18000;
        Projectile.minion = true;
        Projectile.minionSlots = 0f;
        Projectile.netImportant = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || !owner.GetModPlayer<ConquestDroneRelayPlayer>().conquestDroneRelayEquipped) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        Projectile.originalDamage = ConquestDroneRelay.BaseDroneDamage;
        Projectile.damage = Math.Max(1,
            (int)Math.Round(owner.GetDamage<HeroDamage>().ApplyTo(ConquestDroneRelay.BaseDroneDamage)));

        int formationIndex = GetFormationIndex();
        Vector2 idlePosition = GetIdlePosition(owner, formationIndex);
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.Center = idlePosition;
            FireCooldown = formationIndex * (FireCooldownMax * 0.5f);
        }

        if (Vector2.Distance(Projectile.Center, idlePosition) > TeleportDistance) {
            Projectile.Center = idlePosition;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;
        }

        MoveTowards(idlePosition, HoverSpeed, HoverInertia);
        NPC target = FindTarget(owner);
        if (FireCooldown > 0f)
            FireCooldown--;

        if (target != null && FireCooldown <= 0f && owner.whoAmI == Main.myPlayer) {
            Vector2 fireDirection = Projectile.Center.DirectionTo(target.Center);
            if (fireDirection == Vector2.Zero)
                fireDirection = Vector2.UnitY;

            int projectileIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                fireDirection * FireSpeed, ModContent.ProjectileType<ConquestDroneBoltProjectile>(), Projectile.damage, 0.7f,
                owner.whoAmI, target.whoAmI + 1f);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                Main.projectile[projectileIndex].netUpdate = true;

            FireCooldown = FireCooldownMax;
            SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.28f, Volume = 0.42f }, Projectile.Center);
        }

        Projectile.rotation = Projectile.velocity.X * 0.04f;
        Projectile.spriteDirection = Projectile.Center.X >= owner.Center.X ? 1 : -1;
        Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.2f, 0.3f));

        if (!Main.dedServ && Main.rand.NextBool(7)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.Electric,
                Vector2.Zero, 120, new Color(130, 220, 255), Main.rand.NextFloat(0.8f, 1.05f));
            dust.noGravity = true;
        }
    }

    private int GetFormationIndex() {
        int index = 0;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile other = Main.projectile[i];
            if (!other.active || other.owner != Projectile.owner || other.type != Projectile.type || other.whoAmI == Projectile.whoAmI)
                continue;

            if (other.whoAmI < Projectile.whoAmI)
                index++;
        }

        return index % 2;
    }

    private static Vector2 GetIdlePosition(Player owner, int formationIndex) {
        float side = formationIndex == 0 ? -1f : 1f;
        return owner.MountedCenter + new Vector2(34f * side, -62f);
    }

    private void MoveTowards(Vector2 destination, float speed, float inertia) {
        Vector2 toDestination = destination - Projectile.Center;
        if (toDestination == Vector2.Zero)
            return;

        Vector2 desiredVelocity = toDestination.SafeNormalize(Vector2.Zero) * Math.Min(speed, toDestination.Length());
        Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desiredVelocity) / inertia;
    }

    private NPC FindTarget(Player owner) {
        if (owner.HasMinionAttackTargetNPC) {
            NPC forcedTarget = Main.npc[owner.MinionAttackTargetNPC];
            if (forcedTarget.CanBeChasedBy(Projectile) && Projectile.Center.Distance(forcedTarget.Center) <= TargetRange)
                return forcedTarget;
        }

        NPC bestTarget = null;
        float bestDistance = TargetRange;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestTarget = npc;
        }

        return bestTarget;
    }
}
