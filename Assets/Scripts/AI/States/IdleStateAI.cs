using AI.States;

namespace AI
{
    public class IdleStateAI : BaseStateAI
    {
        public override void EnterState()
        {
            if (baseEnemy.patrolPoints.Count >= 2)
            {
                baseEnemy.ChangeState<PatrolStateAI>();
            }
        }

        public override void StateUpdate()
        {
            if (baseEnemy.canSeeTarget)
            {
                baseEnemy.ChangeState<AttackStateAI>();
            }
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }
}