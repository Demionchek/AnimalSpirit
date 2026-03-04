using System.Collections.Generic;
using DefaultNamespace.Features.Interactables.Domain;
using Features.Interactables.Infrastructure;

namespace Features.Interactables.Application
{
    public sealed class DoorService
    {
        private readonly DoorModel _model;

        private readonly List<IOpener> _openers =
            new List<IOpener>();

        private bool _manualOverride;

        public DoorService(DoorModel model)
        {
            _model = model;
        }

        public void RegisterOpener(IOpener opener)
        {
            if (opener != null)
                _openers.Add(opener);
        }

        public void Evaluate()
        {
            if (_manualOverride)
                return;

            bool shouldOpen = AreAllOpenersActive();

            if (shouldOpen != _model.IsOpen)
                _model.SetState(shouldOpen);
        }

        public void SetManualState(bool open)
        {
            _manualOverride = open;
            _model.SetState(open);
        }

        private bool AreAllOpenersActive()
        {
            foreach (var opener in _openers)
            {
                if (!opener.IsActive)
                    return false;
            }

            return true;
        }
    }
}