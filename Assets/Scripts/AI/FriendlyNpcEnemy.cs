using System;
using System.Collections;
using Animations;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI
{
    public class FriendlyNpcEnemy : BaseEnemy, IHittable
    {
        [Header("Friendly Combat")]
        [SerializeField] private float attackRadius = 0.25f;
        [SerializeField] private float attackDistanceMin = 0.5f;
        [SerializeField] private float attackDistanceMax = 0.7f;
        [Header("Friendly Movement")]
        [SerializeField] private float fallSpeed = 3f;
        [SerializeField] private float wallCheckDistance = 0.2f;
        [SerializeField] private float groundNormalMinY = 0.35f;
        private bool isActivated = false;
        private float attackDistance = 0.5f;
        private readonly ContactPoint2D[] groundContacts = new ContactPoint2D[16];

        public void Activate()
        {
            isActivated = true;
            attackDistance = Random.Range(attackDistanceMin, attackDistanceMax);
            Initialize();
            StartCoroutine(DetectionRoutine());
        }

        public void SetSortingOrder(int sortingOrder)
        {
            Initialize();
            AnimController.GetSpriteRenderer().sortingOrder = sortingOrder;
        }

        private void Update()
        {
            if (isDead || !isActivated) return;

            currentAttackTime = Time.time;
            HandleBehaviour();
        }

        private void HandleBehaviour()
        {
            Transform combatTarget = GetCombatTarget();

            if (combatTarget != null)
            {
                MoveToTarget(combatTarget, attackDistance, true);

                if (Mathf.Abs(transform.position.x - combatTarget.position.x) <= attackDistance &&
                    currentAttackTime > lastAttackTime + attackDelay)
                {
                    AttackTarget();
                }

                return;
            }

            Transform moveTarget = GetFallbackTarget();
            if (moveTarget != null)
            {
                MoveToTarget(moveTarget, 0f, false);

                if (TryInvokeFallbackAction())
                {
                    rb.linearVelocity = GetGroundAwareIdleVelocity();
                    AnimController.SetAnimatorFloat(AnimationController.SPEED_S, 0f);
                }

                return;
            }

            rb.linearVelocity = GetGroundAwareIdleVelocity();
            AnimController.SetAnimatorFloat(AnimationController.SPEED_S, 0f);
        }

        private void MoveToTarget(Transform moveTarget, float stopDistance, bool updateCanAttack)
        {
            float deltaX = moveTarget.position.x - transform.position.x;
            float horizontalDistance = Mathf.Abs(deltaX);
            bool isOnSolid = IsOnSolid();
            Vector2 fallVelocity = isOnSolid ? Vector2.zero : Vector2.down * fallSpeed;

            if (horizontalDistance <= stopDistance)
            {
                rb.linearVelocity = fallVelocity;
                AnimController.SetAnimatorFloat(AnimationController.SPEED_S, 0f);
                if (updateCanAttack) canAttack = true;
                return;
            }

            Vector2 horizontalDirection = new Vector2(Mathf.Sign(deltaX), 0f);
            Vector2 moveVelocity = horizontalDirection * speed;

            if (TryGetWallHit(horizontalDirection, out RaycastHit2D wallHit))
            {
                Vector2 wallTangent = Vector2.Perpendicular(wallHit.normal).normalized;
                if (Vector2.Dot(wallTangent, horizontalDirection) < 0f)
                {
                    wallTangent = -wallTangent;
                }

                moveVelocity = Vector3.Project(moveVelocity, wallTangent);
            }

            Vector2 finalVelocity = moveVelocity + fallVelocity;
            rb.linearVelocity = finalVelocity;
            AnimController.SetAnimatorFloat(AnimationController.SPEED_S, finalVelocity.sqrMagnitude > 0.0001f ? 1f : 0f);
            AnimController.GetSpriteRenderer().flipX = horizontalDirection.x < 0f;

            if (updateCanAttack) canAttack = false;
        }

        private bool IsOnSolid()
        {
            ContactFilter2D filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = obstacleMask,
                useTriggers = false
            };

            int contactsCount = capsule.GetContacts(filter, groundContacts);
            for (int i = 0; i < contactsCount; i++)
            {
                if (groundContacts[i].normal.y >= groundNormalMinY)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector2 GetGroundAwareIdleVelocity()
        {
            return IsOnSolid() ? Vector2.zero : Vector2.down * fallSpeed;
        }

        private bool TryGetWallHit(Vector2 direction, out RaycastHit2D hit)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                hit = default;
                return false;
            }

            Vector2 castDirection = direction.normalized;
            Vector2 origin = (Vector2)capsule.bounds.center + castDirection * (capsule.bounds.extents.x + 0.01f);
            float rayLength = wallCheckDistance + 0.01f;
            hit = Physics2D.Raycast(origin, castDirection, rayLength, obstacleMask);
            return hit.collider != null && hit.collider != capsule;
        }

        private void AttackTarget()
        {
            AnimController.SetAnimatorTrigger(AnimationController.ATTACK_S);
            lastAttackTime = Time.time;

            bool isFlip = AnimController.GetSpriteRenderer().flipX;
            Vector2 origin = (Vector2)transform.position + new Vector2(isFlip ? -attackDistance : attackDistance, 0.15f);

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, attackRadius, targetMask);
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider == capsule) continue;

                if (hitCollider.TryGetComponent(out IHittable hittable))
                {
                    hittable.Hit();
                }
            }
        }

        public void Hit()
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            AnimController.SetAnimatorBool(AnimationController.IS_DEAD_S, true);
            PlaySound(deathSound);
        }

        private void PlaySound(AudioClip clip)
        {
            audioSource?.PlayOneShot(clip);
        }
    }
}
