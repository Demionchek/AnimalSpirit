using DefaultNamespace;
using Features.Interactables.Infrastructure;
using Features.Interactables.Presentation;
using Features.Openers.Presentation;

namespace Features.Interactables.Application
{
    public sealed class OpenDoorAction :
        IInteractionAction
    {
        private readonly DoorView _door;
        private readonly bool _condition;

        public OpenDoorAction(
            DoorView door,
            bool condition)
        {
            _door = door;
            _condition = condition;
        }

        public void Execute()
        {
            if (_door != null)
                _door.SetOpen(_condition);
        }
    }
}