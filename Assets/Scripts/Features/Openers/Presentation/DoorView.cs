using Animations;
using Features.Openers.Application;
using UnityEngine;
using VContainer;

namespace Features.Openers.Presentation
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    public sealed class DoorView : MonoBehaviour
    {
        [SerializeField] private AudioClip onOpenSound;
        [SerializeField] private AudioClip onCloseSound;

        private DoorFacade _facade;

        private Animator _animator;
        private BoxCollider2D _collider;
        private AudioSource _audio;

        private bool _lastState;

        [Inject]
        public void Construct(DoorFacade facade)
        {
            _facade = facade;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _collider = GetComponent<BoxCollider2D>();
            _audio = GetComponent<AudioSource>();
        }

        private void Update()
        {
            bool state = _facade.IsOpen;

            if (state == _lastState)
                return;

            Apply(state);
            _lastState = state;
        }

        public void SetOpen(bool open) => _facade.SetOpen(open);

        private void Apply(bool open)
        {
            _animator.SetBool(
                AnimationParams.IS_OPEN_S,
                open);

            _collider.enabled = !open;

            var clip = open ? onOpenSound : onCloseSound;

            if (clip != null)
                _audio.PlayOneShot(clip);
        }
    }
}