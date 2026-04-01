using UnityEngine;

namespace Features.Core.Settings.Player
{
    [CreateAssetMenu(fileName = "PlayerSphereCastSettings", menuName = "Game Settings/PlayerSphereCastSettings")]
    public class PlayerSphereCastSettingsSO : ScriptableObject
    {
        public float radius;
        public float offsetY;
        public float offsetX;
    }
}