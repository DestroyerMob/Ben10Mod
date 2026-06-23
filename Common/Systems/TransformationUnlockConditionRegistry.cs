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

            Register("Ben10Mod:DiamondHead", "Defeat King Slime and collect its Soul of Transformation.");
            Register("Ben10Mod:XLR8", "Defeat the Eye of Cthulhu and collect its Soul of Transformation.");
            Register("Ben10Mod:FourArms", "Defeat the Brain of Cthulhu or the Eater of Worlds and collect its Soul of Transformation.");
            Register("Ben10Mod:StinkFly", "Defeat Queen Bee and collect its Soul of Transformation.");
            Register("Ben10Mod:BuzzShock", "Defeat Skeletron and collect its Soul of Transformation.");
            Register("Ben10Mod:WildVine", "Defeat Deerclops and collect its Soul of Transformation.");
            Register("Ben10Mod:Rath", "Defeat the Wall of Flesh and collect its Soul of Transformation.");
            Register("Ben10Mod:ChromaStone", "Defeat Queen Slime and collect its Soul of Transformation.");
            Register("Ben10Mod:Humungousaur", "Defeat the Destroyer and collect its Soul of Transformation.");
            Register("Ben10Mod:EyeGuy", "Defeat the Twins and collect their Soul of Transformation.");
            Register("Ben10Mod:EchoEcho", "Defeat Skeletron Prime and collect its Soul of Transformation.");
            Register("Ben10Mod:Swampfire", "Defeat Plantera and collect its Soul of Transformation.");
            Register("Ben10Mod:Armodrillo", "Defeat Golem and collect its Soul of Transformation.");
            Register("Ben10Mod:Jetray", "Defeat Duke Fishron and collect its Soul of Transformation.");
            Register("Ben10Mod:AmpFibian", "Defeat the Empress of Light and collect her Soul of Transformation.");
            Register("Ben10Mod:Terraspin", "Defeat the Lunatic Cultist and collect its Soul of Transformation.");
            Register("Ben10Mod:WayBig", "Defeat a Celestial Pillar and collect its Soul of Transformation.");
            Register("Ben10Mod:AlienX", "Defeat the Moon Lord and collect its Soul of Transformation.");
            Register("Ben10Mod:GrayMatter", "Defeat Albedo and collect his Soul of Transformation.");
            Register("Ben10Mod:PeskyDust", "Defeat Dark Mage in the Old One's Army and collect its Soul of Transformation.");
            Register("Ben10Mod:Cannonbolt", "Defeat Ogre in the Old One's Army and collect its Soul of Transformation.");
            Register("Ben10Mod:Clockwork", "Defeat Betsy in the Old One's Army and collect her Soul of Transformation.");
            Register("Ben10Mod:SnareOh", "Defeat Mourning Wood during Pumpkin Moon and collect its Soul of Transformation.");
            Register("Ben10Mod:Blitzwolfer", "Defeat Pumpking during Pumpkin Moon and collect its Soul of Transformation.");
            Register("Ben10Mod:Arctiguana", "Defeat Everscream during Frost Moon and collect its Soul of Transformation.");
            Register("Ben10Mod:NRG", "Defeat Santa-NK1 during Frost Moon and collect its Soul of Transformation.");
            Register("Ben10Mod:BigChill", "Defeat Ice Queen during Frost Moon and collect her Soul of Transformation.");

            Register("Ben10Mod:GhostFreak", "Defeat enemies during a Blood Moon and collect their Soul of Transformation.");
            Register("Ben10Mod:Frankenstrike", "Defeat enemies during a Solar Eclipse and collect their Soul of Transformation.");
            Register("Ben10Mod:Goop", "Defeat slimes during Slime Rain and collect their Soul of Transformation.");
            Register("Ben10Mod:Whampire", "Defeat enemies during Pumpkin Moon and collect their Soul of Transformation.");
            Register("Ben10Mod:Lodestar", "Defeat enemies during Frost Moon and collect their Soul of Transformation.");
            Register("Ben10Mod:RipJaws", "Defeat enemies during the Goblin Army and collect their Soul of Transformation.");
            Register("Ben10Mod:Fasttrack", "Defeat enemies during the Frost Legion and collect their Soul of Transformation.");
            Register("Ben10Mod:WaterHazard", "Defeat enemies during a Pirate Invasion and collect their Soul of Transformation.");
            Register("Ben10Mod:Astrodactyl", "Defeat enemies during Martian Madness and collect their Soul of Transformation.");
            Register("Ben10Mod:Upgrade", "Unlock Humungousaur, Eye Guy, and Echo Echo on this character.");
        }

        internal static void Clear() {
            UnlockConditions.Clear();
        }
    }
}
