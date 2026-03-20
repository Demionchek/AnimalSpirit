using UnityEngine;

namespace Features.AI.Infrastructure
{
    public interface IEnemyPatrolPort
    {
        Vector2 GetPoint(int index);
        int Count { get; }
    }
}