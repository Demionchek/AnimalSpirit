using System;
using Animations;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CapsuleCollider2D), typeof(Rigidbody2D), typeof(InputHandler))]
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
        [SerializeField] private Vector2 ratColliderSize = new Vector2(0.8f, 0.6f);
        [SerializeField] private Vector2 birdColliderSize = new Vector2(0.8f, 0.5f);

        private Rigidbody2D rb;
        private CapsuleCollider2D capsuleCollider;
        private InputHandler inputHandler;
        private AnimationController animationController;

        public enum Shape { Dog, Rat, Bird }
        public Shape CurrentShape { get; private set; } = Shape.Dog;

        public bool IsGrounded { get; private set; }
        public float CurrentSpeed { get; private set; }
        public Vector2 Velocity => rb.velocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            inputHandler = GetComponent<InputHandler>();
            animationController = GetComponent<AnimationController>();

            ChangeShape(Shape.Dog);
        }

        private void Update()
        {
            HandleShapeChange();

            switch (CurrentShape)
            {
                case Shape.Dog:
                    HandleBarking();
                    HandleJump();
                    break;
                case Shape.Rat:
                    HandleJump();
                    break;
                case Shape.Bird:
                    break;
            }

            UpdateAnimationParameters();
        }

        private void FixedUpdate()
        {
            switch (CurrentShape)
            {
                case Shape.Dog:
                    HandleGroundMovement();
                    break;
                case Shape.Rat:
                    HandleGroundMovement();
                    break;
                case Shape.Bird:
                    HandleFlyingMovement();
                    break;
            }
        }

        private void HandleGroundMovement()
        {
            CurrentSpeed = CurrentShape == Shape.Dog ? dogSpeed : ratSpeed;
            rb.velocity = new Vector2(inputHandler.MoveInput.x * CurrentSpeed, rb.velocity.y);
        }

        private void HandleFlyingMovement()
        {
            rb.velocity = new Vector2(
                inputHandler.MoveInput.x * birdFlySpeed,
                inputHandler.MoveInput.y * birdAscendSpeed
            );
        }

        private void HandleJump()
        {
            if (inputHandler.JumpPressed && IsGrounded)
            {
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            }
        }

        private void HandleBarking()
        {
            if (inputHandler.BarkPressed)
            {
                Debug.Log("Woof! Woof!");
            }
        }

        private void HandleShapeChange()
        {
            if (inputHandler.ChangeShapePressed)
            {
                Shape newShape = CurrentShape switch
                {
                    Shape.Dog => Shape.Rat,
                    Shape.Rat => Shape.Bird,
                    Shape.Bird => Shape.Dog,
                    _ => Shape.Dog
                };

                ChangeShape(newShape);
            }
        }

        public void ChangeShape(Shape newShape)
        {
            CurrentShape = newShape;

            switch (CurrentShape)
            {
                case Shape.Dog:
                    capsuleCollider.size = dogColliderSize;
                    capsuleCollider.direction = CapsuleDirection2D.Horizontal;
                    capsuleCollider.offset = new Vector2(0, 0.11f) ;
                    rb.gravityScale = 3f;
                    break;
                case Shape.Rat:
                    capsuleCollider.size = ratColliderSize;
                    capsuleCollider.direction = CapsuleDirection2D.Horizontal;
                    capsuleCollider.offset = new Vector2(0, 0.07f) ;

                    rb.gravityScale = 3f;
                    break;
                case Shape.Bird:
                    capsuleCollider.size = birdColliderSize;
                    capsuleCollider.direction = CapsuleDirection2D.Vertical;
                    capsuleCollider.offset = new Vector2(0, 0.11f) ;
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;
                    break;
            }

            animationController.OnShapeChanged(CurrentShape);
        }

        private void UpdateAnimationParameters()
        {
            animationController.SetMovementParameters(
                Mathf.Abs(inputHandler.MoveInput.x),
                IsGrounded
            );
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    IsGrounded = true;
                    return;
                }
            }
            IsGrounded = false;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            IsGrounded = false;
        }
    }
}