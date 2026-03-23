using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles.UltimateAttacks;

public abstract class ChargedThrownUltimateProjectile : ModProjectile
{
    private bool _launched;
    private int _sustainTimer;
    private Vector2 _syncedAimDirection = Vector2.UnitX;
    private bool _hasSyncedAimDirection;
    private int _aimSyncTimer;

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
        if (Main.netMode == NetmodeID.MultiplayerClient && Projectile.owner != Main.myPlayer) {
            UpdateRemoteClientAI(owner);
            return;
        }

        if (!_launched) {
            Vector2 aimDirection = GetLaunchDirection(owner);
            if (ShouldKeepCharging(owner)) {
                if (!TryConsumeSustainCost(owner)) {
                    owner.channel = false;
                }

                if (!ShouldKeepCharging(owner)) {
                    _launched = true;
                    Projectile.velocity = aimDirection * LaunchSpeed;
                    if (Main.netMode != NetmodeID.SinglePlayer)
                        Projectile.netUpdate = true;
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
            Projectile.velocity = aimDirection * LaunchSpeed;
            if (Main.netMode != NetmodeID.SinglePlayer)
                Projectile.netUpdate = true;
            OnLaunched(owner);
        }

        UpdateReleased(owner);
    }

    protected virtual bool ShouldKeepCharging(Player owner) {
        return owner.active && !owner.dead && owner.channel;
    }

    protected virtual float GetChargingRotation(Player owner) => 0f;

    protected virtual Vector2 GetLocalAimDirection(Player owner) {
        return (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * owner.direction);
    }

    protected virtual Vector2 GetLaunchDirection(Player owner) {
        if (Main.netMode == NetmodeID.SinglePlayer || Projectile.owner == Main.myPlayer) {
            Vector2 localDirection = GetLocalAimDirection(owner);
            SyncAimDirection(localDirection);
            return localDirection;
        }

        return GetSyncedAimDirection(owner);
    }

    protected virtual void UpdateCharging(Player owner) { }

    protected virtual void OnLaunched(Player owner) { }

    protected virtual void UpdateReleased(Player owner) { }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_launched);
        writer.Write(_syncedAimDirection.X);
        writer.Write(_syncedAimDirection.Y);
        writer.Write(_hasSyncedAimDirection);
        writer.Write(Projectile.scale);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _launched = reader.ReadBoolean();
        _syncedAimDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        _hasSyncedAimDirection = reader.ReadBoolean();
        Projectile.scale = reader.ReadSingle();
        ResizeHitboxToScale();
    }

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

    private void UpdateRemoteClientAI(Player owner) {
        if (!_launched) {
            Projectile.Center = owner.Center + ChargeOffset;
            Projectile.rotation = GetChargingRotation(owner);
            ResizeHitboxToScale();
            UpdateCharging(owner);
            return;
        }

        UpdateReleased(owner);
    }

    private void SyncAimDirection(Vector2 direction) {
        bool changed = !_hasSyncedAimDirection || Vector2.DistanceSquared(direction, _syncedAimDirection) > 0.0004f;
        _aimSyncTimer++;
        if (!changed && _aimSyncTimer < 4)
            return;

        _syncedAimDirection = direction;
        _hasSyncedAimDirection = true;
        _aimSyncTimer = 0;
        if (Main.netMode != NetmodeID.SinglePlayer)
            Projectile.netUpdate = true;
    }

    private Vector2 GetSyncedAimDirection(Player owner) {
        if (_hasSyncedAimDirection && _syncedAimDirection.LengthSquared() > 0.0001f)
            return _syncedAimDirection;

        Vector2 fallback = Projectile.velocity.LengthSquared() > 0.0001f
            ? Projectile.velocity
            : Vector2.UnitX * owner.direction;
        return fallback.SafeNormalize(Vector2.UnitX * owner.direction);
    }
}
