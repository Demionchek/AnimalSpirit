namespace Features.AI.Infrastructure
{
    public interface IEnemyState
    {
        void Enter();
        void Exit();
        void Tick();
        void FixedTick();
    }
}