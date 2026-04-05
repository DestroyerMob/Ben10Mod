using System;
using System.Collections.Generic;
using Ben10Mod.Content.Items.Accessories;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Prefixes
{
    public abstract class OmnitrixPrefix : ModPrefix
    {
        public override PrefixCategory Category => PrefixCategory.Accessory;

        public virtual int OmnitrixEnergyMaxBonus => 0;
        public virtual int OmnitrixEnergyRegenBonus => 0;
        public virtual int OmnitrixEnergyDrainBonus => 0;
        public virtual int TransformationSwapCostBonus => 0;
        public virtual float TransformationDurationMultiplier => 1f;
        public virtual float CooldownDurationMultiplier => 1f;
        protected virtual float ValueMultiplier => 1f;

        protected virtual bool CanRollOn(Omnitrix omnitrix) {
            return true;
        }

        public override bool CanRoll(Item item) {
            return item?.ModItem is Omnitrix omnitrix && CanRollOn(omnitrix);
        }

        public override void ModifyValue(ref float valueMult) {
            valueMult *= ValueMultiplier;
        }

        public override IEnumerable<TooltipLine> GetTooltipLines(Item item) {
            int lineIndex = 0;

            foreach (string effectLine in GetEffectLines()) {
                yield return new TooltipLine(Mod, $"{Name}Effect{lineIndex++}", effectLine) {
                    IsModifier = true,
                    IsModifierBad = false
                };
            }
        }

        private IEnumerable<string> GetEffectLines() {
            if (OmnitrixEnergyMaxBonus != 0)
                yield return $"{FormatSigned(OmnitrixEnergyMaxBonus)} OE capacity";

            if (OmnitrixEnergyRegenBonus != 0)
                yield return $"{FormatSigned(OmnitrixEnergyRegenBonus)} OE regen";

            if (OmnitrixEnergyDrainBonus != 0)
                yield return $"{FormatSigned(OmnitrixEnergyDrainBonus)} OE transformed upkeep";

            if (TransformationSwapCostBonus != 0)
                yield return $"{FormatSigned(TransformationSwapCostBonus)} OE form swap cost";

            if (Math.Abs(TransformationDurationMultiplier - 1f) > 0.001f)
                yield return FormatPercentLine(TransformationDurationMultiplier, "transformation duration");

            if (Math.Abs(CooldownDurationMultiplier - 1f) > 0.001f)
                yield return FormatCooldownLine(CooldownDurationMultiplier);
        }

        private static string FormatSigned(int value) {
            return value > 0 ? $"+{value}" : value.ToString();
        }

        private static string FormatPercentLine(float multiplier, string label) {
            int percent = (int)Math.Round(Math.Abs(multiplier - 1f) * 100f);
            string direction = multiplier >= 1f ? "increased" : "reduced";
            return $"{percent}% {direction} {label}";
        }

        private static string FormatCooldownLine(float multiplier) {
            int percent = (int)Math.Round(Math.Abs(multiplier - 1f) * 100f);
            string direction = multiplier < 1f ? "faster" : "slower";
            return $"{percent}% {direction} cooldown";
        }

        public static List<int> GetRollablePrefixTypes(Item item) {
            List<int> candidates = new();
            TryAddPrefix<Calibrated>(item, candidates);
            TryAddPrefix<Enduring>(item, candidates);
            TryAddPrefix<Responsive>(item, candidates);
            TryAddPrefix<Efficient>(item, candidates);
            TryAddPrefix<Prime>(item, candidates);
            return candidates;
        }

        private static void TryAddPrefix<TPrefix>(Item item, List<int> candidates) where TPrefix : OmnitrixPrefix {
            int prefixType = ModContent.PrefixType<TPrefix>();
            if (PrefixLoader.GetPrefix(prefixType) is OmnitrixPrefix prefix && prefix.CanRoll(item))
                candidates.Add(prefixType);
        }
    }

    public sealed class Calibrated : OmnitrixPrefix
    {
        public override int OmnitrixEnergyMaxBonus => 60;
        public override int OmnitrixEnergyRegenBonus => 1;
        protected override float ValueMultiplier => 1.08f;
    }

    public sealed class Enduring : OmnitrixPrefix
    {
        public override int OmnitrixEnergyMaxBonus => 40;
        public override float TransformationDurationMultiplier => 1.2f;
        protected override float ValueMultiplier => 1.1f;
    }

    public sealed class Responsive : OmnitrixPrefix
    {
        public override int TransformationSwapCostBonus => -10;
        public override float CooldownDurationMultiplier => 0.85f;
        protected override float ValueMultiplier => 1.1f;
    }

    public sealed class Efficient : OmnitrixPrefix
    {
        public override int OmnitrixEnergyRegenBonus => 1;
        public override int OmnitrixEnergyDrainBonus => -2;
        public override int TransformationSwapCostBonus => -10;
        protected override float ValueMultiplier => 1.12f;

        protected override bool CanRollOn(Omnitrix omnitrix) {
            return omnitrix.UseEnergyForTransformation;
        }
    }

    public sealed class Prime : OmnitrixPrefix
    {
        public override int OmnitrixEnergyMaxBonus => 90;
        public override int OmnitrixEnergyRegenBonus => 1;
        public override float TransformationDurationMultiplier => 1.15f;
        public override float CooldownDurationMultiplier => 0.9f;
        protected override float ValueMultiplier => 1.18f;
    }
}
