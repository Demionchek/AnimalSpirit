using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Features.Player.Infrastructure
{
    public class PlayerInputProvider : IInitializable
    {
        public PlayerInput PlayerInput { get; private set; }

        [Inject]
        public PlayerInputProvider(PlayerInput playerInput)
        {
            PlayerInput = playerInput;
        }
        public void Initialize() { }
    }
}