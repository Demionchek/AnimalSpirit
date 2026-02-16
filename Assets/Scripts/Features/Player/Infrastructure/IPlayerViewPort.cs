using UnityEngine;

namespace Features.Player.Infrastructure
{
    public interface IPlayerViewPort
    {
        Vector2 Position { get; protected set; }
        Vector2 CurrentVelocity { get; protected set; }
        void ApplyVelocity(Vector2 velocity);
        void ApplyForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse);
        void ApplyCollider(Vector2 size, Vector2 offset, CapsuleDirection2D direction);
        void ApplyGravity(float gravity);
        void ApplyLayer(int layer);
    }
}