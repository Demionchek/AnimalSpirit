using UnityEngine;

namespace Features.AI.Infrastructure
{
    public interface IEnemyPhysicsPort
    {
        Vector2 Position { get; }

        void SetVelocity(Vector2 velocity);

        int OverlapCircle(
            Vector2 position,
            float radius,
            int layerMask,
            Collider2D[] results);

        bool Raycast(
            Vector2 origin,
            Vector2 direction,
            float distance,
            int mask);

        void Stop();
    }
}