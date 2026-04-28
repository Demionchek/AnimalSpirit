using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class BoxRespawner : MonoBehaviour
    {
        [SerializeField] private Transform respawnPos;
        [SerializeField] private GameObject destroyEffect;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (respawnPos == null) return;
            
            if (other.gameObject.layer == LayerMask.NameToLayer("Laser"))
            {
                if (destroyEffect != null)
                {
                    GameObject effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
                    Destroy(effect, 1f);
                }
                transform.position = respawnPos.position;
                rb.linearVelocity = Vector2.zero;

            }
        }
    }
}