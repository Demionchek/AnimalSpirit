using Features.AI.Infrastructure;

namespace Features.AI.Application
{
    public sealed class EnemyStateMachine
    {
        private IEnemyState _current;

        public void SetState(IEnemyState state)
        {
            _current?.Exit();

            _current = state;

            _current.Enter();
        }

        public void Tick()
        {
            _current?.Tick();
        }

        public void FixedTick()
        {
            _current?.FixedTick();
        }
    }
}