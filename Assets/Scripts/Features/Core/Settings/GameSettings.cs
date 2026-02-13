using UnityEngine;

namespace Features.Core.Settings
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] public PlayerMovementsSO PlayerMovements;
        [SerializeField] public PlayerSphereCastSettingsSO PlayerSphereCastSettings;
        [SerializeField] public ShapesColliderSettingSO ShapesColliderSettings;
    }
}