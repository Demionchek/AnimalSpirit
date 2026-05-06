using System;
using System.Collections;
using System.Collections.Generic;
using Animations;
using DefaultNamespace;
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
        [SerializeField] private LayerMask damageMask;
        [SerializeField] private RandomSoundPlayer randomWoofPlayer;
        [SerializeField] private bool ignoreDamage = false;

        [Header("Additional Health")]
        [SerializeField] private int maxAdditionalHealth = 1;
        [SerializeField] private bool activateAdditionalHealthOnStart = false;
        [SerializeField] private GameObject additionalHealthVfx;
        [SerializeField] private float additionalHealthInvulnerabilityTime = 1f;
        [SerializeField] private float invulnerabilityBlinkInterval = 0.1f;
        [SerializeField, Range(0f, 1f)] private float invulnerabilityBlinkAlpha = 0.35f;

        [Inject]
        private InputHandler inputHandler;
        [Inject]
        private DialogueSystem dialogueSystem;

        [System.Serializable]
        public class ShapeSettings
        {
            public Shape shapeType;
            public bool isUnlocked = false;
        }

        [Header("Shape Settings")]
        [SerializeField] private List<ShapeSettings> shapes;
        [SerializeField] private Shape startingShape = Shape.Dog;

        private Rigidbody2D rb;
        private CapsuleCollider2D capsuleCollider;
        private BoxCollider2D boxTriggerCollider;
        private SpriteRenderer spriteRenderer;
        private PlayerAnimationController playerAnimationController;
        public bool isDead {get; private set;}
        private bool isFlip = false;
        private bool isEnoughtSpaceForShape = false;
        public bool canMove = true;
        private bool isInvulnerable = false;
        private bool isAdditionalHealthActivated = false;
        private int currentAdditionalHealth = 0;
        private Coroutine invulnerabilityCoroutine;
        private Color spriteColorBeforeInvulnerability;
        private bool isSpriteBlinking = false;

        private LayerMask ratMask;
        private LayerMask dogMask;
        private LayerMask birdMask;

        [Inject]
        private CheckPoints checkPoints;


        public enum Shape { Dog, Rat, Bird }
        public Shape CurrentShape { get; private set; } = Shape.Dog;

        public List<ShapeSettings> Shapes { get => shapes; set => shapes = value; }

        public bool IsGrounded { get; private set; }
        public float CurrentSpeed { get; private set; }
        public Vector2 Velocity => rb.linearVelocity;
        public bool IsAdditionalHealthActivated => isAdditionalHealthActivated;
        public int CurrentAdditionalHealth => currentAdditionalHealth;
        public int MaxAdditionalHealth => Mathf.Max(1, maxAdditionalHealth);

        private Vector2 effectorVelocity = Vector2.zero;

        public event Action OnRevive;
        public event Action OnDeath;
        public event Action<int, int, bool> OnAdditionalHealthChanged;
        public event Action<Shape> OnShapeUnlocked;
        public event Action<Shape> OnShapeChanged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            boxTriggerCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerAnimationController = GetComponent<PlayerAnimationController>();

            // Инициализация начальной формы
            CurrentShape = startingShape;
            ChangeShape(startingShape);

            if (activateAdditionalHealthOnStart)
            {
                ActivateAdditionalHealth();
            }
            else
            {
                RefreshAdditionalHealthState();
            }
        }

        public void UnlockShape(Shape shapeToUnlock)
        {
            foreach (var shape in shapes)
            {
                if (shape.shapeType == shapeToUnlock)
                {
                    shape.isUnlocked = true;
                    OnShapeUnlocked?.Invoke(shapeToUnlock);
                    Debug.Log($"Форма {shapeToUnlock} теперь доступна!");
                    return;
                }
            }
            Debug.LogWarning($"Форма {shapeToUnlock} не найдена в настройках!");
        }
        
        public void SetCanMove(bool canMove) => this.canMove = canMove;

        private void Update()
        {
            isFlip = playerAnimationController.IsSpriteFliped();
            if (playerAnimationController.isAttacking ||
                isDead || dialogueSystem.isDialogRunning) return;

            HandleShapeChange();

            switch (CurrentShape)
            {
                case Shape.Dog:
                    HandleBarking();
                    HandleJump();
                    break;
                case Shape.Rat:
                    HandleBarking();
                    HandleJump();
                    break;
                case Shape.Bird:
                    HandleBarking();
                    break;
            }

            UpdateAnimationParameters();
        }

        private void FixedUpdate()
        {
            if (isDead || dialogueSystem.isDialogRunning)
                return;

            if (playerAnimationController.isAttacking) return;

            CheckIfEnoughSpace();

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

            if (!canMove) CurrentSpeed = 0;

            // Проверяем, есть ли стена перед игроком в направлении движения
            bool isWallInFront = false;
            if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
            {
                float direction = Mathf.Sign(inputHandler.MoveInput.x);
                float rayLength = capsuleCollider.size.x * 0.6f;
                Vector2 rayOrigin = (Vector2)transform.position + capsuleCollider.offset;

                float rayHeight = capsuleCollider.size.y * 0.4f;

                RaycastHit2D hitLower = Physics2D.Raycast(
                    rayOrigin + Vector2.up * (capsuleCollider.offset.y - rayHeight * 1.75f),
                    Vector2.right * direction,
                    rayLength,
                    LayerMask.GetMask("Ground", "Wall", "Platform"));

                isWallInFront = hitLower.collider != null;

                Debug.DrawRay(rayOrigin + Vector2.up * (capsuleCollider.offset.y - rayHeight * 1.75f),
                             Vector2.right * direction * rayLength, Color.red);
            }

            // Если есть стена перед нами, не применяем горизонтальное движение, но оставляем возможность прыжка
            float horizontalVelocity = isWallInFront ? 0 : inputHandler.MoveInput.x * CurrentSpeed;
            rb.linearVelocity = new Vector2(horizontalVelocity, rb.linearVelocity.y) + effectorVelocity;
        }

        private void HandleFlyingMovement()
        {
            float flySpeed = birdFlySpeed;
            float ascendSpeed = birdAscendSpeed;

            if (!canMove)
            {
                flySpeed = 0;
                ascendSpeed = 0;
            }

            rb.linearVelocity = new Vector2(
                inputHandler.MoveInput.x * birdFlySpeed,
                inputHandler.MoveInput.y * birdAscendSpeed
            ) + effectorVelocity;
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
                if (CurrentShape == Shape.Dog)
                {
                    playerAnimationController.SetTrigger("Attack");
                    playerAnimationController.isAttacking = true;
                    StartCoroutine(PlayWoofWithDelay());
                }

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
            if (!isEnoughtSpaceForShape) return;

            if (inputHandler.ShapeDog && CheckAvailableShape(Shape.Dog))
            {
                ChangeShape(Shape.Dog);
            }

            if (inputHandler.ShapeBird && CheckAvailableShape(Shape.Bird))
            {
                ChangeShape(Shape.Bird);
            }

            if (inputHandler.ShapeRat && CheckAvailableShape(Shape.Rat))
            {
                ChangeShape(Shape.Rat);
            }
        }

        //Check if there is nothing above before change shape to avoid stuck
        private void CheckIfEnoughSpace()
        {
            Vector2 origin = ratColliderOffset + (Vector2)transform.position;
            int layerMask = 1 << LayerMask.NameToLayer("Ground");
            float distance = (dogColliderOffset.y + transform.position.y + dogColliderSize.y / 2) - origin.y;

            Debug.DrawRay(origin,
                Vector2.up * distance, Color.red);

            if (Physics2D.Raycast(origin, Vector2.up, distance, layerMask))
            {
                isEnoughtSpaceForShape = false;
                return;
            }
            isEnoughtSpaceForShape = true;
        }

        private bool CheckAvailableShape(Shape shape)
        {
            foreach (var shapeSettings in shapes)
            {
                if(shapeSettings.shapeType == shape && shapeSettings.isUnlocked == true)
                    return true;
            }

            return false;
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

        public void ChangeShapeDog() => ChangeShape(Shape.Dog);

        private void ChangeShape(Shape newShape)
        {
            CurrentShape = newShape;

            switch (CurrentShape)
            {
                case Shape.Dog:
                    capsuleCollider.size = dogColliderSize;
                    capsuleCollider.direction = CapsuleDirection2D.Horizontal;
                    capsuleCollider.offset = dogColliderOffset;
                    boxTriggerCollider.size = dogColliderSize;
                    boxTriggerCollider.offset = dogColliderOffset;
                    rb.gravityScale = 1f;
                    gameObject.layer = LayerMask.NameToLayer("Player");
                    break;
                case Shape.Rat:
                    capsuleCollider.size = ratColliderSize;
                    capsuleCollider.direction = CapsuleDirection2D.Horizontal;
                    capsuleCollider.offset = ratColliderOffset;
                    boxTriggerCollider.size = ratColliderSize;
                    boxTriggerCollider.offset = ratColliderOffset;
                    rb.gravityScale = 1f;
                    gameObject.layer = LayerMask.NameToLayer("Rat");
                    break;
                case Shape.Bird:
                    capsuleCollider.size = birdColliderSize;
                    capsuleCollider.direction = CapsuleDirection2D.Vertical;
                    capsuleCollider.offset = birdColliderOffset;
                    boxTriggerCollider.size = birdColliderSize;
                    boxTriggerCollider.offset = birdColliderOffset;
                    rb.gravityScale = 0f;
                    rb.linearVelocity = Vector2.zero;
                    gameObject.layer = LayerMask.NameToLayer("Bird");
                    break;
            }
            OnShapeChanged?.Invoke(newShape);
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
            if (isDead || dialogueSystem.isDialogRunning || ignoreDamage || isInvulnerable) return;

            if (TryConsumeAdditionalHealth())
            {
                return;
            }

            playerAnimationController.SetTrigger(AnimationController.IS_DEAD_S);
            isDead = true;
            rb.gravityScale = 1;
            OnDeath?.Invoke();

            StartCoroutine(ReviveCoroutine());
        }

        private IEnumerator ReviveCoroutine()
        {
            yield return new WaitForSeconds(3f);

            isDead = false;
            transform.position = checkPoints.CurrentCheckPoint.position;
            playerAnimationController.SetTrigger(AnimationController.REVIVE_S);
            ChangeShape(CurrentShape);
            RestoreAdditionalHealthOnRevive();

            OnRevive?.Invoke();
        }

        public void ActivateAdditionalHealth()
        {
            isAdditionalHealthActivated = true;
            currentAdditionalHealth = MaxAdditionalHealth;
            RefreshAdditionalHealthState();
        }

        public void DeactivateAdditionalHealth()
        {
            isAdditionalHealthActivated = false;
            currentAdditionalHealth = 0;
            StopInvulnerability();
            RefreshAdditionalHealthState();
        }

        private bool TryConsumeAdditionalHealth()
        {
            if (!isAdditionalHealthActivated || currentAdditionalHealth <= 0)
            {
                return false;
            }

            currentAdditionalHealth = Mathf.Max(0, currentAdditionalHealth - 1);
            RefreshAdditionalHealthState();
            StartAdditionalHealthInvulnerability();
            return true;
        }

        private void RestoreAdditionalHealthOnRevive()
        {
            if (!isAdditionalHealthActivated)
            {
                RefreshAdditionalHealthState();
                return;
            }

            currentAdditionalHealth = MaxAdditionalHealth;
            RefreshAdditionalHealthState();
        }

        private void RefreshAdditionalHealthState()
        {
            if (additionalHealthVfx != null)
            {
                additionalHealthVfx.SetActive(isAdditionalHealthActivated && currentAdditionalHealth > 0);
            }

            OnAdditionalHealthChanged?.Invoke(currentAdditionalHealth, MaxAdditionalHealth, isAdditionalHealthActivated);
        }

        private void StartAdditionalHealthInvulnerability()
        {
            StopInvulnerability();

            if (additionalHealthInvulnerabilityTime <= 0f)
            {
                return;
            }

            invulnerabilityCoroutine = StartCoroutine(AdditionalHealthInvulnerabilityCoroutine());
        }

        private IEnumerator AdditionalHealthInvulnerabilityCoroutine()
        {
            isInvulnerable = true;
            spriteColorBeforeInvulnerability = spriteRenderer != null ? spriteRenderer.color : Color.white;
            isSpriteBlinking = spriteRenderer != null;

            float elapsedTime = 0f;
            float blinkTimer = 0f;
            bool isBlinkDimmed = false;

            while (elapsedTime < additionalHealthInvulnerabilityTime)
            {
                elapsedTime += Time.deltaTime;
                blinkTimer += Time.deltaTime;

                if (blinkTimer >= Mathf.Max(0.02f, invulnerabilityBlinkInterval))
                {
                    blinkTimer = 0f;
                    isBlinkDimmed = !isBlinkDimmed;
                    SetInvulnerabilityBlink(isBlinkDimmed);
                }

                yield return null;
            }

            isInvulnerable = false;
            RestoreInvulnerabilityBlink();
            invulnerabilityCoroutine = null;
        }

        private void StopInvulnerability()
        {
            if (invulnerabilityCoroutine != null)
            {
                StopCoroutine(invulnerabilityCoroutine);
                invulnerabilityCoroutine = null;
            }

            isInvulnerable = false;
            RestoreInvulnerabilityBlink();
        }

        private void SetInvulnerabilityBlink(bool isDimmed)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            Color color = spriteColorBeforeInvulnerability;
            color.a = isDimmed ? invulnerabilityBlinkAlpha : spriteColorBeforeInvulnerability.a;
            spriteRenderer.color = color;
        }

        private void RestoreInvulnerabilityBlink()
        {
            if (spriteRenderer != null && isSpriteBlinking)
            {
                spriteRenderer.color = spriteColorBeforeInvulnerability;
            }

            isSpriteBlinking = false;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            bool isInMask = (damageMask.value & (1 << other.gameObject.layer)) != 0;

            if (isInMask)
            {
                if (!isDead)
                    Hit();
            }

            if (other.collider.gameObject.TryGetComponent(out SurfaceEffector2D surfaceEffector))
            {
                effectorVelocity = new Vector2(surfaceEffector.speed, 0);
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

            if (collision.collider.gameObject.TryGetComponent(out SurfaceEffector2D surfaceEffector))
            {
                effectorVelocity = Vector2.zero;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            bool isInMask = (damageMask.value & (1 << other.gameObject.layer)) != 0;

            if (isInMask)
            {
                if (!isDead)
                    Hit();
            }
        }
    }
}
