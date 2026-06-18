using Ben10Mod.Content.Transformations.EyeGuy;
using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct EyeGuyNpcState {
    public int Owner;
    public int FireMarkTime;
    public int FrostMarkTime;
    public int ShockMarkTime;
    public int ExposedTime;

    public bool IsExposedFor(int owner) => Owner == owner && ExposedTime > 0;

    public int GetExposedOwner() => ExposedTime > 0 ? Owner : -1;

    public bool HasMark(int owner, EyeGuyElement element) {
        if (Owner != owner)
            return false;

        return element switch {
            EyeGuyElement.Fire => FireMarkTime > 0,
            EyeGuyElement.Frost => FrostMarkTime > 0,
            _ => ShockMarkTime > 0
        };
    }

    public int GetMarkCount(int owner) {
        if (Owner != owner)
            return 0;

        int count = 0;
        if (FireMarkTime > 0)
            count++;
        if (FrostMarkTime > 0)
            count++;
        if (ShockMarkTime > 0)
            count++;
        return count;
    }

    public EyeGuyElement GetPreferredMark(int owner, EyeGuyElement fallback) {
        if (Owner != owner || ExposedTime > 0)
            return fallback;

        if (FireMarkTime <= 0)
            return EyeGuyElement.Fire;
        if (FrostMarkTime <= 0)
            return EyeGuyElement.Frost;
        if (ShockMarkTime <= 0)
            return EyeGuyElement.Shock;

        return fallback;
    }

    public bool ApplyMark(int owner, EyeGuyElement element, int time, int exposedTime) {
        if (Owner != owner) {
            Clear();
            Owner = owner;
        }

        if (ExposedTime > 0)
            return false;

        int clampedTime = Utils.Clamp(time, 1, 420);
        switch (element) {
            case EyeGuyElement.Fire:
                FireMarkTime = System.Math.Max(FireMarkTime, clampedTime);
                break;
            case EyeGuyElement.Frost:
                FrostMarkTime = System.Math.Max(FrostMarkTime, clampedTime);
                break;
            default:
                ShockMarkTime = System.Math.Max(ShockMarkTime, clampedTime);
                break;
        }

        if (FireMarkTime <= 0 || FrostMarkTime <= 0 || ShockMarkTime <= 0)
            return false;

        ExposedTime = Utils.Clamp(exposedTime, 1, 600);
        FireMarkTime = ExposedTime;
        FrostMarkTime = ExposedTime;
        ShockMarkTime = ExposedTime;
        return true;
    }

    public bool ConsumeExposed(int owner) {
        if (!IsExposedFor(owner))
            return false;

        Clear();
        return true;
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (ExposedTime > 0) {
            drawColor = Color.Lerp(drawColor, new Color(255, 228, 170), 0.36f);
        }
        else if (FireMarkTime > 0 || FrostMarkTime > 0 || ShockMarkTime > 0) {
            int red = 180;
            int green = 180;
            int blue = 180;
            int markCount = 0;
            if (FireMarkTime > 0) {
                red += 75;
                green += 10;
                markCount++;
            }
            if (FrostMarkTime > 0) {
                green += 55;
                blue += 75;
                markCount++;
            }
            if (ShockMarkTime > 0) {
                green += 20;
                blue += 75;
                markCount++;
            }

            Color markColor = new Color((byte)Utils.Clamp(red, 0, 255), (byte)Utils.Clamp(green, 0, 255),
                (byte)Utils.Clamp(blue, 0, 255));
            drawColor = Color.Lerp(drawColor, markColor, 0.12f + markCount * 0.06f);
        }
    }

    public void Tick() {
        if (ExposedTime > 0) {
            ExposedTime--;
            FireMarkTime = System.Math.Max(FireMarkTime - 1, 0);
            FrostMarkTime = System.Math.Max(FrostMarkTime - 1, 0);
            ShockMarkTime = System.Math.Max(ShockMarkTime - 1, 0);
            if (ExposedTime <= 0)
                Clear();
        }
        else if (Owner != -1) {
            FireMarkTime = System.Math.Max(FireMarkTime - 1, 0);
            FrostMarkTime = System.Math.Max(FrostMarkTime - 1, 0);
            ShockMarkTime = System.Math.Max(ShockMarkTime - 1, 0);
            if (FireMarkTime <= 0 && FrostMarkTime <= 0 && ShockMarkTime <= 0)
                Clear();
        }
    }

    private void Clear() {
        Owner = -1;
        FireMarkTime = 0;
        FrostMarkTime = 0;
        ShockMarkTime = 0;
        ExposedTime = 0;
    }
}
