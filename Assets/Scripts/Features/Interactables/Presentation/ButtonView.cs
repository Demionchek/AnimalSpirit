using Animations;
using DefaultNamespace.Features.Interactables.Domain;
using Features.Interactables.Application;
using Features.Interactables.Infrastructure;
using UnityEngine;
using VContainer;

namespace Features.Interactables.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Animator))]
    public sealed class ButtonView :
        MonoBehaviour,
        IOpener
    {
        [SerializeField] private LayerMask interactMask;

        private ButtonService _service;
        private ButtonModel _model;

        private Animator _animator;
        private bool _lastState;

        [Inject]
        public void Construct(
            ButtonService service,
            ButtonModel model)
        {
            _service = service;
            _model = model;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public bool IsActive => _model.IsPressed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsLayerAllowed(other.gameObject.layer))
                return;

            _service.Enter(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsLayerAllowed(other.gameObject.layer))
                return;

            _service.Exit(other);
        }

        private bool IsLayerAllowed(int layer)
        {
            return (interactMask.value & (1 << layer)) != 0;
        }

        private void Update()
        {
            if (_model.IsPressed != _lastState)
            {
                ApplyState(_model.IsPressed);
                _lastState = _model.IsPressed;
            }
        }

        private void ApplyState(bool pressed)
        {
            _animator.SetBool(
                AnimationController.IS_ACTIVE_S,
                pressed);
        }
    }
}