using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using UnityEngine;

namespace Features.Player.Application
{
    public sealed class GroundMovementStrategy : IMovementStrategy
    {
        private readonly GameSettings _settings;
        private readonly PlayerModel _model;
        private readonly IPlayerPhysicsPort _physics;

        public GroundMovementStrategy(
            GameSettings settings,
            PlayerModel model,
            IPlayerPhysicsPort physics)
        {
            _settings = settings;
            _model = model;
            _physics = physics;
        }

        public Vector2 CalculateVelocity(
            Vector2 input,
            Vector2 currentVelocity)
        {
            float speed =
                _settings.PlayerMovements
                         .GetSpeed(_model.CurrentShape);

            bool isWall = false;

            if (Mathf.Abs(input.x) > 0.1f)
            {
                float dir = Mathf.Sign(input.x);
                float rayDistance =
                    _settings.ShapesColliderSettings
                             .GetSize(_model.CurrentShape).x * 0.5f + 0.05f;

                int mask = ~_settings.Layers.PlayerMask.value;

                Vector2 offset = _settings.ShapesColliderSettings.GetOffset(_model.CurrentShape);

                isWall = _physics.HasWall( offset,dir, rayDistance, mask);
            }

            float horizontal = isWall
                ? 0f
                : input.x * speed;

            return new Vector2(horizontal, currentVelocity.y);
        }
    }
}