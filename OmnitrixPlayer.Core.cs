using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content;
using Ben10Mod.Content.Items.Accessories;

namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        public Item GetActiveOmnitrixItem() {
            if (equippedOmnitrixItem != null &&
                !equippedOmnitrixItem.IsAir &&
                equippedOmnitrixItem.ModItem is Omnitrix)
                return equippedOmnitrixItem;

            return null;
        }

        public Omnitrix GetActiveOmnitrix() {
            if (equippedOmnitrix != null)
                return equippedOmnitrix;

            return GetActiveOmnitrixItem()?.ModItem as Omnitrix;
        }

        public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback) {
            itemConsumedCallback = null;

            Item activeOmnitrixItem = GetActiveOmnitrixItem();
            if (!CanUseActiveOmnitrixAsRecalibratedCraftingMaterial(activeOmnitrixItem))
                return null;

            itemConsumedCallback = (_, index) => {
                if (index != 0)
                    return;

                if (!string.IsNullOrEmpty(currentTransformationId))
                    TransformationHandler.Detransform(Player, 0, showParticles: true, addCooldown: false);

                activeOmnitrixItem.TurnToAir();
                if (ReferenceEquals(equippedOmnitrixItem, activeOmnitrixItem)) {
                    equippedOmnitrixItem = null;
                    equippedOmnitrix = null;
                    omnitrixEquipped = false;
                }
            };

            return new[] { activeOmnitrixItem };
        }

        private bool CanUseActiveOmnitrixAsRecalibratedCraftingMaterial(Item activeOmnitrixItem) {
            return string.Equals(currentTransformationId, "Ben10Mod:GrayMatter", StringComparison.Ordinal) &&
                   activeOmnitrixItem != null &&
                   !activeOmnitrixItem.IsAir &&
                   activeOmnitrixItem.type == ModContent.ItemType<RecalibratedOmnitrix>();
        }

        internal void ApplyOmnitrixEvolutionSync(int resultType) {
            if (Player.whoAmI != Main.myPlayer || resultType <= 0)
                return;

            Item activeOmnitrixItem = GetActiveOmnitrixItem();
            if (activeOmnitrixItem == null || activeOmnitrixItem.IsAir)
                return;

            activeOmnitrixItem.SetDefaults(resultType);
        }

        public bool HasEquippedOsmosianHarness() {
            return osmosianEquipped;
        }

        public bool HasAnyEquippedOmnitrix() {
            return omnitrixEquipped && GetActiveOmnitrix() != null;
        }

        public bool HasTransformationFailsafeEquipped() {
            if (transformationFailsafeEquipped)
                return true;

            Omnitrix activeOmnitrix = GetActiveOmnitrix();
            return activeOmnitrix?.BuiltInTransformationFailsafe == true;
        }

        public bool HasEquippedAnoditeCatalyst() {
            return anoditeCatalystEquipped;
        }

        public static byte NormalizeTransformationSpeedBoostPercent(int percent) {
            int clamped = Utils.Clamp(percent, 0, TransformationSpeedBoostPercentMax);
            int snapped = (int)Math.Round(clamped / (double)TransformationSpeedBoostPercentStep) *
                          TransformationSpeedBoostPercentStep;
            return (byte)Utils.Clamp(snapped, 0, TransformationSpeedBoostPercentMax);
        }

        public bool SetTransformationSpeedBoostPercent(byte percent, bool sync = true, bool showFeedback = false) {
            byte normalizedPercent = NormalizeTransformationSpeedBoostPercent(percent);
            bool changed = transformationSpeedBoostPercent != normalizedPercent;
            transformationSpeedBoostPercent = normalizedPercent;

            if (showFeedback && Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer)
                ShowTransformationSpeedBoostFeedback();

            if (changed && sync)
                SyncTransformationSpeedBoostSettingToServerOrClients();

            return changed;
        }

        public void CycleTransformationSpeedBoostPercent(bool sync = true, bool showFeedback = true) {
            int nextPercent = transformationSpeedBoostPercent >= TransformationSpeedBoostPercentMax
                ? 0
                : transformationSpeedBoostPercent + TransformationSpeedBoostPercentStep;
            SetTransformationSpeedBoostPercent((byte)nextPercent, sync, showFeedback);
        }

        public void ApplyTransformationSpeedBoostSettingSync(byte percent) {
            transformationSpeedBoostPercent = NormalizeTransformationSpeedBoostPercent(percent);
        }

        public float ScaleTransformationSpeedBoost(float boostAmount) {
            return boostAmount > 0f ? boostAmount * TransformationSpeedBoostScale : boostAmount;
        }

        public float ScaleTransformationSpeedMultiplier(float multiplier) {
            return multiplier > 1f
                ? 1f + (multiplier - 1f) * TransformationSpeedBoostScale
                : multiplier;
        }

        private void ShowTransformationSpeedBoostFeedback() {
            float scale = TransformationSpeedBoostScale;
            Color textColor = Color.Lerp(new Color(255, 120, 80), Color.LimeGreen, scale);
            Main.NewText($"Transformation speed boost set to {transformationSpeedBoostPercent}%.", textColor);
        }

        private float ScalePositiveTransformationMovementDelta(float baseValue, float currentValue) {
            if (currentValue <= baseValue)
                return currentValue;

            return baseValue + (currentValue - baseValue) * TransformationSpeedBoostScale;
        }

        private void ApplyTransformationMovementBoostScale(float baseMoveSpeed, float baseMaxRunSpeed,
            float baseAccRunSpeed, float baseRunAcceleration) {
            if (transformationSpeedBoostPercent >= TransformationSpeedBoostPercentMax)
                return;

            Player.moveSpeed = ScalePositiveTransformationMovementDelta(baseMoveSpeed, Player.moveSpeed);
            Player.maxRunSpeed = ScalePositiveTransformationMovementDelta(baseMaxRunSpeed, Player.maxRunSpeed);
            Player.accRunSpeed = ScalePositiveTransformationMovementDelta(baseAccRunSpeed, Player.accRunSpeed);
            Player.runAcceleration = ScalePositiveTransformationMovementDelta(baseRunAcceleration, Player.runAcceleration);
        }

        public void SetTransformationScale(float targetScale, int transitionTicks = 1,
            float? targetHitboxWidthScale = null, float? targetHitboxHeightScale = null) {
            requestedTransformationScale = Math.Max(MinimumTransformationScale, targetScale);
            requestedTransformationScaleTime = Math.Max(1, transitionTicks);
            requestedTransformationHitboxScale = new Vector2(
                Math.Max(MinimumTransformationScale, targetHitboxWidthScale ?? targetScale),
                Math.Max(MinimumTransformationScale, targetHitboxHeightScale ?? targetScale)
            );

            if (Math.Abs(requestedTransformationScale - 1f) > 0.001f)
                lastExpandedTransformationScale = requestedTransformationScale;

            if (Vector2.DistanceSquared(requestedTransformationHitboxScale, Vector2.One) > 0.0001f)
                lastExpandedTransformationHitboxScale = requestedTransformationHitboxScale;
        }

        public Vector2 GetScaledVisualPoint(Vector2 worldPoint) {
            if (Math.Abs(CurrentTransformationScale - 1f) <= 0.001f)
                return worldPoint;

            Vector2 pivot = Player.Bottom;
            return pivot + (worldPoint - pivot) * CurrentTransformationScale;
        }

        private void UpdateTransformationScale(bool forceReset) {
            if (forceReset) {
                requestedTransformationScale = 1f;
                requestedTransformationScaleTime = 1;
                requestedTransformationHitboxScale = Vector2.One;
            }

            float targetScale = requestedTransformationScale;
            float scaleReference = Math.Abs(targetScale - 1f) > 0.001f
                ? lastExpandedTransformationScale
                : (Math.Abs(lastExpandedTransformationScale - 1f) > 0.001f ? lastExpandedTransformationScale : CurrentTransformationScale);
            float step = Math.Abs(scaleReference - 1f) / requestedTransformationScaleTime;
            if (step <= 0f)
                step = Math.Abs(targetScale - CurrentTransformationScale);

            CurrentTransformationScale = MoveTowards(CurrentTransformationScale, targetScale, step);
            float hitboxReferenceX = Math.Abs(requestedTransformationHitboxScale.X - 1f) > 0.001f
                ? lastExpandedTransformationHitboxScale.X
                : (Math.Abs(lastExpandedTransformationHitboxScale.X - 1f) > 0.001f
                    ? lastExpandedTransformationHitboxScale.X
                    : CurrentTransformationHitboxScale.X);
            float hitboxReferenceY = Math.Abs(requestedTransformationHitboxScale.Y - 1f) > 0.001f
                ? lastExpandedTransformationHitboxScale.Y
                : (Math.Abs(lastExpandedTransformationHitboxScale.Y - 1f) > 0.001f
                    ? lastExpandedTransformationHitboxScale.Y
                    : CurrentTransformationHitboxScale.Y);
            float hitboxStepX = Math.Abs(hitboxReferenceX - 1f) / requestedTransformationScaleTime;
            float hitboxStepY = Math.Abs(hitboxReferenceY - 1f) / requestedTransformationScaleTime;

            if (hitboxStepX <= 0f)
                hitboxStepX = Math.Abs(requestedTransformationHitboxScale.X - CurrentTransformationHitboxScale.X);

            if (hitboxStepY <= 0f)
                hitboxStepY = Math.Abs(requestedTransformationHitboxScale.Y - CurrentTransformationHitboxScale.Y);

            CurrentTransformationHitboxScale = new Vector2(
                MoveTowards(CurrentTransformationHitboxScale.X, requestedTransformationHitboxScale.X, hitboxStepX),
                MoveTowards(CurrentTransformationHitboxScale.Y, requestedTransformationHitboxScale.Y, hitboxStepY)
            );

            ApplyHitboxTransformationScale(CurrentTransformationHitboxScale);

            if (Math.Abs(CurrentTransformationScale - 1f) <= 0.001f)
                lastExpandedTransformationScale = 1f;

            if (Vector2.DistanceSquared(CurrentTransformationHitboxScale, Vector2.One) <= 0.0001f)
                lastExpandedTransformationHitboxScale = Vector2.One;

            requestedTransformationScale = 1f;
            requestedTransformationScaleTime = 1;
            requestedTransformationHitboxScale = Vector2.One;
        }

        private void ApplyHitboxTransformationScale(Vector2 scale) {
            int targetWidth = (int)Math.Round(BaseTransformationWidth * scale.X);
            int targetHeight = (int)Math.Round(BaseTransformationHeight * scale.Y);

            if (Player.width == targetWidth && Player.height == targetHeight)
                return;

            float left = Player.position.X;
            float bottom = Player.position.Y + Player.height;
            Player.width = targetWidth;
            Player.height = targetHeight;
            Player.position = new Vector2(left, bottom - targetHeight);
        }

        private static float MoveTowards(float current, float target, float maxDelta) {
            if (Math.Abs(target - current) <= maxDelta)
                return target;

            return current + Math.Sign(target - current) * maxDelta;
        }
    }
}
