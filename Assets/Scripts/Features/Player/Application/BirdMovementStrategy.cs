using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using UnityEngine;

namespace Features.Player.Application
{
    public sealed class BirdMovementStrategy : IMovementStrategy
    {
        private readonly GameSettings _settings;
        private readonly PlayerModel _model;

        public BirdMovementStrategy(
            GameSettings settings,
            PlayerModel model)
        {
            _settings = settings;
            _model = model;
        }

        public Vector2 CalculateVelocity(
            Vector2 input,
            Vector2 currentVelocity)
        {
            float speed =
                _settings.PlayerMovements
                         .GetSpeed(_model.CurrentShape);

            return input * speed;
        }
    }
}