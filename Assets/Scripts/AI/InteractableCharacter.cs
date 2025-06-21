using System;
using DefaultNamespace;
using Interfaces;
using UnityEngine;
using Zenject;

namespace AI
{
    public class InteractableCharacter : MonoBehaviour, IInteractable
    {
        public DialogType type;
        public bool hasDialog = false;
        public bool canAttack = false;
        public bool canHit = false;

        public float interactDelay = 0.5f;

        [SerializeField] private GameObject interactSign;

        [Inject]
        private DialogueSystem dialogueSystem;

        private Animator animator;
        private SpriteRenderer spriteRenderer;

        private float lastTime;
        private float currentTime;
        private bool canInteract = true;

        private bool isFlip;

        private void Start()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (currentTime - lastTime > lastTime + interactDelay)
            {
                canInteract = true;
            }

            isFlip = spriteRenderer.flipX;

            currentTime = Time.time;
        }

        public void Interact()
        {
            lastTime = Time.time;
            canInteract = false;
            interactSign.SetActive(false);

            if(hasDialog)
                dialogueSystem.InitDialogue(type);

            if (canAttack)
                animator.SetTrigger("Attack");

            if (canHit)
            {
                CircleCastAll();
            }
        }

        private void CircleCastAll()
        {
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            Vector2 direction = (isFlip ? new Vector2(-0.25f,0) : new Vector2(0.25f,0));
            origin += direction;

            // Выполняем CircleCast
            // Получаем все коллайдеры в радиусе
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, radius);

            if (hitColliders.Length == 0)
            {
                Debug.Log("В радиусе нет объектов.");
                return;
            }

            // Перебираем все найденные коллайдеры
            foreach (Collider2D collider in hitColliders)
            {
                Debug.Log($"Обнаружен объект: {collider.name}");

                // Проверяем, есть ли у него компонент для взаимодействия
                IHittable hittable = collider.GetComponent<IHittable>();
                if (hittable != null)
                {
                    hittable.Hit(); // Взаимодействуем
                }
            }
        }

        private void OnDrawGizmos()
        {
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            float distance = 0.02f;
            Vector2 direction = (isFlip ? new Vector2(-0.1f,0) : new Vector2(0.1f,0));
            origin += direction;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, radius);
        }
    }
}