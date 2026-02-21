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
        private readonly IMovementStrategy _groundStrategy;
        private readonly IMovementStrategy _birdStrategy;

        private Vector2 _currentInput;
        private Vector2 _effectorVelocity;

        public PlayerMovementService(
            PlayerModel model,
            GameSettings settings,
            GroundMovementStrategy groundStrategy,
            BirdMovementStrategy birdStrategy)
        {
            _model = model;
            _settings = settings;
            _groundStrategy = groundStrategy;
            _birdStrategy = birdStrategy;
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
            IMovementStrategy strategy =
                _model.CurrentShape == Shape.Bird
                    ? _birdStrategy
                    : _groundStrategy;

            return strategy.CalculateVelocity(
                _currentInput,
                currentVelocity);
        }

        public float GetJumpForce()
        {
            if (!_model.IsGrounded ||
                _model.IsDead ||
                _model.CurrentShape == Shape.Bird)
                return 0;

            float jumpForce =
                _settings.PlayerMovements
                         .GetJumpForce(_model.CurrentShape);

            return  jumpForce;
        }
    }
}