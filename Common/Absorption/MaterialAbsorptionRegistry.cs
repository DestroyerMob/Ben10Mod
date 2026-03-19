using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace Ben10Mod.Common.Absorption;

public static class MaterialAbsorptionRegistry {
    private static readonly Dictionary<int, MaterialAbsorptionRegistration> Definitions = new();
    private static readonly Dictionary<int, MaterialAbsorptionProfile> Profiles = new();

    public static void Register(int sourceItemType, int swordItemType, int helmetItemType, int bodyItemType, int legItemType) {
        Register(new MaterialAbsorptionRegistration(sourceItemType, swordItemType, helmetItemType, bodyItemType, legItemType));
    }

    public static void Register(MaterialAbsorptionRegistration registration) {
        Definitions[registration.SourceItemType] = registration;
        Profiles.Remove(registration.SourceItemType);
    }

    public static MaterialAbsorptionRegistration CreateRegistration(int sourceItemType, int swordItemType, int helmetItemType, int bodyItemType, int legItemType) {
        return new MaterialAbsorptionRegistration(sourceItemType, swordItemType, helmetItemType, bodyItemType, legItemType);
    }

    public static bool TryGetRegistration(int sourceItemType, out MaterialAbsorptionRegistration registration) {
        return Definitions.TryGetValue(sourceItemType, out registration);
    }

    public static bool TryGetProfile(int sourceItemType, out MaterialAbsorptionProfile profile) {
        if (Profiles.TryGetValue(sourceItemType, out profile))
            return true;

        if (!Definitions.TryGetValue(sourceItemType, out MaterialAbsorptionRegistration definition)) {
            profile = null;
            return false;
        }

        profile = BuildProfile(definition);
        Profiles[sourceItemType] = profile;
        return true;
    }

    public static bool IsRegistered(int sourceItemType) => Definitions.ContainsKey(sourceItemType);

    public static void Clear() {
        Definitions.Clear();
        Profiles.Clear();
    }

    private static MaterialAbsorptionProfile BuildProfile(MaterialAbsorptionRegistration definition) {
        Item source = CreateItem(definition.SourceItemType);
        Item sword = CreateItem(definition.SwordItemType);
        Item helmet = CreateItem(definition.HelmetItemType);
        Item body = CreateItem(definition.BodyItemType);
        Item legs = CreateItem(definition.LegItemType);

        int armorDefense = helmet.defense + body.defense + legs.defense;
        int swordDamage = Math.Max(1, sword.damage);
        float swordKnockback = Math.Max(0f, sword.knockBack);

        return new MaterialAbsorptionProfile {
            SourceItemType = definition.SourceItemType,
            DisplayName = source.Name,
            TintColor = GetAverageItemColor(definition.SourceItemType),
            ConsumeAmount = definition.ConsumeAmountOverride ?? Math.Clamp((int)Math.Round(swordDamage / 12f), 3, 10),
            DurationTicks = definition.DurationTicksOverride ?? 60 * 90,
            GenericDamageBonus = definition.GenericDamageBonusOverride ?? Math.Clamp(swordDamage / 220f, 0.04f, 0.34f),
            DefenseBonus = definition.DefenseBonusOverride ?? Math.Max(2, (int)Math.Round(armorDefense * 0.45f)),
            EnduranceBonus = definition.EnduranceBonusOverride ?? Math.Clamp(armorDefense / 300f, 0.02f, 0.12f),
            MeleeKnockbackBonus = definition.MeleeKnockbackBonusOverride ?? Math.Clamp(swordKnockback * 0.1f, 0.35f, 1.8f),
            HitEffects = definition.HitEffects.ToArray()
        };
    }

    private static Item CreateItem(int itemType) {
        Item item = new();
        item.SetDefaults(itemType);
        return item;
    }

    private static Color GetAverageItemColor(int itemType) {
        if (Main.dedServ)
            return new Color(190, 190, 190);

        Texture2D texture = TextureAssets.Item[itemType].Value;
        Color[] pixels = new Color[texture.Width * texture.Height];
        texture.GetData(pixels);

        long r = 0;
        long g = 0;
        long b = 0;
        long count = 0;

        foreach (Color pixel in pixels) {
            if (pixel.A < 20)
                continue;

            r += pixel.R;
            g += pixel.G;
            b += pixel.B;
            count++;
        }

        if (count == 0)
            return Color.White;

        return new Color((int)(r / count), (int)(g / count), (int)(b / count));
    }
}
