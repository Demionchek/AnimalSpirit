using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.ObjectPool.Infrastructure;
using Features.Core.ObjectPool.Presentation;
using UnityEngine;

namespace Features.AI.Application.Attacks
{
    public sealed class ShooterAttack : IEnemyAttack
    {
        private readonly IObjectPool<BulletView> _pool;
        private readonly Transform _shootPoint;

        public ShooterAttack(
            IObjectPool<BulletView> pool,
            Transform shootPoint)
        {
            _pool = pool;
            _shootPoint = shootPoint;
        }

        public void Execute(EnemyModel model)
        {
            if (model.Target == null)
                return;

            var bullet = _pool.Get();

            bullet.transform.position = _shootPoint.position;

            Vector2 dir =
                (model.Target.position - _shootPoint.position).normalized;

            bullet.Fire(dir);
        }
    }
}