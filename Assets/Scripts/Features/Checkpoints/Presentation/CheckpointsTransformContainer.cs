using Features.Checkpoints.Infrastructure;
using UnityEngine;

namespace Features.Checkpoints.Presentation
{
    public class CheckpointsTransformContainer : MonoBehaviour , ICheckpointsContainer
    {
        [SerializeField] private Transform[] _checkpoints;

        public Vector3 GetPositionByIndex(int index)
        {
            return _checkpoints[index].position;
        }
    }
}