using System;
using DefaultNamespace;
using Features.Player.Application;
using Features.Player.Presentation;
using Features.Core.Settings;
using Features.Cutscene.Application;
using Features.Cutscene.Domain;
using Features.Cutscene.Infrastructure;
using Features.Cutscene.Presentation;
using Features.Dialogue.Application;
using Features.Dialogue.Domain;
using Features.Dialogue.Infrastructure;
using Features.Dialogue.Presentation;
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
        [SerializeField] private DialogueDatabase dialogueData;

        public override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterInstance(gameSettings);
            builder.RegisterInstance(dialogueData);

            //
            //  MESSAGES
            //
            builder.RegisterMessageBroker<DialogueRequested>(options);
            builder.RegisterMessageBroker<DialogueFinished>(options);
            builder.RegisterMessageBroker<PlayerControlStateChanged>(options);
            builder.RegisterMessageBroker<PlayerMoveInput>(options);
            builder.RegisterMessageBroker<PlayerJumpPressed>(options);
            builder.RegisterMessageBroker<PlayerBarkPressed>(options);
            builder.RegisterMessageBroker<PlayerShapeRequest>(options);

            //
            //  PLAYER
            //
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
            builder.Register<GroundMovementStrategy>(Lifetime.Scoped);
            builder.Register<BirdMovementStrategy>(Lifetime.Scoped);
            builder.Register<PlayerShapeService>(Lifetime.Scoped);
            builder.Register<PlayerLifeService>(Lifetime.Scoped);
            builder.Register<PlayerInteractionService>(Lifetime.Scoped);

            //
            // TIMELINE
            //
            builder.Register<CutsceneModel>(Lifetime.Scoped);
            builder.Register<CutsceneService>(Lifetime.Scoped);
            builder.Register<CutsceneFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .AsSelf();
            builder.RegisterComponentInHierarchy<UnityTimelinePort>()
                   .As<ICutscenePort>();

            //
            // DIALOGUE
            //
            builder.RegisterComponentInHierarchy<DialogueTrigger>();
            builder.Register<DialogueModel>(Lifetime.Scoped);
            builder.Register<DialogueService>(Lifetime.Scoped);
            builder.Register<DialogueFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .AsSelf();

            builder.RegisterComponentInHierarchy<UnityDialogueView>()
                   .As<IDialogueViewPort>();
        }
    }
}