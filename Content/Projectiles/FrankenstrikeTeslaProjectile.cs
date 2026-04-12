using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Transformations.Frankenstrike;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FrankenstrikeTeslaProjectile : ModProjectile {
    public enum ShotVariant {
        Main = 0,
        Side = 1,
        Spire = 2
    }

    private ShotVariant Variant => (ShotVariant)Utils.Clamp((int)Projectile.ai[0], 0, 2);
    private bool StormheartBolt => Projectile.ai[1] >= 0.5f;
    private bool IsSpireBolt => Variant == ShotVariant.Spire;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 78;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 9;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            if (Variant == ShotVariant.Side)
                Projectile.penetrate = 2;
            else if (IsSpireBolt)
                Projectile.penetrate = 1;
        }

        float maxSpeed = IsSpireBolt ? 18.5f : StormheartBolt ? 32f : 28f;
        if (Projectile.velocity.LengthSquared() < maxSpeed * maxSpeed)
            Projectile.velocity *= StormheartBolt ? 1.02f : 1.015f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, StormheartBolt
            ? new Vector3(0.36f, 0.56f, 1f)
            : IsSpireBolt
                ? new Vector3(0.24f, 0.44f, 0.86f)
                : new Vector3(0.2f, 0.42f, 0.82f));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                Main.rand.NextBool(3) ? DustID.Electric : DustID.BlueTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.11f), 108, GetBoltColor(),
                Main.rand.NextFloat(0.92f, StormheartBolt ? 1.28f : 1.1f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = direction.ToRotation();
        float length = Variant switch {
            ShotVariant.Side => 28f,
            ShotVariant.Spire => 24f,
            _ => 36f
        };
        float width = Variant switch {
            ShotVariant.Side => 5.4f,
            ShotVariant.Spire => 4.6f,
            _ => 7.4f
        };

        Main.EntitySpriteDraw(pixel, center, null, GetBoltColor() * 0.68f, rotation, Vector2.One * 0.5f,
            new Vector2(length, width), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(235, 245, 255, 220), rotation, Vector2.One * 0.5f,
            new Vector2(length * 0.62f, width * 0.46f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center + perpendicular * 5f, null, GetBoltColor() * 0.4f, rotation + 0.3f,
            Vector2.One * 0.5f, new Vector2(12f, 2.2f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center - perpendicular * 5f, null, GetBoltColor() * 0.4f, rotation - 0.3f,
            Vector2.One * 0.5f, new Vector2(12f, 2.2f), SpriteEffects.None, 0);
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        if (identity.IsFrankenstrikeOverchargedFor(Projectile.owner))
            modifiers.SourceDamage *= IsSpireBolt ? 1.08f : 1.18f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
            return;

        FrankenstrikeTransformation.ApplyConductiveHit(owner, target, Variant == ShotVariant.Main ? 2 : 1, 240);

        if (!IsSpireBolt) {
            FrankenstrikeTransformation.TryConsumeOvercharged(owner, target, Projectile.GetSource_FromThis(),
                System.Math.Max(1, (int)System.Math.Round(Projectile.damage * (Variant == ShotVariant.Main ? 0.82f : 0.56f))),
                Projectile.knockBack + 0.65f, chainBurst: true, lightningStrike: Variant == ShotVariant.Main);
        }
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 110, GetBoltColor(), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }

    private Color GetBoltColor() {
        return Variant switch {
            ShotVariant.Side => new Color(145, 205, 255),
            ShotVariant.Spire => new Color(125, 185, 255),
            _ when StormheartBolt => new Color(175, 220, 255),
            _ => new Color(135, 190, 255)
        };
    }
}
