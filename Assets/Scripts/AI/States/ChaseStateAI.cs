using UnityEngine;

namespace AI.States
{
    public class ChaseStateAI : BaseStateAI
    {
        private float rayDistance = 0.35f;
        private float rayYOffset = 0.2f;
        private float rayXOffset = 0.3f;
        bool isGrounded = false;


        public override void EnterState()
        {

        }

        public override void StateFixedUpdate()
        {
            if (baseEnemy.target == null)
                return;

            Vector2 direction = baseEnemy.target.position - baseEnemy.transform.position;
            float distance = direction.magnitude;

            CheckGround();

            animatonController.GetSpriteRenderer().flipX = direction.x < 0;

            if (isGrounded && distance > baseEnemy.stoppingDistance)
            {
                direction.Normalize();
                baseEnemy.rb.velocity = direction * baseEnemy.speed;
            }
            else
            {
                baseEnemy.rb.velocity = Vector2.zero;

                if (!isGrounded)
                    baseEnemy.ChangeState<PatrolStateAI>();
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
    }
}