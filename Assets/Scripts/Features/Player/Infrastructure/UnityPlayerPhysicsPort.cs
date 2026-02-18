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
        public bool HasWall(float direction, float distance, int mask)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(mask);
            filter.useTriggers = false;

            RaycastHit2D[] hits = new RaycastHit2D[1];

            int count = Physics2D.Raycast(
                transform.position,
                Vector2.right * direction,
                filter,
                hits,
                distance
            );

            bool hasHit = count > 0;

            return hasHit;
        }

        public bool HasSpaceAbove(Vector2 offset, float distance, int mask)
        {
            Vector2 origin = offset + (Vector2)transform.position;

            return !Physics2D.Raycast(
                origin,
                Vector2.up,
                distance,
                mask);
        }

        public IReadOnlyList<IInteractable> OverlapInteractables(Vector2 offset, float radius)
        {
            Vector2 origin = offset + (Vector2)transform.position;

            Collider2D[] results = new Collider2D[10];

            int count = Physics2D.OverlapCircleNonAlloc(
                origin,
                radius,
                results);

            var list = new List<IInteractable>(count);

            for (int i = 0; i < count; i++)
            {
                if (results[i] != null &&
                    results[i].TryGetComponent<IInteractable>(out var interactable))
                {
                    list.Add(interactable);
                }
            }

            return list;
        }
    }
}