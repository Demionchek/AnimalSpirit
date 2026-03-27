using Features.Core.Settings.Scene;
using Features.Interactables.Infrastructure;
using Features.Player.Infrastructure;
using UnityEngine;

namespace Features.Interactables.Presentation
{
    public sealed class InteractableCharacterPhysicsPort : MonoBehaviour
    {
        [SerializeField] private InteractionConfig _config;

        public void CircleCastHit()
        {
            Collider2D[] _overlapResults = new Collider2D[10];

            Vector2 offset = new Vector2(_config.xDistance, _config.yDistance);
            Vector2 origin = (Vector2)transform.position + offset;
            float radius = _config.circleRadius;

            int count = Physics2D.OverlapCircleNonAlloc(
                origin,
                radius,
                _overlapResults
            );

#if UNITY_EDITOR
                DebugExtension.DrawCircle(origin, radius, Color.deepSkyBlue);
#endif

            if (count <= 0) return;

            foreach (var collider2D in _overlapResults)
            {
                if (collider2D == null) continue;

                if (collider2D.gameObject.TryGetComponent<IHittable>(out var hittable))
                {
                    hittable.Hit();
                    Debug.Log("Hit " + collider2D.gameObject.name);
                }
            }
        }
    }
}