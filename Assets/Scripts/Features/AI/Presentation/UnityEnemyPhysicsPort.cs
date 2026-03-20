using Features.AI.Infrastructure;
using UnityEngine;

namespace Features.AI.Presentation
{
    public sealed class UnityEnemyPhysicsPort :
        MonoBehaviour,
        IEnemyPhysicsPort
    {
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public Vector2 Position => _rb.position;

        public void SetVelocity(Vector2 velocity)
        {
            _rb.linearVelocity = velocity;
        }

        public void Stop()
        {
            _rb.linearVelocity = Vector2.zero;
        }

        public int OverlapCircle(
            Vector2 position,
            float radius,
            int layerMask,
            Collider2D[] results)
        {
            return Physics2D.OverlapCircleNonAlloc(
                position,
                radius,
                results,
                layerMask);
        }

        public bool Raycast(
            Vector2 origin,
            Vector2 direction,
            float distance,
            int mask)
        {
            var hit = Physics2D.Raycast(
                origin,
                direction,
                distance,
                mask);

            return hit.collider != null;
        }


    }
}