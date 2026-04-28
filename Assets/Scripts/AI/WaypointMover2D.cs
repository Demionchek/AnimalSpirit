using UnityEngine;

namespace AI
{
    public class WaypointMover2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 3f;
        [SerializeField] private float reachThreshold = 0.05f;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private int currentIndex = 0;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (waypoints == null || waypoints.Length == 0)
                return;

            Transform target = waypoints[currentIndex];

            Vector2 direction = (target.position - transform.position);

            if (direction.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (direction.x < -0.01f)
                spriteRenderer.flipX = true;

            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, target.position) <= reachThreshold)
            {
                currentIndex++;

                if (currentIndex >= waypoints.Length)
                    currentIndex = 0;
            }
        }
    }
}