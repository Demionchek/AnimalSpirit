using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.Settings.AI;

namespace Features.AI.Application.States
{
    public sealed class EnemyStateContext
    {
        public readonly EnemyModel Model;
        public readonly EnemyTypeConfig Config;
        public readonly EnemyPatrolService Patrol;
        public readonly IEnemyPhysicsPort Physics;
        public readonly IEnemyPatrolPort PatrolPort;
        public readonly EnemyMovementService Movement;
        public readonly EnemyAnimationService Animation;
        public readonly IEnemyCombatPort Combat;

        public EnemyStateMachine StateMachine;

        public bool HasPatrol =>
            PatrolPort != null && PatrolPort.Count > 0;

        public EnemyStateContext(
            EnemyModel model,
            EnemyTypeConfig config,
            EnemyPatrolService patrol,
            IEnemyPhysicsPort physics,
            IEnemyPatrolPort patrolPort,
            EnemyMovementService movement,
            EnemyAnimationService animation,
            IEnemyCombatPort combat)
        {
            Model = model;
            Config = config;
            Movement = movement;
            Physics = physics;
            Animation = animation;
            Patrol = patrol;
            PatrolPort = patrolPort;
            Combat = combat;
        }
    }
}
