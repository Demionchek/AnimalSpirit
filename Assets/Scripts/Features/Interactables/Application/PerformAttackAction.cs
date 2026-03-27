using Features.Interactables.Infrastructure;
using Features.Interactables.Presentation;
using UnityEngine;

namespace Features.Interactables.Application
{
    public sealed class PerformAttackAction : IInteractionAction
    {
        private readonly Animator animator;
        private readonly string triggerName;
        private readonly InteractableCharacterPhysicsPort physicsPort;

        public PerformAttackAction(
            Animator animator,
            string triggerName,
            InteractableCharacterPhysicsPort physicsPort)
        {
            this.animator = animator;
            this.triggerName = triggerName;
            this.physicsPort = physicsPort;
        }

        public void Execute()
        {
            animator.SetTrigger(triggerName);
        }
    }
}
