using Features.AI.Application;
using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.AI.Presentation;
using Features.Core.ObjectPool.Application;
using Features.Core.ObjectPool.Infrastructure;
using Features.Core.ObjectPool.Presentation;
using Features.Core.Settings.AI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Core.Installers
{
    public sealed class EnemyLifetimeScope : LifetimeScope
    {
        [SerializeField] private EnemyTypeConfig _config;
        [SerializeField] private BulletView _bulletPrefab;
        [SerializeField] private Transform _poolRoot;

        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<EnemyModel>(Lifetime.Scoped);

            builder.RegisterInstance(_config);

            builder.RegisterComponentInHierarchy<UnityEnemyPhysicsPort>()
                   .As<IEnemyPhysicsPort>();

            builder.RegisterComponentInHierarchy<UnityEnemyAnimationPort>()
                   .As<IEnemyAnimationPort>();

            builder.RegisterComponentInHierarchy<UnityEnemyPatrolPort>()
                   .As<IEnemyPatrolPort>();

            builder.Register<EnemyMovementService>(Lifetime.Scoped);
            builder.Register<EnemyPerceptionService>(Lifetime.Scoped);
            builder.Register<EnemyCombatService>(Lifetime.Scoped);
            builder.Register<EnemyAnimationService>(Lifetime.Scoped);
            builder.Register<EnemyPatrolService>(Lifetime.Scoped);

            builder.Register<EnemyStateMachine>(Lifetime.Scoped);

            builder.RegisterInstance(
                new BulletPool(_bulletPrefab, 10, _poolRoot)
            ).As<IObjectPool<BulletView>>();

            builder.Register<EnemyFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .As<ITickable>();

            builder.RegisterComponentInHierarchy<EnemyView>();
        }
    }
}