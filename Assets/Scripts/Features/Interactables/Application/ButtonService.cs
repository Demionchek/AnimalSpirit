using System.Collections.Generic;
using DefaultNamespace.Features.Interactables.Domain;

namespace Features.Interactables.Application
{
    public sealed class ButtonService
    {
        private readonly ButtonModel _model;

        private readonly HashSet<object> _colliders =
            new HashSet<object>();

        public ButtonService(ButtonModel model)
        {
            _model = model;
        }

        public void Enter(object collider)
        {
            if (_colliders.Add(collider))
                Evaluate();
        }

        public void Exit(object collider)
        {
            if (_colliders.Remove(collider))
                Evaluate();
        }

        private void Evaluate()
        {
            _model.SetPressed(_colliders.Count > 0);
        }
    }
}