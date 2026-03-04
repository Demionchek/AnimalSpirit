using Features.Interactables.Infrastructure;

namespace Features.Interactables.Application
{
    public sealed class SetCheckpointAction :
        IInteractionAction
    {
        //        private readonly CheckpointService _checkpoint;
        private readonly int _index;

        public SetCheckpointAction(
        //    CheckpointService checkpoint,
            int index)
        {
         //   _checkpoint = checkpoint;
            _index = index;
        }

        public void Execute()
        {
         //   _checkpoint.SetCurrent(_index);
        }
    }
}