using System;
using Features.AI.Application;
using Features.AI.Application.Attacks;
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
        [SerializeField] private UnityEnemyPhysicsPort _physicsPort;
        [SerializeField] private UnityEnemyAnimationPort _animationPort;
        [SerializeField] private UnityEnemyPatrolPort _patrolPort;
        [SerializeField] private EnemyView _enemyView;

        public override void Configure(IContainerBuilder builder)
        {
            ResolveLocalDependencies();

            builder.Register<EnemyModel>(Lifetime.Scoped);

            builder.Register<EnemyMovementService>(Lifetime.Scoped);
            builder.Register<EnemyPerceptionService>(Lifetime.Scoped);
            builder.Register<EnemyCombatService>(Lifetime.Scoped);
            builder.Register<EnemyAnimationService>(Lifetime.Scoped);
            builder.Register<EnemyPatrolService>(Lifetime.Scoped);
            builder.Register<EnemyStateMachine>(Lifetime.Scoped);

            builder.Register<MeleeAttack>(Lifetime.Scoped);

            builder.Register<EnemyAttackFactory>(Lifetime.Scoped);

            builder.Register<EnemyFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .As<ITickable>()
                   .As<IFixedTickable>()
                   .AsSelf();

            builder.RegisterInstance(_config);

            builder.RegisterInstance(_physicsPort)
                   .As<IEnemyPhysicsPort>();

            builder.RegisterInstance(_animationPort)
                   .As<IEnemyAnimationPort>();

            builder.RegisterInstance(_patrolPort)
                   .As<IEnemyPatrolPort>();

            builder.RegisterInstance(_enemyView);
            builder.RegisterBuildCallback(container =>
            {
                container.Inject(_enemyView);
            });

            if (_config.attackType == EnemyAttackType.Shooter || _config.hasPool)
            {
                builder.RegisterInstance(
                    new BulletPool(_bulletPrefab, 10, _poolRoot)
                ).As<IObjectPool<BulletView>>();
            }
        }

        private void ResolveLocalDependencies()
        {
            _physicsPort ??= GetComponentInChildren<UnityEnemyPhysicsPort>(true);
            _animationPort ??= GetComponentInChildren<UnityEnemyAnimationPort>(true);
            _patrolPort ??= GetComponentInChildren<UnityEnemyPatrolPort>(true);
            _enemyView ??= GetComponentInChildren<EnemyView>(true);

            if (_config == null)
                throw new InvalidOperationException($"{nameof(EnemyLifetimeScope)} on {name} requires {nameof(_config)}.");

            if (_physicsPort == null)
                throw new InvalidOperationException($"{nameof(EnemyLifetimeScope)} on {name} requires {nameof(UnityEnemyPhysicsPort)} in children.");

            if (_animationPort == null)
                throw new InvalidOperationException($"{nameof(EnemyLifetimeScope)} on {name} requires {nameof(UnityEnemyAnimationPort)} in children.");

            if (_patrolPort == null)
                throw new InvalidOperationException($"{nameof(EnemyLifetimeScope)} on {name} requires {nameof(UnityEnemyPatrolPort)} in children.");

            if (_enemyView == null)
                throw new InvalidOperationException($"{nameof(EnemyLifetimeScope)} on {name} requires {nameof(EnemyView)} in children.");

            if ((_config.attackType == EnemyAttackType.Shooter || _config.hasPool) && (_bulletPrefab == null || _poolRoot == null))
                throw new InvalidOperationException($"{nameof(EnemyLifetimeScope)} on {name} requires pool references for shooter/pool enemies.");
        }
    }
}
