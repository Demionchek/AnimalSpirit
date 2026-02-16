using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Features.Player.Domain
{
    public class PlayerModel
    {
        private readonly HashSet<Shape> _unlocked = new() { Shape.Dog };

        public Shape CurrentShape { get; private set; } = Shape.Dog;
        public bool IsDead { get; private set; }
        public bool IsGrounded { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Vector2 EffectorVelocity { get; private set; }

        public IReadOnlyCollection<Shape> UnlockedShapes => _unlocked;

        public void Unlock(Shape shape)
        {
            _unlocked.Add(shape);
        }

        public bool IsUnlocked(Shape shape) => UnlockedShapes.Contains(shape);

        public void SetShape(Shape shape) => CurrentShape = shape;

        public void SetDead(bool dead)
        {
            IsDead = dead;
        }

        public void SetGrounded(bool grounded)
        {
            IsGrounded = grounded;
        }

        public void SetVelocity(Vector2 velocity)
        {
            Velocity = velocity;
        }

        public void SetEffectorVelocity(Vector2 velocity)
        {
            EffectorVelocity = velocity;
        }
    }
}