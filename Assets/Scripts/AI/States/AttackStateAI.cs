using Animations;
using UnityEngine;

namespace AI
{
    public class AttackStateAI : BaseStateAI
    {
        public override void EnterState()
        {
            AttackTrigger();
        }
        public override void ExitState() { }

        public override void StateUpdate()
        {
            if (baseEnemy.currentAttackTime > baseEnemy.lastAttackTime + baseEnemy.attackDelay)
            {
                AttackTrigger();
            }

            if (!animatonController.isAttacking && !baseEnemy.canSeeTarget)
            {
                baseEnemy.ChangeState<IdleStateAI>();
            }
        }

        private void AttackTrigger()
        {
            animatonController.SetAnimatorTrigger(AnimationController.ATTACK_S);
            animatonController.isAttacking = true;
            baseEnemy.lastAttackTime = Time.time;
            if (baseEnemy.target != null && baseEnemy.canSeeTarget)
            {
                Vector2 dir = baseEnemy.transform.position - baseEnemy.target.transform.position;
                animatonController.GetSpriteRenderer().flipX = dir.x > 0;
            }
        }

        public override void StateFixedUpdate() { }
    }
}