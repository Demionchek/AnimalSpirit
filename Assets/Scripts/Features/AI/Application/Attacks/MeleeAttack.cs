using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.Settings.AI;
using Interfaces;
using UnityEngine;

namespace Features.AI.Application.Attacks
{
    public sealed class MeleeAttack : IEnemyAttack
    {
        private readonly EnemyTypeConfig _config;
        private readonly IEnemyPhysicsPort _physics;

        public MeleeAttack(
            EnemyTypeConfig config,
            IEnemyPhysicsPort physics)
        {
            _config = config;
            _physics = physics;
        }

        public void Execute(EnemyModel model)
        {
            if (model.Target == null)
                return;

            Vector2 origin = _physics.Position;
            float radius = _config.attackDistance;

            Collider2D[] hits =
                Physics2D.OverlapCircleAll(origin, radius);

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