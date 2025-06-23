using System;
using System.Collections;
using System.Collections.Generic;
using Animations;
using Interfaces;
using UnityEngine;
using UnityEngine.XR.WSA;
using Zenject;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IHittable
    {
        [Header("Movement Settings")]
        [SerializeField] private float dogSpeed = 8f;
        [SerializeField] private float ratSpeed = 6f;
        [SerializeField] private float birdFlySpeed = 5f;
        [SerializeField] private float birdAscendSpeed = 10f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float interactDelay = 0.5f;

        [Header("Collider Settings")]
        [SerializeField] private Vector2 dogColliderSize = new Vector2(1f, 2f);
        [SerializeField] private Vector2 dogColliderOffset = new Vector2(0, 0.09f);
        [SerializeField] private Vector2 ratColliderSize = new Vector2(0.8f, 0.6f);
        [SerializeField] private Vector2 ratColliderOffset = new Vector2(0, 0.09f);
        [SerializeField] private Vector2 birdColliderSize = new Vector2(0.8f, 0.5f);
        [SerializeField] private Vector2 birdColliderOffset = new Vector2(0, 0.09f);
        [SerializeField] private Collider2D interactCollider_R;
        [SerializeField] private Collider2D interactCollider_L;

        [Header("Other Settings")]
        [SerializeField] private RandomSoundPlayer randomWoofPlayer;

        [Inject]
        private InputHandler inputHandler;

        [System.Serializable]
        public class ShapeSettings
        {
            public Shape shapeType;
            public bool isUnlocked = false;
        }

        [Header("Shape Settings")]
        [SerializeField] private ShapeSettings[] shapes;
        [SerializeField] private Shape startingShape = Shape.Dog;

        private Rigidbody2D rb;
        private CapsuleCollider2D capsuleCollider;
        private PlayerAnimationController playerAnimationController;
        private bool isDead = false;
        private bool isFlip = false;

        public Transform checkPoint;

        public enum Shape { Dog, Rat, Bird }
        public Shape CurrentShape { get; private set; } = Shape.Dog;

        public bool IsGrounded { get; private set; }
        public float CurrentSpeed { get; private set; }
        public Vector2 Velocity => rb.velocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            playerAnimationController = GetComponent<PlayerAnimationController>();

            // Инициализация начальной формы
            CurrentShape = startingShape;
            ChangeShape(startingShape);
        }

        public void UnlockShape(Shape shapeToUnlock)
        {
            foreach (var shape in shapes)
            {
                if (shape.shapeType == shapeToUnlock)
                {
                    shape.isUnlocked = true;
                    Debug.Log($"Форма {shapeToUnlock} теперь доступна!");
                    return;
                }
            }
            Debug.LogWarning($"Форма {shapeToUnlock} не найдена в настройках!");
        }

        private void Update()
        {
            isFlip = playerAnimationController.IsSpriteFliped();
            if (playerAnimationController.isAttacking || isDead) return;

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
            if (isDead)
            {
                rb.gravityScale = 3;
                return;
            }

            if (playerAnimationController.isAttacking) return;

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
            bool collidersActive = interactCollider_R.enabled || interactCollider_L.enabled;
            if (inputHandler.BarkPressed && !collidersActive)
            {
                playerAnimationController.SetTrigger("Attack");
                playerAnimationController.isAttacking = true;

                StartCoroutine(PlayWoofWithDelay());

                Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
                float radius = 0.2f;
                float distance = 0.02f;
                Vector2 direction = (isFlip ? new Vector2(-0.1f,0) : new Vector2(0.1f,0));
                origin += direction;

                // Выполняем CircleCast
                // Получаем все коллайдеры в радиусе
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, radius);

                if (hitColliders.Length == 0)
                {
                    Debug.Log("В радиусе нет объектов.");
                    return;
                }

                // Перебираем все найденные коллайдеры
                foreach (Collider2D collider in hitColliders)
                {
                    Debug.Log($"Обнаружен объект: {collider.name}");

                    // Проверяем, есть ли у него компонент для взаимодействия
                    IInteractable interactable = collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact(); // Взаимодействуем
                    }
                }
            }
        }

        private IEnumerator PlayWoofWithDelay()
        {
            yield return new WaitForSeconds(0.1f);
            if (randomWoofPlayer != null)
                randomWoofPlayer.PlayRandomSoundNow();
        }

        // Отрисовка радиуса в редакторе
        private void OnDrawGizmos()
        {
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            float distance = 0.02f;
            Vector2 direction = (isFlip ? new Vector2(-0.25f,0) : new Vector2(0.25f,0));
            origin += direction;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, radius);
        }

        private IEnumerator DisableCollidersWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            interactCollider_R.enabled = false;
            interactCollider_L.enabled = false;
        }

        private void HandleShapeChange()
        {
            if (!inputHandler.ChangeShapePressed) return;

            Shape newShape = GetNextAvailableShape();
            ChangeShape(newShape);
        }

        private Shape GetNextAvailableShape()
        {
            int currentIndex = (int)CurrentShape;
            int nextIndex = (currentIndex + 1) % System.Enum.GetValues(typeof(Shape)).Length;

            // Поиск следующей открытой формы
            for (int i = 0; i < System.Enum.GetValues(typeof(Shape)).Length; i++)
            {
                Shape potentialShape = (Shape)nextIndex;
                if (IsShapeUnlocked(potentialShape))
                {
                    return potentialShape;
                }

                nextIndex = (nextIndex + 1) % System.Enum.GetValues(typeof(Shape)).Length;
            }

            return CurrentShape; // Если нет других открытых форм
        }

        private bool IsShapeUnlocked(Shape shape)
        {
            foreach (var shapeSetting in shapes)
            {
                if (shapeSetting.shapeType == shape)
                {
                    return shapeSetting.isUnlocked;
                }
            }
            return false;
        }

        public void ChangeShape(Shape newShape)
        {
            CurrentShape = newShape;

            switch (CurrentShape)
            {
                case Shape.Dog:
                    capsuleCollider.size = dogColliderSize;
                    capsuleCollider.direction = CapsuleDirection2D.Horizontal;
                    capsuleCollider.offset = dogColliderOffset;
                    rb.gravityScale = 2.5f;
                    break;
                case Shape.Rat:
                    capsuleCollider.size = ratColliderSize;
                    capsuleCollider.direction = CapsuleDirection2D.Horizontal;
                    capsuleCollider.offset = ratColliderOffset;
                    rb.gravityScale = 2.5f;
                    break;
                case Shape.Bird:
                    capsuleCollider.size = birdColliderSize;
                    capsuleCollider.direction = CapsuleDirection2D.Vertical;
                    capsuleCollider.offset = birdColliderOffset;
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;
                    break;
            }

            playerAnimationController.OnShapeChanged(CurrentShape);
        }

        private void UpdateAnimationParameters()
        {
            playerAnimationController.SetMovementParameters(
                Mathf.Abs(inputHandler.MoveInput.x),
                IsGrounded
            );
        }

        public void Hit()
        {
            playerAnimationController.SetTrigger(AnimationController.IS_DEAD_S);
            isDead = true;

            StartCoroutine(ReviveCoroutine());
        }

        private IEnumerator ReviveCoroutine()
        {
            yield return new WaitForSeconds(3f);

            isDead = false;
            transform.position = checkPoint.position;
            playerAnimationController.SetTrigger(AnimationController.REVIVE_S);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                other.collider.gameObject.layer == LayerMask.NameToLayer("Bullet"))
            {
                if (!isDead)
                    Hit();
            }
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


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
            {
                if (!isDead)
                    Hit();
            }
        }
    }
}