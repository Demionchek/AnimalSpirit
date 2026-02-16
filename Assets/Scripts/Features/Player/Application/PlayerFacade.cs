using System;
using Features.Player.Domain;
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

        private IDisposable _moveSub;
        private IDisposable _jumpSub;
        private IDisposable _barkSub;
        private IDisposable _shapeSub;

        public PlayerFacade(
            PlayerMovementService movement,
            PlayerShapeService shape,
            PlayerLifeService life,
            PlayerInteractionService interaction,
            PlayerModel model)
        {
            _movement = movement;
            _shape = shape;
            _life = life;
            _interaction = interaction;
            _model = model;
        }

        [Inject]
        private void Construct(
            ISubscriber<PlayerMoveInput> moveSub,
            ISubscriber<PlayerJumpPressed> jumpSub,
            ISubscriber<PlayerBarkPressed> barkSub,
            ISubscriber<PlayerShapeRequest> shapeSub)
        {
            _moveSub = moveSub.Subscribe(e => _movement.SetInput(e.Value));
            _jumpSub = jumpSub.Subscribe(_ => _movement.Jump());
            _barkSub = barkSub.Subscribe(_ => _interaction.TryBark());
            _shapeSub = shapeSub.Subscribe(e => _shape.TryChangeShape(e.Target));
        }

        public void Initialize() { }

        public void Tick()
        {
            _interaction.Tick();
        }

        public void FixedTick()
        {
            _movement.FixedTick();
        }

        public void NotifyCollisionEnter(CollisionTypes type, float? effectorSpeed)
        {
            if (type == CollisionTypes.Enemy ||
                type == CollisionTypes.Bullet)
            {
                _life.Kill();
            }

            if (type == CollisionTypes.Effector && effectorSpeed.HasValue)
                _movement.SetEffectorVelocity(effectorSpeed.Value);
        }

        public void NotifyGroundedState(bool grounded)
        {
            _model.SetGrounded(grounded);
        }

        public void NotifyEffectorExit()
        {
            _movement.SetEffectorVelocity(0);
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