using System.Collections.Generic;
using Ben10Mod.Common.Omnitrix;
using Ben10Mod.Content.Transformations;
using Terraria;

namespace Ben10Mod {
    public partial class OmnitrixPlayer {
        internal bool CanAcceptClientUnlockRequest(Transformation transformation) {
            return Progression.CanAcceptClientUnlockRequest(this, transformation);
        }

        public IReadOnlyList<string> GetTransformationsForCodexDisplay() {
            return Progression.GetTransformationsForCodexDisplay(this);
        }

        public string GetTransformationUnlockConditionText(string transformationId) {
            return Progression.GetTransformationUnlockConditionText(this, transformationId);
        }

        public string GetTransformationUnlockConditionText(Transformation transformation) {
            return Progression.GetTransformationUnlockConditionText(this, transformation);
        }

        public string GetTransformationUnlockCategoryText(Transformation transformation) {
            return Progression.GetTransformationUnlockCategoryText(this, transformation);
        }

        public string GetTransformationUnlockProgressText(Transformation transformation) {
            return Progression.GetTransformationUnlockProgressText(this, transformation);
        }

        public string GetTransformationCodexSubtitle(Transformation transformation) {
            return Progression.GetTransformationCodexSubtitle(this, transformation);
        }

        public bool TransformationHasUnlockCondition(Transformation transformation) {
            return Progression.TransformationHasUnlockCondition(this, transformation);
        }

        public string GetTransformationAvailabilityStateText(Transformation transformation) {
            return Progression.GetTransformationAvailabilityStateText(this, transformation);
        }

        public string GetTransformationAccessHeaderText(Transformation transformation) {
            return Progression.GetTransformationAccessHeaderText(this, transformation);
        }

        public void RecordEventParticipation(NPC npc) {
            Progression.RecordEventParticipation(this, npc);
        }

        internal void ApplyRecordedEventParticipation(IEnumerable<int> eventIds) {
            Progression.ApplyRecordedEventParticipation(eventIds);
        }

        private void UpdateEventTransformationUnlocks() {
            Progression.UpdateEventTransformationUnlocks(this);
        }

        private void UpdateProgressionTransformationUnlocks() {
            Progression.UpdateProgressionTransformationUnlocks(this);
        }

        private int CompareTransformationDisplayName(string leftTransformationId, string rightTransformationId) {
            return TransformationProgressionSystem.CompareTransformationDisplayName(this, leftTransformationId,
                rightTransformationId);
        }
    }
}
