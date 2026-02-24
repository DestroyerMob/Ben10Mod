using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ben10Mod.Content.Buffs.Abilities.ChromaStone;
using Ben10Mod.Content.Buffs.Abilities.DiamondHead;
using Ben10Mod.Content.Buffs.Abilities.HeatBlast;
using Ben10Mod.Content.Buffs.Abilities.XLR8;
using Ben10Mod.Content.Buffs.Abilities.BuzzShock;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Enums;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.PlayerDrawLayer;
using Terraria.Audio;

namespace Ben10Mod.Content {
    public static class TransformationHandler {

        public static void Transform(Player player, TransformationEnum transformation, int seconds, bool showParticles = true, bool playSound = true) {
            if (transformation.GetTransformation() == -1)
                return;
            if (showParticles) {
                Random random = new Random();
                for (int i = 0; i < 25; i++) {
                    int dustNum = Dust.NewDust(player.position - new Vector2(1, 1), player.width + 1, player.height + 1, DustID.GreenTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White, 4);
                    Main.dust[dustNum].noGravity = true;
                }

                CombatText.NewText(
                    new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height),
                    new Color(0, 255, 0),
                    transformation.GetName() + "!",
                    dramatic: true,
                    dot: false
                );
            }
            if (playSound) {
                SoundEngine.PlaySound(new SoundStyle("Ben10Mod/Content/Sounds/OmnitrixTransformation"), player.position);
            }
            // Main.NewText(transformation.GetName() + "!", Color.Green);
            player.AddBuff(transformation.GetTransformation(), 60 * seconds);
            // player.GetModPlayer<OmnitrixPlayer>().currTransformation = transformation;
        }

        public static void Detransform(Player player, int seconds, bool showParticles = true, bool addCooldown = true, bool playSound = true) {
            if (addCooldown)
                player.AddBuff(ModContent.BuffType<TransformationCooldown_Buff>(), 60 * seconds);
            

            if (showParticles) {
                Random random = new Random();
                for (int i = 0; i < 25; i++) {
                    int dustNum = Dust.NewDust(player.position - new Vector2(1, 1), player.width + 1, player.height + 1, DustID.RedTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White, 4);
                    Main.dust[dustNum].noGravity = true;
                }
            }

            if (playSound) {
                SoundEngine.PlaySound(new SoundStyle("Ben10Mod/Content/Sounds/OmnitrixTimeout"), player.position);
            }

            player.ClearBuff(ModContent.BuffType<HeatBlast_Buff>());
            player.ClearBuff(ModContent.BuffType<XLR8_Buff>());
            player.ClearBuff(ModContent.BuffType<DiamondHead_Buff>());
            player.ClearBuff(ModContent.BuffType<FourArms_Buff>());
            player.ClearBuff(ModContent.BuffType<BuzzShock_Buff>());
            player.ClearBuff(ModContent.BuffType<GhostFreak_Buff>());
            player.ClearBuff(ModContent.BuffType<RipJaws_Buff>());
            player.ClearBuff(ModContent.BuffType<StinkFly_Buff>());
            player.ClearBuff(ModContent.BuffType<WildVine_Buff>());
            player.ClearBuff(ModContent.BuffType<ChromaStone_Buff>());
            player.ClearBuff(ModContent.BuffType<EyeGuy_Buff>());

            // player.ClearBuff(ModContent.BuffType<HeatBlast_Primary_Buff>());
            // player.ClearBuff(ModContent.BuffType<DiamondHead_Primary_Buff>());
            // player.ClearBuff(ModContent.BuffType<ChromaStone_Primary_Buff>());
            // player.ClearBuff(ModContent.BuffType<XLR8_Primary_Buff>());
            // player.ClearBuff(ModContent.BuffType<BuzzShock_Primary_Buff>());

            player.GetModPlayer<OmnitrixPlayer>().currTransformation = TransformationEnum.None;

        }

        public static void NextTransformation(Player player, ref TransformationEnum transformation) {

            int getTransNum = 0;

            List<TransformationEnum> unlockedTransformation = player.GetModPlayer<OmnitrixPlayer>().unlockedTransformation;

            for (int i = 0; i < unlockedTransformation.Count; i++) {
                if (transformation == unlockedTransformation[i]) {
                    getTransNum = i + 1;
                    if (getTransNum == unlockedTransformation.Count) {
                        getTransNum = 0;
                    }
                }
            }

            transformation = unlockedTransformation[getTransNum];
        }

        public static void PrevTransformation(Player player, ref TransformationEnum transformation) {

            int getTransNum = 0;

            List<TransformationEnum> unlockedTransformation = player.GetModPlayer<OmnitrixPlayer>().unlockedTransformation;

            for (int i = 0; i < unlockedTransformation.Count; i++) {
                if (transformation == unlockedTransformation[i]) {
                    getTransNum = i - 1;
                    if (getTransNum < 0) {
                        getTransNum = unlockedTransformation.Count - 1;
                    }
                }
            }

            transformation = unlockedTransformation[getTransNum];
        }

        public static bool HasTransformation(Player player, TransformationEnum transformation) {
            bool hasTransformation = false;
            foreach (TransformationEnum t in player.GetModPlayer<OmnitrixPlayer>().unlockedTransformation) {
                if (t == transformation) {
                    hasTransformation = true;
                }
            }
            return hasTransformation;
        }

    }
}
