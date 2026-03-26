using Features.AI.Infrastructure;
using Features.AI.Presentation;
using Features.Core.ObjectPool.Infrastructure;
using Features.Core.ObjectPool.Presentation;
using Features.Core.Settings.AI;
using System;

namespace Features.AI.Application.Attacks
{
    public sealed class EnemyAttackFactory
    {
        private readonly MeleeAttack _melee;

        public EnemyAttackFactory(MeleeAttack melee)
        {
            _melee = melee;
        }

        public IEnemyAttack Create(
            EnemyTypeConfig config,
            EnemyView view,
            IObjectPool<BulletView> pool)
        {
            return config.attackType switch
            {
                EnemyAttackType.Melee =>
                    _melee,

                EnemyAttackType.Shooter =>
                    new ShooterAttack(
                        pool ?? throw new InvalidOperationException("Shooter enemy requires a bullet pool registration."),
                        view != null
                            ? view.ShootPoint
                            : throw new InvalidOperationException("Shooter enemy requires EnemyView in the scene.")),

                EnemyAttackType.Flame =>
                    new FlameAttack(
                        view != null
                            ? view.FireGO
                            : throw new InvalidOperationException("Flame enemy requires EnemyView in the scene.")),

                _ => throw new InvalidOperationException($"Unsupported attack type: {config.attackType}")
            };
        }
    }
}
