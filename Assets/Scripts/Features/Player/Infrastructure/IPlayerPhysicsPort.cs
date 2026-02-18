using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Features.Player.Infrastructure
{
    public interface IPlayerPhysicsPort
    {
        bool HasWall(float direction, float distance, int mask);
        bool HasSpaceAbove(Vector2 offset, float distance, int mask);
        IReadOnlyList<IInteractable> OverlapInteractables(Vector2 offset,float radius);

    }
}