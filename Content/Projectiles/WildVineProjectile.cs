using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles
{
    public class WildVineProjectile : ModProjectile
    {
        private const int StateFlying    = 0;
        private const int StateLatched   = 1;
        private const int StateReturning = 2;

        private const float ReturnSpeed = 22f;
        private const float ReturnLerp  = 0.25f;

        private const float ThrowSpeed = 18f;

        private const float PullStrength = 0.6f;
        private const float PullMaxSpeed = 14f;

        private const float DetachRange   = 700f;           // max chain length (break if too far)
        private const float DetachRangeSq = DetachRange * DetachRange;

        private const float ReleaseDistance   = 90f;        // min distance (release when close enough)
        private const float ReleaseDistanceSq = ReleaseDistance * ReleaseDistance;

        private const float ReleasePushSpeed = 6f;        // small shove away on release


        private const int LatchTime = 180;
        
        

        // ai[0] = state, ai[1] = npcIndex+1
        public override string Texture => "Ben10Mod/Content/Projectiles/WildVineProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;

            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 300;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.DamageType = DamageClass.Summon;
            Projectile.netImportant = true;
        }
        


        private void StartReturn()
        {
            Projectile.ai[0] = StateReturning;
            Projectile.ai[1] = 0;

            Projectile.tileCollide = false; // don’t get stuck on the way back
            Projectile.netUpdate   = true;
        }


        public override bool? CanHitNPC(NPC target)
        {
            if ((int)Projectile.ai[0] != StateFlying)
                return false;

            if (!target.CanBeChasedBy(this))
                return false;

            return null;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Safety: if boss or not pullable, return instead of latching forever
            if (target.boss || !target.CanBeChasedBy(this))
            {
                StartReturn();
                return;
            }

            Projectile.ai[0] = StateLatched;
            Projectile.ai[1] = target.whoAmI + 1;

            Projectile.velocity    = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.timeLeft    = LatchTime;

            Projectile.netUpdate = true;
        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            StartReturn();
            return false; // we handled it
        }


public override void AI()
{
    Player player = Main.player[Projectile.owner];
    if (!player.active || player.dead)
    {
        Projectile.Kill();
        return;
    }

    int state = (int)Projectile.ai[0];

    if (state == StateReturning)
    {
        Projectile.timeLeft = 2;

        Vector2 toPlayer = player.MountedCenter - Projectile.Center;
        if (toPlayer.LengthSquared() < 24f * 24f)
        {
            Projectile.Kill();
            return;
        }

        Vector2 desiredVel = toPlayer.SafeNormalize(Vector2.Zero) * ReturnSpeed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, ReturnLerp);
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        return;
    }

    Projectile.timeLeft = 2;

    if (Vector2.DistanceSquared(player.MountedCenter, Projectile.Center) > DetachRangeSq)
    {
        StartReturn();
        return;
    }

    if (state == StateFlying)
    {
        Projectile.localAI[1]++;

        if (Projectile.localAI[1] > 90f)
        {
            StartReturn();
            return;
        }

        if (Projectile.velocity.LengthSquared() > 0.01f)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        return;
    }

    int npcIndex = (int)Projectile.ai[1] - 1;
    if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
    {
        StartReturn();
        return;
    }

    NPC npc = Main.npc[npcIndex];
    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.boss)
    {
        StartReturn();
        return;
    }

    Projectile.Center = npc.Center;
    Projectile.rotation = (player.MountedCenter - npc.Center).ToRotation() + MathHelper.PiOver2;

    if (Main.netMode != NetmodeID.MultiplayerClient)
    {
        Vector2 toPlayer = player.MountedCenter - npc.Center;
        float distSq = toPlayer.LengthSquared();

        if (distSq <= ReleaseDistanceSq)
        {
            Vector2 pushAway = (-toPlayer).SafeNormalize(Vector2.UnitX);
            npc.velocity = pushAway * ReleasePushSpeed;
            npc.netUpdate = true;

            StartReturn();
            return;
        }

        Vector2 pullDir = toPlayer.SafeNormalize(Vector2.Zero);

        float resist = MathHelper.Clamp(1f - npc.knockBackResist, 0f, 0.85f);
        float strength = PullStrength * (1f - resist);

        npc.velocity += pullDir * strength;

        float along = Vector2.Dot(npc.velocity, pullDir);
        if (along > PullMaxSpeed)
            npc.velocity -= pullDir * (along - PullMaxSpeed);

        npc.netUpdate = true;
    }
}


        public override bool PreDraw(ref Color lightColor)
        {
            DrawChain();
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }

        private void DrawChain()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active) return;

            Texture2D chainTex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;

            Vector2 start = player.MountedCenter;
            Vector2 end = Projectile.Center;

            Vector2 dir = end - start;
            float length = dir.Length();
            if (length < 8f) return;

            dir /= length;

            float segmentLength = chainTex.Height; // assume vertical segment
            float rotation = dir.ToRotation() - MathHelper.PiOver2;

            for (float i = 0; i < length; i += segmentLength)
            {
                Vector2 pos = start + dir * i;

                Color c = Lighting.GetColor((int)(pos.X / 16f), (int)(pos.Y / 16f));

                Main.EntitySpriteDraw(
                    chainTex,
                    pos - Main.screenPosition,
                    null,
                    c,
                    rotation,
                    new Vector2(chainTex.Width / 2f, chainTex.Height / 2f),
                    1f,
                    SpriteEffects.None,
                    0
                );
            }
        }
    }
}
