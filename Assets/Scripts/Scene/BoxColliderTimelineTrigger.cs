using UnityEngine;

namespace Scene
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BoxColliderTimelineTrigger : TimelineTrigger
    {
        private void Start()
        {
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.enabled = true;
        }

        public override void Trigger()
        {
            manager.PlayCutscene(timelineIndex);
            triggerCollider.enabled = false;
        }
    }
}