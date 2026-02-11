using DefaultNamespace;
using UnityEngine;
using VContainer;

namespace Scene
{
    public abstract class TimelineTrigger : MonoBehaviour
    {
        public LayerMask layerMask;
        public int timelineIndex;
        [Inject]
        protected TimelineManager manager;
        protected Collider2D triggerCollider;

        public abstract void Trigger();

        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if ((layerMask.value & (1 << collision.gameObject.layer)) != 0)
            {
                Trigger();
            }
        }
    }
}