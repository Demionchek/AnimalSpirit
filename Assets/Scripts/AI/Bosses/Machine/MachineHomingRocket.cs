using System;
using Interfaces;
using ObjectPool;
using Player;
using UnityEngine;

namespace AI.Bosses.Machine
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MachineHomingRocket : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float riseSpeed = 5f;
        [SerializeField] private float flightSpeed = 7f;
        [SerializeField] private float turnSpeed = 240f;
        [SerializeField] private float riseReachDistance = 0.1f;
        [SerializeField] private float lifeTime = 8f;
        [SerializeField] private float homingTime;

        [Header("Explosion")]
        [SerializeField] private float explosionRadius = 1.5f;
        [SerializeField] private GameObject explosionVfxPrefab;
        [SerializeField] private LayerMask explosionDamageMask;
        [SerializeField] private LayerMask explodeOnContactMask;

        private Rigidbody2D rb;
        private PlayerController player;
        private GameObjectPool pool;
        private Vector2 riseTarget;
        private Vector2 flightDirection;
        private float homingTimer;
        private float lifeTimer;
        private bool isRising = true;
        private bool isExploded;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            flightDirection = transform.right.sqrMagnitude > 0.0001f ? (Vector2)transform.right : Vector2.right;
        }

        public void SetPool(GameObjectPool rocketPool)
        {
            pool = rocketPool;
        }

        public void Launch(PlayerController targetPlayer, Vector2 targetRisePoint)
        {
            player = targetPlayer;
            riseTarget = targetRisePoint;
            isRising = true;
            isExploded = false;
            lifeTimer = 0f;
            homingTimer = 0f;
            rb.linearVelocity = Vector2.zero;
            flightDirection = transform.right.sqrMagnitude > 0.0001f ? (Vector2)transform.right : Vector2.right;
        }

        private void FixedUpdate()
        {
            if (isExploded) return;

            lifeTimer += Time.fixedDeltaTime;
            if (lifeTimer >= lifeTime)
            {
                Explode();
                return;
            }

            if (isRising)
            {
                MoveToRisePoint();
                return;
            }
            
            if (homingTimer > 0f)
            {
                homingTimer -= Time.fixedDeltaTime;
            }

            UpdateFlightDirection();
            rb.linearVelocity = flightDirection * flightSpeed;
            transform.right = flightDirection;
        }

        private void MoveToRisePoint()
        {
            Vector2 currentPosition = rb.position;
            Vector2 direction = riseTarget - currentPosition;

            if (direction.magnitude <= riseReachDistance)
            {
                isRising = false;
                homingTimer = homingTime;
                flightDirection = transform.right.sqrMagnitude > 0.0001f ? (Vector2)transform.right : Vector2.right;
                rb.linearVelocity = flightDirection * flightSpeed;
                return;
            }

            Vector2 moveDirection = direction.normalized;
            rb.linearVelocity = moveDirection * riseSpeed;
            transform.right = moveDirection;
        }

        private void UpdateFlightDirection()
        {
            if (player == null) return;

            bool canHome = homingTimer > 0f &&
                           (player.CurrentShape == PlayerController.Shape.Bird ||
                           player.CurrentShape == PlayerController.Shape.Dog);

            if (!canHome) return;

            Vector2 targetDirection = ((Vector2)player.transform.position - rb.position).normalized;
            if (targetDirection.sqrMagnitude < 0.0001f) return;

            flightDirection = Vector3.RotateTowards(
                flightDirection,
                targetDirection,
                turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime,
                0f).normalized;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryExplodeOnContact(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryExplodeOnContact(other.gameObject);
        }

        private void TryExplodeOnContact(GameObject other)
        {
            if (isExploded) return;

            bool shouldExplode = explodeOnContactMask.value == 0 ||
                                 (explodeOnContactMask.value & (1 << other.layer)) != 0;

            if (shouldExplode)
            {
                Explode();
            }
        }

        private void Explode()
        {
            if (isExploded) return;

            isExploded = true;
            rb.linearVelocity = Vector2.zero;

            if (explosionVfxPrefab != null)
            {
                GameObject vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 1f);
            }

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionDamageMask);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].TryGetComponent(out IHittable hittable))
                {
                    hittable.Hit();
                }
            }

            if (pool != null)
            {
                pool.Return(gameObject);
                return;
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
