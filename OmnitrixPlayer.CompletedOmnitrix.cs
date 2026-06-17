using Ben10Mod.Common.Absorption;
using Ben10Mod.Common.Networking;
using Ben10Mod.Common.Omnitrix;
using Ben10Mod.Keybinds;
using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Consumable;
using Terraria.Audio;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Common.CustomVisuals;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Content.Transformations.XLR8;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Events;

namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        private bool TryTriggerCompletedOmnitrixRevival() {
            return TryTriggerCompletedOmnitrixRevival(string.Empty, requireLethalState: false);
        }

        internal void HandleCompletedOmnitrixRevivalRequest(string requestedTransformationId) {
            if (Main.netMode != NetmodeID.Server)
                return;

            if (!Player.dead && Player.statLife > 0)
                return;

            TryTriggerCompletedOmnitrixRevival(requestedTransformationId, requireLethalState: true);
        }

        private bool TryTriggerCompletedOmnitrixRevival(string requestedTransformationId, bool requireLethalState) {
            if (requireLethalState && !Player.dead && Player.statLife > 0)
                return false;

            if (!completedOmnitrixEquipped || completedOmnitrixRevivalCooldown > 0)
                return false;

            if (GetActiveOmnitrix() is not CompletedOmnitrix completedOmnitrix)
                return false;

            string transformationId;
            if (!string.IsNullOrWhiteSpace(requestedTransformationId)) {
                if (!TryResolveCompletedOmnitrixRevivalTransformation(requestedTransformationId, out transformationId))
                    return false;
            }
            else {
                transformationId = ResolveCompletedOmnitrixRevivalTransformation(completedOmnitrix);
            }

            if (string.IsNullOrEmpty(transformationId))
                return false;

            bool showEffects = Player.whoAmI == Main.myPlayer;
            int durationSeconds = completedOmnitrix.GetTransformationDuration(this);
            int restoredLife = Math.Max(CompletedOmnitrixRevivalMinimumLife,
                (int)Math.Ceiling(Player.statLifeMax2 * CompletedOmnitrixRevivalLifeRatio));
            int targetLife = Math.Min(Player.statLifeMax2, Math.Max(Player.statLife, restoredLife));
            float targetEnergy = omnitrixEnergyMax > 0f
                ? Math.Min(omnitrixEnergyMax, omnitrixEnergy + CompletedOmnitrixRevivalEnergyRestoreAmount)
                : omnitrixEnergy + CompletedOmnitrixRevivalEnergyRestoreAmount;

            ApplyCompletedOmnitrixRevivalState(
                transformationId,
                durationSeconds,
                targetLife,
                targetEnergy,
                CompletedOmnitrixRevivalCooldownTicks,
                showEffects);

            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
                RequestCompletedOmnitrixRevivalSync(transformationId);
            else if (Main.netMode == NetmodeID.Server)
                SyncCompletedOmnitrixRevival(transformationId, durationSeconds);

            return true;
        }

        internal void ApplyCompletedOmnitrixRevivalSync(string transformationId, int durationSeconds, int cooldownTicks,
            int statLife, float syncedOmnitrixEnergy) {
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null ||
                transformation.ParentTransformation != null ||
                transformation.IsAccessoryTransformation(this))
                return;

            ApplyCompletedOmnitrixRevivalState(
                transformation.FullID,
                Math.Max(1, durationSeconds),
                statLife,
                syncedOmnitrixEnergy,
                cooldownTicks,
                showEffects: Player.whoAmI == Main.myPlayer);
        }

        private void ApplyCompletedOmnitrixRevivalState(string transformationId, int durationSeconds, int statLife,
            float syncedOmnitrixEnergy, int cooldownTicks, bool showEffects) {
            if (IsTransformed && !string.Equals(currentTransformationId, transformationId, StringComparison.OrdinalIgnoreCase))
                TransformationHandler.Detransform(Player, 0, showParticles: false, addCooldown: false, playSound: false);

            TransformationHandler.Transform(Player, transformationId, Math.Max(1, durationSeconds),
                showParticles: showEffects, playSound: showEffects);

            Player.statLife = Math.Min(Player.statLifeMax2, Math.Max(1, statLife));
            Player.dead = false;
            Player.immuneNoBlink = true;
            Player.immuneTime = Math.Max(Player.immuneTime, 180);
            omnitrixEnergy = omnitrixEnergyMax > 0f
                ? Math.Min(omnitrixEnergyMax, Math.Max(0f, syncedOmnitrixEnergy))
                : Math.Max(0f, syncedOmnitrixEnergy);
            completedOmnitrixRevivalCooldown = Math.Max(0, cooldownTicks);

            if (showEffects)
                CombatText.NewText(Player.getRect(), new Color(96, 255, 160), "Emergency Transform!", dramatic: true);
        }

        private void RequestCompletedOmnitrixRevivalSync(string transformationId) {
            if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI != Main.myPlayer)
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.RequestCompletedOmnitrixRevival);
            packet.Write(transformationId ?? string.Empty);
            packet.Send();
        }

        private void SyncCompletedOmnitrixRevival(string transformationId, int durationSeconds) {
            if (Main.netMode != NetmodeID.Server)
                return;

            NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, Player.whoAmI);

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.SyncCompletedOmnitrixRevival);
            packet.Write((byte)Player.whoAmI);
            packet.Write(transformationId ?? string.Empty);
            packet.Write(durationSeconds);
            packet.Write(completedOmnitrixRevivalCooldown);
            packet.Write(Player.statLife);
            packet.Write(omnitrixEnergy);
            packet.Send();
        }

        private string ResolveCompletedOmnitrixRevivalTransformation(Omnitrix activeOmnitrix) {
            if (TryResolveCompletedOmnitrixRevivalTransformation(
                    activeOmnitrix.transformationSlots,
                    activeOmnitrix.transformationNum,
                    out string selectedTransformationId))
                return selectedTransformationId;

            List<string> candidates = new();
            foreach (string unlockedTransformationId in unlockedTransformations) {
                if (TryResolveCompletedOmnitrixRevivalTransformation(unlockedTransformationId,
                        out string candidateTransformationId) &&
                    !candidates.Contains(candidateTransformationId))
                    candidates.Add(candidateTransformationId);
            }

            if (candidates.Count <= 0)
                return string.Empty;

            if (candidates.Count > 1 && !string.IsNullOrEmpty(currentTransformationId)) {
                List<string> inactiveCandidates = candidates
                    .Where(candidate => !string.Equals(candidate, currentTransformationId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (inactiveCandidates.Count > 0)
                    candidates = inactiveCandidates;
            }

            return candidates[Main.rand.Next(candidates.Count)];
        }

        private bool TryResolveCompletedOmnitrixRevivalTransformation(string[] slots, int slotIndex,
            out string transformationId) {
            transformationId = string.Empty;
            if (slots == null || slotIndex < 0 || slotIndex >= slots.Length)
                return false;

            return TryResolveCompletedOmnitrixRevivalTransformation(slots[slotIndex], out transformationId);
        }

        private bool TryResolveCompletedOmnitrixRevivalTransformation(string candidateId, out string transformationId) {
            transformationId = string.Empty;
            Transformation transformation = TransformationLoader.Resolve(candidateId);
            if (transformation == null ||
                transformation.ParentTransformation != null ||
                transformation.IsAccessoryTransformation(this) ||
                !unlockedTransformations.Contains(transformation.FullID))
                return false;

            transformationId = transformation.FullID;
            return true;
        }

        private void TriggerTransformationFailsafe() {
            Omnitrix activeOmnitrix = GetActiveOmnitrix();
            int cooldownSeconds = activeOmnitrix?.GetDetransformCooldownDuration(this) ?? cooldownTime;
            if (cooldownSeconds <= 0)
                cooldownSeconds = cooldownTime;
            cooldownSeconds = Math.Max(cooldownSeconds, (int)Math.Ceiling(cooldownSeconds * 1.35f));
            bool showEffects = Player.whoAmI == Main.myPlayer;

            skipAutomaticForcedDetransformHandling = true;
            TransformationHandler.Detransform(Player, cooldownSeconds, showParticles: showEffects, addCooldown: true,
                playSound: showEffects);

            Player.statLife = 1;
            Player.dead = false;
            Player.immuneNoBlink = true;
            Player.immuneTime = Math.Max(Player.immuneTime, 180);
            omnitrixEnergy = 0;

            if (showEffects)
                CombatText.NewText(Player.getRect(), new Color(96, 255, 160), "Failsafe!", dramatic: true);

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, Player.whoAmI);
        }

        public void TriggerCompletedOmnitrixSync(string previousTransformationId, string nextTransformationId) {
            if (!completedOmnitrixEquipped ||
                string.IsNullOrWhiteSpace(previousTransformationId) ||
                string.IsNullOrWhiteSpace(nextTransformationId) ||
                string.Equals(previousTransformationId, nextTransformationId, StringComparison.OrdinalIgnoreCase))
                return;

            completedOmnitrixSyncTime = CompletedOmnitrixSyncDurationTicks;
            RestoreOmnitrixEnergy(CompletedOmnitrixSyncRestoreAmount);

            if (Player.whoAmI != Main.myPlayer)
                return;

            CombatText.NewText(Player.getRect(), new Color(132, 255, 210), "Omni Sync", dramatic: true);
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.22f, Volume = 0.52f }, Player.Center);
        }
    }
}
