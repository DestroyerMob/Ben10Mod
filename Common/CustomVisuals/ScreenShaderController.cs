using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Common.CustomVisuals;

public class ScreenShaderController : ModSystem {
    private sealed class ScreenShaderRule {
        public string FilterKey { get; init; }
        public Func<Player, bool> ShouldActivate { get; init; }
        public Func<Player, bool> ShouldDeactivate { get; init; }
        public Action<ScreenShaderData, Player> WhileActive { get; init; }
        public Action<ScreenShaderData, Player> OnDeactivate { get; init; }
    }

    private static readonly List<ScreenShaderRule> Rules = new();

    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        RegisterDefaults();
    }

    public override void Unload() {
    }

    internal static void ClearRules() {
        Rules.Clear();
    }

    public static void Register(
        string filterKey,
        Func<Player, bool> shouldActivate,
        Func<Player, bool> shouldDeactivate = null,
        Action<ScreenShaderData, Player> whileActive = null,
        Action<ScreenShaderData, Player> onDeactivate = null) {
        Rules.Add(new ScreenShaderRule {
            FilterKey = filterKey,
            ShouldActivate = shouldActivate,
            ShouldDeactivate = shouldDeactivate ?? (player => !shouldActivate(player)),
            WhileActive = whileActive,
            OnDeactivate = onDeactivate
        });
    }

    public static void UpdateForLocalPlayer(Player player) {
        if (Main.netMode == NetmodeID.Server || player == null || player.whoAmI != Main.myPlayer)
            return;

        foreach (ScreenShaderRule rule in Rules) {
            Filter filter = Filters.Scene[rule.FilterKey];
            if (filter == null)
                continue;

            bool shouldActivate = rule.ShouldActivate(player);
            bool shouldDeactivate = rule.ShouldDeactivate(player);

            if (shouldActivate) {
                if (!filter.IsActive())
                    Filters.Scene.Activate(rule.FilterKey);

                rule.WhileActive?.Invoke(filter.GetShader(), player);
                continue;
            }

            if (!shouldDeactivate || !filter.IsActive())
                continue;

            rule.OnDeactivate?.Invoke(filter.GetShader(), player);
            Filters.Scene.Deactivate(rule.FilterKey);
        }
    }

    private static void RegisterDefaults() {
        if (Rules.Count > 0)
            return;

        Register(
            "Ben10Mod:Bluescale",
            shouldActivate: player => {
                var omp = player.GetModPlayer<OmnitrixPlayer>();
                return omp.IsUltimateAbilityActive && omp.currentTransformationId == "Ben10Mod:BigChill";
            });

        Register(
            "Ben10Mod:Grayscale",
            shouldActivate: player => {
                var omp = player.GetModPlayer<OmnitrixPlayer>();
                return omp.IsUltimateAbilityActive && omp.currentTransformationId == "Ben10Mod:XLR8";
            },
            whileActive: (shader, _) => shader.Shader.Parameters["strength"]?.SetValue(1f),
            onDeactivate: (shader, _) => shader.Shader.Parameters["strength"]?.SetValue(0f));
    }
}
