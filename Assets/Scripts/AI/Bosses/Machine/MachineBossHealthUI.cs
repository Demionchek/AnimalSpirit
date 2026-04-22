using UnityEngine;
using UnityEngine.UI;

namespace AI.Bosses.Machine
{
    public class MachineBossHealthUI : MonoBehaviour
    {
        [SerializeField] private Image[] segments = new Image[10];

        public void SetHealth(int currentHealth, int maxHealth)
        {
            if (segments == null || segments.Length == 0)
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
    }
}
