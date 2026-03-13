using Features.Interactables.Infrastructure;
using UnityEngine;

namespace Features.Interactables.Application
{
    public sealed class PerformAttackAction : IInteractionAction
    {
        private readonly Animator animator;
        private readonly string triggerName;

        public PerformAttackAction(Animator animator, string triggerName)
        {
            this.animator = animator;
            this.triggerName = triggerName;
        }

        public void Execute()
        {
            animator.SetTrigger(triggerName);
        }
    }
}