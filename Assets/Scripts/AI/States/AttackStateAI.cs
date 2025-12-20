using AI.States;
using Animations;
using UnityEngine;

namespace AI
{
    public class AttackStateAI : BaseStateAI
    {
        private float rayDistance = 0.35f;
        private float rayYOffset = 0.2f;
        private float rayXOffset = 0.3f;
        bool isGrounded = false;


        public override void EnterState()
        {
            AttackTrigger();
            animatonController.SetAnimatorFloat(AnimationController.SPEED_S, 0);
        }
        public override void ExitState() { }

        public override void StateUpdate()
        {
            if (baseEnemy.target != null)
            {
                Vector2 direction = baseEnemy.target.position - baseEnemy.transform.position;
                float distance = direction.magnitude;

                if (distance > baseEnemy.stoppingDistance && isGrounded)
                {
                    direction.Normalize();
                    baseEnemy.rb.velocity = direction * baseEnemy.speed;
                    baseEnemy.canAttack = false;
                    baseEnemy.ChangeState<ChaseStateAI>();
                    return;
                }
            }

            if (baseEnemy.currentAttackTime > baseEnemy.lastAttackTime + baseEnemy.attackDelay)
            {
                AttackTrigger();
            }

            if (!animatonController.isAttacking && !baseEnemy.canSeeTarget)
            {
                baseEnemy.ChangeState<IdleStateAI>();
            }
        }

        private void AttackTrigger()
        {
            animatonController.SetAnimatorTrigger(AnimationController.ATTACK_S);
            animatonController.isAttacking = true;
            baseEnemy.lastAttackTime = Time.time;
            if (baseEnemy.target != null && baseEnemy.canSeeTarget)
            {
                Vector2 dir = baseEnemy.target.transform.position - baseEnemy.transform.position;
                animatonController.GetSpriteRenderer().flipX = dir.x < 0;
            }
        }

        private void CheckGround()
        {
            Vector2 currPos = baseEnemy.transform.position;

            // Определяем сторону лучом на основе направления спрайта
            bool isFacingLeft = animatonController.GetSpriteRenderer().flipX;
            float xOffset = isFacingLeft ? -rayXOffset : rayXOffset;

            // Начальная точка луча (смещение от центра врага)
            Vector2 origin = new Vector2(currPos.x + xOffset, currPos.y + rayYOffset);

            int mask = LayerMask.GetMask("Ground");

            // Луч должен быть направлен ВНИЗ для проверки земли
            RaycastHit2D raycastHit = Physics2D.Raycast(
                origin,
                Vector2.down,
                rayDistance,
                mask
            );

            // Визуализация луча в редакторе
            Debug.DrawRay(origin, Vector2.down * rayDistance, Color.red);

            // Обновляем состояние grounded
            isGrounded = raycastHit.collider != null;

            // Дебаг информация
            Debug.Log($"ChaseStateAI: Grounded = {isGrounded}, Hit: {raycastHit.collider?.gameObject.name}");
        }

        public override void StateFixedUpdate()
        {
            CheckGround();
        }
    }
}