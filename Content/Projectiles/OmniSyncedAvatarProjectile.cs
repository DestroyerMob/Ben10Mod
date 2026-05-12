using System;
using Ben10Mod.Content.Buffs.Summons;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class OmniSyncedAvatarProjectile : ModProjectile {
    private const float HoverSpeed = 12f;
    private const float HoverInertia = 18f;
    private const float TeleportDistance = 1600f;
    private const float TargetRange = 680f;

    private ref float FireCooldown => ref Projectile.ai[0];
    private ref float LastSyncedBuffId => ref Projectile.ai[1];

    public override string Texture => "Ben10Mod/Content/Interface/EmptyAlien";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = true;
        Main.projFrames[Type] = 1;
    }

    public override void SetDefaults() {
        Projectile.width = 44;
        Projectile.height = 54;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18000;
        Projectile.minion = true;
        Projectile.minionSlots = 1f;
        Projectile.netImportant = true;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            if (owner.active)
                owner.ClearBuff(ModContent.BuffType<OmniSyncedAvatarBuff>());

            Projectile.Kill();
            return;
        }

        if (owner.HasBuff(ModContent.BuffType<OmniSyncedAvatarBuff>()))
            Projectile.timeLeft = 2;

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        Transformation transformation = omp.CurrentTransformation;
        SyncTransformationVisuals(transformation);

        int formationIndex = GetFormationIndex();
        Vector2 idlePosition = GetIdlePosition(owner, formationIndex);
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.Center = idlePosition;
            FireCooldown = 18f + formationIndex * 8f;
        }

        if (Vector2.Distance(Projectile.Center, idlePosition) > TeleportDistance) {
            Projectile.Center = idlePosition;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;
        }

        NPC target = transformation == null ? null : FindTarget(owner);
        Vector2 destination = target == null
            ? idlePosition
            : GetCombatPosition(owner, target, formationIndex);
        MoveTowards(destination, HoverSpeed, HoverInertia);

        if (FireCooldown > 0f)
            FireCooldown--;

        if (transformation != null && target != null && FireCooldown <= 0f && owner.whoAmI == Main.myPlayer) {
            if (transformation.TryShootAvatar(Projectile, owner, omp, target, Projectile.damage, Projectile.knockBack,
                    out int cooldownTicks)) {
                FireCooldown = cooldownTicks + formationIndex * 4f;
                Projectile.netUpdate = true;
                SoundEngine.PlaySound(SoundID.Item92 with { Pitch = 0.08f, Volume = 0.38f }, Projectile.Center);
            }
            else {
                FireCooldown = 30f;
            }
        }

        Projectile.rotation = Projectile.velocity.X * 0.035f;
        Projectile.spriteDirection = Projectile.Center.X >= owner.Center.X ? 1 : -1;

        Color syncColor = transformation?.TransformTextColor ?? new Color(70, 255, 140);
        Lighting.AddLight(Projectile.Center, syncColor.ToVector3() * 0.34f);

        if (!Main.dedServ && Main.rand.NextBool(transformation == null ? 12 : 6)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 16f),
                DustID.GreenTorch, Vector2.Zero, 140, syncColor, Main.rand.NextFloat(0.75f, 1.15f));
            dust.noGravity = true;
            dust.velocity *= 0.25f;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Player owner = Main.player[Projectile.owner];
        Transformation transformation = owner.active ? owner.GetModPlayer<OmnitrixPlayer>().CurrentTransformation : null;
        Color syncColor = transformation?.TransformTextColor ?? new Color(70, 255, 140);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + Projectile.whoAmI) * 0.05f;
        SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Main.EntitySpriteDraw(pixel, drawPosition, null, syncColor * 0.2f, Projectile.rotation + MathHelper.PiOver4,
            Vector2.One * 0.5f, new Vector2(36f, 42f) * pulse, SpriteEffects.None);
        Main.EntitySpriteDraw(pixel, drawPosition + new Vector2(0f, 26f), null, syncColor * 0.12f, 0f,
            Vector2.One * 0.5f, new Vector2(30f, 5f), SpriteEffects.None);

        Texture2D icon = transformation?.GetTransformationIcon().Value ?? TextureAssets.Projectile[Type].Value;
        float iconScale = Math.Min(0.5f, 30f / Math.Max(icon.Width, icon.Height)) * pulse;
        Vector2 origin = icon.Size() * 0.5f;
        Main.EntitySpriteDraw(icon, drawPosition, null, syncColor * 0.35f, Projectile.rotation, origin,
            iconScale * 1.16f, effects);
        Main.EntitySpriteDraw(icon, drawPosition, null, Color.White, Projectile.rotation, origin, iconScale, effects);

        return false;
    }

    private void SyncTransformationVisuals(Transformation transformation) {
        int syncedBuffId = transformation?.TransformationBuffId ?? 0;
        if (LastSyncedBuffId == syncedBuffId)
            return;

        LastSyncedBuffId = syncedBuffId;
        Projectile.localAI[1] = 0f;
        Projectile.netUpdate = true;

        if (Main.dedServ)
            return;

        Color syncColor = transformation?.TransformTextColor ?? new Color(70, 255, 140);
        for (int i = 0; i < 16; i++) {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.4f, 4.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Teleporter, velocity, 90, syncColor,
                Main.rand.NextFloat(0.9f, 1.25f));
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

        return index;
    }

    private static Vector2 GetIdlePosition(Player owner, int formationIndex) {
        float side = formationIndex % 2 == 0 ? 1f : -1f;
        float row = formationIndex / 2;
        return owner.MountedCenter + new Vector2(58f * side, -66f - row * 36f);
    }

    private static Vector2 GetCombatPosition(Player owner, NPC target, int formationIndex) {
        Vector2 ownerToTarget = owner.Center.DirectionTo(target.Center);
        if (ownerToTarget == Vector2.Zero)
            ownerToTarget = new Vector2(owner.direction == 0 ? 1f : owner.direction, 0f);

        Vector2 lateral = new Vector2(-ownerToTarget.Y, ownerToTarget.X);
        float spread = formationIndex % 2 == 0 ? 34f : -34f;
        return target.Center - ownerToTarget * 150f + lateral * spread + new Vector2(0f, -18f);
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
