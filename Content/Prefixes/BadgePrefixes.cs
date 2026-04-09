using System;
using System.Collections.Generic;
using Ben10Mod.Content.Items.Weapons;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Ben10Mod.Content.Prefixes;

public abstract class BadgePrefix : ModPrefix {
    public override PrefixCategory Category => PrefixCategory.Ranged;

    public virtual float BadgeDamageMultiplier => 1f;
    public virtual int BadgeCritBonus => 0;
    public virtual int BadgeArmorPenetrationBonus => 0;
    public virtual float BadgeKnockbackMultiplier => 1f;
    protected virtual float ValueMultiplier => 1f;

    public override bool CanRoll(Item item) {
        return item?.ModItem is PlumbersBadge;
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
        if (Math.Abs(BadgeDamageMultiplier - 1f) > 0.001f)
            yield return FormatPercentLine(BadgeDamageMultiplier, "badge damage");

        if (BadgeCritBonus != 0)
            yield return $"{FormatSigned(BadgeCritBonus)}% critical strike chance";

        if (BadgeArmorPenetrationBonus != 0)
            yield return $"{FormatSigned(BadgeArmorPenetrationBonus)} armor penetration";

        if (Math.Abs(BadgeKnockbackMultiplier - 1f) > 0.001f)
            yield return FormatPercentLine(BadgeKnockbackMultiplier, "knockback");
    }

    private static string FormatSigned(int value) {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private static string FormatPercentLine(float multiplier, string label) {
        int percent = (int)Math.Round(Math.Abs(multiplier - 1f) * 100f);
        string direction = multiplier >= 1f ? "increased" : "reduced";
        return $"{percent}% {direction} {label}";
    }

    public static bool IsBadgePrefixType(int prefixType) {
        return PrefixLoader.GetPrefix(prefixType) is BadgePrefix;
    }
}

public sealed class FieldTested : BadgePrefix {
    public override float BadgeDamageMultiplier => 1.08f;
    protected override float ValueMultiplier => 1.06f;
}

public sealed class Pinpoint : BadgePrefix {
    public override float BadgeDamageMultiplier => 1.06f;
    public override int BadgeCritBonus => 4;
    protected override float ValueMultiplier => 1.09f;
    public override float RollChance(Item item) => 0.85f;
}

public sealed class Breaching : BadgePrefix {
    public override float BadgeDamageMultiplier => 1.06f;
    public override int BadgeArmorPenetrationBonus => 8;
    protected override float ValueMultiplier => 1.11f;
    public override float RollChance(Item item) => 0.75f;
}

public sealed class Impact : BadgePrefix {
    public override float BadgeDamageMultiplier => 1.08f;
    public override float BadgeKnockbackMultiplier => 1.15f;
    protected override float ValueMultiplier => 1.09f;
    public override float RollChance(Item item) => 0.85f;
}

public sealed class Exemplar : BadgePrefix {
    public override float BadgeDamageMultiplier => 1.12f;
    public override int BadgeCritBonus => 3;
    public override int BadgeArmorPenetrationBonus => 6;
    public override float BadgeKnockbackMultiplier => 1.1f;
    protected override float ValueMultiplier => 1.18f;
    public override float RollChance(Item item) => 0.35f;
}
