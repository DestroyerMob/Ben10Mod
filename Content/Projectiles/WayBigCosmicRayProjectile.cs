using Ben10Mod.Content.Projectiles.UltimateAttacks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class WayBigCosmicRayProjectile : ChannelBeamUltimateProjectile {
    protected override float MaxLength => 3200f;
    protected override float BeamThickness => 52f;
    protected override float StartOffset => 34f;
    protected override int MinEnergyToSustain => 15;
    protected override Vector2 StartScale => new(2.5f, 1.2f);
    protected override Vector2 OuterScale => new(3.6f, 1.2f);
    protected override Vector2 MidScale => new(2.7f, 1.15f);
    protected override Vector2 InnerScale => new(1.8f, 1.1f);
    protected override Color BeamColor => new(110, 255, 235);
    protected override Color BeamHighlightColor => new(225, 255, 255);
    protected override int EndDustType => DustID.GemSapphire;
    protected override int EndDustCount => 7;
    protected override float LightR => 0.16f;
    protected override float LightG => 1.2f;
    protected override float LightB => 1.25f;

    protected override Vector2 GetBeamStart(Player owner, Vector2 direction) {
        float scale = System.Math.Max(1f, owner.GetModPlayer<OmnitrixPlayer>().CurrentTransformationScale);
        Vector2 chestAnchor = owner.Bottom + new Vector2(0f, -owner.height * 0.82f);
        float forwardOffset = 24f + scale * 6f;
        return chestAnchor + direction * forwardOffset + new Vector2(direction.X * scale * 8f, 0f);
    }

    protected override void OnBeamUpdated(Player owner, OmnitrixPlayer omp, Vector2 start, Vector2 direction) {
        if (!Main.rand.NextBool(2))
            return;

        Vector2 end = start + direction * BeamHitLength;
        Dust startDust = Dust.NewDustPerfect(start + Main.rand.NextVector2Circular(18f, 18f), DustID.GemSapphire,
            Main.rand.NextVector2Circular(1.2f, 1.2f), 110, new Color(180, 255, 250), Main.rand.NextFloat(1.2f, 1.6f));
        startDust.noGravity = true;

        Dust endDust = Dust.NewDustPerfect(end + Main.rand.NextVector2Circular(26f, 26f), DustID.GemDiamond,
            Main.rand.NextVector2Circular(2f, 2f), 110, Color.White, Main.rand.NextFloat(1.2f, 1.75f));
        endDust.noGravity = true;
    }
}
