using UnityEngine;

namespace Features.Player.Domain
{
    public readonly struct PlayerMoveInput { public readonly Vector2 Value; public PlayerMoveInput(Vector2 v) => Value = v; }
    public readonly struct PlayerJumpPressed { }
    public readonly struct PlayerBarkPressed { }
    public readonly struct PlayerShapeRequest { public readonly Shape Target; public PlayerShapeRequest(Shape t) => Target = t; }

    public readonly struct PlayerShapeChanged { public readonly Shape NewShape; public PlayerShapeChanged(Shape s) => NewShape = s; }
    public readonly struct PlayerBarked { public readonly bool IsDogForm; public PlayerBarked(bool d) => IsDogForm = d; }
    public readonly struct PlayerDied { }
    public readonly struct PlayerRevived { public readonly Vector2 Position; public PlayerRevived(Vector2 v) => Position = v; }

    public readonly struct PlayerGroundedChanged { public readonly bool IsGrounded; public PlayerGroundedChanged(bool g) => IsGrounded = g; }
    public readonly struct PlayerVelocityChanged { public readonly Vector2 Velocity; public PlayerVelocityChanged(Vector2 v) => Velocity = v; }
    public readonly struct PlayerWallDetected { public readonly bool IsWall; public PlayerWallDetected(bool w) => IsWall = w; }

    public readonly struct DesiredVelocityChanged
    {
        public readonly Vector2 Velocity;
        public DesiredVelocityChanged(Vector2 v) => Velocity = v;
    }

    public readonly struct ActualVelocityChanged
    {
        public readonly Vector2 Velocity;
        public ActualVelocityChanged(Vector2 v) => Velocity = v;
    }

    public readonly struct DesiredGravityScaleChanged
    {
        public readonly float Value;
        public DesiredGravityScaleChanged(float v) => Value = v;
    }

    public readonly struct DesiredColliderChanged
    {
        public readonly Vector2 Size;
        public readonly Vector2 Offset;
        public readonly CapsuleDirection2D Direction;
        public DesiredColliderChanged(Vector2 size, Vector2 offset, CapsuleDirection2D dir)
        {
            Size = size; Offset = offset; Direction = dir;
        }
    }

    public readonly struct DesiredLayerChanged
    {
        public readonly int Layer;
        public DesiredLayerChanged(int l) => Layer = l;
    }
}