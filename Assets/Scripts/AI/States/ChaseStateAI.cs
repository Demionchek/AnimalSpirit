using UnityEngine;

namespace AI.States
{
    public class ChaseStateAI : BaseStateAI
    {
        public override void EnterState()
        {

        }

        public override void StateFixedUpdate()
        {
            if (baseEnemy.target == null)
                return;

            Vector2 direction = baseEnemy.target.position - baseEnemy.transform.position;
            float distance = direction.magnitude;

            animatonController.GetSpriteRenderer().flipX = direction.x > 0;

            if (distance > baseEnemy.stoppingDistance)
            {
                direction.Normalize();
                baseEnemy.rb.velocity = direction * baseEnemy.speed;
                baseEnemy.canAttack = false;
            }
            else
            {
                baseEnemy.rb.velocity = Vector2.zero;
                baseEnemy.canAttack = true;
            }
        }
    }
}