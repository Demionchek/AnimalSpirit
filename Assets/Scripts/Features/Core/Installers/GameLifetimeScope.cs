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

namespace Features.Core.Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSettings gameSettings;

        public override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(gameSettings);

            builder.RegisterComponentInHierarchy<PlayerInput>();
            builder.Register<PlayerInputProvider>(Lifetime.Singleton).AsSelf();
            builder.Register<PlayerModel>(Lifetime.Scoped).AsSelf();
            builder.Register<PlayerFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .As<ITickable>()
                   .As<IFixedTickable>()
                   .As<IDisposable>();
            builder.Register<PlayerMovementService>(Lifetime.Scoped);
            builder.Register<PlayerShapeService>(Lifetime.Scoped);
            builder.Register<PlayerLifeService>(Lifetime.Scoped);
            builder.Register<PlayerInteractionService>(Lifetime.Scoped);
            builder.Register<UnityPlayerPhysicsPort>(Lifetime.Scoped)
                   .As<IPlayerPhysicsPort>();
            builder.RegisterComponentInHierarchy<PlayerView>();
            builder.RegisterComponentInHierarchy<PlayerAnimationView>();
            builder.RegisterComponentInHierarchy<RandomSoundPlayer>(); // если нужно
            builder.RegisterComponentInHierarchy<CheckPoints>();
            builder.RegisterComponentInHierarchy<DialogueSystem>();
        }
    }
}