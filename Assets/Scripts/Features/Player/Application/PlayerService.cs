using Cysharp.Threading.Tasks;
using DefaultNamespace.Features.Player.Presentation;
using Features.Core.Settings;
using Features.Player.Domain;
using Interfaces;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using IInitializable = Unity.VisualScripting.IInitializable;

namespace DefaultNamespace.Features.Player.Application
{
    public class PlayerService : IInitializable, ITickable, IFixedTickable
    {
        private readonly IPublisher<PlayerShapeChanged> _shapePub;
        private readonly IPublisher<PlayerBarked> _barkPub;
        private readonly IPublisher<PlayerDied> _deathPub;
        private readonly IPublisher<PlayerRevived> _revivePub;
        private readonly IPublisher<PlayerGroundedChanged> _groundedPub;
        private readonly IPublisher<DesiredVelocityChanged> _desiredVelPub;
        private readonly IPublisher<DesiredColliderChanged> _desiredColliderPub;
        private readonly IPublisher<DesiredGravityScaleChanged> _desiredGravityPub;
        private readonly IPublisher<DesiredLayerChanged> _desiredLayerPub;

        private readonly GameSettings _gameSettings;
        private readonly PlayerModel _model;
        private readonly CheckPoints _checkPoints;
        private readonly DialogueSystem _dialogueSystem;
        private readonly RandomSoundPlayer _woofPlayer; // если инжектится
        private readonly bool ignoreDamage;

        public Shape CurrentShape { get; private set; } = Shape.Dog;
        public bool IsDead { get; private set; }
        public bool IsGrounded { get; private set; }

        private Vector2 effectorVelocity = Vector2.zero;
        private Vector2 currentVelocity = Vector2.zero;
        private bool isEnoughtSpaceForShape;
        private float interactTimer;

        private readonly LayerMask ratMask, dogMask, birdMask,
            groundMask = LayerMask.GetMask("Ground", "Platform", "Wall");

        [Inject]
        private void Construct(ISubscriber<ActualVelocityChanged> velSub)
        {
            velSub.Subscribe(e => currentVelocity = e.Velocity);
        }

        public PlayerService(
            IPublisher<PlayerShapeChanged> shapePub,
            IPublisher<PlayerBarked> barkPub,
            IPublisher<PlayerDied> deathPub,
            IPublisher<PlayerRevived> revivePub,
            IPublisher<PlayerGroundedChanged> groundedPub,
            IPublisher<DesiredVelocityChanged> desiredVelPub,
            IPublisher<DesiredColliderChanged> desiredColliderPub,
            IPublisher<DesiredGravityScaleChanged> desiredGravityPub,
            IPublisher<DesiredLayerChanged> desiredLayerPub,
            GameSettings gameSettings)
        {
            _shapePub = shapePub; _barkPub = barkPub; _deathPub = deathPub; _revivePub = revivePub;
            _groundedPub = groundedPub; _desiredVelPub = desiredVelPub; _desiredColliderPub = desiredColliderPub;
            _desiredGravityPub = desiredGravityPub; _desiredLayerPub = desiredLayerPub;
            _gameSettings = gameSettings;

            ratMask = 1 << LayerMask.NameToLayer("Rat");
            dogMask = 1 << LayerMask.NameToLayer("Player");
            birdMask = 1 << LayerMask.NameToLayer("Bird");
        }

        public void Initialize()
        {
            CurrentShape = Shape.Dog;
            ApplyShapeParameters(CurrentShape);
            _shapePub.Publish(new PlayerShapeChanged(CurrentShape));
        }

        public void Tick()
        {
            if (IsDead) return;
            interactTimer -= Time.deltaTime;
        }

        public void FixedTick()
        {
            if (IsDead) return;
            Vector2 desiredVel = CalculateDesiredVelocity(_currentInput);
            _desiredVelPub.Publish(new DesiredVelocityChanged(desiredVel));
        }

        public void UnlockShape(Shape shape)
        {
            _model.Unlock(shape);
            Debug.Log($"Форма {shape} разблокирована");
            // Publish событие если нужно
        }

        public void ChangeShape(Shape newShape)
        {
            CurrentShape = newShape;
            _shapePub.Publish(new PlayerShapeChanged(newShape));
            // View отреагирует и изменит collider/gravity/layer
        }

