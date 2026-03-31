using UnityEngine;

namespace Features.Checkpoints.Domain
{
    public readonly struct CheckpointRequest { }

    public readonly struct CheckpointCallback
    {
        public readonly Vector2 position;
        public CheckpointCallback(Vector2 position)
        {
            this.position = position;
        }
    }

    public readonly struct CheckpointSetter
    {
        public readonly int index;
        public CheckpointSetter(int index)
        {
            this.index = index;
        }
    }
}