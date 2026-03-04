using Features.Dialogue.Domain;
using Features.Interactables.Infrastructure;
using MessagePipe;

namespace Features.Interactables.Application
{
    public sealed class StartDialogueAction : IInteractionAction
    {
        private readonly int _dialogueId;
        private readonly IPublisher<DialogueRequested> _publisher;

        public StartDialogueAction(
            int dialogueId,
            IPublisher<DialogueRequested> publisher)
        {
            _dialogueId = dialogueId;
            _publisher = publisher;
        }

        public void Execute()
        {
            _publisher.Publish(new DialogueRequested(_dialogueId));
        }
    }
}