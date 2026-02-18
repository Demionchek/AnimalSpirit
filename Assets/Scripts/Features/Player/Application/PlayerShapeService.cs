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

        public PlayerShapeService(
            PlayerModel model,
            GameSettings settings,
            IPlayerPhysicsPort physics)
        {
            _model = model;
            _settings = settings;
            _physics = physics;
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

            if (!_physics.HasSpaceAbove(offset, distance, groundMask))
                return false;

            _model.SetShape(target);

            parameters = new ShapeParameters(
                _settings.ShapesColliderSettings.GetSize(target),
                _settings.ShapesColliderSettings.GetOffset(target),
                _settings.ShapesColliderSettings.GetDirection(target),
                _settings.PlayerMovements.GetGravityScale(target),
                _settings.ShapesColliderSettings.GetLayer(target)
            );

            return true;
        }
    }
}