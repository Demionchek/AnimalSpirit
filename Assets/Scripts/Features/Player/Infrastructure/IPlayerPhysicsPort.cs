using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Features.Player.Infrastructure
{
    public interface IPlayerPhysicsPort
    {
        bool HasWall(float direction, float distance);
        bool HasSpaceAbove();
        IReadOnlyList<IInteractable> OverlapInteractables(float radius);

    }
}