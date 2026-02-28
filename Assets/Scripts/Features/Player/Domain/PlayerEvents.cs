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

    public readonly struct PlayerControlStateChanged
    {
        public readonly bool IsEnabled;

        public PlayerControlStateChanged(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}