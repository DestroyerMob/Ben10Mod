using System;
using Ben10Mod.Content.Transformations;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Ben10Mod.Common.Systems;

public static class TransformationRecipeConditions {
    private const string MustBeTransformationKey = "Mods.Ben10Mod.RecipeConditions.MustBeTransformedAs";

    public static Recipe AddTransformationCondition(this Recipe recipe, string transformationId) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        string resolvedTransformationId = transformation?.FullID ?? transformationId;
        string displayName = transformation?.TransformationName ?? transformationId;
        LocalizedText description = Language.GetOrRegister(MustBeTransformationKey, () => "Must be transformed as {0}")
            .WithFormatArgs(displayName);

        return recipe.AddCondition(description, () => IsLocalPlayerTransformedAs(resolvedTransformationId));
    }

    public static Recipe AddTransformationCondition(this Recipe recipe, Transformation transformation) {
        ArgumentNullException.ThrowIfNull(transformation);
        return recipe.AddTransformationCondition(transformation.FullID);
    }

    public static bool IsLocalPlayerTransformedAs(string transformationId) {
        if (Main.gameMenu || string.IsNullOrWhiteSpace(transformationId))
            return false;

        Player player = Main.LocalPlayer;
        if (player == null || !player.active)
            return false;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        return string.Equals(omp.currentTransformationId, transformationId, StringComparison.Ordinal);
    }
}
