using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WildVineProjectile : ModProjectile {
    private const int StateFlying = 0;
    private const int StateLatched = 1;
    private const int StateReturning = 2;

    private const float ReturnSpeed = 22f;
    private const float ReturnLerp = 0.25f;
    private const float PullStrength = 0.6f;
    private const float PullMaxSpeed = 14f;
    private const float DetachRange = 700f;
    private const float DetachRangeSq = DetachRange * DetachRange;
    private const float ReleaseDistance = 90f;
    private const float ReleaseDistanceSq = ReleaseDistance * ReleaseDistance;
    private const float ReleasePushSpeed = 6f;
    private const int LatchTime = 180;

    public override string Texture => "Ben10Mod/Content/Projectiles/WildVineProjectile";

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.netImportant = true;
    }

    public override bool? CanHitNPC(NPC target) {
        if ((int)Projectile.ai[0] != StateFlying)
            return false;

        if (!target.CanBeChasedBy(this))
            return false;

        return null;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (target.boss || !target.CanBeChasedBy(this)) {
            StartReturn();
            return;
        }

        target.AddBuff(BuffID.Poisoned, 5 * 60);
        Projectile.ai[0] = StateLatched;
        Projectile.ai[1] = target.whoAmI + 1;
        Projectile.velocity = Vector2.Zero;
        Projectile.tileCollide = false;
        Projectile.timeLeft = LatchTime;
        Projectile.netUpdate = true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        StartReturn();
        return false;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead) {
            Projectile.Kill();
            return;
        }

        int state = (int)Projectile.ai[0];
        if (state == StateReturning) {
            Projectile.timeLeft = 2;

            Vector2 toPlayer = player.MountedCenter - Projectile.Center;
            if (toPlayer.LengthSquared() < 24f * 24f) {
                Projectile.Kill();
                return;
            }

            Vector2 desiredVelocity = toPlayer.SafeNormalize(Vector2.Zero) * ReturnSpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, ReturnLerp);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            return;
        }

        Projectile.timeLeft = 2;
        if (Vector2.DistanceSquared(player.MountedCenter, Projectile.Center) > DetachRangeSq) {
            StartReturn();
            return;
        }

        if (state == StateFlying) {
            Projectile.localAI[1]++;
            if (Projectile.localAI[1] > 90f) {
                StartReturn();
                return;
            }

            if (Projectile.velocity.LengthSquared() > 0.01f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            return;
        }

        int npcIndex = (int)Projectile.ai[1] - 1;
        if (npcIndex < 0 || npcIndex >= Main.maxNPCs) {
            StartReturn();
            return;
        }

        NPC npc = Main.npc[npcIndex];
        if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.boss) {
            StartReturn();
            return;
        }

        Projectile.Center = npc.Center;
        Projectile.rotation = (player.MountedCenter - npc.Center).ToRotation() + MathHelper.PiOver2;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Vector2 toPlayerCenter = player.MountedCenter - npc.Center;
        float distanceSquared = toPlayerCenter.LengthSquared();
        if (distanceSquared <= ReleaseDistanceSq) {
            Vector2 pushAway = (-toPlayerCenter).SafeNormalize(Vector2.UnitX);
            npc.velocity = pushAway * ReleasePushSpeed;
            npc.netUpdate = true;
            StartReturn();
            return;
        }

        Vector2 pullDirection = toPlayerCenter.SafeNormalize(Vector2.Zero);
        float resist = MathHelper.Clamp(1f - npc.knockBackResist, 0f, 0.85f);
        float strength = PullStrength * (1f - resist);

        npc.velocity += pullDirection * strength;

        float alongPull = Vector2.Dot(npc.velocity, pullDirection);
        if (alongPull > PullMaxSpeed)
            npc.velocity -= pullDirection * (alongPull - PullMaxSpeed);

        npc.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        DrawChain();

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 origin = texture.Size() / 2f;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation,
            origin, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }

    private void StartReturn() {
        Projectile.ai[0] = StateReturning;
        Projectile.ai[1] = 0;
        Projectile.tileCollide = false;
        Projectile.netUpdate = true;
    }

    private void DrawChain() {
        Player player = Main.player[Projectile.owner];
        if (!player.active)
            return;

        Texture2D chainTexture = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
        Vector2 start = player.MountedCenter;
        Vector2 end = Projectile.Center;
        Vector2 direction = end - start;
        float length = direction.Length();
        if (length < 8f)
            return;

        direction /= length;
        float segmentLength = chainTexture.Height;
        float rotation = direction.ToRotation() - MathHelper.PiOver2;

        for (float offset = 0f; offset < length; offset += segmentLength) {
            Vector2 drawPosition = start + direction * offset;
            Color color = Lighting.GetColor((int)(drawPosition.X / 16f), (int)(drawPosition.Y / 16f));

            Main.EntitySpriteDraw(chainTexture, drawPosition - Main.screenPosition, null, color, rotation,
                new Vector2(chainTexture.Width / 2f, chainTexture.Height / 2f), 1f, SpriteEffects.None, 0);
        }
    }
}
