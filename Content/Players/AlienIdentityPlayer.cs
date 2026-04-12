using System;
using Ben10Mod.Content.Transformations.ChromaStone;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Players;

public class AlienIdentityPlayer : ModPlayer {
    public const string ChromaStoneTransformationId = "Ben10Mod:ChromaStone";
    public const string FasttrackTransformationId = "Ben10Mod:Fasttrack";
    public const string AstrodactylTransformationId = "Ben10Mod:Astrodactyl";
    public const string FrankenstrikeTransformationId = "Ben10Mod:Frankenstrike";
    public const string WaterHazardTransformationId = "Ben10Mod:WaterHazard";

    private const float ChromaStoneMaxRadiance = 100f;
    private const float FasttrackMaxMomentum = 100f;
    private const float AstrodactylMaxAirSupremacy = 100f;
    private const float FrankenstrikeMaxStaticCharge = 100f;
    private const float WaterHazardMaxPressure = 100f;

    public float ChromaStoneRadiance { get; private set; }
    public float FasttrackMomentum { get; private set; }
    public float AstrodactylAirSupremacy { get; private set; }
    public float FrankenstrikeStaticCharge { get; private set; }
    public float WaterHazardPressure { get; private set; }

    public float ChromaStoneRadianceRatio => ChromaStoneRadiance / ChromaStoneMaxRadiance;
    public float ChromaStonePrismCharge => ChromaStoneRadiance;
    public float ChromaStonePrismChargeRatio => ChromaStoneRadianceRatio;
    public float FasttrackMomentumRatio => FasttrackMomentum / FasttrackMaxMomentum;
    public float AstrodactylAirSupremacyRatio => AstrodactylAirSupremacy / AstrodactylMaxAirSupremacy;
    public float FrankenstrikeStaticChargeRatio => FrankenstrikeStaticCharge / FrankenstrikeMaxStaticCharge;
    public float WaterHazardPressureRatio => WaterHazardPressure / WaterHazardMaxPressure;

    public override void PostUpdate() {
        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        UpdateChromaStoneRadiance(omp);
        UpdateFasttrackMomentum(omp);
        UpdateAstrodactylAirSupremacy(omp);
        UpdateFrankenstrikeStaticCharge(omp);
        UpdateWaterHazardPressure(omp);
    }

    public void AddChromaStoneRadiance(float amount) {
        ChromaStoneRadiance = MathHelper.Clamp(ChromaStoneRadiance + amount, 0f, ChromaStoneMaxRadiance);
    }

    public void ConsumeChromaStoneRadiance(float amount) {
        ChromaStoneRadiance = Math.Max(0f, ChromaStoneRadiance - amount);
    }

    public void SetChromaStoneRadiance(float amount) {
        ChromaStoneRadiance = MathHelper.Clamp(amount, 0f, ChromaStoneMaxRadiance);
    }

    public void AddChromaStonePrismCharge(float amount) {
        AddChromaStoneRadiance(amount);
    }

    public void ConsumeChromaStonePrismCharge(float amount) {
        ConsumeChromaStoneRadiance(amount);
    }

    public void SetChromaStonePrismCharge(float amount) {
        SetChromaStoneRadiance(amount);
    }

    public void AddFasttrackMomentum(float amount) {
        FasttrackMomentum = MathHelper.Clamp(FasttrackMomentum + amount, 0f, FasttrackMaxMomentum);
    }

    public void ConsumeFasttrackMomentum(float amount) {
        FasttrackMomentum = Math.Max(0f, FasttrackMomentum - amount);
    }

    public void AddAstrodactylAirSupremacy(float amount) {
        AstrodactylAirSupremacy = MathHelper.Clamp(AstrodactylAirSupremacy + amount, 0f, AstrodactylMaxAirSupremacy);
    }

    public void ConsumeAstrodactylAirSupremacy(float amount) {
        AstrodactylAirSupremacy = Math.Max(0f, AstrodactylAirSupremacy - amount);
    }

    public void AddFrankenstrikeStaticCharge(float amount) {
        FrankenstrikeStaticCharge = MathHelper.Clamp(FrankenstrikeStaticCharge + amount, 0f, FrankenstrikeMaxStaticCharge);
    }

    public void ConsumeFrankenstrikeStaticCharge(float amount) {
        FrankenstrikeStaticCharge = Math.Max(0f, FrankenstrikeStaticCharge - amount);
    }

    public void AddWaterHazardPressure(float amount) {
        WaterHazardPressure = MathHelper.Clamp(WaterHazardPressure + amount, 0f, WaterHazardMaxPressure);
    }

    public void ConsumeWaterHazardPressure(float amount) {
        WaterHazardPressure = Math.Max(0f, WaterHazardPressure - amount);
    }

