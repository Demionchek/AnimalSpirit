using Player;

namespace Animations
{
    using UnityEngine;

    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class AnimationController : MonoBehaviour
    {
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private ShapeShifterController shapeShifter;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            shapeShifter = GetComponent<ShapeShifterController>();
        }

        private void Update()
        {
            UpdateSpriteDirection();
        }

        public void OnShapeChanged(ShapeShifterController.Shape newShape)
        {
            animator.SetInteger("Shape", (int)newShape);
        }

        public void SetMovementParameters(float speed, bool isGrounded)
        {
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsGrounded", isGrounded);
        }

        private void UpdateSpriteDirection()
        {
            if (shapeShifter.Velocity.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (shapeShifter.Velocity.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }
}