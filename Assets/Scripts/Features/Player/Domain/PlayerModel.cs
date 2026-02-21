using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Features.Player.Domain
{
    public sealed class PlayerModel
    {
        private readonly HashSet<Shape> _unlocked = new();

        public Shape CurrentShape { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsGrounded { get; private set; }

        public void Initialize(IEnumerable<Shape> unlocked, Shape startShape)
        {
            _unlocked.Clear();

            foreach (var shape in unlocked)
                _unlocked.Add(shape);

            CurrentShape = startShape;
        }

        public bool IsUnlocked(Shape shape) => _unlocked.Contains(shape);

        public void Unlock(Shape shape)
        {
            _unlocked.Add(shape);
        }

        public void SetShape(Shape shape)
        {
            if (!_unlocked.Contains(shape))
                return;

            CurrentShape = shape;
        }

        public void SetDead(bool dead) => IsDead = dead;
        public void SetGrounded(bool grounded) => IsGrounded = grounded;
    }
}