    public static bool IsGrounded(Player player) {
        if (player.velocity.Y < 0f || !player.active || player.dead)
            return false;

        if (Collision.SolidCollision(player.position + new Vector2(0f, player.height - 2f), player.width, 8))
            return true;

        float feetY = player.position.Y + player.height;
        int tileY = (int)Math.Floor((feetY + 2f) / 16f);
        int leftTileX = (int)Math.Floor((player.position.X + 2f) / 16f);
        int centerTileX = (int)Math.Floor(player.Center.X / 16f);
        int rightTileX = (int)Math.Floor((player.position.X + player.width - 2f) / 16f);

        return IsLandingSurface(leftTileX, tileY, feetY) ||
               IsLandingSurface(centerTileX, tileY, feetY) ||
               IsLandingSurface(rightTileX, tileY, feetY);
    }

    private void UpdateChromaStoneRadiance(OmnitrixPlayer omp) {
        if (omp.currentTransformationId != ChromaStoneTransformationId) {
            ChromaStoneRadiance = 0f;
            return;
        }

        ChromaStoneRadiance = MathHelper.Clamp(ChromaStoneRadiance, 0f, ChromaStoneMaxRadiance);
    }

    private void UpdateFasttrackMomentum(OmnitrixPlayer omp) {
        if (omp.currentTransformationId != FasttrackTransformationId) {
            FasttrackMomentum = Math.Max(0f, FasttrackMomentum - 4f);
            return;
        }

        float horizontalSpeed = Math.Abs(Player.velocity.X);
        bool grounded = IsGrounded(Player);
        bool pressingMovement = Player.controlLeft || Player.controlRight;

        if (horizontalSpeed >= 7.5f) {
            AddFasttrackMomentum(omp.PrimaryAbilityEnabled ? 3.4f : 2.4f);
        }
        else if (horizontalSpeed >= 4.5f) {
            AddFasttrackMomentum(omp.PrimaryAbilityEnabled ? 1.9f : 1.35f);
        }
        else {
            FasttrackMomentum = Math.Max(0f, FasttrackMomentum - (grounded ? 1.45f : 0.7f));
        }

        if (grounded && !pressingMovement)
            FasttrackMomentum = Math.Max(0f, FasttrackMomentum - 2.2f);
    }

    private void UpdateAstrodactylAirSupremacy(OmnitrixPlayer omp) {
        if (omp.currentTransformationId != AstrodactylTransformationId) {
            AstrodactylAirSupremacy = Math.Max(0f, AstrodactylAirSupremacy - 5f);
            return;
        }

        bool grounded = IsGrounded(Player);
        bool winging = Player.wingTimeMax > 0 && Player.wingTime < Player.wingTimeMax - 2f;
        bool airborne = !grounded || winging || Math.Abs(Player.velocity.Y) > 0.35f;

        if (!airborne) {
            AstrodactylAirSupremacy = Math.Max(0f, AstrodactylAirSupremacy - 2.6f);
            return;
        }

        float gain = omp.PrimaryAbilityEnabled ? 1.8f : 1.15f;
        if (Player.velocity.Y < -0.4f)
            gain += 0.45f;
        if (Player.controlJump || Player.controlUp)
            gain += 0.3f;

        AddAstrodactylAirSupremacy(gain);
    }

    private void UpdateFrankenstrikeStaticCharge(OmnitrixPlayer omp) {
        if (omp.currentTransformationId != FrankenstrikeTransformationId) {
            FrankenstrikeStaticCharge = Math.Max(0f, FrankenstrikeStaticCharge - 5f);
            return;
        }

        if (omp.PrimaryAbilityEnabled) {
            AddFrankenstrikeStaticCharge(0.7f);
            if (Math.Abs(Player.velocity.X) > 3.5f || Player.velocity.Y < -0.5f)
                AddFrankenstrikeStaticCharge(0.18f);
            return;
        }

        FrankenstrikeStaticCharge = Math.Max(0f, FrankenstrikeStaticCharge - 0.28f);
    }

    private void UpdateWaterHazardPressure(OmnitrixPlayer omp) {
        if (omp.currentTransformationId != WaterHazardTransformationId) {
            WaterHazardPressure = Math.Max(0f, WaterHazardPressure - 6f);
            return;
        }

        bool saturated = Player.wet || (Main.raining && Player.ZoneRain);
        if (saturated)
            AddWaterHazardPressure(omp.PrimaryAbilityEnabled ? 1.5f : 0.9f);

        float naturalDrain = omp.PrimaryAbilityEnabled ? 0.22f : 0.75f;
        WaterHazardPressure = Math.Max(0f, WaterHazardPressure - naturalDrain);
    }

    private static bool IsLandingSurface(int tileX, int tileY, float feetY) {
        Tile tile = Framing.GetTileSafely(tileX, tileY);
        if (!tile.HasTile)
            return false;

        if (WorldGen.SolidTileAllowBottomSlope(tileX, tileY))
            return true;

        if (!Main.tileSolidTop[tile.TileType])
            return false;

        float tileTop = tileY * 16f;
        return feetY <= tileTop + 8f;
    }
}
