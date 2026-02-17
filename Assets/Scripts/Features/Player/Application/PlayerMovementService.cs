using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using UnityEngine;

namespace Features.Player.Application
{
    public sealed class PlayerMovementService
    {
        private readonly PlayerModel _model;
        private readonly GameSettings _settings;
        private readonly IPlayerPhysicsPort _physics;

        private Vector2 _currentInput;
        private Vector2 _effectorVelocity;

        public PlayerMovementService(
            PlayerModel model,
            GameSettings settings,
            IPlayerPhysicsPort physics)
        {
            _model = model;
            _settings = settings;
            _physics = physics;
        }

        public void SetInput(Vector2 input)
        {
            _currentInput = input;
        }

        public void SetGrounded(bool grounded)
        {
            if (_model.IsGrounded == grounded)
                return;

            _model.SetGrounded(grounded);
        }

        public void SetEffectorVelocity(float speed)
        {
            _effectorVelocity = new Vector2(speed, 0);
        }

        public void ClearEffector()
        {
            _effectorVelocity = Vector2.zero;
        }

        public Vector2 CalculateVelocity(Vector2 currentVelocity)
        {
            if (_model.IsDead)
                return currentVelocity;

            float speed = _settings.PlayerMovements
                                   .GetSpeed(_model.CurrentShape);

            if (_model.CurrentShape == Shape.Bird)
                return _currentInput * speed;

            bool wallInFront = false;

            if (Mathf.Abs(_currentInput.x) > 0.1f)
            {
                float dir = Mathf.Sign(_currentInput.x);
                float rayDistance =
                    _settings.ShapesColliderSettings
                             .GetSize(_model.CurrentShape).x * 0.5f;

                wallInFront = _physics.HasWall(dir, rayDistance);
            }

            float horizontal = wallInFront
                ? 0f
                : _currentInput.x * speed;

            return new Vector2(
                horizontal + _effectorVelocity.x,
                currentVelocity.y
            );
        }

        public Vector2 GetJumpForce()
        {
            if (!_model.IsGrounded ||
                _model.IsDead ||
                _model.CurrentShape == Shape.Bird)
                return Vector2.zero;

            float jumpForce =
                _settings.PlayerMovements
                         .GetJumpForce(_model.CurrentShape);

            return Vector2.up * jumpForce;
        }
    }
}