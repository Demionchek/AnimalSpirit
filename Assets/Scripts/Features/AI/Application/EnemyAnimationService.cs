using Features.AI.Infrastructure;

namespace Features.AI.Application
{
    public sealed class EnemyAnimationService
    {
        private readonly IEnemyAnimationPort _port;

        public EnemyAnimationService(IEnemyAnimationPort port)
        {
            _port = port;
        }

        public void Move(float speed, bool flip)
        {
            _port.SetSpeed(speed);
            _port.SetFlip(flip);
        }

        public void Idle()
        {
            _port.SetSpeed(0f);
        }

        public void Attack()
        {
            _port.SetAttack();
        }

        public void Die()
        {
            _port.SetDead();
        }
    }
}