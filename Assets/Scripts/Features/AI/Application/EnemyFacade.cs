using Features.AI.Application.Attacks;
using Features.AI.Application.States;
using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.AI.Presentation;
using Features.Core.ObjectPool.Infrastructure;
using Features.Core.ObjectPool.Presentation;
using Features.Core.Settings.AI;
using VContainer;
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
        private readonly EnemyAttackFactory _factory;
        private readonly EnemyStateMachine _stateMachine;
        private readonly IObjectResolver _resolver;

        private EnemyStateContext _context;

        public EnemyFacade(
            EnemyModel model,
            EnemyTypeConfig config,
            IEnemyPhysicsPort physics,
            EnemyMovementService movement,
            EnemyPerceptionService perception,
            EnemyStateMachine stateMachine,
            EnemyAnimationService animation,
            EnemyCombatService combat,
            EnemyPatrolService patrol,
            IEnemyPatrolPort patrolPort,
            EnemyAttackFactory factory,
            IObjectResolver resolver)
        {
            _model = model;
            _config = config;
            _physics = physics;
            _movement = movement;
            _perception = perception;
            _stateMachine = stateMachine;
            _animation = animation;
            _combat = combat;
            _patrol = patrol;
            _patrolPort = patrolPort;
            _factory = factory;
            _resolver = resolver;
        }

        public void Initialize()
        {
            _resolver.TryResolve(out EnemyView view);
            _resolver.TryResolve(out IObjectPool<BulletView> pool);

            _context = new EnemyStateContext(
                _model,
                _config,
                _patrol,
                _physics,
                _patrolPort,
                _movement,
                _animation);

            var attack = _factory.Create(
                _config,
                view,
                pool);

            _combat.Initialize(attack);

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
            if(_model.IsDead)
                return;

            _stateMachine.FixedTick();
        }
    }
}
