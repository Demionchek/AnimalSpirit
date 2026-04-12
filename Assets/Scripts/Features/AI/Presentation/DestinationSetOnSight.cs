
using System.Collections;
using Pathfinding;
using UnityEngine;

namespace Features.AI.Presentation
{
    [RequireComponent(typeof(AIDestinationSetter))]
    public class DestinationSetOnSight : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private float sightRange = 10f;
        [SerializeField] [Range(0, 360)] private float sightAngle = 90f;
        [SerializeField] private float checkFrequency = 0.2f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private bool ignoreWalls = false;
        [SerializeField] private bool ignoreOutOfRange = false;
        [SerializeField] private bool respawnAfterEliminated = false;
        [SerializeField] private bool ignorePlayerShape = false;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoColor = Color.yellow;

        private Transform target;
        private bool canSeeTarget = false;
        [SerializeField] private AIDestinationSetter aiDestinationSetter;
        private SpriteRenderer spriteRenderer;
        private Transform homeTransform;

        public bool CanSeeTarget => canSeeTarget;
        public Transform Target => target;

        private void Start()
        {
            if (aiDestinationSetter == null)
                aiDestinationSetter = GetComponent<AIDestinationSetter>();

            spriteRenderer = GetComponent<SpriteRenderer>();
            homeTransform = Instantiate(new GameObject($"home pos {gameObject.name}")).transform;
            homeTransform.position = transform.position;
            aiDestinationSetter.enabled = true;
            StartCoroutine(DetectionRoutine());
        }

        private IEnumerator DetectionRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkFrequency);
                DetectTarget();
            }
        }

        private void ResetOnRevive()
        {
            transform.position = homeTransform.position;
            target = homeTransform;
            aiDestinationSetter.SetTarget(target);
        }

        private void DetectTarget()
        {
            canSeeTarget = false;
            target = null;

            Vector2 sightPoint = new Vector2(transform.position.x, transform.position.y);

            Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(sightPoint, sightRange, targetMask);

            foreach (Collider2D targetCollider in targetsInViewRadius)
            {

                Transform potentialTarget = targetCollider.transform;
                Vector2 potentialTargetPos = new Vector2(potentialTarget.position.x, potentialTarget.position.y);
                Vector2 directionToTarget = (potentialTargetPos - sightPoint).normalized;

                spriteRenderer.flipX = directionToTarget.x < 0;

                Vector2 sightDirection = spriteRenderer.flipX ? -transform.right : transform.right;

                if (Vector2.Angle(sightDirection, directionToTarget) < sightAngle / 2)
                {
                    float distanceToTarget = Vector2.Distance(sightPoint, potentialTargetPos);

                    sightPoint = new Vector2(transform.position.x, transform.position.y);

                    RaycastHit2D hit = Physics2D.Raycast(sightPoint, directionToTarget, distanceToTarget, obstacleMask);

                    if (ignoreWalls || hit.collider == null)
                    {
                        target = potentialTarget;
                        aiDestinationSetter.SetTarget(target);
                        canSeeTarget = true;

                        break;
                    }
                }
            }

            if (!canSeeTarget && !ignoreOutOfRange)
                aiDestinationSetter.SetTarget(homeTransform);
        }
    }
}
