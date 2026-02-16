using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using Interfaces;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using IInitializable = Unity.VisualScripting.IInitializable;

namespace Features.Player.Application
{
    public sealed class PlayerService :
        IInitializable,
        ITickable,
        IFixedTickable,
        IDisposable
    {
        private readonly PlayerModel _model;
        private readonly GameSettings _settings;
        private readonly IPlayerPhysicsPort _physics;
        private readonly IPlayerViewPort _view;

        private readonly IPublisher<PlayerShapeChanged> _shapePub;
        private readonly IPublisher<PlayerBarked> _barkPub;
        private readonly IPublisher<PlayerDied> _deathPub;
        private readonly IPublisher<PlayerRevived> _revivePub;

        private Vector2 _currentInput;
        private Vector2 _effectorVelocity;
        private float _interactTimer;

        private IDisposable _moveSub;
        private IDisposable _jumpSub;
        private IDisposable _barkSub;
        private IDisposable _shapeSub;

        public PlayerService(
            PlayerModel model,
            GameSettings settings,
            IPlayerPhysicsPort physics,
            IPlayerViewPort view,
            IPublisher<PlayerShapeChanged> shapePub,
            IPublisher<PlayerBarked> barkPub,
            IPublisher<PlayerDied> deathPub,
            IPublisher<PlayerRevived> revivePub)
        {
            _model = model;
            _settings = settings;
            _physics = physics;
            _view = view;

            _shapePub = shapePub;
            _barkPub = barkPub;
            _deathPub = deathPub;
            _revivePub = revivePub;
        }

        [Inject]
        private void Construct(
            ISubscriber<PlayerMoveInput> moveSub,
            ISubscriber<PlayerJumpPressed> jumpSub,
            ISubscriber<PlayerBarkPressed> barkSub,
            ISubscriber<PlayerShapeRequest> shapeSub,
            ISubscriber<ActualVelocityChanged> velocitySub)
        {
            _moveSub = moveSub.Subscribe(e => _currentInput = e.Value);
            _jumpSub = jumpSub.Subscribe(_ => TryJump());
            _barkSub = barkSub.Subscribe(_ => TryBark());
            _shapeSub = shapeSub.Subscribe(e => RequestShapeChange(e.Target));
        }

        public void Initialize()
        {
            _model.SetShape(Shape.Dog);
            ApplyShapeParameters(_model.CurrentShape);
            _shapePub.Publish(new PlayerShapeChanged(_model.CurrentShape));
        }

        public void Tick()
        {
            if (_model.IsDead) return;
            _interactTimer -= Time.deltaTime;
        }

        public void FixedTick()
        {
            if (_model.IsDead) return;

            Vector2 velocity = CalculateDesiredVelocity(_currentInput);
            _view.ApplyVelocity(velocity);
        }

        private Vector2 CalculateDesiredVelocity(Vector2 input)
        {
            float speed = _settings.PlayerMovements.GetSpeed(_model.CurrentShape);

            if (_model.CurrentShape == Shape.Bird)
                return input * speed;

            bool wall = false;

            if (Mathf.Abs(input.x) > 0.1f)
            {
                float dir = Mathf.Sign(input.x);
                float dist = _settings.ShapesColliderSettings
                                      .GetSize(_model.CurrentShape).x * 0.5f;

                wall = _physics.HasWall(dir, dist);
            }

            float horizontal = wall ? 0f : input.x * speed;
            return new Vector2(horizontal + _effectorVelocity.x, _view.CurrentVelocity.y);
        }

        private void TryJump()
        {
            if (!_model.IsGrounded || _model.IsDead ||
                _model.CurrentShape == Shape.Bird)
                return;

            float force = _settings.PlayerMovements
                                   .GetJumpForce(_model.CurrentShape);
            Vector2 forceVector = Vector2.up * force;

            _view.ApplyForce(forceVector);
        }

        private void TryBark()
        {
            if (_model.IsDead || _interactTimer > 0)
                return;

            _interactTimer = 0.5f;

            var interactables =
                _physics.OverlapInteractables(
                    _settings.PlayerSphereCastSettings.radius);

            foreach (var i in interactables)
                i.Interact();

            _barkPub.Publish(
                new PlayerBarked(
                    _model.CurrentShape == Shape.Dog));
        }

        private void RequestShapeChange(Shape target)
        {
            if (_model.IsDead ||
                target == _model.CurrentShape ||
                !_model.IsUnlocked(target))
                return;

            if (!_physics.HasSpaceAbove())
                return;

            _model.SetShape(target);
            ApplyShapeParameters(target);

            _shapePub.Publish(new PlayerShapeChanged(target));
        }

        private void ApplyShapeParameters(Shape shape)
        {
            var size = _settings.ShapesColliderSettings.GetSize(shape);
            var offset = _settings.ShapesColliderSettings.GetOffset(shape);
            var direction = _settings.ShapesColliderSettings.GetDirection(shape);
            var gravity = _settings.PlayerMovements.GetGravity(shape);
            var layer = _settings.ShapesColliderSettings.GetLayer(shape);

            _view.ApplyGravity(gravity);
            _view.ApplyCollider(size, offset, direction);
            _view.ApplyLayer(layer);
        }

        public void SetGrounded(bool grounded)
        {
            if (_model.IsGrounded == grounded)
                return;

            _model.SetGrounded(grounded);
        }

        public void SetEffectorVelocity(float speed)
        {
            _effectorVelocity = new Vector2(speed, 0);
        }

        public void Kill()
        {
            if (_model.IsDead)
                return;

            _model.SetDead(true);
            _deathPub.Publish(new PlayerDied());

            _ = ReviveAsync();
        }

        private async UniTask ReviveAsync()
        {
            await UniTask.Delay(3000);
            _model.SetDead(false);
            _revivePub.Publish(new PlayerRevived());
        }

        public void NotifyCollisionEnter(CollisionTypes type, float? effectorSpeed)
        {
            if (type == CollisionTypes.Enemy ||
                type == CollisionTypes.Bullet)
            {
                Kill();
            }

            if (type == CollisionTypes.Effector && effectorSpeed.HasValue)
                _effectorVelocity = new Vector2(effectorSpeed.Value, 0);
        }

        public void NotifyGroundedState(bool grounded)
        {
            _model.SetGrounded(grounded);
        }

        public void NotifyEffectorExit()
        {
            _effectorVelocity = Vector2.zero;
        }

        public void NotifyTriggerEnter(CollisionTypes type) { }

        public void Dispose()
        {
            _moveSub?.Dispose();
            _jumpSub?.Dispose();
            _barkSub?.Dispose();
            _shapeSub?.Dispose();
        }
    }
}