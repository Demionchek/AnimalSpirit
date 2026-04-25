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
        [SerializeField] private float endSegmentOffset = 0f;
        [SerializeField] private float lastSegmentOffset = 0f;

        [Header("Collision")]
        [SerializeField] private LayerMask wallMask;

        [Header("Update")]
        [SerializeField] private bool rebuildEveryFrame = true;
        [SerializeField] private Opener opener;

        [Header("Periodic")]
        [SerializeField] private bool usePeriodicToggle = false;
        [SerializeField] private float toggleInterval = 1f;
        [SerializeField] private bool startEnabled = true;

        [Header("Warmup")]
        [SerializeField] private float warmupDelay = 0f;
        [SerializeField] private GameObject warmupSegmentPrefab;
        [SerializeField] private float warmupSegmentSpacing = 0.35f;

        private readonly List<GameObject> middleSegments = new();
        private readonly List<GameObject> warmupSegments = new();
        private GameObject startSegment;
        private GameObject endSegment;
        private bool isLaserEnabled;
        private bool isWarmupActive;
        private bool wasBlockedByOpener;
        private float toggleTimer;
        private float warmupTimer;

        public bool IsWarmupActive => isWarmupActive;

        private void OnEnable()
        {
            toggleTimer = 0f;
            isWarmupActive = false;
            isLaserEnabled = false;
            wasBlockedByOpener = false;
            warmupTimer = 0f;

            if (usePeriodicToggle || startEnabled)
            {
                EnableRay();
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
                if (isLaserEnabled || isWarmupActive)
                {
                    DisableRay();
                }

                wasBlockedByOpener = true;
                return;
            }

            if (wasBlockedByOpener)
            {
                wasBlockedByOpener = false;
                if (!isLaserEnabled && !isWarmupActive)
                {
                    EnableRay();
                }
            }

            if (usePeriodicToggle)
            {
                UpdatePeriodicToggle();
            }

            if (isWarmupActive)
            {
                UpdateWarmup();
                return;
            }

            if (!isLaserEnabled || blockedByOpener) return;

            if (rebuildEveryFrame)
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

            for (int i = 0; i < warmupSegments.Count; i++)
            {
                if (warmupSegments[i] != null)
                {
                    warmupSegments[i].SetActive(false);
                }
            }

            isLaserEnabled = false;
            isWarmupActive = false;
            warmupTimer = 0f;
        }

        public void EnableRay()
        {
            if (ShouldUseWarmup())
            {
                isLaserEnabled = false;
                isWarmupActive = true;
                warmupTimer = warmupDelay;
                RebuildWarmup();
                return;
            }

            isWarmupActive = false;
            isLaserEnabled = true;
            Rebuild();
        }

        public void SetRayEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                EnableRay();
            }
            else
            {
                DisableRay();
            }
        }

        public void Rebuild()
        {
            if (!TryGetRayData(out Vector3 origin, out Vector2 direction, out float safeLength, out Quaternion rotation))
            {
                DisableWarmupSegments();
                return;
            }

            Vector3 startPosition = origin + (Vector3)(direction * startOffset);
            Vector3 endPosition = origin + (Vector3)(direction * Mathf.Max(startOffset, safeLength - endSegmentOffset));

            PlaceOrCreate(ref startSegment, startSegmentPrefab, startPosition, rotation);
            PlaceOrCreate(ref endSegment, endSegmentPrefab, endPosition, rotation);
            DisableWarmupSegments();

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

        private void RebuildWarmup()
        {
            if (!TryGetRayData(out Vector3 origin, out Vector2 direction, out float safeLength, out Quaternion rotation))
            {
                DisableWarmupSegments();
                return;
            }

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

            float spacing = warmupSegmentSpacing > 0f ? warmupSegmentSpacing : segmentSpacing;
            int warmupCount = GetSegmentCount(safeLength, spacing, 0f);
            EnsureWarmupCount(warmupCount);

            float currentDistance = startOffset;
            for (int i = 0; i < warmupCount; i++)
            {
                Vector3 segmentPosition = origin + (Vector3)(direction * currentDistance);
                warmupSegments[i].transform.SetPositionAndRotation(segmentPosition, rotation);
                warmupSegments[i].SetActive(true);
                currentDistance += spacing;
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
            float middleEnd = length - lastSegmentOffset;

            if (middleEnd <= middleStart || segmentSpacing <= 0f)
            {
                return 0;
            }

            return Mathf.FloorToInt((middleEnd - middleStart) / segmentSpacing) + 1;
        }

        private int GetSegmentCount(float length, float spacing, float endOffset)
        {
            if (spacing <= 0f)
            {
                return 0;
            }

            float fillStart = startOffset;
            float fillEnd = length - endOffset;
            if (fillEnd < fillStart)
            {
                return 0;
            }

            return Mathf.FloorToInt((fillEnd - fillStart) / spacing) + 1;
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

        private void EnsureWarmupCount(int requiredCount)
        {
            if (warmupSegmentPrefab == null)
            {
                DisableWarmupSegments();
                return;
            }

            while (warmupSegments.Count < requiredCount)
            {
                GameObject segment = Instantiate(warmupSegmentPrefab, transform);
                warmupSegments.Add(segment);
            }

            for (int i = 0; i < warmupSegments.Count; i++)
            {
                if (warmupSegments[i] != null)
                {
                    warmupSegments[i].SetActive(i < requiredCount);
                }
            }
        }

        private void DisableWarmupSegments()
        {
            for (int i = 0; i < warmupSegments.Count; i++)
            {
                if (warmupSegments[i] != null)
                {
                    warmupSegments[i].SetActive(false);
                }
            }
        }

        private bool TryGetRayData(out Vector3 origin, out Vector2 direction, out float safeLength, out Quaternion rotation)
        {
            origin = startPoint != null ? startPoint.position : transform.position;
            direction = GetDirection();

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, wallMask);
            float laserLength = hit.collider != null ? hit.distance : maxDistance;
            safeLength = Mathf.Max(0f, laserLength);
            rotation = Quaternion.FromToRotation(Vector3.right, direction);
            return true;
        }

        private bool ShouldUseWarmup()
        {
            return warmupDelay > 0f && warmupSegmentPrefab != null;
        }

        private void UpdateWarmup()
        {
            if (!isWarmupActive)
            {
                return;
            }

            warmupTimer -= Time.deltaTime;

            if (rebuildEveryFrame)
            {
                RebuildWarmup();
            }

            if (warmupTimer > 0f)
            {
                return;
            }

            isWarmupActive = false;
            isLaserEnabled = true;
            Rebuild();
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

            bool currentlyActive = isLaserEnabled || isWarmupActive;
            if (currentlyActive)
            {
                DisableRay();
            }
            else
            {
                EnableRay();
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

            for (int i = 0; i < warmupSegments.Count; i++)
            {
                if (warmupSegments[i] != null)
                {
                    warmupSegments[i].SetActive(false);
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
