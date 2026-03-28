using System;
using Ben10Mod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class HeatBlastSuperheatAuraProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    private const float AuraRadius = 144f;
    private const int RangeDustPoints = 36;
    private const int BurnDuration = 3 * 60;
    private const int DamageInterval = 15;

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.netImportant = true;
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) {
            Projectile.Kill();
            return;
        }

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        if (!omp.IsTransformed || omp.currentTransformationId != "Ben10Mod:HeatBlast" || !omp.IsTertiaryAbilityActive) {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = 0f;

        EmitSuperheatDust(owner, omp);
        Lighting.AddLight(Projectile.Center, omp.snowflake
            ? new Vector3(0.55f, 0.85f, 1.15f) * 0.72f
            : new Vector3(1.2f, 0.46f, 0.08f) * 0.92f);

        UpdateLocalHitCooldowns();
        TryDamageNearbyNPCs(owner, omp);
    }

    private void EmitSuperheatDust(Player owner, OmnitrixPlayer omp) {
        int flameDustType = omp.snowflake ? DustID.IceTorch : DustID.Torch;
        int glowDustType = omp.snowflake ? DustID.SnowflakeIce : DustID.Flare;
        int hazeDustType = omp.snowflake ? DustID.BlueTorch : DustID.Smoke;
        Color flameColor = omp.snowflake ? new Color(175, 235, 255) : new Color(255, 118, 45);
        Color glowColor = omp.snowflake ? new Color(220, 245, 255) : new Color(255, 205, 125);

        if (Projectile.localAI[0]++ % 5f == 0f) {
            float rotation = Main.GlobalTimeWrappedHourly * 1.9f;
            for (int i = 0; i < RangeDustPoints; i++) {
                float angle = rotation + MathHelper.TwoPi * i / RangeDustPoints;
                Vector2 unit = angle.ToRotationVector2();
                Vector2 ringPosition = Projectile.Center + unit * AuraRadius;
                Vector2 drift = unit.RotatedBy(MathHelper.PiOver2) * 0.55f;
                Dust ringDust = Dust.NewDustPerfect(ringPosition, i % 4 == 0 ? glowDustType : flameDustType, drift, 92,
                    Color.Lerp(flameColor, glowColor, Main.rand.NextFloat(0.2f, 0.75f)), Main.rand.NextFloat(1f, 1.3f));
                ringDust.noGravity = true;
            }
        }

        for (int i = 0; i < 3; i++) {
            Vector2 offset = Main.rand.NextVector2Circular(owner.width * 0.4f, owner.height * 0.6f);
            Vector2 velocity = new Vector2(offset.X * 0.018f, Main.rand.NextFloat(-2.6f, -1.1f));
            Dust bodyDust = Dust.NewDustPerfect(owner.Center + offset, i == 0 ? glowDustType : flameDustType, velocity, 100,
                Color.Lerp(flameColor, glowColor, Main.rand.NextFloat(0.1f, 0.55f)), Main.rand.NextFloat(1.05f, 1.55f));
            bodyDust.noGravity = true;
        }

        if (Main.rand.NextBool(2)) {
            Vector2 hazeOffset = Main.rand.NextVector2Circular(owner.width * 0.45f, owner.height * 0.7f);
            Dust hazeDust = Dust.NewDustPerfect(owner.Center + hazeOffset, hazeDustType,
                new Vector2(hazeOffset.X * 0.008f, Main.rand.NextFloat(-0.9f, -0.25f)), 120,
                Color.Lerp(flameColor, Color.White, 0.25f), Main.rand.NextFloat(0.85f, 1.1f));
            hazeDust.noGravity = true;
            hazeDust.fadeIn = 0.9f;
        }
    }

    private void UpdateLocalHitCooldowns() {
        for (int i = 0; i < Projectile.localNPCImmunity.Length; i++) {
            if (Projectile.localNPCImmunity[i] > 0)
                Projectile.localNPCImmunity[i]--;
        }
    }

    private void TryDamageNearbyNPCs(Player owner, OmnitrixPlayer omp) {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int auraDamage = Math.Max(1, Projectile.damage);

        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy(Projectile))
                continue;

            if (Projectile.localNPCImmunity[npc.whoAmI] > 0)
                continue;

            if (!IsWithinAura(npc))
                continue;

            if (omp.snowflake)
                npc.AddBuff(BuffID.Frostburn2, BurnDuration);
            else
                npc.AddBuff(BuffID.OnFire3, BurnDuration);

            int hitDirection = npc.Center.X >= owner.Center.X ? 1 : -1;
            npc.SimpleStrikeNPC(auraDamage, hitDirection, false, 0f, ModContent.GetInstance<HeroDamage>());
            Projectile.localNPCImmunity[npc.whoAmI] = DamageInterval;
        }
    }

    private bool IsWithinAura(NPC npc) {
        float npcRadius = Math.Max(npc.width, npc.height) * 0.5f;
        return Vector2.Distance(npc.Center, Projectile.Center) <= AuraRadius + npcRadius;
    }
}
