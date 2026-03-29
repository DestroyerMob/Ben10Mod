using System;
using System.Collections.Generic;

namespace Ben10Mod.Common.Systems {
    public static class TransformationUnlockConditionRegistry {
        private static readonly Dictionary<string, string> UnlockConditions = new(StringComparer.OrdinalIgnoreCase);

        public static void Register(string transformationId, string unlockConditionText) {
            if (string.IsNullOrWhiteSpace(transformationId))
                throw new ArgumentException("Transformation ID cannot be empty.", nameof(transformationId));

            if (string.IsNullOrWhiteSpace(unlockConditionText))
                throw new ArgumentException("Unlock condition text cannot be empty.", nameof(unlockConditionText));

            UnlockConditions[transformationId.Trim()] = unlockConditionText.Trim();
        }

        public static bool TryGet(string transformationId, out string unlockConditionText) {
            unlockConditionText = string.Empty;
            if (string.IsNullOrWhiteSpace(transformationId))
                return false;

            return UnlockConditions.TryGetValue(transformationId.Trim(), out unlockConditionText);
        }

        public static string Get(string transformationId) {
            return TryGet(transformationId, out string unlockConditionText) ? unlockConditionText : string.Empty;
        }

        internal static void RegisterBaseConditions() {
            Register("Ben10Mod:HeatBlast", "Starter transformation.");

            Register("Ben10Mod:DiamondHead", "Participate in and defeat King Slime.");
            Register("Ben10Mod:XLR8", "Participate in and defeat the Eye of Cthulhu.");
            Register("Ben10Mod:FourArms", "Participate in and defeat the Brain of Cthulhu or the Eater of Worlds.");
            Register("Ben10Mod:StinkFly", "Participate in and defeat Queen Bee.");
            Register("Ben10Mod:BuzzShock", "Participate in and defeat Skeletron.");
            Register("Ben10Mod:WildVine", "Participate in and defeat Deerclops.");
            Register("Ben10Mod:Rath", "Participate in and defeat the Wall of Flesh.");
            Register("Ben10Mod:ChromaStone", "Participate in and defeat Queen Slime.");
            Register("Ben10Mod:Humungousaur", "Participate in and defeat the Destroyer.");
            Register("Ben10Mod:EyeGuy", "Participate in and defeat the Twins.");
            Register("Ben10Mod:EchoEcho", "Participate in and defeat Skeletron Prime.");
            Register("Ben10Mod:Swampfire", "Participate in and defeat Plantera.");
            Register("Ben10Mod:Armodrillo", "Participate in and defeat Golem.");
            Register("Ben10Mod:Jetray", "Participate in and defeat Duke Fishron.");
            Register("Ben10Mod:AmpFibian", "Participate in and defeat the Empress of Light.");
            Register("Ben10Mod:Terraspin", "Participate in and defeat the Lunatic Cultist.");
            Register("Ben10Mod:WayBig", "Participate in and defeat the Moon Lord.");
            Register("Ben10Mod:PeskyDust", "Participate in and defeat Dark Mage in the Old One's Army.");
            Register("Ben10Mod:Cannonbolt", "Participate in and defeat Ogre in the Old One's Army.");
            Register("Ben10Mod:Clockwork", "Participate in and defeat Betsy in the Old One's Army.");
            Register("Ben10Mod:SnareOh", "Participate in and defeat Mourning Wood during Pumpkin Moon.");
            Register("Ben10Mod:Blitzwolfer", "Participate in and defeat Pumpking during Pumpkin Moon.");
            Register("Ben10Mod:Arctiguana", "Participate in and defeat Everscream during Frost Moon.");
            Register("Ben10Mod:NRG", "Participate in and defeat Santa-NK1 during Frost Moon.");
            Register("Ben10Mod:BigChill", "Participate in and defeat Ice Queen during Frost Moon.");

            Register("Ben10Mod:GhostFreak", "Participate in and complete a Blood Moon.");
            Register("Ben10Mod:Frankenstrike", "Participate in and complete a Solar Eclipse.");
            Register("Ben10Mod:Goop", "Participate in and complete a Slime Rain.");
            Register("Ben10Mod:Whampire", "Participate in and complete a Pumpkin Moon.");
            Register("Ben10Mod:Lodestar", "Participate in and complete a Frost Moon.");
            Register("Ben10Mod:RipJaws", "Participate in and defeat the Goblin Army.");
            Register("Ben10Mod:Fasttrack", "Participate in and complete the Frost Legion.");
            Register("Ben10Mod:WaterHazard", "Participate in and complete a Pirate Invasion.");
            Register("Ben10Mod:Astrodactyl", "Participate in and complete Martian Madness.");
            Register("Ben10Mod:Upgrade", "Unlock Humungousaur, Eye Guy, and Echo Echo on this character.");
        }

        internal static void Clear() {
            UnlockConditions.Clear();
        }
    }
}
