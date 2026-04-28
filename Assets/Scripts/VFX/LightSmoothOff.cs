using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VFX
{
    public class LightSmoothOff : MonoBehaviour
    {
        [SerializeField] private float smoothing = 0.1f;
        [SerializeField] private float delay = 0.1f;
        [SerializeField] private float startAmount = 1f;
        private Light2D _light2D;
        private Coroutine _coroutine;

        private void OnEnable()
        {
            if (_light2D == null)
                _light2D =  GetComponent<Light2D>();
            
            if (_coroutine != null) 
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(SmoothingLight());
        }

        private IEnumerator SmoothingLight()
        {
            _light2D.intensity = startAmount;
            
            while (_light2D.intensity > 0)
            {
                yield return new WaitForSeconds(smoothing);
                _light2D.intensity -= smoothing;
            }
            
            _coroutine = null;
        }
    }
}