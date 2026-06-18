using System;
using System.Collections.Generic;
using Ben10Mod.Content.Transformations;

namespace Ben10Mod.Common.Omnitrix;

public sealed class TransformationRosterState {
    public const string DefaultStarterTransformationId = "Ben10Mod:HeatBlast";

    private string[] _slots = { DefaultStarterTransformationId, "", "", "", "" };

    public string[] Slots {
        get => _slots;
        set => _slots = value ?? Array.Empty<string>();
    }

    public List<string> Unlocked { get; } = new() { "Ben10Mod:HeatBlast" };
    public HashSet<string> Favorites { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> NewlyUnlocked { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsUnlocked(string transformationId) {
        string canonicalId = ResolveTransformationId(transformationId);
        return canonicalId.Length > 0 && Unlocked.Contains(canonicalId);
    }

    public bool TryUnlock(string transformationId, bool markNew = true) {
        string canonicalId = ResolveTransformationId(transformationId);
        if (canonicalId.Length == 0 || Unlocked.Contains(canonicalId))
            return false;

        Unlocked.Add(canonicalId);
        if (markNew)
            NewlyUnlocked.Add(canonicalId);
        return true;
    }

    public bool Remove(string transformationId) {
        string canonicalId = ResolveTransformationId(transformationId);
        if (canonicalId.Length == 0)
            return false;

        bool removed = Unlocked.Remove(canonicalId);
        NewlyUnlocked.Remove(canonicalId);
        Favorites.Remove(canonicalId);

        for (int i = 0; i < Slots.Length; i++) {
            if (string.Equals(Slots[i], canonicalId, StringComparison.OrdinalIgnoreCase))
                Slots[i] = string.Empty;
        }

        return removed;
    }

    public bool TryAssignSlot(int index, string transformationId, int slotCount) {
        Slots = EnsureSlotArray(Slots, slotCount);
        if (index < 0 || index >= Slots.Length)
            return false;

        if (string.IsNullOrWhiteSpace(transformationId)) {
            Slots[index] = string.Empty;
            return true;
        }

        string canonicalId = ResolveTransformationId(transformationId);
        if (canonicalId.Length == 0 || !Unlocked.Contains(canonicalId))
            return false;

        Slots[index] = canonicalId;
        return true;
    }

    public bool SetFavorite(string transformationId, bool isFavorite) {
        string canonicalId = ResolveTransformationId(transformationId);
        if (canonicalId.Length == 0)
            return false;

        return isFavorite ? Favorites.Add(canonicalId) : Favorites.Remove(canonicalId);
    }

    public bool ToggleFavorite(string transformationId) {
        string canonicalId = ResolveTransformationId(transformationId);
        return canonicalId.Length > 0 && SetFavorite(canonicalId, !Favorites.Contains(canonicalId));
    }

    public bool MarkSeen(string transformationId) {
        string canonicalId = ResolveTransformationId(transformationId);
        return canonicalId.Length > 0 && NewlyUnlocked.Remove(canonicalId);
    }

    public void Normalize(int slotCount, string starterTransformationId = DefaultStarterTransformationId) {
        string[] normalizedUnlocks = NormalizeUnlockedTransformations(Unlocked, starterTransformationId);
        Unlocked.Clear();
        Unlocked.AddRange(normalizedUnlocks);

        NormalizeFavorites();
        NormalizeNewlyUnlocked();
        Slots = NormalizeTransformationSlots(Slots, Unlocked, slotCount);
    }

    public static string[] NormalizeUnlockedTransformations(IEnumerable<string> unlocked,
        string starterTransformationId = DefaultStarterTransformationId) {
        List<string> normalizedUnlocks = new();

        if (unlocked != null) {
            foreach (string unlockedId in unlocked) {
                string canonicalId = ResolveTransformationId(unlockedId);
                if (canonicalId.Length == 0 || normalizedUnlocks.Contains(canonicalId))
                    continue;

                normalizedUnlocks.Add(canonicalId);
            }
        }

        string starterId = ResolveTransformationId(starterTransformationId);
        if (starterId.Length > 0 && !normalizedUnlocks.Contains(starterId))
            normalizedUnlocks.Insert(0, starterId);

        return normalizedUnlocks.ToArray();
    }

    public static string[] NormalizeTransformationSlots(string[] slots, IList<string> unlocked, int slotCount) {
        string[] normalizedSlots = EnsureSlotArray(null, slotCount);
        if (slots == null)
            return normalizedSlots;

        for (int i = 0; i < normalizedSlots.Length && i < slots.Length; i++) {
            string canonicalId = ResolveTransformationId(slots[i]);
            if (canonicalId.Length == 0)
                continue;

            if (unlocked != null && !unlocked.Contains(canonicalId))
                continue;

            normalizedSlots[i] = canonicalId;
        }

        return normalizedSlots;
    }

    public static string ResolveTransformationId(string transformationId) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        return transformation?.FullID ?? string.Empty;
    }

    private void NormalizeFavorites() {
        HashSet<string> normalizedFavorites = new(StringComparer.OrdinalIgnoreCase);
        foreach (string favoriteTransformationId in Favorites) {
            string canonicalId = ResolveTransformationId(favoriteTransformationId);
            if (canonicalId.Length > 0 && Unlocked.Contains(canonicalId))
                normalizedFavorites.Add(canonicalId);
        }

        Favorites.Clear();
        foreach (string favoriteTransformationId in normalizedFavorites)
            Favorites.Add(favoriteTransformationId);
    }

    private void NormalizeNewlyUnlocked() {
        HashSet<string> normalizedNew = new(StringComparer.OrdinalIgnoreCase);
        foreach (string transformationId in NewlyUnlocked) {
            string canonicalId = ResolveTransformationId(transformationId);
            if (canonicalId.Length > 0 && Unlocked.Contains(canonicalId))
                normalizedNew.Add(canonicalId);
        }

        NewlyUnlocked.Clear();
        foreach (string transformationId in normalizedNew)
            NewlyUnlocked.Add(transformationId);
    }

    private static string[] EnsureSlotArray(string[] slots, int slotCount) {
        int normalizedSlotCount = Math.Max(0, slotCount);
        string[] normalizedSlots = new string[normalizedSlotCount];
        for (int i = 0; i < normalizedSlots.Length; i++)
            normalizedSlots[i] = i < (slots?.Length ?? 0) ? slots[i] ?? string.Empty : string.Empty;

        return normalizedSlots;
    }
}
