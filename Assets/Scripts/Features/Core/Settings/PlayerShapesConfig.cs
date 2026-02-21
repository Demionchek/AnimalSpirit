using System.Collections.Generic;
using Features.Player.Domain;
using UnityEngine;

namespace Features.Core.Settings
{
    [CreateAssetMenu(menuName = "Game Settings/Player Shapes Config")]
    public class PlayerShapesConfig : ScriptableObject
    {
        public List<Shape> initiallyUnlocked = new()
        {
            Shape.Dog
        };
    }
}