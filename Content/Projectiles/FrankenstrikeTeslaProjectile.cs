using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Ben10Mod.Content.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FrankenstrikeTeslaProjectile : ModProjectile {
    private float ChargeRatio => MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
    private bool CapacitorCore => Projectile.ai[1] >= 0.5f;
    private bool Overcharged => ChargeRatio >= 0.55f || CapacitorCore;

    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 82;
        Projectile.extraUpdates = 1;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Projectile.velocity.LengthSquared() < (Overcharged ? 784f : 676f))
            Projectile.velocity *= Overcharged ? 1.016f : 1.012f;

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, Overcharged ? new Vector3(0.3f, 0.55f, 1f) : new Vector3(0.2f, 0.44f, 0.9f));

        if (Main.rand.NextBool(2)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                Main.rand.NextBool(3) ? DustID.Electric : DustID.BlueTorch,
                -Projectile.velocity * Main.rand.NextFloat(0.04f, 0.12f), 110, new Color(165, 225, 255),
                Main.rand.NextFloat(0.9f, Overcharged ? 1.25f : 1.08f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 center = Projectile.Center - Main.screenPosition;
        float rotation = direction.ToRotation();
        float length = Overcharged ? 38f : 32f;

        Main.EntitySpriteDraw(pixel, center, null, new Color(105, 155, 255, 120), rotation, Vector2.One * 0.5f,
            new Vector2(length, Overcharged ? 8f : 6.5f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center, null, new Color(230, 245, 255, 210), rotation, Vector2.One * 0.5f,
            new Vector2(length * 0.62f, Overcharged ? 3.6f : 3f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center + perpendicular * 6f, null, new Color(145, 205, 255, 110), rotation + 0.34f,
            Vector2.One * 0.5f, new Vector2(12f, 2.4f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(pixel, center - perpendicular * 6f, null, new Color(145, 205, 255, 110), rotation - 0.34f,
            Vector2.One * 0.5f, new Vector2(12f, 2.4f), SpriteEffects.None, 0);
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int conductiveStacks = identity.GetFrankenstrikeConductiveStacks(Projectile.owner);
        if (conductiveStacks > 0)
            modifiers.SourceDamage *= 1f + conductiveStacks * 0.08f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        AlienIdentityGlobalNPC identity = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        identity.ApplyFrankenstrikeConductive(Projectile.owner, CapacitorCore ? 2 : 1, CapacitorCore ? 260 : 210);
        Main.player[Projectile.owner].GetModPlayer<AlienIdentityPlayer>()
            .AddFrankenstrikeStaticCharge(CapacitorCore ? 13f : 8f);
        target.AddBuff(BuffID.Electrified, Overcharged ? 240 : 180);
        target.netUpdate = true;
    }

    public override void OnKill(int timeLeft) {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 10; i++) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.Electric : DustID.BlueTorch,
                Main.rand.NextVector2Circular(2.8f, 2.8f), 110, new Color(170, 230, 255), Main.rand.NextFloat(0.95f, 1.2f));
            dust.noGravity = true;
        }
    }
}
