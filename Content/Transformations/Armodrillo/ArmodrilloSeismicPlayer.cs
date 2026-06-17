using System;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Armodrillo;

public class ArmodrilloSeismicPlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:Armodrillo";
    public const int MaxGroundCharge = 3;

    private int groundCharge;
    private int groundChargeTime;

    public int GroundCharge => groundChargeTime > 0 ? groundCharge : 0;
    public int GroundChargeTime => groundChargeTime;
    public bool HasGroundCharge => GroundCharge > 0;

    public override void PostUpdate() {
        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        if (omp.currentTransformationId != TransformationId) {
            ClearGroundCharge();
            return;
        }

        if (groundChargeTime > 0) {
            groundChargeTime--;
            return;
        }

        groundCharge = 0;
    }

    public void AddGroundCharge(int amount, int duration) {
        if (amount <= 0 || duration <= 0)
            return;

        groundCharge = Math.Clamp(GroundCharge + amount, 0, MaxGroundCharge);
        groundChargeTime = Math.Max(groundChargeTime, duration);
    }

    public int ConsumeGroundCharge(int maxAmount = MaxGroundCharge) {
        int consumed = Math.Min(GroundCharge, Math.Max(0, maxAmount));
        if (consumed <= 0)
            return 0;

        groundCharge -= consumed;
        if (groundCharge <= 0)
            ClearGroundCharge();

        return consumed;
    }

    private void ClearGroundCharge() {
        groundCharge = 0;
        groundChargeTime = 0;
    }
}
