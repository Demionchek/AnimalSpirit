using DefaultNamespace;
using Features.Dialogue.Domain;
using Features.Interactables.Infrastructure;
using Features.Interactables.Presentation;
using Features.Player.Application;
using Features.Player.Domain;
using MessagePipe;
using UnityEngine;

namespace Features.Interactables.Application
{
    public sealed class InteractionActionFactory
    {
        private readonly IPublisher<DialogueRequested> _dialoguePublisher;
        private readonly PlayerFacade _player;
        //private readonly CheckpointService _checkpoint;

        public InteractionActionFactory(
            IPublisher<DialogueRequested> dialoguePublisher,
            PlayerFacade player
            //CheckpointService checkpoint
            )
        {
            _dialoguePublisher = dialoguePublisher;
            _player = player;
            //_checkpoint = checkpoint;
        }

        public IInteractionAction CreateDialogue(int id)
            => new StartDialogueAction(id, _dialoguePublisher);

        public IInteractionAction CreateUnlock(Shape shape)
            => new UnlockShapeAction(_player, shape);

        // public IInteractionAction CreateCheckpoint(int index)
        //     => new SetCheckpointAction(_checkpoint, index);

        public IInteractionAction CreateActivate(GameObject target)
            => new ActivateObjectAction(target);

        public IInteractionAction CreateDoor(DoorView door, bool open)
            => new OpenDoorAction(door, open);
    }
}