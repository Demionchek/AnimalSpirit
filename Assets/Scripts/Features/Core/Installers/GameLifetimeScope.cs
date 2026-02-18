using System;
using DefaultNamespace;
using Features.Player.Application;
using Features.Player.Presentation;
using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UnityEngine.InputSystem;
using MessagePipe;

namespace Features.Core.Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSettings gameSettings;

        public override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<PlayerMoveInput>(options);
            builder.RegisterMessageBroker<PlayerJumpPressed>(options);
            builder.RegisterMessageBroker<PlayerBarkPressed>(options);
            builder.RegisterMessageBroker<PlayerShapeRequest>(options);

            builder.RegisterInstance(gameSettings);
            builder.RegisterComponentInHierarchy<UnityPlayerPhysicsPort>()
                   .As<IPlayerPhysicsPort>();

            builder.RegisterComponentInHierarchy<PlayerAnimationView>();
            builder.RegisterComponentInHierarchy<PlayerView>()
                   .AsSelf()
                   .As<IPlayerViewPort>();
            builder.Register<PlayerFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .As<ITickable>()
                   .As<IFixedTickable>()
                   .As<IDisposable>()
                   .AsSelf();
            builder.RegisterComponentInHierarchy<PlayerInput>();
            builder.Register<PlayerInputProvider>(Lifetime.Singleton).AsSelf();
            builder.Register<InputService>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .As<ITickable>()
                   .AsSelf();
            builder.Register<PlayerModel>(Lifetime.Scoped).AsSelf();
            builder.Register<PlayerMovementService>(Lifetime.Scoped);
            builder.Register<PlayerShapeService>(Lifetime.Scoped);
            builder.Register<PlayerLifeService>(Lifetime.Scoped);
            builder.Register<PlayerInteractionService>(Lifetime.Scoped);

        }
    }
}