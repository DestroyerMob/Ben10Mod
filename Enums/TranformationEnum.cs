using Ben10Mod.Content.Buffs.Abilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Ben10Mod.Enums
{
    public enum TransformationEnum {
        None = 0,
        Arctiguana = 1,
        BuzzShock = 2,
        ChromaStone = 3,
        DiamondHead = 4,
        FourArms = 5,
        GhostFreak = 6,
        HeatBlast = 7,
        RipJaws = 8,
        StinkFly = 9,
        WildVine = 10,
        XLR8 = 11,
        EyeGuy = 12,
    }

    static class TransformationMethods {
        public static int GetTransformation(this TransformationEnum te) {
            switch (te) {
                case TransformationEnum.BuzzShock:
                    return ModContent.BuffType<BuzzShock_Buff>();
                case TransformationEnum.ChromaStone:
                    return ModContent.BuffType<ChromaStone_Buff>();
                case TransformationEnum.DiamondHead:
                    return ModContent.BuffType<DiamondHead_Buff>();
                case TransformationEnum.FourArms:
                    return ModContent.BuffType<FourArms_Buff>();
                case TransformationEnum.GhostFreak:
                    return ModContent.BuffType<GhostFreak_Buff>();
                case TransformationEnum.HeatBlast:
                    return ModContent.BuffType<HeatBlast_Buff>();
                case TransformationEnum.RipJaws:
                    return ModContent.BuffType<RipJaws_Buff>();
                case TransformationEnum.StinkFly:
                    return ModContent.BuffType<StinkFly_Buff>();
                case TransformationEnum.WildVine:
                    return ModContent.BuffType<WildVine_Buff>();
                case TransformationEnum.XLR8:
                    return ModContent.BuffType<XLR8_Buff>();
                case TransformationEnum.EyeGuy:
                    return ModContent.BuffType<EyeGuy_Buff>();
                default: return -1;
            }
        }

        public static string GetName(this TransformationEnum te) {
            switch (te) {
                case TransformationEnum.BuzzShock:
                    return "Buzzshock";
                case TransformationEnum.ChromaStone:
                    return "Chromastone";
                case TransformationEnum.DiamondHead:
                    return "Diamondhead";
                case TransformationEnum.FourArms:
                    return "Fourarms";
                case TransformationEnum.GhostFreak:
                    return "Ghostfreak";
                case TransformationEnum.HeatBlast:
                    return "Heatblast";
                case TransformationEnum.RipJaws:
                    return "Ripjaws";
                case TransformationEnum.StinkFly:
                    return "Stinkfly";
                case TransformationEnum.WildVine:
                    return "Wildvine";
                case TransformationEnum.XLR8:
                    return "XLR8";
                case TransformationEnum.EyeGuy:
                    return "Eye Guy";
                default:
                    return "None";
            }
        }

        public static ReLogic.Content.Asset<Texture2D> GetTransformationIcon(this TransformationEnum te) {
            switch (te) {
                case TransformationEnum.BuzzShock:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/BuzzShockSelect");
                case TransformationEnum.ChromaStone:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/ChromaStoneSelect");
                case TransformationEnum.DiamondHead:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/DiamondHeadSelect");
                case TransformationEnum.FourArms:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/FourArmsSelect");
                case TransformationEnum.GhostFreak:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/GhostFreakSelect");
                case TransformationEnum.HeatBlast:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/HeatBlastSelect");
                case TransformationEnum.RipJaws:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/RipJawsSelect");
                case TransformationEnum.StinkFly:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien");
                case TransformationEnum.WildVine:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien");
                case TransformationEnum.EyeGuy:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien");
                case TransformationEnum.XLR8:
                    return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/XLR8Select");
                default: return ModContent.Request<Texture2D>("Ben10Mod/Content/Interface/EmptyAlien");
            }
        }
        
        public static string GetDescription(this TransformationEnum trans)
        {
            return trans switch
            {
                TransformationEnum.None => "No alien selected. Choose one from the Omnitrix!",

                TransformationEnum.HeatBlast => "A fiery Pyronite from the blazing star Pyros. A living inferno of plasma wrapped in molten rock.",

                TransformationEnum.DiamondHead => "A crystalline Petrosapien from the shattered planet Petropia. Body forged from unbreakable diamond-like crystal.",

                TransformationEnum.XLR8 => "A lightning-fast Kineceleran from the planet Kinet. Built like a velociraptor and engineered for pure speed.",

                TransformationEnum.ChromaStone => "A radiant Crystalsapien from Petropia. Living energy crystal that absorbs and unleashes raw power.",

                TransformationEnum.FourArms => "A mighty Tetramand from the harsh desert world Khoros. Four powerful arms of raw, unstoppable strength.",

                TransformationEnum.BuzzShock => "A hyper-charged Nosedeenian from the Nosideen Quasar. Electric plasma being that crackles with limitless energy.",

                TransformationEnum.RipJaws => "A ferocious Piscciss Volann from the ocean planet Piscciss. Aquatic predator with razor-sharp jaws and gills.",

                TransformationEnum.GhostFreak => "A terrifying Ectonurite from the nightmare dimension Anur Phaetos. Intangible phantom that haunts the darkness.",

                TransformationEnum.WildVine => "A versatile Florauna from the lush planet Flors Verdance. Living plant with stretching vines and natural camouflage.",

                TransformationEnum.StinkFly => "A winged Lepidopterran from the insect world Lepidopterra. Acid-spitting flyer with a signature pungent aroma.",

                _ => "A mysterious alien from the Omnitrix database."
            };
        }

        public static List<string> GetAbilities(this TransformationEnum trans)
        {
            return trans switch
            {
                TransformationEnum.None => new List<string> { "None" },
                TransformationEnum.HeatBlast => new List<string> { "Flamethrower blast", "Flight via Propulsion", "Heat Immunity", "Explosive Fireballs" },
                // ← Add real abilities for every alien (this is where the fun Ben 10 flavor goes!)
                _ => new List<string> { "Unknown abilities" }
            };
        }

    }
}
