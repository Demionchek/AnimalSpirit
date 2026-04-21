using System.Collections;
using Animations;
using Interfaces;
using UnityEngine;

namespace AI
{
    public class FriendlyNpcEnemy : BaseEnemy, IHittable
    {
        [Header("Friendly Combat")]
        [SerializeField] private float attackRadius = 0.25f;
        [SerializeField] private float attackDistance = 0.35f;
        private bool isActivated = false;

        public void Activate()
        {
            isActivated = true;
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
                MoveToTarget(combatTarget, stoppingDistance, true);

                if (Vector2.Distance(transform.position, combatTarget.position) <= stoppingDistance &&
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
                    rb.linearVelocity = Vector2.zero;
                    AnimController.SetAnimatorFloat(AnimationController.SPEED_S, 0f);
                }

                return;
            }

            rb.linearVelocity = Vector2.zero;
            AnimController.SetAnimatorFloat(AnimationController.SPEED_S, 0f);
        }

        private void MoveToTarget(Transform moveTarget, float stopDistance, bool updateCanAttack)
        {
            Vector2 direction = moveTarget.position - transform.position;
            float distance = direction.magnitude;

            if (distance <= stopDistance)
            {
                rb.linearVelocity = Vector2.zero;
                AnimController.SetAnimatorFloat(AnimationController.SPEED_S, 0f);
                if (updateCanAttack) canAttack = true;
                return;
            }

            direction.Normalize();
            rb.linearVelocity = direction * speed;
            AnimController.SetAnimatorFloat(AnimationController.SPEED_S, 1f);
            AnimController.GetSpriteRenderer().flipX = direction.x < 0f;

            if (updateCanAttack) canAttack = false;
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
