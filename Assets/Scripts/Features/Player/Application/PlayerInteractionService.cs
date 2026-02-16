using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using MessagePipe;
using UnityEngine;

namespace Features.Player.Application
{
    public sealed class PlayerInteractionService
    {
        private readonly PlayerModel _model;
        private readonly GameSettings _settings;
        private readonly IPlayerPhysicsPort _physics;
        private readonly IPublisher<PlayerBarked> _barkPub;

        private float _interactTimer;

        public PlayerInteractionService(
            PlayerModel model,
            GameSettings settings,
            IPlayerPhysicsPort physics,
            IPublisher<PlayerBarked> barkPub)
        {
            _model = model;
            _settings = settings;
            _physics = physics;
            _barkPub = barkPub;
        }

        public void Tick()
        {
            if (_model.IsDead) return;
            _interactTimer -= Time.deltaTime;
        }

        public void TryBark()
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
    }

}