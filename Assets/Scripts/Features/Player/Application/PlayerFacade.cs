using System;
using Cysharp.Threading.Tasks;
using Features.Checkpoints.Domain;
using Features.Core.Settings;
using Features.Core.Settings.Scene;
using Features.Player.Domain;
using Features.Player.Infrastructure;
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
        private readonly SceneShapeConfig _sceneConfig;

        private IPlayerViewPort _view;
        private bool _isInitialized;

        private bool _controlsEnabled = true;

        private readonly IPublisher<CheckpointRequest> _checkpointRequestPub;
        private readonly IPublisher<PlayerRevived> _playerRevivedPubPub;
        private readonly IPublisher<PlayerDied> _playerDiedPub;
        private readonly IPublisher<PlayerBarked> _playerBarkPub;

        private IDisposable _controlSub;
        private IDisposable _moveSub;
        private IDisposable _jumpSub;
        private IDisposable _barkSub;
        private IDisposable _shapeSub;
        private IDisposable _checkpointSub;

        public PlayerFacade(
            PlayerModel model,
            PlayerLifeService life,
            PlayerShapeService shape,
            SceneShapeConfig sceneConfig,
            PlayerMovementService movement,
            PlayerInteractionService interaction,
            IPublisher<PlayerDied> playerDiedPub,
            IPublisher<PlayerBarked> playerBarkPub,
            IPublisher<PlayerRevived> playerRevivedPub,
            IPublisher<CheckpointRequest> checkpointRequest)
        {
            _life = life;
            _model = model;
            _shape = shape;
            _movement = movement;
            _interaction = interaction;
            _sceneConfig = sceneConfig;
            _playerDiedPub = playerDiedPub;
            _playerBarkPub = playerBarkPub;
            _playerRevivedPubPub = playerRevivedPub;
            _checkpointRequestPub = checkpointRequest;
        }

        public void BindView(IPlayerViewPort view)
        {
            _view = view;

            if (_isInitialized)
                ApplyCurrentShapeToView();
        }

        [Inject]
        private void Construct(
            ISubscriber<PlayerMoveInput> moveSub,
            ISubscriber<PlayerJumpPressed> jumpSub,
            ISubscriber<PlayerBarkPressed> barkSub,
            ISubscriber<PlayerShapeRequest> shapeSub,
            ISubscriber<PlayerControlStateChanged> controlSub,
            ISubscriber<CheckpointCallback> checkpointCallback)
        {
            _moveSub = moveSub.Subscribe(e => SetInput(e.Value));
            _jumpSub = jumpSub.Subscribe(_ => Jump());
            _barkSub = barkSub.Subscribe(_ => Interact());
            _shapeSub = shapeSub.Subscribe(e => ChangeShape(e.Target));
            _checkpointSub = checkpointCallback.Subscribe(e => ApplyRevivePosition(e.position));
            _controlSub = controlSub.Subscribe(e =>
            {
                _controlsEnabled = e.IsEnabled;
            });
        }

        public void Initialize()
        {
            _model.Initialize(
                _sceneConfig.unlockedShapes,
                _sceneConfig.startShape
            );

            foreach (var shape in _sceneConfig.unlockedShapes)
            {
                _shape.UnlockShape(shape);
            }

            _isInitialized = true;
            ApplyCurrentShapeToView();
        }

        public void Tick()
        {
            _interaction.Tick(Time.deltaTime);
        }

        public void FixedTick()
        {
            if (_model.IsDead || _view == null)
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

            if (_view == null)
                return;

            float jumpForce = _movement.GetJumpForce();

            _view.ApplyForce(Vector2.up * jumpForce);
        }

        public void Interact()
        {
            if (!_controlsEnabled)
                return;

            if (_interaction.TryInteract()) _playerBarkPub.Publish(new PlayerBarked(_model.CurrentShape == Shape.Dog));
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
                _playerDiedPub.Publish(new PlayerDied());
                _view.ApplyGravity(1);
                ReviveAsync().Forget();
            }
        }

        private async UniTaskVoid ReviveAsync()
        {
            await UniTask.Delay(3000);
            _life.Revive();
            _view.ApplyGravity(_shape.GetShapeParameters(_model.CurrentShape).Gravity);
            _checkpointRequestPub.Publish(new CheckpointRequest());
            _playerRevivedPubPub.Publish(new PlayerRevived());
        }

        private void ApplyRevivePosition(Vector2 position)
        {
            if (_view != null) _view.ApplyPosition(position);
        }

        private void ApplyShape(ShapeParameters parameters)
        {
            if (_view == null)
                return;

            _view.ApplyCollider(parameters.Size, parameters.Offset, parameters.Direction);
            _view.ApplyGravity(parameters.Gravity);
            _view.ApplyLayer(parameters.Layer);
        }

        private void ApplyCurrentShapeToView()
        {
            if (_view == null)
                return;

            ApplyShape(
                _shape.GetShapeParameters(_model.CurrentShape));
        }

        public void UnlockShape(Shape shape)
        {
            _shape.UnlockShape(shape);
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
            _checkpointSub?.Dispose();
        }
    }
}
