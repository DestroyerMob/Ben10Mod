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

            Register("Ben10Mod:DiamondHead", "Defeat King Slime.");
            Register("Ben10Mod:XLR8", "Defeat the Eye of Cthulhu.");
            Register("Ben10Mod:FourArms", "Defeat the Brain of Cthulhu or the Eater of Worlds.");
            Register("Ben10Mod:StinkFly", "Defeat Queen Bee.");
            Register("Ben10Mod:BuzzShock", "Defeat Skeletron.");
            Register("Ben10Mod:WildVine", "Defeat Deerclops.");
            Register("Ben10Mod:Rath", "Defeat the Wall of Flesh.");
            Register("Ben10Mod:ChromaStone", "Defeat Queen Slime.");
            Register("Ben10Mod:Humungousaur", "Defeat the Destroyer.");
            Register("Ben10Mod:EyeGuy", "Defeat the Twins.");
            Register("Ben10Mod:EchoEcho", "Defeat Skeletron Prime.");
            Register("Ben10Mod:Swampfire", "Defeat Plantera.");
            Register("Ben10Mod:Armodrillo", "Defeat Golem.");
            Register("Ben10Mod:Jetray", "Defeat Duke Fishron.");
            Register("Ben10Mod:AmpFibian", "Defeat the Empress of Light.");
            Register("Ben10Mod:Terraspin", "Defeat the Lunatic Cultist.");
            Register("Ben10Mod:WayBig", "Defeat the Moon Lord.");
            Register("Ben10Mod:PeskyDust", "Defeat Dark Mage in the Old One's Army.");
            Register("Ben10Mod:Cannonbolt", "Defeat Ogre in the Old One's Army.");
            Register("Ben10Mod:Clockwork", "Defeat Betsy in the Old One's Army.");
            Register("Ben10Mod:SnareOh", "Defeat Mourning Wood during Pumpkin Moon.");
            Register("Ben10Mod:Blitzwolfer", "Defeat Pumpking during Pumpkin Moon.");
            Register("Ben10Mod:Arctiguana", "Defeat Everscream during Frost Moon.");
            Register("Ben10Mod:NRG", "Defeat Santa-NK1 during Frost Moon.");
            Register("Ben10Mod:BigChill", "Defeat Ice Queen during Frost Moon.");

            Register("Ben10Mod:GhostFreak", "Participate in and complete a Blood Moon.");
            Register("Ben10Mod:Frankenstrike", "Participate in and complete a Solar Eclipse.");
            Register("Ben10Mod:Goop", "Participate in and complete a Slime Rain.");
            Register("Ben10Mod:Whampire", "Participate in and complete a Pumpkin Moon.");
            Register("Ben10Mod:Lodestar", "Participate in and complete a Frost Moon.");
            Register("Ben10Mod:RipJaws", "Participate in and defeat the Goblin Army.");
            Register("Ben10Mod:Fasttrack", "Participate in and complete the Frost Legion.");
            Register("Ben10Mod:WaterHazard", "Participate in and complete a Pirate Invasion.");
            Register("Ben10Mod:Astrodactyl", "Participate in and complete the Old One's Army.");
            Register("Ben10Mod:Upgrade", "Defeat all three Mechanical Bosses.");
        }

        internal static void Clear() {
            UnlockConditions.Clear();
        }
    }
}
