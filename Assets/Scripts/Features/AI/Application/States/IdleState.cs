namespace Features.AI.Application.States
{
    public sealed class IdleState : EnemyState
    {
        public override void Tick()
        {
            if (Ctx.Model.IsAttacking) return;

            if (Ctx.Model.CanSeeTarget && Ctx.Model.Target != null)
            {
                Ctx.StateMachine.SetState(
                    new ChaseState().Init(Ctx));
                return;
            }

            if (Ctx.PatrolPort.Count > 1)
            {
                Ctx.StateMachine.SetState(
                    new PatrolState().Init(Ctx));
            }
        }
    }
}
