using Animations;
using UnityEngine;

namespace AI
{
    public class AttackStateAI : BaseStateAI
    {
        public override void EnterState()
        {
            animatonController.SetAnimatorTrigger(AnimationController.ATTACK_S);
            animatonController.isAttacking = true;
            Vector2 dir = baseEnemy.transform.position - baseEnemy.target.transform.position;
            animatonController.GetSpriteRenderer().flipX = dir.x < 0;
        }
        public override void ExitState() { }

        public override void StateUpdate()
        {
            if (!animatonController.isAttacking)
            {
                baseEnemy.ChangeState<IdleStateAI>();
            }
        }
        public override void StateFixedUpdate() { }
    }
}