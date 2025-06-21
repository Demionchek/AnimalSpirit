using System;
using AI.States;
using Interfaces;
using UnityEngine;

namespace AI
{
    public class LoopedFireManEnemy : BaseEnemy, IHittable
    {
        [SerializeField] private GameObject fireGO;

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            ChangeState<AttackStateAI>();
        }

        private void Update()
        {
            bool isAttackState = currState is AttackStateAI;

            if (loopOverrideState && !isAttackState && !isDead)
            {
                ChangeState<AttackStateAI>();
            }

            currState.StateUpdate();
        }

        public override void ActivateSpecial(bool isActive)
        {
            fireGO.SetActive(isActive);
        }

        public void Hit()
        {
            isDead = false;
            ChangeState<DeathState>();
        }
    }
}