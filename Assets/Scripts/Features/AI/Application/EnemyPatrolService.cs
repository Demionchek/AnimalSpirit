using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.Settings.AI;
using UnityEngine;

namespace Features.AI.Application
{
    public sealed class EnemyPatrolService
    {
        private readonly EnemyModel _model;
        private readonly EnemyTypeConfig _config;
        private readonly IEnemyPatrolPort _patrol;
        private readonly IEnemyPhysicsPort _physics;

        public EnemyPatrolService(
            EnemyModel model,
            EnemyTypeConfig config,
            IEnemyPatrolPort patrol,
            IEnemyPhysicsPort  physics)
        {
            _model = model;
            _config = config;
            _patrol = patrol;
            _physics = physics;
        }

        public Vector2 Tick(float deltaTime)
        {
            if (_patrol.Count == 0)
                return Vector2.zero;

            if (_model.IsWaiting)
            {
                _model.WaitTimer -= deltaTime;

                if (_model.WaitTimer <= 0f)
                {
                    _model.IsWaiting = false;
                    NextPoint();
                }

                return Vector2.zero;
            }

            Vector2 current = _physics.Position;
            Vector2 target = _patrol.GetPoint(_model.PatrolIndex);

            float distance = Vector2.Distance(current, target);

            if (distance < _config.reachedPointDistance)
            {
                _model.IsWaiting = true;
                _model.WaitTimer = _config.waitTimeAtPoint;
                return Vector2.zero;
            }

            return (target - current).normalized;
        }

        private void NextPoint()
        {
            if (_patrol.Count <= 1)
                return;

            _model.PatrolIndex =
                (_model.PatrolIndex + 1) % _patrol.Count;
        }
    }
}