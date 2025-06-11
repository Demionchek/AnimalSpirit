using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(CapsuleCollider2D), typeof(Rigidbody2D))]
public class ShapeShifterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float dogSpeed = 8f;
    [SerializeField] private float ratSpeed = 6f;
    [SerializeField] private float birdFlySpeed = 5f;
    [SerializeField] private float birdAscendSpeed = 10f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Collider Settings")]
    [SerializeField] private Vector2 dogColliderSize = new Vector2(1f, 2f);
    [SerializeField] private Vector2 ratColliderSize = new Vector2(0.6f, 0.8f);
    [SerializeField] private Vector2 birdColliderSize = new Vector2(0.8f, 0.5f);

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction barkAction;
    private InputAction changeShapeAction;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private enum Shape { Dog, Rat, Bird }
    private Shape currentShape = Shape.Dog;

    private bool isGrounded;
    private float moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        barkAction = playerInput.actions["Bark"];
        changeShapeAction = playerInput.actions["ChangeShape"];

        // Initial shape setup
        ChangeShape(Shape.Dog);
    }

    private void OnEnable()
    {
        jumpAction.performed += OnJump;
        barkAction.performed += OnBark;
        changeShapeAction.performed += OnChangeShape;
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJump;
        barkAction.performed -= OnBark;
        changeShapeAction.performed -= OnChangeShape;
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>().x;

        // Handle movement based on shape
        switch (currentShape)
        {
            case Shape.Dog:
            case Shape.Rat:
                GroundMovement();
                break;
            case Shape.Bird:
                FlyingMovement();
                break;
        }

        UpdateAnimations();
    }

    private void GroundMovement()
    {
        float speed = currentShape == Shape.Dog ? dogSpeed : ratSpeed;
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        // Flip sprite based on movement direction
        if (moveInput != 0)
        {
            spriteRenderer.flipX = moveInput < 0;
        }
    }

    private void FlyingMovement()
    {
        Vector2 moveDirection = moveAction.ReadValue<Vector2>();
        rb.velocity = new Vector2(moveDirection.x * birdFlySpeed,
                                 moveDirection.y * birdAscendSpeed);

        // Flip sprite based on movement direction
        if (moveDirection.x != 0)
        {
            spriteRenderer.flipX = moveDirection.x < 0;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && currentShape != Shape.Bird)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    private void OnBark(InputAction.CallbackContext context)
    {
        if (currentShape == Shape.Dog)
        {
            Debug.Log("Woof! Woof!");
            // Play bark sound or animation
        }
    }

    private void OnChangeShape(InputAction.CallbackContext context)
    {
        Shape newShape = currentShape switch
        {
            Shape.Dog => Shape.Rat,
            Shape.Rat => Shape.Bird,
            Shape.Bird => Shape.Dog,
            _ => Shape.Dog
        };

        ChangeShape(newShape);
    }

    private void ChangeShape(Shape newShape)
    {
        currentShape = newShape;

        switch (currentShape)
        {
            case Shape.Dog:
                capsuleCollider.size = dogColliderSize;
                capsuleCollider.direction = CapsuleDirection2D.Vertical;
                rb.gravityScale = 3f;
                break;
            case Shape.Rat:
                capsuleCollider.size = ratColliderSize;
                capsuleCollider.direction = CapsuleDirection2D.Vertical;
                rb.gravityScale = 3f;
                break;
            case Shape.Bird:
                capsuleCollider.size = birdColliderSize;
                capsuleCollider.direction = CapsuleDirection2D.Horizontal;
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
                break;
        }

        Debug.Log("Changed shape to: " + currentShape);
    }

    private void UpdateAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetInteger("Shape", (int)currentShape);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentShape == Shape.Bird) return;

        // Simple ground check
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
}