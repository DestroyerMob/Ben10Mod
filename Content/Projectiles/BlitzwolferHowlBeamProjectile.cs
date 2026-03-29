using System;
using System.IO;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class BlitzwolferHowlBeamProjectile : ModProjectile {
    private const int DefaultPulseInterval = 14;
    private const float PulseSpeed = 10.5f;
    private const float MouthOffset = 16f;
    private const float PulseSpawnOffset = 26f;

    private SlotId _loopSlot;
    private bool _loopStarted;
    private int _shotTimer;
    private int _sustainTimer;
    private bool _spawnedInitialPulse;
    private Vector2 _syncedAimDirection = Vector2.UnitX;
    private bool _hasSyncedAimDirection;
    private int _aimSyncTimer;

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        Projectile.alpha = 255;
        Projectile.timeLeft = 2;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();

        if (!ShouldStayAlive(owner, omp)) {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;

        Vector2 direction = GetAimDirection(owner);
        Projectile.velocity = direction;
        Projectile.rotation = direction.ToRotation();
        Projectile.Center = owner.MountedCenter + direction * MouthOffset;

        owner.ChangeDir(direction.X >= 0f ? 1 : -1);
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;
        owner.itemRotation = (float)Math.Atan2(direction.Y * owner.direction, direction.X * owner.direction);

        if (Projectile.owner == Main.myPlayer) {
            if (!TryUpdateSustain(owner, omp)) {
                owner.channel = false;
                Projectile.Kill();
                return;
            }

            TryFirePulse(owner, omp, direction);
        }

        UpdateLoopSound();
        Lighting.AddLight(Projectile.Center, 0.16f, 0.85f, 0.24f);

        if (!Main.dedServ && Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.GreenTorch,
                direction * Main.rand.NextFloat(0.2f, 1.2f), 90, new Color(120, 255, 120), Main.rand.NextFloat(1f, 1.25f));
            dust.noGravity = true;
            dust.velocity *= 0.6f;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_syncedAimDirection.X);
        writer.Write(_syncedAimDirection.Y);
        writer.Write(_hasSyncedAimDirection);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _syncedAimDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        _hasSyncedAimDirection = reader.ReadBoolean();
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer &&
            SoundEngine.TryGetActiveSound(_loopSlot, out ActiveSound active)) {
            active.Stop();
        }
    }

    private bool ShouldStayAlive(Player owner, OmnitrixPlayer omp) {
        return owner.active &&
               !owner.dead &&
               omp.currentTransformationId == "Ben10Mod:Blitzwolfer" &&
               omp.IsPrimaryAbilityAttackLoaded &&
               owner.channel &&
               !owner.noItems &&
               !owner.CCed;
    }

    private void TryFirePulse(Player owner, OmnitrixPlayer omp, Vector2 direction) {
        if (!_spawnedInitialPulse) {
            _spawnedInitialPulse = true;
            SpawnPulse(owner, direction);
            return;
        }

        _shotTimer++;
        if (_shotTimer < GetPulseInterval(omp))
            return;

        _shotTimer = 0;
        SpawnPulse(owner, direction);
    }

    private void SpawnPulse(Player owner, Vector2 direction) {
        Vector2 shotDirection = direction.RotatedBy(Main.rand.NextFloat(-0.06f, 0.06f));
        Vector2 spawnPosition = owner.MountedCenter + shotDirection * PulseSpawnOffset;
        bool heightened = global::Ben10Mod.Content.Transformations.Blitzwolfer.BlitzwolferTransformation.HasTrackedPrey(owner);
        int projectileIndex = Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPosition, shotDirection * PulseSpeed,
            ModContent.ProjectileType<BlitzwolferHowlPulseProjectile>(), Projectile.damage, Projectile.knockBack, owner.whoAmI,
            heightened ? 1f : 0f);

        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;

        SoundEngine.PlaySound(SoundID.Item38 with {
            Pitch = -0.32f,
            Volume = 0.34f,
            MaxInstances = 6
        }, spawnPosition);
    }

    private bool TryUpdateSustain(Player owner, OmnitrixPlayer omp) {
        if (!_spawnedInitialPulse)
            return true;

        var transformation = omp.CurrentTransformation;
        if (transformation == null || owner.whoAmI != Main.myPlayer)
            return true;

        int sustainInterval = transformation.GetAttackSustainInterval(OmnitrixPlayer.AttackSelection.PrimaryAbility, omp);
        int sustainCost = transformation.GetAttackSustainEnergyCost(OmnitrixPlayer.AttackSelection.PrimaryAbility, omp);
        if (sustainInterval <= 0 || sustainCost <= 0)
            return true;

        _sustainTimer++;
        if (_sustainTimer < sustainInterval)
            return true;

        _sustainTimer = 0;
        return transformation.TryConsumeAttackSustainCost(OmnitrixPlayer.AttackSelection.PrimaryAbility, omp);
    }

    private int GetPulseInterval(OmnitrixPlayer omp) {
        var transformation = omp.CurrentTransformation;
        if (transformation == null)
            return DefaultPulseInterval;

        int sustainInterval = transformation.GetAttackSustainInterval(OmnitrixPlayer.AttackSelection.PrimaryAbility, omp);
        return sustainInterval > 0 ? sustainInterval : DefaultPulseInterval;
    }

    private void UpdateLoopSound() {
        if (Projectile.owner != Main.myPlayer)
            return;

        SoundStyle loopSound = SoundID.Item62 with {
            Pitch = -0.55f,
            Volume = 0.22f,
            IsLooped = true,
            MaxInstances = 1,
            SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
        };

        if (!_loopStarted) {
            _loopSlot = SoundEngine.PlaySound(loopSound, Projectile.Center);
            _loopStarted = true;
        }

        if (SoundEngine.TryGetActiveSound(_loopSlot, out ActiveSound active))
            active.Position = Projectile.Center;
    }

    private Vector2 GetLocalAimDirection(Player owner) {
        Vector2 direction = Main.MouseWorld - owner.Center;
        if (direction.LengthSquared() < 0.0001f)
            direction = new Vector2(owner.direction, 0f);

        direction.Normalize();
        return direction;
    }

    private Vector2 GetAimDirection(Player owner) {
        if (Main.netMode == NetmodeID.SinglePlayer || Projectile.owner == Main.myPlayer) {
            Vector2 localDirection = GetLocalAimDirection(owner);
            SyncAimDirection(localDirection);
            return localDirection;
        }

        return GetSyncedAimDirection(owner);
    }

    private void SyncAimDirection(Vector2 direction) {
        bool changed = !_hasSyncedAimDirection || Vector2.DistanceSquared(direction, _syncedAimDirection) > 0.0004f;
        _aimSyncTimer++;
        if (!changed && _aimSyncTimer < 6)
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
            : new Vector2(owner.direction, 0f);
        return fallback.SafeNormalize(new Vector2(owner.direction, 0f));
    }
}
