using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using MessagePipe;
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

        public readonly IPublisher<PlayerShapeChanged> _shapeSub;

        public PlayerShapeService(
            PlayerModel model,
            GameSettings settings,
            IPlayerPhysicsPort physics,
            IPublisher<PlayerShapeChanged> shapePub)
        {
            _model = model;
            _settings = settings;
            _physics = physics;
            _shapeSub = shapePub;
        }

        public bool TryChangeShape(Shape target, out ShapeParameters parameters)
        {
            parameters = default;

            if (_model.IsDead)
                return false;

            if (target == _model.CurrentShape)
                return false;

            if (!_model.IsUnlocked(target))
                return false;

            Vector2 offset =
                _settings.ShapesColliderSettings
                         .GetOffset(_model.CurrentShape);

            float distance =
                (_settings.ShapesColliderSettings
                          .GetSize(Shape.Dog).y * 0.5f);

            int groundMask =
                _settings.Layers.GroundMask;

            if (_physics.IsBlockedAbove(offset, distance, groundMask))
                return false;

            _model.SetShape(target);

            _shapeSub.Publish(new PlayerShapeChanged(target));

            parameters = new ShapeParameters(
                _settings.ShapesColliderSettings.GetSize(target),
                _settings.ShapesColliderSettings.GetOffset(target),
                _settings.ShapesColliderSettings.GetDirection(target),
                _settings.PlayerMovements.GetGravityScale(target),
                LayerMaskToLayer(_settings.ShapesColliderSettings.GetLayer(target))
            );

            return true;
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