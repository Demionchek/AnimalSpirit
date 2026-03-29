using UnityEngine;

namespace Features.AI.Domain
{
    public sealed class EnemyModel
    {
        public int PatrolIndex {get;set;}
        public float WaitTimer  {get;set;}
        public bool IsWaiting  {get;set;}
        public bool IsDead { get; private set; }
        public bool CanSeeTarget { get; set; }
        public bool CanAttack { get; set; }
        public bool IsAttacking { get; set; }
        public Transform Target { get; set; }
        public float LastAttackTime { get; set; }

        public void Kill()
        {
            IsDead = true;
        }
    }
}