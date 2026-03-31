using Features.Checkpoints.Domain;
using Unity.VisualScripting;
using UnityEngine;

namespace Features.Checkpoints.Application
{
    public class CheckpointService
    {
        private readonly CheckpointModel _model;

        public CheckpointService(
            CheckpointModel model)
        {
            _model = model;
        }

        public void SetCheckpoint(int index, Vector3 position)
        {
            _model.index = index;
            _model.position = position;
        }

        public Vector3 GetCheckpointPosition() => _model.position;
    }
}