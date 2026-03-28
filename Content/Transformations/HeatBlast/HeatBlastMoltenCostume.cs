using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Ben10Mod.Content.Transformations.HeatBlast {
    public sealed class HeatBlastMoltenCostume : TransformationCostume {
        public override string CostumeName => "HeatBlastMolten";
        public override string TargetTransformationId => "Ben10Mod:HeatBlast";
        public override string DisplayName => "Molten Core";
        public override string Description =>
            "A darker Pyronite look with hotter lava seams and a heavier volcanic shell.";
        public override string IconPath => "Ben10Mod/Content/Interface/HeatBlastSelect";
        public override int SortOrder => 10;

        protected override string HeadTexturePath => "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastAlt_Head";
        protected override string BodyTexturePath => "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastAlt_Body";
        protected override string LegsTexturePath => "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastAlt_Legs";

        public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => new[] {
            new TransformationPaletteChannel(
                "flames",
                "Flames",
                new Color(255, 255, 255),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastAlt_Head",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastFlameMask_Head"),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastAlt_Body",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastFlameMask_Body"),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastAlt_Legs",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastFlameMask_Legs")
            ),
            new TransformationPaletteChannel(
                "rock",
                "Rock",
                new Color(255, 255, 255),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastAlt_Head",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastRockMask_Head"),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastAlt_Body",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastRockMask_Body"),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastAlt_Legs",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastRockMask_Legs")
            )
        };
    }
}
