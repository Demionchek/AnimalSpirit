using UnityEngine;
using UnityEngine.Serialization;

namespace AI.Bosses.Machine
{
    public class MachineAnimationController : MonoBehaviour
    {
        [SerializeField] private string laserTriggerName = "attack1";
        [FormerlySerializedAs("rockerTriggerName")]
        [SerializeField] private string rocketTriggerName = "attack2";
        [SerializeField] private string hurtTriggerName = "hurt";
        [SerializeField] private string deathTriggerName = "death";

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void TriggerLaser() => animator.SetTrigger(laserTriggerName);
        public void TriggerRocket() => animator.SetTrigger(rocketTriggerName);
        public void TriggerHurt() => animator.SetTrigger(hurtTriggerName);
        public void TriggerDeath() => animator.SetTrigger(deathTriggerName);
    }
}
