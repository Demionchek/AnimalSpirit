using Animations;

namespace AI
{
    public class AttackStateAI : BaseStateAI
    {
        public override void EnterState()
        {
            animatonController.SetAnimatorTrigger(AnimationController.ATTACK_S);
            animatonController.isAttacking = true;
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