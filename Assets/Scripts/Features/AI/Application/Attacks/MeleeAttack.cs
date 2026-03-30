using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.Settings.AI;
using Features.Interactables.Infrastructure;
using Features.Player.Infrastructure;
using Interfaces;
using UnityEngine;

namespace Features.AI.Application.Attacks
{
    public sealed class MeleeAttack : IEnemyAttack
    {
        private readonly EnemyTypeConfig _config;
        private readonly IEnemyPhysicsPort _physics;
        private readonly IEnemyAnimationPort  _animation;

        public MeleeAttack(
            EnemyTypeConfig config,
            IEnemyPhysicsPort physics,
            IEnemyAnimationPort animation)
        {
            _config = config;
            _physics = physics;
            _animation = animation;
        }

        public void Execute(EnemyModel model)
        {
            if (model.Target == null)
                return;

            Vector2 origin = _physics.Position + (_animation.IsFlipped ? Vector2.left : Vector2.right) * _config.attackXOffset;
            origin.y += _config.attackYOffset;
            float radius = _config.attackDistance;

            int layerMask = 1 << model.Target.gameObject.layer;

            Collider2D[] hits =
                Physics2D.OverlapCircleAll(origin, radius, layerMask);

#if UNITY_EDITOR
            Color color =  Color.red;
            DebugExtension.DrawCircle(origin, radius, color);
#endif

            foreach (var h in hits)
            {
                if (h.TryGetComponent<IHittable>(out var hittable))
                {
                    hittable.Hit();
                }
            }
        }
    }
}