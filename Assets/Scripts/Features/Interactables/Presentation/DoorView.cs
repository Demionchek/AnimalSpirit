using Animations;
using DefaultNamespace.Features.Interactables.Domain;
using Features.Interactables.Application;
using UnityEngine;
using VContainer;

namespace Features.Interactables.Presentation
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    public sealed class DoorView : MonoBehaviour
    {
        [SerializeField] private AudioClip onOpenSound;
        [SerializeField] private AudioClip onCloseSound;

        private DoorService _service;
        private DoorModel _model;

        private Animator _animator;
        private BoxCollider2D _collider;
        private AudioSource _audio;

        private bool _lastState;

        [Inject]
        public void Construct(
            DoorService service,
            DoorModel model)
        {
            _service = service;
            _model = model;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _collider = GetComponent<BoxCollider2D>();
            _audio = GetComponent<AudioSource>();
        }

        private void Update()
        {
            _service.Evaluate();

            if (_model.IsOpen != _lastState)
            {
                ApplyState(_model.IsOpen);
                _lastState = _model.IsOpen;
            }
        }

        private void ApplyState(bool isOpen)
        {
            _animator.SetBool(
                AnimationController.IS_OPEN_S,
                isOpen);

            _collider.enabled = !isOpen;

            if (_audio != null)
            {
                var clip = isOpen
                    ? onOpenSound
                    : onCloseSound;

                if (clip != null)
                    _audio.PlayOneShot(clip);
            }
        }

        public void OpenManual(bool open)
        {
            _service.SetManualState(open);
            ApplyState(open);
        }
    }
}