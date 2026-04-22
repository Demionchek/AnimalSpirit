using System;
using System.Collections;
using System.Collections.Generic;
using Animations;
using Player;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    public enum AIState
    {
        empty,
        idle,
        attack,
        death,
        patrol
    }

    [RequireComponent(typeof(EnemyAnimationController), typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public abstract class BaseEnemy : MonoBehaviour
    {
        [SerializeField] public float speed;
        [SerializeField] private bool isRange = false;
        [SerializeField] AIState startState = AIState.idle;
        [SerializeField] private LayerMask deathMask;
        [Space(5)]
        [Header("Spotting")]
        [SerializeField] private float sightRange = 2f;
        [SerializeField] private float sightAngle = 90f;
        [SerializeField] private float checkFrequency = 0.2f;
        [SerializeField] protected LayerMask targetMask;
        [SerializeField] protected LayerMask obstacleMask;
        [SerializeField] public float attackDelay = 0.3f;
        [HideInInspector] public bool canSeeTarget = false;

        [Space(5)]
        [Header("Patroll")]
        [SerializeField] public List<Transform> patrolPoints;
        [SerializeField] public float waitTimeAtPoint = 2f;
         public float reachedPointDistance = 0.1f;
        [HideInInspector] public int currentPointIndex = 0;
        [HideInInspector] public bool isWaiting = false;

        [Space(5)]
        [Header("Chasing")]
        [SerializeField] public float stoppingDistance = 1f;
        [HideInInspector] public bool canAttack = false;

        protected bool isDead = false;
        protected BaseStateAI currState;
        protected List<BaseStateAI> createdStates;
        public Transform target;
        public Rigidbody2D rb { get; protected set; }
        public CapsuleCollider2D capsule { get; protected set; }
        public EnemyAnimationController AnimController { get; protected set; }
        [Header("StateOverride")]
        [SerializeField] public bool loopOverrideState = false;
        [SerializeField] protected AIState overrideAIState;

        [Header("Audio")]
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip attackSound;
        [SerializeField] protected AudioClip deathSound;

        [HideInInspector] public float currentAttackTime = 0f;
        [HideInInspector] public float lastAttackTime = 0f;

        [Space(5)]
        [Header("Friendly NPC")]
        [SerializeField] protected bool isFriendlyNpc = false;
        [SerializeField] protected Transform designatedEnemyTarget;
        [SerializeField] protected Transform fallbackTarget;
        [SerializeField] protected float fallbackActionDistance = 0.3f;
        [SerializeField] protected bool invokeFallbackActionOnlyOnce = true;
        [SerializeField] protected UnityEvent onFallbackTargetReached;

        private static float SIGHT_OFFSET = 0.1f;
        private static float TARGET_OFFSET = 0.1f;
        protected bool isInitialized = false;
        private bool fallbackActionInvoked = false;

        protected virtual void Init()
        {
            if (isInitialized) return;

            AnimController = GetComponent<EnemyAnimationController>();
            rb = GetComponent<Rigidbody2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            createdStates = new List<BaseStateAI>();
            isInitialized = true;
        }

        public void Initialize()
        {
            Init();
        }

        public virtual void ActivateSpecial( bool isActive) { }

        public IEnumerator DetectionRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkFrequency);
                DetectTarget();

                if (canSeeTarget)
                {
                    CanAttack();
                }
            }
        }

        private void CanAttack()
        {
            canAttack = false;

            if (target == null)
                return;

            Vector2 direction = target.position - transform.position;
            float distance = direction.magnitude;

            if (distance < stoppingDistance)
            {
                canAttack = true;
            }
        }

        private void DetectTarget()
        {
            // Сбрасываем состояние перед проверкой
            canSeeTarget = false;
            target = null;

            if (TryUseDesignatedEnemyTarget())
            {
                return;
            }

            Vector2 sightPoint = new Vector2(transform.position.x, transform.position.y - SIGHT_OFFSET);

            // Ищем все цели в радиусе через SphereCast
            Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(sightPoint, sightRange, targetMask);

            foreach (Collider2D targetCollider in targetsInViewRadius)
            {
                if (!canSeeTarget && targetCollider.TryGetComponent(out PlayerController player))
                {
                    if (player.CurrentShape == PlayerController.Shape.Rat)
                    {
                        continue;
                    }
                }

                Transform potentialTarget = targetCollider.transform;
                Vector2 potentialTargetPos = new Vector2(potentialTarget.position.x, potentialTarget.position.y + TARGET_OFFSET);
                Vector2 directionToTarget = (potentialTargetPos - sightPoint).normalized;

                Vector2 sightDirection = AnimController.GetSpriteRenderer().flipX ? -transform.right : transform.right;

                // Проверяем, находится ли цель в угле обзора
                if (Vector2.Angle(sightDirection, directionToTarget) < sightAngle / 2)
                {
                    float distanceToTarget = Vector2.Distance(sightPoint, potentialTargetPos);

                    sightPoint = new Vector2(transform.position.x, transform.position.y + SIGHT_OFFSET);

                    // Делаем Raycast для проверки препятствий
                    RaycastHit2D hit = Physics2D.Raycast(sightPoint, directionToTarget, distanceToTarget, obstacleMask);

                    // Если не попали в препятствие - цель видна
                    if (hit.collider == null)
                    {
                        target = potentialTarget;
                        canSeeTarget = true;

                        break; // Выходим из цикла после обнаружения первой видимой цели
                    }
                }
            }

            if (!canSeeTarget && isFriendlyNpc && fallbackTarget != null)
            {
                target = fallbackTarget;
            }
        }

        protected Transform GetCombatTarget()
        {
            if (designatedEnemyTarget != null)
                return designatedEnemyTarget;

            if (canSeeTarget && target != null && target != fallbackTarget)
                return target;

            return null;
        }

        protected Transform GetFallbackTarget()
        {
            return fallbackTarget;
        }

        protected bool TryInvokeFallbackAction()
        {
            if (fallbackTarget == null)
                return false;

            float distance = Vector2.Distance(transform.position, fallbackTarget.position);
            if (distance > fallbackActionDistance)
                return false;

            if (invokeFallbackActionOnlyOnce && fallbackActionInvoked)
                return true;

            onFallbackTargetReached?.Invoke();
            fallbackActionInvoked = true;
            return true;
        }

        public void ResetFallbackAction()
        {
            fallbackActionInvoked = false;
        }

        public void SetDesignatedEnemyTarget(Transform enemyTarget)
        {
            designatedEnemyTarget = enemyTarget;
        }

        public void SetFallbackTarget(Transform moveTarget)
        {
            fallbackTarget = moveTarget;
            fallbackActionInvoked = false;
        }

        private bool TryUseDesignatedEnemyTarget()
        {
            if (designatedEnemyTarget == null)
                return false;

            target = designatedEnemyTarget;
            canSeeTarget = true;
            return true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((deathMask.value & ( 1 << other.gameObject.layer)) != 0)
            {
                if (!isDead)
                {
                    isDead = true;
                    AnimController.SetAnimatorTrigger(AnimationController.IS_DEAD_S);
                }
            }
        }

        #region State switch and creation

        public void ChangeState<T>() where T : BaseStateAI, new()
        {
            BaseStateAI previousState = null;

            if (currState != null)
            {
                if (typeof(T) == currState.GetType()) return;

                previousState = currState;
                currState.ExitState();
            }

            // creates and enters the new state
            currState = CreateState<T>();
            currState.baseEnemy = this;
            currState.animatonController = AnimController;
            currState.prevState = previousState;
            currState.EnterState();
        }

        public BaseStateAI GetStateAI(bool getPrevState)
        {
            if (getPrevState)
                return currState.prevState;

            return currState;
        }

        private BaseStateAI CreateState<T>() where T : BaseStateAI, new()
        {
            //check if state has been created
            for (int i = 0; i < createdStates.Count; i++)
            {
                if (typeof(T) == createdStates[i].GetType())
                    return createdStates[i];
            }

            return new T();
        }
        #endregion
    }
}
