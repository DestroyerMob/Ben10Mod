using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Ben10Mod.Keybinds {
    public class KeybindSystem : ModSystem {
        public static ModKeybind PrimaryAbility { get; private set; }
        public static ModKeybind SecondaryAbility { get; private set; }
        public static ModKeybind TertiaryAbility { get; private set; }
        public static ModKeybind QuaternaryAbility { get; private set; }
        public static ModKeybind UltimateAbility { get; private set; }
        public static ModKeybind TransformationKeybind { get; private set; }
        public static ModKeybind TransformWheel { get; private set; }
        public static ModKeybind AbsorbMaterial { get; private set; }
        public static ModKeybind OpenTransformationScreen { get; private set; }
        public static ModKeybind AlienOneKeybind { get; private set; }
        public static ModKeybind AlienTwoKeybind { get; private set; }
        public static ModKeybind AlienThreeKeybind { get; private set; }
        public static ModKeybind AlienFourKeybind { get; private set; }
        public static ModKeybind AlienFiveKeybind { get; private set; }
        public static ModKeybind AlienNextKeybind { get; private set; }
        public static ModKeybind AlienPrevKeybind { get; private set; }

        public override void Load() {
            PrimaryAbility = KeybindLoader.RegisterKeybind(Mod, "Primary Ability", "F");
            SecondaryAbility = KeybindLoader.RegisterKeybind(Mod, "Secondary Ability", "G");
            TertiaryAbility = KeybindLoader.RegisterKeybind(Mod, "Tertiary Ability", "H");
            QuaternaryAbility = KeybindLoader.RegisterKeybind(Mod, "Quaternary Ability", "J");
            UltimateAbility = KeybindLoader.RegisterKeybind(Mod, "Ultimate Ability", "U");
            TransformationKeybind = KeybindLoader.RegisterKeybind(Mod, "Transform", "P");
            TransformWheel = KeybindLoader.RegisterKeybind(Mod, "Transform Wheel", "Q");
            AbsorbMaterial = KeybindLoader.RegisterKeybind(Mod, "Absorb Material", "K");
            OpenTransformationScreen = KeybindLoader.RegisterKeybind(Mod, "Open Menu", "L");
            AlienOneKeybind = KeybindLoader.RegisterKeybind(Mod, "Alien One", "NumPad1");
            AlienTwoKeybind = KeybindLoader.RegisterKeybind(Mod, "Alien Two", "NumPad2");
            AlienThreeKeybind = KeybindLoader.RegisterKeybind(Mod, "Alien Three", "NumPad3");
            AlienFourKeybind = KeybindLoader.RegisterKeybind(Mod, "Alien Four", "NumPad4");
            AlienFiveKeybind = KeybindLoader.RegisterKeybind(Mod, "Alien Five", "NumPad5");
            AlienNextKeybind = KeybindLoader.RegisterKeybind(Mod, "Next Alien", "Right");
            AlienPrevKeybind = KeybindLoader.RegisterKeybind(Mod, "Prev Alien", "Left");
        }

        public override void Unload() {
            PrimaryAbility = null;
            SecondaryAbility = null;
            TertiaryAbility = null;
            QuaternaryAbility = null;
            UltimateAbility = null;
            TransformationKeybind = null;
            TransformWheel = null;
            AbsorbMaterial = null;
            OpenTransformationScreen = null;
            AlienOneKeybind = null;
            AlienTwoKeybind = null;
            AlienThreeKeybind = null;
            AlienFourKeybind = null;
            AlienFiveKeybind = null;
            AlienNextKeybind = null;
            AlienPrevKeybind = null;
        }
    }
}
