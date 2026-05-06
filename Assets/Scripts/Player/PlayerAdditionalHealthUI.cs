using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Player
{
    public class PlayerAdditionalHealthUI : MonoBehaviour
    {
        [SerializeField] private Image[] segments = new Image[1];
        [SerializeField] private GameObject root;

        [Inject]
        private PlayerController playerController;

        private void Awake()
        {
            playerController.OnAdditionalHealthChanged += SetAdditionalHealth;
            SetAdditionalHealth(
                playerController.CurrentAdditionalHealth,
                playerController.MaxAdditionalHealth,
                playerController.IsAdditionalHealthActivated);
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.OnAdditionalHealthChanged -= SetAdditionalHealth;
            }
        }

        private void SetAdditionalHealth(int currentHealth, int maxHealth, bool isActivated)
        {
            bool isVisible = isActivated && maxHealth > 0;
            SetVisible(isVisible);

            if (!isVisible || segments == null || segments.Length == 0)
            {
                return;
            }

            float safeMaxHealth = Mathf.Max(1, maxHealth);
            float normalizedHealth = Mathf.Clamp01(currentHealth / safeMaxHealth);
            float totalSegmentFill = normalizedHealth * segments.Length;

            for (int segmentOrder = 0; segmentOrder < segments.Length; segmentOrder++)
            {
                int imageIndex = segments.Length - 1 - segmentOrder;
                Image segment = segments[imageIndex];
                if (segment == null)
                {
                    continue;
                }

                segment.fillAmount = Mathf.Clamp01(totalSegmentFill - segmentOrder);
            }
        }

        private void SetVisible(bool isVisible)
        {
            if (root != null)
            {
                root.SetActive(isVisible);
                return;
            }

            if (segments == null)
            {
                return;
            }

            foreach (Image segment in segments)
            {
                if (segment != null)
                {
                    segment.gameObject.SetActive(isVisible);
                }
            }
        }
    }
}
