using Features.Dialogue.Domain;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Features.Dialogue.Presentation
{
    public sealed class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private int _dialogueId;

        private IPublisher<DialogueRequested> _publisher;

        [Inject]
        public void Construct(IPublisher<DialogueRequested> publisher)
        {
            _publisher = publisher;
        }

        public void TriggerDialogue(int dialogueId) => _publisher.Publish(new DialogueRequested(dialogueId));

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((_layerMask.value & (1 << other.gameObject.layer)) == 0)
                return;

            TriggerDialogue(_dialogueId);

            if (TryGetComponent(out Collider2D collider))
            {
                collider.enabled = false;
            };
        }
    }
}