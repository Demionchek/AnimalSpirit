using System.Collections;
using UnityEngine;
using VFX;

namespace AI.Bosses.Machine
{
    public class MachineMovingLaser : MonoBehaviour
    {
        public enum WallOrientation
        {
            Vertical,
            Horizontal
        }

        [SerializeField] private WallOrientation wallOrientation;
        [SerializeField] private SpriteLaser2D laser;
        [SerializeField] private bool resetToStartOnActivate = true;
        [SerializeField] private float delay = 0.3f;
        private Vector3 startPosition;
        private Coroutine moveRoutine;

        public WallOrientation WallOrientationValue { get => wallOrientation; }

        private void Awake()
        {
            startPosition = transform.position;

            if (laser == null)
            {
                laser = GetComponent<SpriteLaser2D>();
            }

            Deactivate();
        }

        public void Activate(float distance, float speed)
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }

            if (resetToStartOnActivate)
            {
                transform.position = startPosition;
            }

            laser?.EnableRay();
            moveRoutine = StartCoroutine(MoveRoutine(distance, speed));
        }

        public void Deactivate()
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }

            laser?.DisableRay();
        }

        private IEnumerator MoveRoutine(float distance, float speed)
        {
            yield return new WaitForSeconds(delay);
            while (laser != null && laser.IsWarmupActive)
            {
                yield return null;
            }

            Vector3 axis = wallOrientation == WallOrientation.Vertical ? Vector3.up : Vector3.right;
            float direction = Random.value < 0.5f ? -1f : 1f;
            Vector3 targetPosition = transform.position + axis * (distance * direction);

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }

            yield return  new WaitForSeconds(delay);
            laser?.DisableRay();

            transform.position = targetPosition;
            moveRoutine = null;
        }
    }
}
