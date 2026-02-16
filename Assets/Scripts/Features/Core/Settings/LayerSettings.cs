using UnityEngine;

namespace Features.Core.Settings
{
    [CreateAssetMenu(fileName = "LayerSettingsSO", menuName = "Game Settings/Layers Settings")]
    public class LayerSettings : ScriptableObject
    {
        public LayerMask PlayerMask;
        public LayerMask GroundMask;
        public LayerMask EnemyMask;
        public LayerMask BulletMask;
    }
}