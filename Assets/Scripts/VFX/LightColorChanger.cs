using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VFX
{
    public class LightColorChanger : MonoBehaviour
    {
        [SerializeField] private Color[] colors;
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private float duration2 = 0.3f;
        [SerializeField] private float startDelay = 0.2f;
        private Light2D light2D;
        private int lastIndex;
        private float lastDuration; 
        
        private void Start()
        {
            light2D = GetComponent<Light2D>();
            StartCoroutine(ColorChangerCoroutine());
        }

        private IEnumerator ColorChangerCoroutine()
        {
            yield return new WaitForSeconds(startDelay);
            while (true)
            {
                lastDuration = Mathf.Approximately(lastDuration, duration) ? duration2 : duration;
                yield return new WaitForSeconds(duration);

                int randomIndex = UnityEngine.Random.Range(0, colors.Length);
                lastIndex = randomIndex;
                light2D.color = colors[randomIndex];
            }
        }
    }
}