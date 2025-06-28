using Animations;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public class HammerController : MonoBehaviour
    {

        [Header("Timing Settings")]
        [SerializeField] private float activeTime = 1f;    // Время работы молота
        [SerializeField] private float inactiveTime = 2f; // Время простоя молота

        [Header("Attack Settings")]
        [SerializeField] private Vector2 castSize;        // Размер CubeCast (должен соответствовать коллайдеру)
        [SerializeField] private LayerMask hitLayers;     // Слои для атаки

        private Animator animator;
        private Collider2D hammerCollider;
        private float timer;
        private bool isActive;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            hammerCollider = GetComponent<Collider2D>();

            // Если размер не задан, используем размер коллайдера
            if (castSize == Vector2.zero && hammerCollider != null)
            {
                castSize = hammerCollider.bounds.size;
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (isActive && timer >= inactiveTime)
            {
                isActive = false;
                timer = 0f;
                animator.SetBool(AnimationController.ATTACK_S, false);
                hammerCollider.enabled = true;
            }
            else if (!isActive && timer >= activeTime)
            {
                isActive = true;
                timer = 0f;
                animator.SetBool(AnimationController.ATTACK_S, true);
                hammerCollider.enabled = false;
            }
        }

        private void Attack()
        {
            // Получаем центр и поворот коллайдера
            Vector3 center = hammerCollider.bounds.center;
            Quaternion rotation = transform.rotation;

            int layers = 1 << hitLayers;

            // Делаем CubeCast
            Collider2D hitCollider = Physics2D.OverlapBox(center, castSize, layers);

            if (hitCollider != null)
            {
                // Проверяем, реализует ли объект интерфейс IHittable
                IHittable hittable = hitCollider.GetComponent<IHittable>();
                if (hittable != null)
                {
                    hittable.Hit();
                }
            }
        }
    }
}