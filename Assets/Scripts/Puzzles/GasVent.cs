using System;
using System.Collections.Generic;
using Interfaces;
using Puzzles;
using UnityEngine;

public class GasVent : MonoBehaviour, IInteractable
{
    [Header("Vent Settings")]
    [SerializeField] private List<GasSource> connectedGasSources = new List<GasSource>();
    [SerializeField] private AudioClip turnSound;

    [Header("Visuals")]
    [SerializeField] private Transform wheelTransform;
    [SerializeField] private SpriteRenderer indicatorLight;
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color inactiveColor = Color.green;
    [SerializeField] private float rotationAngle = 45f;

    public event Action OnToggle;
    private bool isOn = false;
    private AudioSource audioSource;

    public bool IsOn => isOn;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        UpdateIndicator();
    }

    public void Interact()
    {
        ToggleVent();
    }

    private void ToggleVent()
    {
        isOn = !isOn;

        // Мгновенное вращение
        if (wheelTransform != null)
        {
            wheelTransform.Rotate(0, 0, isOn ? rotationAngle : -rotationAngle);
        }

        // Переключаем связанные источники газа
        foreach (var gasSource in connectedGasSources)
        {
            if (gasSource != null)
                gasSource.Toggle();
        }

        // Звук
        if (turnSound != null && audioSource != null)
            audioSource.PlayOneShot(turnSound);

        // Обновляем индикатор
        UpdateIndicator();

        OnToggle?.Invoke();

        Debug.Log($"Vent {name} turned {(isOn ? "ON" : "OFF")}");
    }

    private void UpdateIndicator()
    {
        if (indicatorLight != null)
            indicatorLight.color = isOn ? activeColor : inactiveColor;
    }
}