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
        private readonly IEnemyAnimationPort _animation;
        private readonly Transform _shootPointRight;
        private readonly Transform _shootPointLeft;

        public ShooterAttack(
            IObjectPool<BulletView> pool,
            IEnemyAnimationPort animation,
            Transform shootPointRight,
            Transform shootPointLeft)
        {
            _pool = pool;
            _animation = animation;
            _shootPointRight = shootPointRight;
            _shootPointLeft = shootPointLeft;
        }

        public void Execute(EnemyModel model)
        {
            if (model.Target == null)
                return;

            Transform shootPoint =
                _animation.IsFlipped ? _shootPointLeft : _shootPointRight;

            if (shootPoint == null)
                return;

            var bullet = _pool.Get();

            bullet.transform.position = shootPoint.position;

            Vector2 dir =
                (model.Target.position - shootPoint.position).normalized;

            bullet.Fire(dir);
        }
    }
}
