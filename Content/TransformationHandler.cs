using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Transformations;

namespace Ben10Mod.Content
{
    public static class TransformationHandler
    {
        public static void Transform(Player player, string transformationId, int seconds = 300, 
            bool showParticles = true, bool playSound = true)
        {
            var transformation = TransformationLoader.Get(transformationId);
            if (transformation == null || transformation.TransformationBuffId <= 0)
                return;

            var omp = player.GetModPlayer<OmnitrixPlayer>();

            omp.currentTransformationId = transformationId;
            omp.isTransformed           = true;
            omp.ultimateForm            = false;

            player.AddBuff(transformation.TransformationBuffId, 60 * seconds);

            if (showParticles)
            {
                for (int i = 0; i < 25; i++)
                {
                    int dustNum = Dust.NewDust(player.position - new Vector2(1, 1), player.width + 1, player.height + 1,
                        DustID.GreenTorch, Main.rand.Next(-4, 5), Main.rand.Next(-4, 5), 1, Color.White, 4);
                    Main.dust[dustNum].noGravity = true;
                }

                CombatText.NewText(player.getRect(), new Color(0, 255, 0),
                    transformation.TransformationName + "!", dramatic: true);
            }

            if (playSound)
                SoundEngine.PlaySound(new SoundStyle("Ben10Mod/Content/Sounds/OmnitrixTransformation"), player.position);

            transformation.OnTransform(player, omp);
        }

        public static void Detransform(Player player, int cooldownSeconds = 120, 
            bool showParticles = true, bool addCooldown = true, bool playSound = true)
        {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            var current = omp.CurrentTransformation;

            if (addCooldown && cooldownSeconds > 0)
                player.AddBuff(ModContent.BuffType<TransformationCooldown_Buff>(), 60 * cooldownSeconds);

            if (showParticles)
            {
                for (int i = 0; i < 25; i++)
                {
                    int dustNum = Dust.NewDust(player.position - new Vector2(1, 1), player.width + 1, player.height + 1,
                        DustID.RedTorch, Main.rand.Next(-4, 5), Main.rand.Next(-4, 5), 1, Color.White, 4);
                    Main.dust[dustNum].noGravity = true;
                }
            }

            if (playSound)
                SoundEngine.PlaySound(new SoundStyle("Ben10Mod/Content/Sounds/OmnitrixTimeout"), player.position);

            // Clear current alien buff
            if (current?.TransformationBuffId > 0)
                player.ClearBuff(current.TransformationBuffId);

            player.ClearBuff(ModContent.BuffType<PrimaryAbility>());
            player.ClearBuff(ModContent.BuffType<UltimateAbility>());

            omp.currentTransformationId = "";
            omp.isTransformed           = false;
            omp.ultimateForm            = false;
            omp.ultimateAttack          = false;

            if (current != null)
                current.OnDetransform(player, omp);
        }

        public static void GoUltimate(Player player, bool showParticles = true, bool playSound = true)
        {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            var current = omp.CurrentTransformation;
            if (current == null) return;

            omp.ultimateForm = true;

            if (showParticles)
            {
                for (int i = 0; i < 25; i++)
                {
                    int dustNum = Dust.NewDust(player.position - new Vector2(1, 1), player.width + 1, player.height + 1,
                        DustID.GreenTorch, Main.rand.Next(-4, 5), Main.rand.Next(-4, 5), 1, Color.White, 4);
                    Main.dust[dustNum].noGravity = true;
                }

                CombatText.NewText(player.getRect(), new Color(0, 255, 0),
                    "Ultimate " + current.TransformationName + "!", dramatic: true);
            }

            if (playSound)
                SoundEngine.PlaySound(new SoundStyle("Ben10Mod/Content/Sounds/OmnitrixTransformation"), player.position);
        }

        public static void AddTransformation(Player player, string transformationId)
        {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            if (omp.unlockedTransformations.Contains(transformationId))
                return;

            omp.unlockedTransformations.Add(transformationId);
            Main.NewText($"{TransformationLoader.Get(transformationId)?.TransformationName ?? "Unknown"} has been unlocked!", Color.LimeGreen);

            // Network sync
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<Ben10Mod>().GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.UnlockTransformation);
                packet.Write((byte)player.whoAmI);
                packet.Write(transformationId);
                packet.Send();
            }
        }

        public static bool HasTransformation(Player player, string transformationId)
        {
            return player.GetModPlayer<OmnitrixPlayer>().unlockedTransformations.Contains(transformationId);
        }

        public static bool HasTransformation(Player player, Transformation transformation)
        {
            return HasTransformation(player, transformation.FullID);
        }
    }
}