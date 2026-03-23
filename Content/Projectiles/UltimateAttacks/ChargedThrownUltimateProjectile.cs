using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.UltimateAttacks;

public abstract class ChargedThrownUltimateProjectile : ModProjectile
{
    private bool _launched;
    private int _sustainTimer;

    protected virtual Vector2 ChargeOffset => new(0f, -78f);
    protected virtual float InitialScale => 0.3f;
    protected virtual float MaxChargeScale => 2.2f;
    protected virtual float ChargeStep => 0.038f;
    protected virtual float LaunchSpeed => 5f;
    protected virtual int MaxLifetime => 600;

    protected bool IsCharging => !_launched;

    public override void SetDefaults() {
        Projectile.width = 128;
        Projectile.height = 128;
        Projectile.scale = InitialScale;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.timeLeft = MaxLifetime;
        ConfigureDefaults();
    }

    protected virtual void ConfigureDefaults() { }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];

        if (!_launched) {
            if (ShouldKeepCharging(owner)) {
                if (!TryConsumeSustainCost(owner)) {
                    owner.channel = false;
                }

                if (!ShouldKeepCharging(owner)) {
                    _launched = true;
                    Projectile.velocity = GetLaunchDirection(owner) * LaunchSpeed;
                    OnLaunched(owner);
                    UpdateReleased(owner);
                    return;
                }

                Projectile.Center = owner.Center + ChargeOffset;
                Projectile.rotation = GetChargingRotation(owner);

                if (Projectile.scale < MaxChargeScale) {
                    Projectile.scale += ChargeStep;
                    Projectile.scale = System.Math.Min(MaxChargeScale, Projectile.scale);
                }
                else {
                    owner.channel = false;
                }

                ResizeHitboxToScale();
                UpdateCharging(owner);
                return;
            }

            _launched = true;
            Projectile.velocity = GetLaunchDirection(owner) * LaunchSpeed;
            OnLaunched(owner);
        }

        UpdateReleased(owner);
    }

    protected virtual bool ShouldKeepCharging(Player owner) {
        return owner.active && !owner.dead && owner.channel;
    }

    protected virtual float GetChargingRotation(Player owner) => 0f;

    protected virtual Vector2 GetLaunchDirection(Player owner) {
        return (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * owner.direction);
    }

    protected virtual void UpdateCharging(Player owner) { }

    protected virtual void OnLaunched(Player owner) { }

    protected virtual void UpdateReleased(Player owner) { }

    private bool TryConsumeSustainCost(Player owner) {
        if (owner.whoAmI != Main.myPlayer)
            return true;

        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        var trans = omp.CurrentTransformation;
        if (trans == null)
            return true;

        int sustainInterval = trans.GetAttackSustainInterval(OmnitrixPlayer.AttackSelection.Ultimate, omp);
        int sustainCost = trans.GetAttackSustainEnergyCost(OmnitrixPlayer.AttackSelection.Ultimate, omp);
        if (sustainInterval <= 0 || sustainCost <= 0)
            return true;

        _sustainTimer++;
        if (_sustainTimer < sustainInterval)
            return true;

        _sustainTimer = 0;
        return trans.TryConsumeAttackSustainCost(OmnitrixPlayer.AttackSelection.Ultimate, omp);
    }

    protected void ResizeHitboxToScale() {
        Projectile.width = (int)(128 * Projectile.scale);
        Projectile.height = (int)(128 * Projectile.scale);
    }
}
