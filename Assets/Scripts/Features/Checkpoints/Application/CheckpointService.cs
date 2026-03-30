using Features.Checkpoints.Domain;
using Unity.VisualScripting;
using UnityEngine;

namespace Features.Checkpoints.Application
{
    public class CheckpointService
    {
        private CheckpointModel _model;

        public void Initialize()
        {
            _model = new CheckpointModel();
        }

        public void SetCheckpoint(int index, Vector3 position)
        {
            _model.index = index;
            _model.position = position;
        }


    }
}