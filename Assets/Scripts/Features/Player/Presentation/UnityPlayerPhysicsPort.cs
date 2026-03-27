using System.Collections.Generic;
using Features.Core.Settings;
using Features.Player.Domain;
using Interfaces;
using UnityEngine;
using VContainer;

namespace Features.Player.Infrastructure
{
    public sealed class UnityPlayerPhysicsPort : MonoBehaviour, IPlayerPhysicsPort
    {
        [Header("Debug")]
        [SerializeField] private bool debugEnabled = true;

        private static readonly RaycastHit2D[] _rayHits = new RaycastHit2D[1];
        private static readonly Collider2D[] _overlapResults = new Collider2D[10];

        public bool HasWall(Vector2 offset, float direction, float distance, int mask)
        {
            Vector2 origin = (Vector2)transform.position + offset;
            Vector2 dir = Vector2.right * direction;

            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(mask);
            filter.useTriggers = false;

            int count = Physics2D.Raycast(
                origin,
                dir,
                filter,
                _rayHits,
                distance
            );

            bool hasHit = count > 0;

#if UNITY_EDITOR
            if (debugEnabled)
            {
                Color color = hasHit ? Color.red : Color.green;
                Debug.DrawRay(origin, dir * distance, color);
            }
#endif

            return hasHit;
        }

        public bool IsBlockedAbove(Vector2 offset, float distance, int mask)
        {
            Vector2 origin = offset + (Vector2)transform.position;
            Vector2 dir = Vector2.up;

            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(mask);
            filter.useTriggers = false;

            int count = Physics2D.Raycast(
                origin,
                dir,
                filter,
                _rayHits,
                distance
            );

            bool hasHit = count > 0;

#if UNITY_EDITOR
            if (debugEnabled)
            {
                Color color = hasHit ? Color.red : Color.green;
                Debug.DrawRay(origin, dir * distance, color);
            }
#endif

            return hasHit;
        }

        public IReadOnlyList<IInteractable> OverlapInteractables(Vector2 offset, float radius)
        {
            Vector2 origin = offset + (Vector2)transform.position;

            int count = Physics2D.OverlapCircleNonAlloc(
                origin,
                radius,
                _overlapResults
            );

#if UNITY_EDITOR
            if (debugEnabled)
            {
                DebugExtension.DrawCircle(origin, radius, Color.yellow);
            }
#endif

            var list = new List<IInteractable>(count);

            for (int i = 0; i < count; i++)
            {
                if (_overlapResults[i] != null &&
                    _overlapResults[i].TryGetComponent<IInteractable>(out var interactable))
                {
                    list.Add(interactable);
                }
            }

            return list;
        }
    }

#if UNITY_EDITOR
    public static class DebugExtension
    {
        public static void DrawCircle(Vector2 center, float radius, Color color, int segments = 24)
        {
            float angle = 0f;
            Vector2 lastPoint = center + new Vector2(Mathf.Cos(0), Mathf.Sin(0)) * radius;

            for (int i = 1; i <= segments; i++)
            {
                angle = i * Mathf.PI * 2f / segments;
                Vector2 nextPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                Debug.DrawLine(lastPoint, nextPoint, color);
                lastPoint = nextPoint;
            }
        }
    }
#endif
}