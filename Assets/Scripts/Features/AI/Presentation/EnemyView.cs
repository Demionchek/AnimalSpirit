using Features.AI.Application;
using UnityEngine;
using VContainer;

namespace Features.AI.Presentation
{
    public sealed class EnemyView : MonoBehaviour
    {
        private EnemyFacade _facade;

        [SerializeField] private Transform shootPoint;

        public Transform ShootPoint => shootPoint;

        [Inject]
        public void Construct(EnemyFacade facade)
        {
            _facade = facade;
        }

        private void Update()
        {
            _facade.Tick();
        }

        private void FixedUpdate()
        {
            _facade.FixedTick();
        }
    }
}