using Animations;
using Features.AI.Infrastructure;
using UnityEngine;

namespace Features.AI.Presentation
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public sealed class UnityEnemyAnimationPort : MonoBehaviour, IEnemyAnimationPort
    {
        private Animator _animator;
        private SpriteRenderer _sprite;

        private readonly string ATTACK_S = "Attack";
        private readonly string SPEED_S = "Speed";
        private readonly string IS_DEAD_S = "isDead";

        public bool IsFlipped
        {
            get
            {
                if (_sprite != null)
                    return _sprite.flipX;

                return false;
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _sprite = GetComponent<SpriteRenderer>();
        }

        public void SetSpeed(float value)
        {
            _animator.SetFloat(SPEED_S, value);
        }

        public void SetAttack()
        {
            _animator.SetTrigger(ATTACK_S);
        }

        public void SetDead()
        {
            _animator.SetTrigger(IS_DEAD_S);
        }

        public void SetFlip(bool flip)
        {
            _sprite.flipX = flip;
        }
    }
}