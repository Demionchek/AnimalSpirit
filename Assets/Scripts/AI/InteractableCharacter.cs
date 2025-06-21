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

        [SerializeField] private GameObject interactSign;

        [Inject]
        private DialogueSystem dialogueSystem;

        private Animator animator;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Interact()
        {
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
            float radius = 0.3f;
            float distance = 0.2f;
            Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right * distance;
            int layerNumber = LayerMask.NameToLayer("Interact");
            LayerMask layerMask = 1 << layerNumber;
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
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(); // Взаимодействуем
                }
            }
        }
    }
}