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
        public void BeginPossession(int targetIndex, Vector2 returnPosition, int duration = PossessionDuration,
            bool shouldSync = true, bool playEffects = true) {
            if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
                return;

            NPC target = Main.npc[targetIndex];
            if (target == null || !target.active || target.life <= 0)
                return;

            bool sameTarget = inPossessionMode && possessedTargetIndex == targetIndex;

            Possession.Begin(targetIndex, returnPosition, duration);
            Player.invis = true;
            Player.Center = target.Center;
            Player.velocity = target.velocity * 0.8f;

            if (!sameTarget && playEffects && Main.netMode != NetmodeID.Server) {
                SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.5f, Volume = 0.8f }, Player.Center);
                for (int i = 0; i < 40; i++) {
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                        Main.rand.NextVector2Circular(8f, 8f), Scale: 2f);
                    d.noGravity = true;
                }
            }

            if (shouldSync && Main.netMode == NetmodeID.Server)
                SyncPossessionState();
        }

        public void ApplyPossessionStateSync(bool active, int targetIndex, Vector2 returnPosition, int timer) {
            prePossessionPosition = returnPosition;
            if (active) {
                BeginPossession(targetIndex, returnPosition, timer, shouldSync: false);
                return;
            }

            EndPossession(shouldSync: false);
        }

        private void EndPossession(bool shouldSync = true, bool playEffects = true) {
            if (!inPossessionMode && possessedTargetIndex < 0)
                return;

            Possession.End();

            Player.position = prePossessionPosition;
            Player.velocity = Vector2.Zero;
            Player.invis = false;
            Player.immune = true;
            Player.immuneNoBlink = true;
            Player.immuneTime = 60;

            if (playEffects && Main.netMode != NetmodeID.Server) {
                SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.3f, Volume = 0.8f }, Player.Center);
                for (int i = 0; i < 30; i++) {
                    Dust d = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(6f, 6f),
                        Scale: 1.8f);
                    d.noGravity = true;
                }
            }

            if (shouldSync && Main.netMode == NetmodeID.Server)
                SyncPossessionState();
        }

        private void SyncPossessionState() {
            if (Main.netMode != NetmodeID.Server)
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)Ben10Mod.MessageType.SyncGhostFreakPossessionState);
            packet.Write((byte)Player.whoAmI);
            packet.Write(inPossessionMode);
            packet.Write(possessedTargetIndex);
            packet.Write(prePossessionPosition.X);
            packet.Write(prePossessionPosition.Y);
            packet.Write(possessionTimer);
            packet.Send();
        }
    }
}
