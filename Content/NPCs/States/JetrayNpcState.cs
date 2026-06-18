using Microsoft.Xna.Framework;
using Terraria;

namespace Ben10Mod.Content.NPCs;

public struct JetrayNpcState {
    public int LockOwner;
    public int LockTime;

    public bool IsLockedFor(int owner) => LockOwner == owner && LockTime > 0;

    public int GetLockTime(int owner) => IsLockedFor(owner) ? LockTime : 0;

    public void ApplyLock(int owner, int time) {
        LockOwner = owner;
        LockTime = Utils.Clamp(time, 1, 420);
    }

    public bool ConsumeLock(int owner) {
        if (!IsLockedFor(owner))
            return false;

        LockOwner = -1;
        LockTime = 0;
        return true;
    }

    public void ApplyDrawEffects(ref Color drawColor) {
        if (LockTime > 0)
            drawColor = Color.Lerp(drawColor, new Color(100, 255, 225), 0.24f);
    }

    public void Tick() {
        if (LockTime > 0) {
            LockTime--;
        }
        else {
            LockOwner = -1;
        }
    }
}
