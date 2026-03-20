using Features.AI.Domain;

namespace Features.AI.Infrastructure
{
    public interface IEnemyAttack
    {
        void Execute(EnemyModel model);
    }
}