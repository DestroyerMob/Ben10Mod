using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Players;

public class AlienIdentityPlayer : ModPlayer {
    public const string FasttrackTransformationId = "Ben10Mod:Fasttrack";
    public const string AstrodactylTransformationId = "Ben10Mod:Astrodactyl";

    private const float FasttrackMaxMomentum = 100f;
    private const float AstrodactylMaxAirSupremacy = 100f;

    public float FasttrackMomentum { get; private set; }
    public float AstrodactylAirSupremacy { get; private set; }

    public float FasttrackMomentumRatio => FasttrackMomentum / FasttrackMaxMomentum;
    public float AstrodactylAirSupremacyRatio => AstrodactylAirSupremacy / AstrodactylMaxAirSupremacy;

    public override void PostUpdate() {
        OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
        UpdateFasttrackMomentum(omp);
        UpdateAstrodactylAirSupremacy(omp);
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
