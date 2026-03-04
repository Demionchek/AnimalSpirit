using Features.Interactables.Infrastructure;
using UnityEngine;

namespace Features.Interactables.Application
{
    public sealed class ActivateObjectAction :
        IInteractionAction
    {
        private readonly GameObject _target;

        public ActivateObjectAction(GameObject target)
        {
            _target = target;
        }

        public void Execute()
        {
            if (_target != null)
                _target.SetActive(true);
        }
    }
}