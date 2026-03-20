using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.Settings.AI;
using UnityEngine;

namespace Features.AI.Application
{
    public sealed class EnemyMovementService
    {
        private readonly IEnemyPhysicsPort _physics;
        private readonly EnemyTypeConfig _config;

        public EnemyMovementService(
            IEnemyPhysicsPort physics,
            EnemyTypeConfig config)
        {
            _physics = physics;
            _config = config;
        }

        public void Move(Vector2 direction)
        {
            _physics.SetVelocity(direction * _config.speed);
        }

        public void Stop()
        {
            _physics.SetVelocity(Vector2.zero);
        }
    }
}