using System.Collections.Generic;
using Features.Openers.Domain;

namespace Features.Openers.Application
{
    public sealed class DoorService
    {
        private readonly DoorModel _model;
        private List<OpenerModel> _openers;

        private bool _manualOverride;
        private bool _manualState;

        public DoorService(
            DoorModel model)
        {
            _model = model;;
        }

        public void SetOpeners(List<OpenerModel> openers) => _openers =  openers;

        public void Evaluate()
        {
            if (_manualOverride)
            {
                _model.SetState(_manualState);
                return;
            }

            foreach (var opener in _openers)
            {
                if (!opener.IsActive)
                {
                    _model.SetState(false);
                    return;
                }
            }

            _model.SetState(true);
        }

        public void SetManual(bool open)
        {
            _manualOverride = true;
            _manualState = open;
            _model.SetState(open);
        }
    }
}