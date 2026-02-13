using DefaultNamespace;
using DefaultNamespace.Features.Player.Application;
using DefaultNamespace.Features.Player.Presentation;
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
            builder.Register<PlayerService>(Lifetime.Scoped).AsSelf();
            builder.RegisterComponentInHierarchy<PlayerView>();
            builder.RegisterComponentInHierarchy<PlayerAnimationView>();
            builder.RegisterComponentInHierarchy<RandomSoundPlayer>(); // если нужно
            builder.RegisterComponentInHierarchy<CheckPoints>();
            builder.RegisterComponentInHierarchy<DialogueSystem>();
        }
    }
}