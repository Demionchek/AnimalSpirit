using Features.AI.Infrastructure;

namespace Features.AI.Application.States
{
    public abstract class EnemyState : IEnemyState
    {
        protected EnemyStateContext Ctx;
        protected EnemyCombatService Combat;

        public void Initialize(EnemyStateContext ctx)
        {
            Ctx = ctx;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick() { }
        public virtual void FixedTick() { }
    }
}