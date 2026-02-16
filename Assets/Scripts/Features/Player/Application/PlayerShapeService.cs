using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using MessagePipe;

namespace Features.Player.Application
{
    public sealed class PlayerShapeService
    {
        private readonly PlayerModel _model;
        private readonly GameSettings _settings;
        private readonly IPlayerPhysicsPort _physics;
        private readonly IPlayerViewPort _view;
        private readonly IPublisher<PlayerShapeChanged> _shapePub;

        public PlayerShapeService(
            PlayerModel model,
            GameSettings settings,
            IPlayerPhysicsPort physics,
            IPlayerViewPort view,
            IPublisher<PlayerShapeChanged> shapePub)
        {
            _model = model;
            _settings = settings;
            _physics = physics;
            _view = view;
            _shapePub = shapePub;
        }

        public void TryChangeShape(Shape target)
        {
            if (_model.IsDead ||
                target == _model.CurrentShape ||
                !_model.IsUnlocked(target))
                return;

            if (!_physics.HasSpaceAbove())
                return;

            _model.SetShape(target);
            ApplyParameters(target);
            _shapePub.Publish(new PlayerShapeChanged(target));
        }

        private void ApplyParameters(Shape shape)
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
    }
}