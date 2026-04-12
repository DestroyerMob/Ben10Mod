using System;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EchoEcho;

public class EchoEchoStatePlayer : ModPlayer {
    public const string TransformationId = "Ben10Mod:EchoEcho";
    public const int DuplicateCooldownTicks = 150;
    public const int EchoShiftCooldownTicks = 5 * 60;
    public const int ChorusShiftCooldownTicks = 90;
    public const int ChorusOverloadDurationTicks = 8 * 60;
    public const int ChorusOverloadCooldownTicks = 45 * 60;
    public const int BaseMaxEchoes = 2;
    public const int ChorusMaxEchoes = 3;

    private bool echoEchoActive;
    private bool chorusWasActive;

    public bool ChorusActive {
        get {
            OmnitrixPlayer omp = Player.GetModPlayer<OmnitrixPlayer>();
            return echoEchoActive &&
                   omp.IsUltimateAbilityActive &&
                   string.Equals(omp.ultimateAbilityTransformationId, TransformationId, StringComparison.Ordinal);
        }
    }

    public int ActiveEchoCount => EchoEchoCloneProjectile.CountOwnedEchoes(Player);
    public int MaxEchoCount => ChorusActive ? ChorusMaxEchoes : BaseMaxEchoes;
    public bool HasAnyEchoes => ActiveEchoCount > 0;

    public int ChorusTicksRemaining {
        get {
            if (!ChorusActive)
                return 0;

            return Player.GetModPlayer<OmnitrixPlayer>().GetActiveAbilityRemainingTicks(OmnitrixPlayer.AttackSelection.Ultimate);
        }
    }

    public float ChorusProgress {
        get {
            if (!ChorusActive)
                return 0f;

            return MathHelper.Clamp(ChorusTicksRemaining / (float)ChorusOverloadDurationTicks, 0f, 1f);
        }
    }

    public override void ResetEffects() {
        echoEchoActive = Player.GetModPlayer<OmnitrixPlayer>().currentTransformationId == TransformationId;
    }

    public override void PostUpdate() {
        bool chorusActive = ChorusActive;
        if (!echoEchoActive) {
            chorusWasActive = false;
            return;
        }

        if (Player.whoAmI == Main.myPlayer) {
            if (chorusActive && !chorusWasActive)
                EnsureChorusEchoes();
            else if (!chorusActive && chorusWasActive)
                ResolveChorusShutdown();
        }

        chorusWasActive = chorusActive;
    }

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) {
        if (!echoEchoActive || proj == null || !proj.active || !proj.hostile || proj.damage <= 0)
            return;

        modifiers.FinalDamage *= 0.8f;
        modifiers.Knockback *= 0.85f;
    }

    public float GetEchoDamageMultiplier(int relayIndex) {
        return relayIndex switch {
            0 => EchoEchoTransformation.FirstEchoDamageMultiplier,
            1 => EchoEchoTransformation.SecondEchoDamageMultiplier,
            _ => EchoEchoTransformation.ChorusEchoDamageMultiplier
        };
    }

    public int GetEchoRepeatDelayTicks(int relayIndex) {
        if (ChorusActive)
            return 4 + relayIndex * 4;

        return 6 + relayIndex * 6;
    }

    private void EnsureChorusEchoes() {
        int missingEchoes = ChorusMaxEchoes - ActiveEchoCount;
        if (missingEchoes <= 0)
            return;

        int anchorDamage = EchoEchoTransformation.ResolveHeroDamage(Player, 0.42f);
        for (int i = 0; i < missingEchoes; i++) {
            Vector2 position = BuildChorusEchoPosition(i);
            EchoEchoCloneProjectile.SpawnEcho(Player, Player.GetSource_FromThis(), position, anchorDamage, 0f, temporary: true);
        }
    }

    private void ResolveChorusShutdown() {
        EchoEchoCloneProjectile.CollapseTemporaryEchoes(Player, spawnCollapseBursts: true);
        EchoEchoCloneProjectile.PruneOwnedEchoes(Player, BaseMaxEchoes);
    }

    private Vector2 BuildChorusEchoPosition(int addedEchoIndex) {
        Vector2 aimDirection = Player.DirectionTo(Main.MouseWorld);
        if (aimDirection == Vector2.Zero)
            aimDirection = new Vector2(Player.direction == 0 ? 1 : Player.direction, 0f);

        float[] angleOffsets = { -0.6f, 0f, 0.6f };
        float angleOffset = angleOffsets[Math.Min(angleOffsets.Length - 1, ActiveEchoCount + addedEchoIndex)];
        Vector2 offsetDirection = aimDirection.RotatedBy(angleOffset);
        Vector2 desiredCenter = Player.Center + offsetDirection * 88f + new Vector2(0f, -18f);
        return EchoEchoCloneProjectile.ResolvePlacementCenter(Player, desiredCenter);
    }
}
