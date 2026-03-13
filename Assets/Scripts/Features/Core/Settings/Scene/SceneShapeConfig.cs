using Features.Player.Domain;
using UnityEngine;

namespace Features.Core.Settings.Scene
{
    [CreateAssetMenu(
        fileName = "SceneShapeConfig",
        menuName = "Game/Scene Shape Config")]
    public sealed class SceneShapeConfig : ScriptableObject
    {
        public Shape startShape;

        public Shape[] unlockedShapes;
    }
}