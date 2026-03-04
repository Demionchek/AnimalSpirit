using Features.Trigger.Application;
using Features.Trigger.Domain;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Features.Trigger.Presentation
{
    public sealed class WorldTriggerView : MonoBehaviour
    {
        [SerializeField] private LayerMask triggerMask;
        [SerializeField] private TriggerConfigSO config;
        [SerializeField] private bool disableSelfAfterTrigger = true;
        [SerializeField] private bool disableOtherAfterTrigger = false;

        private IPublisher<WorldTriggerRequested> _publisher;

        [Inject]
        public void Construct(IPublisher<WorldTriggerRequested> publisher)
        {
            _publisher = publisher;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((triggerMask.value & (1 << other.gameObject.layer)) == 0)
                return;

            if (config == null)
                return;

            _publisher.Publish(new WorldTriggerRequested(config));

            if (disableSelfAfterTrigger)
                gameObject.SetActive(false);

            if (disableOtherAfterTrigger)
                other.gameObject.SetActive(false);
        }
    }
}