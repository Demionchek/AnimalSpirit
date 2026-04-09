using Animations;
using Features.Openers.Application;
using Features.Openers.Domain;
using UnityEngine;
using VContainer;

namespace Features.Openers.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Animator))]
    public sealed class ButtonView : OpenerView
    {
        private ButtonService _service;
        private Animator _animator;

        private int _count;

        [Inject]
        public void Construct(
            ButtonService service,
            OpenerModel model)
        {
            base.Construct(model);
            _service = service;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _count++;
            UpdateState();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _count--;
            if (_count < 0) _count = 0;

            UpdateState();
        }

        private void UpdateState()
        {
            bool pressed = _count > 0;

            _service.SetPressed(pressed);

            _animator.SetBool(
                AnimationParams.IS_ACTIVE_S,
                pressed);
        }
    }
}