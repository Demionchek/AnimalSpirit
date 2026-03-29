using Features.AI.Application;
using Features.Interactables.Infrastructure;
using UnityEngine;
using VContainer;

namespace Features.AI.Presentation
{
    public sealed class EnemyView : MonoBehaviour, IHittable
    {
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject fireGO;

        private EnemyFacade _facade;

        public GameObject FireGO => fireGO;
        public Transform ShootPoint => shootPoint;

        private Collider2D _collider;
        private Rigidbody2D _rb;

        [Inject]
        public void Construct(EnemyFacade facade)
        {
            _facade = facade;
        }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Hit()
        {
            _facade?.Kill();
            OnKilled();
        }

        public void OnAttackFinished() => _facade.AttackFinished();

        public void PerformAttack() => _facade.PerformAttack();

        private void OnKilled()
        {
            _collider.enabled = false;
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

    }
}
