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

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _sprite = GetComponent<SpriteRenderer>();
        }

        public void SetSpeed(float value)
        {
            _animator.SetFloat(AnimationController.SPEED_S, value);
        }

        public void SetAttack()
        {
            _animator.SetTrigger(AnimationController.ATTACK_S);
        }

        public void SetDead()
        {
            _animator.SetTrigger(AnimationController.IS_DEAD_S);
        }

        public void SetFlip(bool flip)
        {
            _sprite.flipX = flip;
        }
    }
}