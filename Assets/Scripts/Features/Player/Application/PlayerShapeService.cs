using Features.Core.Settings;
using Features.Core.Settings.Scene;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using MessagePipe;
using Unity.VisualScripting;
using UnityEngine;

namespace Features.Player.Application
{
    public readonly struct ShapeParameters
    {
        public readonly Vector2 Size;
        public readonly Vector2 Offset;
        public readonly CapsuleDirection2D Direction;
        public readonly float Gravity;
        public readonly int Layer;

        public ShapeParameters(
            Vector2 size,
            Vector2 offset,
            CapsuleDirection2D direction,
            float gravity,
            int layer)
        {
            Size = size;
            Offset = offset;
            Direction = direction;
            Gravity = gravity;
            Layer = layer;
        }
    }

    public sealed class PlayerShapeService
    {
        private readonly PlayerModel _model;
        private readonly GameSettings _settings;
        private readonly IPlayerPhysicsPort _physics;

        private readonly IPublisher<PlayerShapeChanged> _shapeChangePub;
        private readonly IPublisher<PlayerShapeUnlocked> _shapeUnlockPub;

        public PlayerShapeService(
            PlayerModel model,
            GameSettings settings,
            IPlayerPhysicsPort physics,
            IPublisher<PlayerShapeChanged> shapeChangePub,
            IPublisher<PlayerShapeUnlocked> shapeUnlockPub)
        {
            _model = model;
            _settings = settings;
            _physics = physics;
            _shapeChangePub = shapeChangePub;
            _shapeUnlockPub = shapeUnlockPub;
        }

        public bool TryChangeShape(
            Shape newShape,
            out ShapeParameters parameters)
        {
            parameters = default;

            if (_model.IsDead)
                return false;

            if (newShape == _model.CurrentShape)
                return false;

            if (!_model.IsUnlocked(newShape))
                return false;

            Vector2 offset =
                _settings.ShapesColliderSettings
                         .GetOffset(_model.CurrentShape);

            float distance =
                _settings.ShapesColliderSettings
                         .GetSize(Shape.Dog).y * 0.5f;

            int groundMask =
                _settings.Layers.GroundMask;

            if (_physics.IsBlockedAbove(offset, distance, groundMask))
                return false;

            _model.SetShape(newShape);

            parameters = BuildShapeParameters(newShape);

            _shapeChangePub.Publish(
                new PlayerShapeChanged(newShape));

            return true;
        }

        public void UnlockShape(Shape shape)
        {
            _model.Unlock(shape);

            _shapeUnlockPub.Publish(
                new PlayerShapeUnlocked(shape));
        }

        public ShapeParameters GetShapeParameters(Shape shape)
        {
            return BuildShapeParameters(shape);
        }

        private ShapeParameters BuildShapeParameters(Shape shape)
        {
            return new ShapeParameters(
                _settings.ShapesColliderSettings.GetSize(shape),
                _settings.ShapesColliderSettings.GetOffset(shape),
                _settings.ShapesColliderSettings.GetDirection(shape),
                _settings.PlayerMovements.GetGravityScale(shape),
                LayerMaskToLayer(
                    _settings.ShapesColliderSettings.GetLayer(shape)
                )
            );
        }

        private int LayerMaskToLayer(LayerMask mask)
        {
            int layer = 0;

            while (mask > 1)
            {
                mask >>= 1;
                layer++;
            }

            return layer;
        }
    }
}