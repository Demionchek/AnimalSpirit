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

            if (distance > baseEnemy.stoppingDistance)
            {
                direction.Normalize();
                baseEnemy.rb.velocity = direction * baseEnemy.speed;
            }
            else
            {
                baseEnemy.rb.velocity = Vector2.zero;
            }
            animatonController.GetSpriteRenderer().flipX = direction.x < 0;
        }
    }
}