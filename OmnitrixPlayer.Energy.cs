namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        public bool CanRestoreOmnitrixEnergy(float amount = 1f) {
            return Energy.CanRestore(amount);
        }

        public bool CanSpendOmnitrixEnergy(float amount) {
            return Energy.CanSpend(amount);
        }

        public bool TrySpendOmnitrixEnergy(float amount) {
            return Energy.TrySpend(amount);
        }

        public float RestoreOmnitrixEnergy(float amount) {
            return Energy.Restore(amount);
        }
    }
}
