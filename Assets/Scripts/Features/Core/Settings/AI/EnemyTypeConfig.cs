using UnityEngine;

namespace Features.Core.Settings.AI
{
    public enum EnemyAttackType
    {
        Melee,
        Shooter,
        Flame
    }

    [CreateAssetMenu(menuName = "Game/Enemy/Enemy Type Config")]
    public sealed class EnemyTypeConfig : ScriptableObject
    {
        [Header("Movement")]
        public float speed = 2f;
        public float stoppingDistance = 1f;
        public bool isHorizontal = true;

        [Header("Vision")]
        public float sightRange = 2f;
        public float sightAngle = 90f;
        public float detectionInterval = 0.2f;
        public float sightYOffset = 0.15f;

        [Header("Combat")]
        public float attackDelay = 0.3f;
        public float attackDistance = 0.3f;
        public float attackXOffset = 0.15f;
        public float attackYOffset = 0.2f;

        [Header("Patrol")]
        public float waitTimeAtPoint = 2f;
        public float reachedPointDistance = 0.1f;

        [Header("Layer Masks")]
        public LayerMask targetMask;
        public LayerMask obstacleMask;

        [Header("Enemy Type")]
        public EnemyAttackType attackType;

        [Header("ObjectPool")]
        public bool hasPool = false;
        public GameObject poolPrefab;
    }
}