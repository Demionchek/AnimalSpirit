using Features.Checkpoints.Application;
using Features.Checkpoints.Domain;
using Features.Interactables.Infrastructure;
using MessagePipe;

namespace Features.Interactables.Application
{
    public sealed class SetCheckpointAction :
        IInteractionAction
    {
        private readonly IPublisher<CheckpointSetter> _checkpoint;
        private readonly int _index;

        public SetCheckpointAction(
            IPublisher<CheckpointSetter> checkpoint,
            int index)
        {
            _checkpoint = checkpoint;
            _index = index;
        }

        public void Execute()
        {
            _checkpoint.Publish(new CheckpointSetter(_index));
        }
    }
}