using System;
using Features.Player.Application;
using Features.Player.Domain;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Features.Player.Presentation
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class PlayerAnimationView : MonoBehaviour
    {
        private Animator _animator;
        private SpriteRenderer _renderer;
        private IDisposable shapeSub, barkSub, deathSub, reviveSub, moveSub, controlSub;
        private float _speed;
        private bool _controlsEnabled = true;

        [Inject]
        private void Construct(
            ISubscriber<PlayerShapeChanged> shapeSub,
            ISubscriber<PlayerBarked> barkSub,
            ISubscriber<PlayerDied> deathSub,
            ISubscriber<PlayerRevived> reviveSub,
            ISubscriber<PlayerMoveInput> moveSub,
            ISubscriber<PlayerControlStateChanged> controlSub)
        {
            this.shapeSub = shapeSub.Subscribe(e => _animator.SetInteger("Shape", (int)e.Shape));
            this.barkSub = barkSub.Subscribe(_ => _animator.SetTrigger("Attack"));
            this.deathSub = deathSub.Subscribe(_ => _animator.SetTrigger("isDead"));
            this.reviveSub = reviveSub.Subscribe(_ => _animator.SetTrigger("Revive"));
            this.moveSub = moveSub.Subscribe(e => SetSpeed(e.Value.x));
            this.controlSub = controlSub.Subscribe(e =>
            {
                _controlsEnabled = e.IsEnabled;
            });
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void SetSpeed(float speed)
        {
            if (!_controlsEnabled)
                speed = 0;

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
            moveSub?.Dispose();
        }
    }
}