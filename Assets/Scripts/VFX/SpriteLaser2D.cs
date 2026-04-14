using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace VFX
{
    public class SpriteLaser2D : MonoBehaviour
    {
        [Header("Anchors")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private bool useTransformRight = true;
        [SerializeField] private Vector2 customDirection = Vector2.right;

        [Header("Laser Segments")]
        [SerializeField] private GameObject startSegmentPrefab;
        [SerializeField] private GameObject middleSegmentPrefab;
        [SerializeField] private GameObject endSegmentPrefab;
        [SerializeField] private float segmentSpacing = 0.35f;
        [SerializeField] private float maxDistance = 8f;
        [SerializeField] private float startOffset = 0f;
        [SerializeField] private float endOffset = 0f;

        [Header("Collision")]
        [SerializeField] private LayerMask wallMask;

        [Header("Update")]
        [SerializeField] private bool rebuildEveryFrame = true;
        [SerializeField] private Opener opener;

        [Header("Periodic")]
        [SerializeField] private bool usePeriodicToggle = false;
        [SerializeField] private float toggleInterval = 1f;
        [SerializeField] private bool startEnabled = true;

        private readonly List<GameObject> middleSegments = new();
        private GameObject startSegment;
        private GameObject endSegment;
        private bool isLaserEnabled;
        private float toggleTimer;

        private void OnEnable()
        {
            isLaserEnabled = usePeriodicToggle ? startEnabled : true;
            toggleTimer = 0f;

            if (isLaserEnabled)
            {
                Rebuild();
            }
            else
            {
                DisableRay();
            }
        }

        private void Update()
        {
            bool blockedByOpener = opener != null && opener.isActive;
            if (blockedByOpener)
            {
                if (isLaserEnabled)
                {
                    DisableRay();
                }

                return;
            }

            if (usePeriodicToggle)
            {
                UpdatePeriodicToggle();
            }

            if (isLaserEnabled && rebuildEveryFrame)
            {
                Rebuild();
            }
        }

        public void DisableRay()
        {
            foreach (GameObject segment in middleSegments)
            {
                segment.SetActive(false);
            }

            if (startSegment != null)
            {
                startSegment.SetActive(false);
            }

            if (endSegment != null)
            {
                endSegment.SetActive(false);
            }

            isLaserEnabled = false;
        }

        public void Rebuild()
        {
            Vector3 origin = startPoint != null ? startPoint.position : transform.position;
            Vector2 direction = GetDirection();

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, wallMask);
            float laserLength = hit.collider != null ? hit.distance : maxDistance;
            float safeLength = Mathf.Max(0f, laserLength);

            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

            Vector3 startPosition = origin + (Vector3)(direction * startOffset);
            Vector3 endPosition = origin + (Vector3)(direction * Mathf.Max(startOffset, safeLength - endOffset));

            PlaceOrCreate(ref startSegment, startSegmentPrefab, startPosition, rotation);
            PlaceOrCreate(ref endSegment, endSegmentPrefab, endPosition, rotation);

            int middleCount = GetMiddleCount(safeLength);
            EnsureMiddleCount(middleCount);

            float currentDistance = startOffset + segmentSpacing;
            for (int i = 0; i < middleCount; i++)
            {
                Vector3 segmentPosition = origin + (Vector3)(direction * currentDistance);
                middleSegments[i].transform.SetPositionAndRotation(segmentPosition, rotation);
                middleSegments[i].SetActive(true);
                currentDistance += segmentSpacing;
            }
        }

        private Vector2 GetDirection()
        {
            if (useTransformRight)
            {
                Transform directionSource = startPoint != null ? startPoint : transform;
                return directionSource.right.normalized;
            }

            return customDirection.normalized;
        }

        private int GetMiddleCount(float length)
        {
            float middleStart = startOffset + segmentSpacing;
            float middleEnd = length - endOffset;

            if (middleEnd <= middleStart || segmentSpacing <= 0f)
            {
                return 0;
            }

            return Mathf.FloorToInt((middleEnd - middleStart) / segmentSpacing) + 1;
        }

        private void EnsureMiddleCount(int requiredCount)
        {
            if (middleSegmentPrefab == null)
            {
                for (int i = 0; i < middleSegments.Count; i++)
                {
                    if (middleSegments[i] != null)
                    {
                        middleSegments[i].SetActive(false);
                    }
                }

                return;
            }

            while (middleSegments.Count < requiredCount)
            {
                GameObject segment = Instantiate(middleSegmentPrefab, transform);
                middleSegments.Add(segment);
            }

            for (int i = 0; i < middleSegments.Count; i++)
            {
                middleSegments[i].SetActive(i < requiredCount);
            }
        }

        private void PlaceOrCreate(ref GameObject segmentObject, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                if (segmentObject != null)
                {
                    segmentObject.SetActive(false);
                }

                return;
            }

            if (segmentObject == null)
            {
                segmentObject = Instantiate(prefab, transform);
            }

            segmentObject.transform.SetPositionAndRotation(position, rotation);
            segmentObject.SetActive(true);
        }

        private void UpdatePeriodicToggle()
        {
            if (toggleInterval <= 0f)
            {
                return;
            }

            toggleTimer += Time.deltaTime;

            if (toggleTimer < toggleInterval)
            {
                return;
            }

            int toggleCount = Mathf.FloorToInt(toggleTimer / toggleInterval);
            toggleTimer -= toggleCount * toggleInterval;

            // If count is even, state stays the same; if odd, state is toggled.
            if ((toggleCount & 1) == 0)
            {
                return;
            }

            isLaserEnabled = !isLaserEnabled;

            if (isLaserEnabled)
            {
                Rebuild();
            }
            else
            {
                DisableRay();
            }
        }

        private void OnDisable()
        {
            if (startSegment != null)
            {
                startSegment.SetActive(false);
            }

            if (endSegment != null)
            {
                endSegment.SetActive(false);
            }

            for (int i = 0; i < middleSegments.Count; i++)
            {
                if (middleSegments[i] != null)
                {
                    middleSegments[i].SetActive(false);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = startPoint != null ? startPoint.position : transform.position;
            Vector2 direction = GetDirection();
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + (Vector3)(direction * maxDistance));
        }
    }
}
