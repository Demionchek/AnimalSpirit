using UnityEngine;

public class TriggerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector2 moveDirection = Vector2.right;
    [SerializeField] private float moveDistance = 5f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D triggerCollider;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        startPosition = transform.position;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                StopMovement();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            triggerCollider.enabled = false;
            StartMovement();
        }
    }

    void StartMovement()
    {
        if (isMoving) return;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        targetPosition = startPosition + (moveDirection.normalized * moveDistance);

        isMoving = true;
    }

    void StopMovement()
    {
        isMoving = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }


}