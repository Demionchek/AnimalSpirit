using UnityEngine;

namespace Features.AI.Presentation
{
    public class EnemyAttackStateMachine : StateMachineBehaviour
    {
        private EnemyView _view;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _view = animator.GetComponent<EnemyView>();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_view != null) _view.OnAttackFinished();
        }
    }
}