using Features.Player.Domain;
using Features.Player.Infrastructure;
using MessagePipe;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Features.Player.Application
{
    public class InputService : IInitializable, ITickable
    {
        private readonly IPublisher<PlayerMoveInput> _movePub;
        private readonly IPublisher<PlayerJumpPressed> _jumpPub;
        private readonly IPublisher<PlayerBarkPressed> _barkPub;
        private readonly IPublisher<PlayerShapeRequest> _shapePub;

        private InputAction move, jump, bark, shapeDog, shapeBird, shapeRat;
        private readonly PlayerInputProvider _provider;
        private bool _jumpHeld;

        public InputService(
            PlayerInputProvider playerInputProvider,
            IPublisher<PlayerMoveInput> movePub,
            IPublisher<PlayerJumpPressed> jumpPub,
            IPublisher<PlayerBarkPressed> barkPub,
            IPublisher<PlayerShapeRequest> shapePub)
        {
            _provider = playerInputProvider; _movePub = movePub; _jumpPub = jumpPub; _barkPub = barkPub; _shapePub = shapePub;
        }

        public void Initialize()
        {
            var actions = _provider.PlayerInput.actions;

            move = actions["Move"];       move.Enable();
            jump = actions["Jump"];       jump.Enable();
            bark = actions["Bark"];       bark.Enable();
            shapeDog = actions["ShapeDog"];   shapeDog.Enable();
            shapeBird = actions["ShapeBird"]; shapeBird.Enable();
            shapeRat = actions["ShapeRat"];   shapeRat.Enable();
        }

        public void Tick()
        {
            _movePub.Publish(new PlayerMoveInput(move.ReadValue<Vector2>()));

            bool isJumpPressed = jump.IsPressed();
            if (isJumpPressed && !_jumpHeld)
                _jumpPub.Publish(new PlayerJumpPressed());
            _jumpHeld = isJumpPressed;

            if (bark.WasPressedThisFrame())   _barkPub.Publish(new PlayerBarkPressed());
            if (shapeDog.WasPressedThisFrame())  _shapePub.Publish(new PlayerShapeRequest(Shape.Dog));
            if (shapeBird.WasPressedThisFrame()) _shapePub.Publish(new PlayerShapeRequest(Shape.Bird));
            if (shapeRat.WasPressedThisFrame())  _shapePub.Publish(new PlayerShapeRequest(Shape.Rat));
        }
    }
}
