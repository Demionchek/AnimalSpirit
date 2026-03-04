using Features.Interactables.Infrastructure;
using Features.Player.Application;
using Features.Player.Domain;

namespace Features.Interactables.Application
{
    public sealed class UnlockShapeAction : IInteractionAction
    {
        private readonly PlayerFacade _player;
        private readonly Shape _shape;

        public UnlockShapeAction(PlayerFacade player, Shape shape)
        {
            _player = player;
            _shape = shape;
        }

        public void Execute()
        {
            _player.UnlockShape(_shape);
        }
    }
}