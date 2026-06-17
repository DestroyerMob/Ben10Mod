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
        public bool TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile profile) {
            return Absorption.TryGetActiveProfile(out profile);
        }

        private void ApplyAbsorptionHitEffects(NPC target) {
            if (target == null || !target.active || target.life <= 0)
                return;

            if (!TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile profile))
                return;

            foreach (MaterialAbsorptionHitEffect effect in profile.HitEffects) {
                if (effect.BuffType <= 0 || effect.BuffTime <= 0)
                    continue;

                int buffTime = Math.Max(1, (int)Math.Round(effect.BuffTime * absorptionDebuffDurationMultiplier));
                target.AddBuff(effect.BuffType, buffTime);
            }
        }

        private void TryAbsorbHeldMaterial() {
            if (!osmosianEquipped) {
                if (Player.whoAmI == Main.myPlayer)
                    ShowAbsorptionFeedback("You need an Osmosian Harness equipped to absorb materials.", new Color(230, 120, 120));
                return;
            }

            if (omnitrixEquipped) {
                if (Player.whoAmI == Main.myPlayer)
                    ShowAbsorptionFeedback("Osmosian absorption cannot be used while an Omnitrix is equipped.", new Color(230, 120, 120));
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.RequestAbsorbMaterial);
                packet.Send();
                return;
            }

            TryAbsorbHeldMaterialDirect();
        }

        public void HandleAbsorbMaterialRequest() {
            TryAbsorbHeldMaterialDirect();
        }

        private void TryAbsorbHeldMaterialDirect() {
            Item heldItem = Player.HeldItem;
            if (heldItem == null || heldItem.IsAir) {
                if (absorbedMaterialTime > 0)
                    ClearAbsorbedMaterial();
                return;
            }

            if (absorbedMaterialTime > 0 && heldItem.type == absorbedMaterialItemType) {
                ClearAbsorbedMaterial();
                return;
            }

            if (!MaterialAbsorptionRegistry.TryGetProfile(heldItem.type, out MaterialAbsorptionProfile profile)) {
                if (absorbedMaterialTime > 0)
                    ClearAbsorbedMaterial();
                else
                    ShowAbsorptionFeedback("That material cannot be absorbed.", new Color(230, 120, 120));
                return;
            }

            int consumeAmount = Math.Max(1, (int)Math.Round(profile.ConsumeAmount * absorptionCostMultiplier));
            int durationTicks = Math.Max(60, (int)Math.Round(profile.DurationTicks * absorptionDurationMultiplier));

            if (heldItem.stack < consumeAmount) {
                ShowAbsorptionFeedback($"You need {consumeAmount} {profile.DisplayName} to absorb it.", new Color(255, 210, 110));
                return;
            }

            heldItem.stack -= consumeAmount;
            if (heldItem.stack <= 0)
                heldItem.TurnToAir();

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Player.whoAmI, Player.selectedItem);

            SetAbsorbedMaterial(profile.SourceItemType, durationTicks, showEffects: true);
        }

        private void ClearAbsorbedMaterial(bool showEffects = true) {
            if (!TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile profile)) {
                absorbedMaterialItemType = 0;
                absorbedMaterialTime = 0;
                Player.ClearBuff(ModContent.BuffType<MaterialAbsorptionBuff>());

                if (Main.netMode == NetmodeID.Server)
                    SyncAbsorbedMaterial(showEffects);
                return;
            }

            SetAbsorbedMaterial(0, 0, showEffects, profile);
        }

        public void ApplyAbsorbedMaterialSync(int itemType, int timeLeft, bool showEffects) {
            MaterialAbsorptionProfile previousProfile = null;
            if (TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile currentProfile))
                previousProfile = currentProfile;

            SetAbsorbedMaterial(itemType, timeLeft, showEffects, previousProfile, shouldSync: false);
        }

        private void SetAbsorbedMaterial(int itemType, int timeLeft, bool showEffects, MaterialAbsorptionProfile previousProfile = null,
            bool shouldSync = true) {
            if (previousProfile == null && TryGetActiveAbsorptionProfile(out MaterialAbsorptionProfile activeProfile))
                previousProfile = activeProfile;

            absorbedMaterialItemType = itemType;
            absorbedMaterialTime = itemType > 0 ? Math.Max(1, timeLeft) : 0;

            int buffType = ModContent.BuffType<MaterialAbsorptionBuff>();
            Player.ClearBuff(buffType);
            if (itemType > 0 && absorbedMaterialTime > 0)
                Player.AddBuff(buffType, absorbedMaterialTime);

            if (showEffects && Main.netMode != NetmodeID.Server) {
                if (itemType > 0 && MaterialAbsorptionRegistry.TryGetProfile(itemType, out MaterialAbsorptionProfile newProfile)) {
                    if (Player.whoAmI == Main.myPlayer) {
                        Main.NewText($"Absorbed {newProfile.DisplayName}.", newProfile.TintColor);
                        CombatText.NewText(Player.getRect(), newProfile.TintColor, newProfile.DisplayName, dramatic: true);
                    }

                    for (int i = 0; i < 30; i++) {
                        Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(24f, 30f), DustID.GemDiamond,
                            Main.rand.NextVector2Circular(2.4f, 2.4f), 80, Color.Lerp(newProfile.TintColor, Color.White, 0.2f), 1.35f);
                        dust.noGravity = true;
                        dust.fadeIn = 1.2f;
                    }
                }
                else if (previousProfile != null) {
                    if (Player.whoAmI == Main.myPlayer)
                        Main.NewText($"{previousProfile.DisplayName} absorption cleared.", new Color(220, 220, 220));

                    for (int i = 0; i < 18; i++) {
                        Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(18f, 22f), DustID.GemDiamond,
                            Main.rand.NextVector2Circular(1.8f, 1.8f), 100, previousProfile.TintColor, 1f);
                        dust.noGravity = true;
                    }
                }
            }

            if (shouldSync && Main.netMode == NetmodeID.Server)
                SyncAbsorbedMaterial(showEffects);
        }

        private void SyncAbsorbedMaterial(bool showEffects, int toWho = -1, int ignoreClient = -1) {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.SyncAbsorbedMaterial);
            packet.Write((byte)Player.whoAmI);
            packet.Write(absorbedMaterialItemType);
            packet.Write(absorbedMaterialTime);
            packet.Write(showEffects);
            packet.Send(toWho, ignoreClient);
        }

        private void ShowAbsorptionFeedback(string message, Color color) {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (Main.netMode == NetmodeID.Server) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.AbsorbMaterialFeedback);
                packet.Write(message);
                packet.Write(color.R);
                packet.Write(color.G);
                packet.Write(color.B);
                packet.Send(Player.whoAmI);
                return;
            }

            if (Player.whoAmI == Main.myPlayer)
                Main.NewText(message, color);
        }
    }
}
