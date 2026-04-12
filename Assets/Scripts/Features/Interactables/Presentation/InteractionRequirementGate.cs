using System.Collections.Generic;
using Features.Openers.Presentation;
using UnityEngine;

namespace Features.Interactables.Presentation
{
    public sealed class InteractionRequirementGate : MonoBehaviour
    {
        private enum RequirementMode
        {
            All = 0,
            Any = 1
        }

        [SerializeField] private List<OpenerView> requiredOpeners =
            new List<OpenerView>();

        [SerializeField] private RequirementMode mode = RequirementMode.All;
        [SerializeField] private int missingDialogueId = -1;

        public int MissingDialogueId => missingDialogueId;

        public bool IsSatisfied()
        {
            if (requiredOpeners == null || requiredOpeners.Count == 0)
                return true;

            int total = 0;
            int active = 0;

            foreach (var opener in requiredOpeners)
            {
                if (opener == null)
                    continue;

                total++;

                if (opener.Model != null && opener.Model.IsActive)
                    active++;
            }

            if (total == 0)
                return true;

            return mode == RequirementMode.All
                ? active == total
                : active > 0;
        }
    }
}
