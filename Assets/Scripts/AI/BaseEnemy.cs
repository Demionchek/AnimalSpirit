using System.Collections;
using System.Collections.Generic;
using Animations;
using UnityEngine;

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
        [Space(5)]
        [Header("Spotting")]
        [SerializeField] private float sightRange;
        [SerializeField] private float sightAngle;
        [SerializeField] private float checkFrequency;
        [SerializeField] protected LayerMask targetMask;
        [SerializeField] protected LayerMask obstacleMask;
        [Space(5)]
        [Header("Patroll")]
        [SerializeField] public List<Transform> patrolPoints;
        [SerializeField] public float waitTimeAtPoint;

        public int currentPointIndex = 0;
        public int reachedPointDistance = 0;

        public bool isWaiting = false;
        protected bool isAlive = true;
        protected BaseStateAI currState;
        protected List<BaseStateAI> createdStates;
        protected Transform target;
        protected bool canSeeTarget = false;
        public EnemyAnimationController AnimationController { get; protected set; }
        public Rigidbody2D rb { get; protected set; }

        [SerializeField] public bool loopOverrideState = false;
        [SerializeField] protected AIState overrideAIState;

        protected virtual void Init()
        {
            AnimationController = GetComponent<EnemyAnimationController>();
            createdStates = new List<BaseStateAI>();
        }

        public virtual void ActivateSpecial( bool isActive) { }

        public IEnumerator DetectionRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkFrequency);
                DetectTarget();
            }
        }

        private void DetectTarget()
        {
            // Сбрасываем состояние перед проверкой
            canSeeTarget = false;
            target = null;

            // Ищем все цели в радиусе через SphereCast
            Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, sightRange, targetMask);

            foreach (Collider2D targetCollider in targetsInViewRadius)
            {
                Transform potentialTarget = targetCollider.transform;
                Vector2 directionToTarget = (potentialTarget.position - transform.position).normalized;

                // Проверяем, находится ли цель в угле обзора
                if (Vector2.Angle(transform.right, directionToTarget) < sightAngle / 2)
                {
                    float distanceToTarget = Vector2.Distance(transform.position, potentialTarget.position);

                    // Делаем Raycast для проверки препятствий
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask);

                    // Если не попали в препятствие - цель видна
                    if (hit.collider == null)
                    {
                        target = potentialTarget;
                        canSeeTarget = true;
                        break; // Выходим из цикла после обнаружения первой видимой цели
                    }
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
            currState.animatonController = AnimationController;
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
