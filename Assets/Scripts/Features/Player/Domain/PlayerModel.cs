using System.Collections.Generic;

namespace Features.Player.Domain
{
    public class PlayerModel
    {
        public List<Shape> UnlockedShapes { get; private set; } = new() { Shape.Dog };

        public void Unlock(Shape shape)
        {
            if (!UnlockedShapes.Contains(shape))
                UnlockedShapes.Add(shape);
        }

        public bool IsUnlocked(Shape shape) => UnlockedShapes.Contains(shape);

        public Shape GetNext(Shape current)
        {
            int index = UnlockedShapes.IndexOf(current);
            return UnlockedShapes[(index + 1) % UnlockedShapes.Count];
        }
    }
}