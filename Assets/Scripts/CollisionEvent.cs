using System;
using UnityEngine;
using UnityEngine.Events;

namespace DefaultNamespace
{
    public class CollisionEvent : MonoBehaviour
    {
        [SerializeField] private UnityEvent onCollisionEnter;
        [SerializeField] private LayerMask mask;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (mask == (mask | (1 << other.gameObject.layer)))
                onCollisionEnter?.Invoke();
        }
    }
}