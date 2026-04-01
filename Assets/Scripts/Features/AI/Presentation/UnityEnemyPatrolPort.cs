using Features.AI.Infrastructure;
using UnityEngine;

namespace Features.AI.Presentation
{
    public sealed class UnityEnemyPatrolPort : MonoBehaviour, IEnemyPatrolPort
    {
        [SerializeField] private PatrolPointsContainer _container;

        public int Count => _container != null ? _container.Points.Length : 0;

        public Vector2 GetPoint(int index)
        {
            if (_container.Points.Length > 0)
            {
                return _container.Points[index].position;
            }
            return Vector2.zero;
        }
    }
}