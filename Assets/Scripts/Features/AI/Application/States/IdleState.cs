namespace Features.AI.Application.States
{
    public sealed class IdleState : EnemyState
    {
        public override void Tick()
        {
            if (Ctx.PatrolPort.Count > 1)
            {
                Ctx.StateMachine.SetState(
                    new PatrolState().Init(Ctx));
            }
        }
    }
}