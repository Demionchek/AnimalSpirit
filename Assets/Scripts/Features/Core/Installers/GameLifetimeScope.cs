using System;
using DefaultNamespace;
using DefaultNamespace.Features.Interactables.Domain;
using Features.Trigger.Application;
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
using Features.Interactables.Application;
using Features.Interactables.Infrastructure;
using Features.Interactables.Presentation;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using Features.Trigger.Application;
using Features.Trigger.Domain;
using Features.Trigger.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UnityEngine.InputSystem;
using MessagePipe;
using Object = UnityEngine.Object;

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
            builder.Register<PlayerFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .As<ITickable>()
                   .As<IFixedTickable>()
                   .As<IDisposable>()
                   .AsSelf();

            builder.RegisterComponentInHierarchy<UnityPlayerPhysicsPort>()
                   .As<IPlayerPhysicsPort>();
            builder.RegisterComponentInHierarchy<PlayerAnimationView>();
            builder.RegisterComponentInHierarchy<PlayerView>()
                   .AsSelf()
                   .As<IPlayerViewPort>();
            builder.RegisterComponentInHierarchy<PlayerInput>();

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
            builder.Register<DialogueModel>(Lifetime.Scoped);
            builder.Register<DialogueService>(Lifetime.Scoped);
            builder.Register<DialogueFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .AsSelf();

            builder.RegisterComponentInHierarchy<DialogueTrigger>();
            builder.RegisterComponentInHierarchy<UnityDialogueView>()
                   .As<IDialogueViewPort>();

            //
            // INTERACTABLE
            //
            builder.Register<InteractionActionFactory>(
                   Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<
                   InteractableCharacter>();

            builder.Register<DoorModel>(Lifetime.Transient);
            builder.Register<DoorService>(Lifetime.Transient);

            builder.RegisterComponentInHierarchy<DoorView>()
                   .AsSelf();

            //
            // TRIGGERS
            //
            builder.RegisterMessageBroker<WorldTriggerRequested>(options);

            builder.Register<WorldTriggerService>(Lifetime.Scoped);

            builder.Register<WorldTriggerFacade>(Lifetime.Scoped)
                   .As<IInitializable>();

            //
            //Callback
            //
            builder.RegisterBuildCallback(container =>
            {
                   foreach (var doors in Object.FindObjectsOfType<DoorView>(true))
                     container.Inject(doors);

                   foreach (var trigger in Object.FindObjectsOfType<WorldTriggerView>(true))
                          container.Inject(trigger);
            });
        }
    }
}