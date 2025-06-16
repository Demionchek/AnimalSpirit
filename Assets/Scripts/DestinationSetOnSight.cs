using System.Collections;
using Pathfinding;
using UnityEngine;

namespace DefaultNamespace
{
    public class DestinationSetOnSight : MonoBehaviour
    {

        [Header("Vision Settings")]
        [SerializeField] private float sightRange = 10f; // Радиус обзора
        [SerializeField] [Range(0, 360)] private float sightAngle = 90f; // Угол обзора
        [SerializeField] private float checkFrequency = 0.2f; // Частота проверок в секундах
        [SerializeField] private LayerMask targetMask; // Маска целей
        [SerializeField] private LayerMask obstacleMask; // Маска препятствий

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoColor = Color.yellow;

        private Transform target;
        private bool canSeeTarget = false;
        private AIDestinationSetter aiDestinationSetter;

        public bool CanSeeTarget => canSeeTarget;
        public Transform Target => target;

        private void Start()
        {
            aiDestinationSetter = GetComponent<AIDestinationSetter>();
            aiDestinationSetter.enabled = false;
            StartCoroutine(DetectionRoutine());
        }

        private IEnumerator DetectionRoutine()
        {
            while (!canSeeTarget)
            {
                yield return new WaitForSeconds(checkFrequency);
                DetectTarget();
            }
        }

        private void SetDestinationEnable() => aiDestinationSetter.enabled = true;

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
                        SetDestinationEnable();
                        break; // Выходим из цикла после обнаружения первой видимой цели
                    }
                }
            }
        }
    }
}