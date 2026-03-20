using Features.AI.Application.States;
using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.Settings.AI;
using UnityEngine;
using VContainer.Unity;

namespace Features.AI.Application
{
    public sealed class EnemyFacade :
        IInitializable,
        ITickable,
        IFixedTickable
    {
        private readonly EnemyModel _model;
        private readonly EnemyTypeConfig _config;
        private readonly EnemyMovementService _movement;
        private readonly IEnemyPhysicsPort  _physics;
        private readonly EnemyPerceptionService _perception;
        private readonly EnemyAnimationService _animation;
        private readonly EnemyCombatService _combat;
        private readonly EnemyPatrolService _patrol;
        private readonly IEnemyPatrolPort _patrolPort;

        private readonly EnemyStateMachine _stateMachine;

        private EnemyStateContext _context;

        public EnemyFacade(
            EnemyModel model,
            EnemyTypeConfig config,
            IEnemyPhysicsPort  physics,
            EnemyMovementService movement,
            EnemyPerceptionService perception,
            EnemyStateMachine stateMachine,
            EnemyCombatService  combat)
        {
            _model = model;
            _config = config;
            _physics = physics;
            _movement = movement;
            _perception = perception;
            _stateMachine = stateMachine;
            _combat = combat;
        }

        public void Initialize()
        {
            _context = new EnemyStateContext(
                _model,
                _config,
                _patrol,
                _physics,
                _patrolPort,
                _movement,
                _animation);

            _context.StateMachine = _stateMachine;

            _stateMachine.SetState(
                new IdleState().Init(_context));
        }

        public void Tick()
        {
            if(_model.IsDead)
                return;

            _perception.Tick();

            _stateMachine.Tick();
        }

        public void FixedTick()
        {
            _stateMachine.FixedTick();
        }
    }
}