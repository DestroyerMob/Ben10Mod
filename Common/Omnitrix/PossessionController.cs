using System;
using Microsoft.Xna.Framework;

namespace Ben10Mod.Common.Omnitrix;

public sealed class PossessionController {
    public bool Active { get; set; }
    public Vector2 ReturnPosition { get; set; } = Vector2.Zero;
    public int TargetIndex { get; set; } = -1;
    public int Timer { get; set; }

    public void Begin(int targetIndex, Vector2 returnPosition, int duration) {
        ReturnPosition = returnPosition;
        TargetIndex = targetIndex;
        Timer = Math.Max(1, duration);
        Active = true;
    }

    public bool End() {
        bool wasActive = Active || TargetIndex >= 0;
        Active = false;
        TargetIndex = -1;
        Timer = 0;
        return wasActive;
    }
}
