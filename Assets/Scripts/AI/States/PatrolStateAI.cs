using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.States
{
    public class PatrolStateAI : BaseStateAI
    {
        public override void EnterState()
        {
            base.EnterState();
        }

        public override void StateFixedUpdate()
        {
            if (baseEnemy.patrolPoints.Count < 2)
            {
                baseEnemy.ChangeState<IdleStateAI>();
            }

            if(baseEnemy.isWaiting) return;

            MoveToPosition();
        }

        public override void ExitState()
        {
            base.ExitState();
        }

        private void MoveToPosition()
        {
            Vector2 targetPosition = baseEnemy.patrolPoints[baseEnemy.currentPointIndex].position;
            Vector2 moveDirection = (targetPosition - baseEnemy.rb.position).normalized;

            // Двигаемся с помощью Rigidbody
            baseEnemy.rb.velocity = moveDirection * baseEnemy.speed;
            baseEnemy.AnimationController.SetAnimatorFloat("Speed", 1);

            // Проверяем, достигли ли точки
            if (Vector2.Distance(baseEnemy.rb.position, targetPosition) < baseEnemy.reachedPointDistance)
            {
                baseEnemy.StartCoroutine(WaitAtPoint());
            }
        }

        private IEnumerator WaitAtPoint()
        {
            baseEnemy.isWaiting = true;
            baseEnemy.rb.velocity = Vector2.zero;
            baseEnemy.AnimationController.SetAnimatorFloat("Speed", 0);

            yield return new WaitForSeconds(baseEnemy.waitTimeAtPoint);

            GetNextPointIndex();
            baseEnemy.isWaiting = false;
        }

        private void GetNextPointIndex()
        {
            if (baseEnemy.patrolPoints.Count == 0) return;

            if (baseEnemy.patrolPoints.Count == 1)
            {
                baseEnemy.currentPointIndex = 0;
                return;
            }

            // Линейное патрулирование A-B-C-D-A-B-C-D...
            baseEnemy.currentPointIndex = (baseEnemy.currentPointIndex + 1) % baseEnemy.patrolPoints.Count;
        }
    }
}