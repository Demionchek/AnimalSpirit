using System;
using Features.Player.Domain;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace DefaultNamespace.Features.Player.Presentation
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class PlayerAnimationView : MonoBehaviour
    {
        private Animator _animator;
        private SpriteRenderer _renderer;
        private IDisposable shapeSub, barkSub, deathSub, reviveSub, velocitySub;
        private float _speed;

        [Inject]
        private void Construct(
            ISubscriber<PlayerShapeChanged> shapeSub,
            ISubscriber<PlayerBarked> barkSub,
            ISubscriber<PlayerDied> deathSub,
            ISubscriber<PlayerRevived> reviveSub,
            ISubscriber<ActualVelocityChanged> velocitySub)
        {
            this.shapeSub = shapeSub.Subscribe(e => _animator.SetInteger("Shape", (int)e.NewShape));
            this.barkSub = barkSub.Subscribe(_ => _animator.SetTrigger("Attack"));
            this.deathSub = deathSub.Subscribe(_ => _animator.SetTrigger("isDead"));
            this.reviveSub = reviveSub.Subscribe(_ => _animator.SetTrigger("Revive"));
            this.velocitySub = velocitySub.Subscribe(e => SetSpeed(e.Velocity.x));
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void SetSpeed(float speed)
        {
            _speed = speed;
            _animator.SetFloat("Speed", Mathf.Abs(_speed));
        }

        private void Update()
        {
            if (_speed > 0.1f) _renderer.flipX = false;
            else if (_speed < -0.1f) _renderer.flipX = true;
        }

        private void OnDestroy()
        {
            shapeSub?.Dispose();
            barkSub?.Dispose();
            deathSub?.Dispose();
            reviveSub?.Dispose();
            velocitySub?.Dispose();
        }
    }
}