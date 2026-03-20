using UnityEngine;

namespace Features.AI.Application.States
{
    public sealed class PatrolState : EnemyState
    {
        public PatrolState Init(EnemyStateContext ctx)
        {
            Initialize(ctx);
            return this;
        }

        public override void Tick()
        {
            if (Ctx.Model.Target != null)
            {
                Ctx.StateMachine.SetState(
                    new ChaseState().Init(Ctx));
                return;
            }

            Vector2 dir =
                Ctx.Patrol.Tick(Time.deltaTime);

            if (dir == Vector2.zero)
            {
                Ctx.Movement.Stop();
                Ctx.Animation.Idle();
                return;
            }

            Ctx.Movement.Move(dir);

            bool flip = dir.x < 0;

            Ctx.Animation.Move(1f, flip);
        }
    }
}