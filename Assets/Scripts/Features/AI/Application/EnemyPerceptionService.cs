using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.Settings.AI;
using UnityEngine;

namespace Features.AI.Application
{
    public sealed class EnemyPerceptionService
    {
        private readonly EnemyModel _model;
        private readonly EnemyTypeConfig _config;
        private readonly IEnemyPhysicsPort _physics;

        private readonly Collider2D[] _buffer = new Collider2D[8];

        public EnemyPerceptionService(
            EnemyModel model,
            EnemyTypeConfig config,
            IEnemyPhysicsPort physics)
        {
            _model = model;
            _config = config;
            _physics = physics;
        }

        public void Tick()
        {
            _model.Target = null;
            _model.CanSeeTarget = false;

            int count = _physics.OverlapCircle(
                _physics.Position,
                _config.sightRange,
                _config.targetMask,
                _buffer);

            for (int i = 0; i < count; i++)
            {
                var t = _buffer[i].transform;

                if (IsVisible(t))
                {
                    _model.Target = t;
                    _model.CanSeeTarget = true;
                    return;
                }
            }
        }

        private bool IsVisible(Transform target)
        {
            Vector2 origin = _physics.Position;
            Vector2 targetPos = target.position;

            Vector2 dir = (targetPos - origin).normalized;

            float angle = Vector2.Angle(GetForward(), dir);

            if (angle > _config.sightAngle * 0.5f)
                return false;

            float distance = Vector2.Distance(origin, targetPos);

            bool blocked = _physics.Raycast(
                origin,
                dir,
                distance,
                _config.obstacleMask);

            return !blocked;
        }

        private Vector2 GetForward()
        {
            // можно заменить на порт анимации
            return Vector2.right;
        }
    }
}