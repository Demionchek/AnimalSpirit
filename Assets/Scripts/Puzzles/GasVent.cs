using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Puzzles
{
    public class GasVent : MonoBehaviour, IInteractable
    {
        [Header("Vent Settings")]
        [SerializeField] private List<GasSource> connectedGasSources = new List<GasSource>();
        [SerializeField] private float rotationAngle = 90f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private AudioClip turnSound;

        [Header("Visuals")]
        [SerializeField] private Transform wheelTransform;
        [SerializeField] private Light2D indicatorLight;
        [SerializeField] private Color activeColor = Color.red;
        [SerializeField] private Color inactiveColor = Color.green;

        public event Action OnToggle;

        private bool isOn = false;
        private bool isRotating = false;
        private Quaternion targetRotation;
        private AudioSource audioSource;

        public bool IsOn => isOn;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            UpdateIndicator();
        }

        public void Interact()
        {
            if (isRotating) return;

            ToggleVent();
        }

        private void ToggleVent()
        {
            isOn = !isOn;

            // Переключаем связанные источники газа
            foreach (var gasSource in connectedGasSources)
            {
                if (gasSource != null)
                    gasSource.Toggle();
            }

            // Анимация вращения
            StartCoroutine(RotateWheel());

            // Звук
            if (turnSound != null && audioSource != null)
                audioSource.PlayOneShot(turnSound);

            // Обновляем индикатор
            UpdateIndicator();

            OnToggle?.Invoke();

            Debug.Log($"Vent {name} turned {(isOn ? "ON" : "OFF")}");
        }

        private System.Collections.IEnumerator RotateWheel()
        {
            isRotating = true;
            float elapsed = 0f;
            Quaternion startRotation = wheelTransform.rotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, rotationAngle);

            while (elapsed < rotationSpeed)
            {
                wheelTransform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / rotationSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            wheelTransform.rotation = endRotation;
            isRotating = false;
        }

        private void UpdateIndicator()
        {
            if (indicatorLight != null)
                indicatorLight.color = isOn ? activeColor : inactiveColor;
        }
    }
}