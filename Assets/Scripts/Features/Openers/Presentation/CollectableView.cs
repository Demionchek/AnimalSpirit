using Features.Openers.Application;
using Features.Openers.Domain;
using UnityEngine;
using VContainer;

namespace Features.Openers.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class CollectableView : OpenerView
    {
        private CollectableService _service;
        private bool _collected;

        [Inject]
        public void Construct(
            CollectableService service,
            OpenerModel model)
        {
            base.Construct(model);
            _service = service;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected)
                return;

            _collected = true;

            _service.Collect();

            gameObject.SetActive(false);
        }
    }
}