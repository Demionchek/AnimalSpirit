using Features.AI.Infrastructure;
using UnityEngine;

namespace Features.AI.Presentation
{
    public sealed class UnityEnemyPatrolPort : MonoBehaviour, IEnemyPatrolPort
    {
        [SerializeField] private PatrolPointsContainer _container;

        public int Count => _container.Points.Length;

        public Vector2 GetPoint(int index)
        {
            return _container.Points[index].position;
        }
    }
}