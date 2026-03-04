using System;
using Cysharp.Threading.Tasks;
using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using Features.Player.Presentation;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Player.Application
{
    public sealed class PlayerFacade :
        IInitializable,
        ITickable,
        IFixedTickable,
        IDisposable
    {
        private readonly PlayerMovementService _movement;
        private readonly PlayerShapeService _shape;
        private readonly PlayerLifeService _life;
        private readonly PlayerInteractionService _interaction;
        private readonly PlayerModel _model;
        private readonly GameSettings _settings;
        private IPlayerViewPort _view;
        private bool _controlsEnabled = true;

        private IDisposable _controlSub;
        private IDisposable _moveSub;
        private IDisposable _jumpSub;
        private IDisposable _barkSub;
        private IDisposable _shapeSub;

        public PlayerFacade(
            PlayerMovementService movement,
            PlayerShapeService shape,
            PlayerLifeService life,
            PlayerInteractionService interaction,
            PlayerModel model,
            GameSettings settings)
        {
            _movement = movement;
            _shape = shape;
            _life = life;
            _interaction = interaction;
            _model = model;
            _settings = settings;
        }

        // BindView is used to avoid circular dependency between
        // PlayerView and PlayerFacade during DI container build.
        public void BindView(IPlayerViewPort view)
        {
            _view = view;
        }

        [Inject]
        private void Construct(
            ISubscriber<PlayerMoveInput> moveSub,
            ISubscriber<PlayerJumpPressed> jumpSub,
            ISubscriber<PlayerBarkPressed> barkSub,
            ISubscriber<PlayerShapeRequest> shapeSub,
            ISubscriber<PlayerControlStateChanged> controlSub)
        {
            _moveSub = moveSub.Subscribe(e => SetInput(e.Value));
            _jumpSub = jumpSub.Subscribe(_ => Jump());
            _barkSub = barkSub.Subscribe(_ => Interact());
            _shapeSub = shapeSub.Subscribe(e => ChangeShape(e.Target));
            _controlSub = controlSub.Subscribe(e =>
            {
                _controlsEnabled = e.IsEnabled;
            });
        }

        public void Initialize()
        {
            _model.Initialize(
                _settings.ShapeConfig.initiallyUnlocked,
                Shape.Dog
            );

            ApplyShape(_model.CurrentShape);
        }

        public void Tick()
        {
            _interaction.Tick(Time.deltaTime);
        }

        public void FixedTick()
        {
            if (_model.IsDead)
                return;

            Vector2 velocity =
                _movement.CalculateVelocity(
                    _view.CurrentVelocity);

            if (_model.CurrentShape == Shape.Bird)
                _view.ApplyVelocity(velocity);
            else
                _view.ApplyHorizontalVelocity(velocity.x);
        }

        public void SetInput(Vector2 input)
        {
            if (!_controlsEnabled)
                input = Vector2.zero;

            _movement.SetInput(input);
        }

        public void Jump()
        {
            if (!_model.IsGrounded)
                return;

            if (!_controlsEnabled)
                return;

            float jumpForce = _movement.GetJumpForce();

            _view.ApplyForce(Vector2.up * jumpForce);
        }

        public void Interact()
        {
            if (!_controlsEnabled)
                return;

            _interaction.TryInteract();
        }

        public void ChangeShape(Shape target)
        {
            if (!_controlsEnabled)
                return;

            if (_shape.TryChangeShape(target, out var parameters))
            {
                ApplyShape(parameters);
            }
        }

        public void Kill()
        {
            if (!_controlsEnabled)
                return;

            if (_life.TryKill())
            {
                _view.PlayDeath();
                ReviveAsync().Forget();
            }
        }

        private async UniTaskVoid ReviveAsync()
        {
            await UniTask.Delay(3000);
            _life.Revive();
            _view.PlayRevive();
        }

        private void ApplyShape(ShapeParameters parameters)
        {
            _view.ApplyCollider(parameters.Size, parameters.Offset, parameters.Direction);
            _view.ApplyGravity(parameters.Gravity);
            _view.ApplyLayer(parameters.Layer);
        }

        private void ApplyShape(Shape shape)
        {
            ChangeShape(shape);
        }

        public void UnlockShape(Shape shape)
        {
            _model.Unlock(shape);
        }

        public void NotifyCollisionEnter(
            CollisionTypes type,
            float? effectorSpeed)
        {
            switch (type)
            {
                case CollisionTypes.Enemy:
                case CollisionTypes.Bullet:
                    Kill();
                    break;

                case CollisionTypes.Effector:
                    if (effectorSpeed.HasValue)
                        _movement.SetEffectorVelocity(effectorSpeed.Value);
                    break;
            }
        }

        public void NotifyGroundedState(bool grounded)
        {
            _movement.SetGrounded(grounded);
        }

        public void NotifyEffectorExit()
        {
            _movement.ClearEffector();
        }

        public void NotifyTriggerEnter(CollisionTypes type)
        {
            if (type == CollisionTypes.Enemy ||
                type == CollisionTypes.Bullet)
            {
                Kill();
            }
        }

        public void Dispose()
        {
            _moveSub?.Dispose();
            _jumpSub?.Dispose();
            _barkSub?.Dispose();
            _shapeSub?.Dispose();
            _controlSub?.Dispose();
        }
    }
}