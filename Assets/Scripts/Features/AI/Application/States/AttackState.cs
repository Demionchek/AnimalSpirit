using Features.Core.Settings.AI;
using UnityEngine;

namespace Features.AI.Application.States
{
    public sealed class AttackState : EnemyState
    {
        private EnemyCombatService _combat;

        public AttackState Init(
            EnemyStateContext ctx,
            EnemyCombatService combat)
        {
            Initialize(ctx);
            _combat = combat;
            return this;
        }

        public override void Tick()
        {
            if (Ctx.Model.Target == null)
            {
                Ctx.StateMachine.SetState(
                    new IdleState().Init(Ctx));
                return;
            }

            float distance = Vector2.Distance(
                Ctx.Model.Target.position,
                Ctx.Physics.Position);

            if (distance > Ctx.Config.stoppingDistance)
            {
                Ctx.StateMachine.SetState(
                    new ChaseState().Init(Ctx));
                return;
            }

            _combat.TryAttack();
        }
    }
}