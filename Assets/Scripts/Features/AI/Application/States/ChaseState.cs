using UnityEngine;

namespace Features.AI.Application.States
{
    public sealed class ChaseState : EnemyState
    {
        public override void Tick()
        {
            if (Ctx.Model.Target == null)
            {
                Ctx.StateMachine.SetState(
                    new IdleState().Init(Ctx));
                return;
            }

            Vector2 current = Ctx.Physics.Position;
            Vector2 target = Ctx.Model.Target.position;

            Vector2 dir = (target - current);
            float distance = dir.magnitude;

            if (distance <= Ctx.Config.stoppingDistance)
            {
                Ctx.Model.CanAttack = true;

                Ctx.StateMachine.SetState(
                    new AttackState().Init(Ctx));
                return;
            }

            dir.Normalize();

            Ctx.Movement.Move(dir);

            Ctx.Animation.Move(1f, dir.x < 0);
        }
    }
}