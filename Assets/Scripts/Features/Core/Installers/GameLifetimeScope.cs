using System;
using DefaultNamespace.Features.Interactables.Domain;
using DefaultNamespace.Features.UIShape.Application;
using Features.AI.Application;
using Features.AI.Infrastructure;
using Features.AI.Presentation;
using Features.Core.ObjectPool.Application;
using Features.Core.ObjectPool.Infrastructure;
using Features.Core.ObjectPool.Presentation;
using Features.Trigger.Application;
using Features.Player.Application;
using Features.Player.Presentation;
using Features.Core.Settings;
using Features.Core.Settings.Scene;
using Features.Cutscene.Application;
using Features.Cutscene.Domain;
using Features.Cutscene.Infrastructure;
using Features.Cutscene.Presentation;
using Features.Dialogue.Application;
using Features.Dialogue.Domain;
using Features.Dialogue.Infrastructure;
using Features.Dialogue.Presentation;
using Features.Interactables.Application;
using Features.Interactables.Presentation;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using Features.Trigger.Domain;
using Features.Trigger.Presentation;
using Features.UIShape.Presentation;
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
        [SerializeField] private SceneShapeConfig sceneShapeConfig;
        [Header("ObjectPool")]
        [SerializeField] private BulletView bulletPrefab;
        [SerializeField] private Transform poolRoot;

        public override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            var hasInteractables = HasComponentInScene<InteractableCharacter>();
            var hasDoors = HasComponentInScene<DoorView>();
            var hasWorldTriggers = HasComponentInScene<WorldTriggerView>();

            //
            //SETTINGS
            //
            builder.RegisterInstance(gameSettings);
            builder.RegisterInstance(dialogueData);
            builder.RegisterInstance(sceneShapeConfig);

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
            builder.RegisterMessageBroker<PlayerShapeChanged>(options);
            builder.RegisterMessageBroker<PlayerShapeUnlocked>(options);
            builder.RegisterMessageBroker<WorldTriggerRequested>(options);

            //
            // UI
            //
            builder.RegisterComponentInHierarchy<UIShapeView>();

            builder.Register<UIShapeFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .As<IDisposable>();

            //
            // OBJECT POOL
            //
            // builder.RegisterComponentInHierarchy<BulletView>();
            //
            // builder.Register<BulletPool>(Lifetime.Singleton)
            //        .WithParameter("initialSize", 10)
            //        .WithParameter("poolParent", poolRoot)
            //        .As<IObjectPool<BulletView>>();

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
            // AI
            //

            builder.RegisterComponentInHierarchy<UnityEnemyAnimationPort>()
                   .As<IEnemyAnimationPort>();
            builder.RegisterComponentInHierarchy<UnityEnemyPhysicsPort>()
                   .As<IEnemyPhysicsPort>();

            builder.Register<EnemyAnimationService>(Lifetime.Scoped);
            builder.Register<EnemyPatrolService>(Lifetime.Scoped);
            builder.Register<EnemyPerceptionService>(Lifetime.Scoped);

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
            if (hasInteractables)
            {
                   builder.Register<InteractionActionFactory>(
                          Lifetime.Scoped);

                   builder.RegisterComponentInHierarchy<
                          InteractableCharacter>();
            }

            if (hasDoors)
            {
                   builder.Register<DoorModel>(Lifetime.Transient);
                   builder.Register<DoorService>(Lifetime.Transient);

                   builder.RegisterComponentInHierarchy<DoorView>()
                          .AsSelf();
            }

            //
            // TRIGGERS
            //
            if (hasWorldTriggers)
            {
                   builder.Register<WorldTriggerService>(Lifetime.Scoped);

                   builder.Register<WorldTriggerFacade>(Lifetime.Scoped)
                          .As<IInitializable>();
            }

            //
            // CALLBACK
            //
            builder.RegisterBuildCallback(container =>
            {
                   if (hasDoors)
                          foreach (var doors in Object.FindObjectsOfType<DoorView>(true))
                                   container.Inject(doors);


                   if (hasWorldTriggers)
                            foreach (var trigger in Object.FindObjectsOfType<WorldTriggerView>(true))
                                   container.Inject(trigger);

                   if (hasInteractables)
                          foreach (var interactableCharacter in Object.FindObjectsOfType<InteractableCharacter>(true))
                                 container.Inject(interactableCharacter);
            });
        }

        private static bool HasComponentInScene<T>()
            where T : Component
        {
            return Object.FindObjectsOfType<T>(true).Length > 0;
        }
    }
}
