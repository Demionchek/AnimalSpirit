namespace Features.AI.Infrastructure
{
    public interface IEnemyAnimationPort
    {
        void SetSpeed(float value);
        void SetAttack();
        void SetDead();
        void SetFlip(bool flip);
    }
}