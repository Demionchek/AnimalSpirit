using UnityEngine;

namespace Features.Checkpoints.Domain
{
    public readonly struct CheckpointRequest { }

    public readonly struct CheckpointCallback
    {
        public readonly Vector2 Position;
        public CheckpointCallback(Vector2 position)
        {
            Position = position;
        }
    }
}