        public void PerformBark(Vector3 origin)
        {
            if (interactTimer > 0 || IsDead) return;
            interactTimer = 0.5f;

            origin = new Vector3(origin.x, 0, origin.z);

            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, _gameSettings.PlayerSphereCastSettings.radius, LayerMask.GetMask("Interactable"));

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IInteractable>(out var interactable))
                    interactable.Interact();
            }
            _barkPub.Publish(new PlayerBarked(CurrentShape == Shape.Dog));
        }

        public void Move(Vector2 input)
        {
            _currentInput = input;
        }

        private Vector2 _currentInput;


        public void TryJump()
        {
            if (!IsGrounded || IsDead || CurrentShape == Shape.Bird) return;
            float force = _gameSettings.PlayerMovements.GetJumpForce(CurrentShape);
            _desiredVelPub.Publish(new DesiredVelocityChanged(currentVelocity + new Vector2(0, force))); // импульс вверх
        }

        public void RequestShapeChange(Shape target, Vector2 origin)
        {
            CheckIfEnoughSpace(origin);
            if (target == CurrentShape || IsDead || !_model.IsUnlocked(target) || !isEnoughtSpaceForShape)
                return;

            CurrentShape = target;
            ApplyShapeParameters(target);
            _shapePub.Publish(new PlayerShapeChanged(target));
        }

        private void CheckIfEnoughSpace(Vector2 origin)
        {

            Vector2 currentOffset = _gameSettings.ShapesColliderSettings.GetOffset(CurrentShape) + origin;
            int layerMask = 1 << LayerMask.NameToLayer("Ground");
            float distance = (_gameSettings.ShapesColliderSettings.GetOffset(CurrentShape).y
                              + origin.y + _gameSettings.ShapesColliderSettings.GetSize(Shape.Dog).y / 2) - currentOffset.y;

            if (Physics2D.Raycast(origin, Vector2.up, distance, layerMask))
            {
                isEnoughtSpaceForShape = false;
                return;
            }
            isEnoughtSpaceForShape = true;
        }

        private void ApplyShapeParameters(Shape shape)
        {
            Vector2 size = _gameSettings.ShapesColliderSettings.GetSize(shape);
            Vector2 offset = _gameSettings.ShapesColliderSettings.GetOffset(shape);
            CapsuleDirection2D colliderDirection = _gameSettings.ShapesColliderSettings.GetDirection(shape);
            float gravity = _gameSettings.PlayerMovements.GetGravity(shape);
            LayerMask layer;
            switch (shape)
            {
                case Shape.Dog:
                    layer = dogMask;
                    break;
                case Shape.Bird:
                    layer = birdMask;
                    break;
                case Shape.Rat:
                    layer = ratMask;
                    break;
                default:
                    layer = dogMask;
                    break;
            }
            _desiredColliderPub.Publish(new DesiredColliderChanged(size, offset, colliderDirection));
            _desiredGravityPub.Publish(new DesiredGravityScaleChanged(gravity));
            _desiredLayerPub.Publish(new DesiredLayerChanged(layer));
        }

        private Vector2 CalculateDesiredVelocity(Vector2 input)
        {
            float speed = _gameSettings.PlayerMovements.GetSpeed(CurrentShape);
            Vector2 vel = Vector2.zero;

            if (CurrentShape == Shape.Bird)
            {
                vel = input * speed;
            }
            else
            {
                // Wall check
                bool wallInFront = false;
                if (Mathf.Abs(input.x) > 0.1f)
                {
                    float dir = Mathf.Sign(input.x);
                    // raycast логика из оригинала — реализуй здесь
                    // wallInFront = ... Raycast ...
                }

                float horizontal = wallInFront ? 0f : input.x * speed;
                vel = new Vector2(horizontal + effectorVelocity.x, currentVelocity.y);
            }

            return vel;
        }

        public void Hit()
        {
            if (IsDead || _dialogueSystem.isDialogRunning || ignoreDamage) return;
            IsDead = true;
            _deathPub.Publish(new PlayerDied());
            ReviveAsync().Forget(); // UniTask для корутины
        }

        public void TryBark()
        {
            if (interactTimer > 0 || IsDead) return;
            interactTimer = 0.5f;

            _barkPub.Publish(new PlayerBarked(CurrentShape == Shape.Dog));
        }

        private async UniTaskVoid ReviveAsync()
        {
            await UniTask.WaitForSeconds(3f);
            Revive();
        }

        public void Revive()
        {
            IsDead = false;
            ChangeShape(CurrentShape);
            _revivePub.Publish(new PlayerRevived(_checkPoints.CurrentCheckPoint.position));
        }

        public void OnCollisionEnter(Collision2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
            {
                if (!IsDead) Hit();
            }

            if (other.gameObject.TryGetComponent<SurfaceEffector2D>(out var effector))
            {
                effectorVelocity = new Vector2(effector.speed, 0);
            }
        }

        public void OnCollisionStay(Collision2D collision)
        {
            bool grounded = false;
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    grounded = true;
                    break;
                }
            }
            if (grounded != IsGrounded)
            {
                IsGrounded = grounded;
                _groundedPub.Publish(new PlayerGroundedChanged(IsGrounded));
            }
        }

        public void OnCollisionExit(Collision2D collision)
        {
            IsGrounded = false;
            _groundedPub.Publish(new PlayerGroundedChanged(IsGrounded));

            if (collision.gameObject.TryGetComponent<SurfaceEffector2D>(out var effector))
            {
                effectorVelocity = Vector2.zero;
            }
        }

        public void OnTriggerEnter(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
            {
                if (!IsDead) Hit();
            }
        }
    }
}