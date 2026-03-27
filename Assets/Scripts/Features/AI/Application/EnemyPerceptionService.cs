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
        private readonly IEnemyAnimationPort  _animation;

        private readonly Collider2D[] _buffer = new Collider2D[8];

        public EnemyPerceptionService(
            EnemyModel model,
            EnemyTypeConfig config,
            IEnemyPhysicsPort physics,
            IEnemyAnimationPort animation)
        {
            _model = model;
            _config = config;
            _physics = physics;
            _animation = animation;
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
#if UNITY_EDITOR
            DrawVisionDebug(_physics.Position, GetForward(), _config.sightRange, false, true);
#endif
        }

        private bool IsVisible(Transform target)
        {
            Vector2 origin = _physics.Position;
            Vector2 targetPos = target.position;

            Vector2 dir = (targetPos - origin).normalized;
            float distance = Vector2.Distance(origin, targetPos);

            float angle = Vector2.Angle(GetForward(), dir);
            bool insideFov = angle <= _config.sightAngle * 0.5f;

            if (!insideFov)
            {
                return false;
            }

            bool blocked = _physics.Raycast(
                origin,
                dir,
                distance,
                _config.obstacleMask);

            bool visible = !blocked;
#if UNITY_EDITOR
            DrawVisionDebug(origin, dir, distance, true, visible);
#endif
            return visible;
        }

        private Vector2 GetForward()
        {
            return _animation.IsFlipped ? Vector2.left : Vector2.right;
        }

        private void DrawVisionDebug(
            Vector2 origin,
            Vector2 directionToTarget,
            float distanceToTarget,
            bool insideFov,
            bool isVisible)
        {
            Vector2 forward = GetForward();
            float halfAngle = _config.sightAngle * 0.5f;

            Vector2 leftBound = Rotate(forward, -halfAngle);
            Vector2 rightBound = Rotate(forward, halfAngle);

            Debug.DrawRay(origin, leftBound * _config.sightRange, Color.yellow);
            Debug.DrawRay(origin, rightBound * _config.sightRange, Color.yellow);
            Debug.DrawRay(origin, forward.normalized * _config.sightRange, Color.cyan);

            Color rayColor = !insideFov
                ? Color.gray
                : (isVisible ? Color.green : Color.red);

            Debug.DrawRay(origin, directionToTarget * distanceToTarget, rayColor);
        }

        private static Vector2 Rotate(Vector2 vector, float angleDeg)
        {
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);

            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos).normalized;
        }
    }
}
