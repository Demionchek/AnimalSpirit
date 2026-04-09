using Features.Checkpoints.Application;
using Features.Checkpoints.Domain;
using Features.Cutscene.Infrastructure;
using Features.Dialogue.Domain;
using Features.Interactables.Infrastructure;
using Features.Interactables.Presentation;
using Features.Openers.Presentation;
using Features.Player.Application;
using Features.Player.Domain;
using MessagePipe;
using UnityEngine;

namespace Features.Interactables.Application
{
    public sealed class InteractionActionFactory
    {
        private readonly IPublisher<DialogueRequested> _dialoguePublisher;
        private readonly IPublisher<CheckpointSetter> _checkpointPublisher;
        private readonly PlayerFacade _player;

        public InteractionActionFactory(
            IPublisher<DialogueRequested> dialoguePublisher,
            IPublisher<CheckpointSetter> checkpointPublisher,
            PlayerFacade player
            )
        {
            _dialoguePublisher = dialoguePublisher;
            _checkpointPublisher = checkpointPublisher;
            _player = player;
        }

        public IInteractionAction CreateDialogue(int id)
            => new StartDialogueAction(id, _dialoguePublisher);

        public IInteractionAction CreateTimeline(ICutscenePort port, int index) =>
            new PlayTimelineAction(port, index);

        public IInteractionAction CreateUnlock(Shape shape)
            => new UnlockShapeAction(_player, shape);

        public IInteractionAction CreateCheckpoint(int index)
             => new SetCheckpointAction(_checkpointPublisher, index);

        public IInteractionAction CreateActivate(GameObject target)
            => new ActivateObjectAction(target);

        public IInteractionAction CreateDoor(DoorView door, bool open)
            => new OpenDoorAction(door, open);

        public IInteractionAction CreateAudioSource(AudioSource source,  AudioClip audioClip)
            => new PlaySoundAction(audioClip, source);

        public IInteractionAction CreateAttackAction(
            Animator animator,
            string triggerName,
            InteractableCharacterPhysicsPort physicsPort)
            => new PerformAttackAction(animator, triggerName, physicsPort);
    }
}
