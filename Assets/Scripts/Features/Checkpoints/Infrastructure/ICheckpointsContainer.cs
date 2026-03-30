using UnityEngine;

namespace Features.Checkpoints.Infrastructure
{
    public interface ICheckpointsContainer
    {
        Vector3 GetPositionByIndex(int index);
    }
}