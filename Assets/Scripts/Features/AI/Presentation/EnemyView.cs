using Features.AI.Application;
using Features.Interactables.Infrastructure;
using UnityEngine;
using VContainer;

namespace Features.AI.Presentation
{
    public sealed class EnemyView : MonoBehaviour, IHittable
    {
        [SerializeField] private Transform shootPointR;
        [SerializeField] private Transform shootPointL;
        [SerializeField] private GameObject fireGO;
        [Header("Audio")]
        [SerializeField] private AudioClip[] _deathClips;
        [SerializeField] private AudioClip[] _attackClips;

        private EnemyFacade _facade;

        public GameObject FireGO => fireGO;
        public Transform ShootPointR => shootPointR;
        public Transform ShootPointL => shootPointL;

        private Collider2D _collider;
        private Rigidbody2D _rb;
        private AudioSource _audioSource;

        [Inject]
        public void Construct(EnemyFacade facade)
        {
            _facade = facade;
        }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _rb = GetComponent<Rigidbody2D>();
            _audioSource = GetComponent<AudioSource>();
        }

        public void Hit()
        {
            _facade?.Kill();
            OnKilled();
        }

        public void OnAttackFinished()
        {
            _facade?.AttackFinished();
            fireGO?.SetActive(false);
        }

        public void PerformAttack()
        {
            _facade.PerformAttack();
            PlayRandomClip(_attackClips);
        }

        private void OnKilled()
        {
            _collider.enabled = false;
            _rb.bodyType = RigidbodyType2D.Kinematic;

            PlayRandomClip(_deathClips);
        }

        private void PlayRandomClip(AudioClip[] clips)
        {
            if (_audioSource == null || clips == null || clips.Length == 0)
                return;

            var clip = clips[Random.Range(0, clips.Length)];
            if (clip != null)
                _audioSource.PlayOneShot(clip);
        }

    }
}
