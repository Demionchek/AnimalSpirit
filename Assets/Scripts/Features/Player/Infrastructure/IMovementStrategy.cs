using UnityEngine;

namespace Features.Player.Infrastructure
{
    public interface IMovementStrategy
    {
        Vector2 CalculateVelocity(
            Vector2 input,
            Vector2 currentVelocity);
    }
}