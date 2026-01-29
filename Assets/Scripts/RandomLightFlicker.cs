using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class RandomLightFlicker : MonoBehaviour
{
    [Header("Настройки мигания")]
    [SerializeField] private float minFrequency = 0.5f;
    [SerializeField] private float maxFrequency = 2f;
    [SerializeField] private float minDelay = 0.05f;
    [SerializeField] private float maxDelay = 0.2f;

    [Header("Активация")]
    [SerializeField] private bool useTriggerActivation = false;
    [SerializeField] private LayerMask triggerLayer = -1;

    [Header("Состояние")]
    [SerializeField] private bool startEnabled = true;
    [SerializeField] private bool isFlickering = false;

    private Light2D light2D;
    private float nextFlickerTime;
    private float flickerEndTime;
    private bool isLightOn = true;
    private bool isInFlickerSequence = false;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();

        if (light2D == null)
        {
            Debug.LogError("Light2D component not found!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // Всегда включаем свет в начале
        SetLightState(true);

        if (startEnabled && !useTriggerActivation)
        {
            StartFlickering();
        }
        else if (!startEnabled)
        {
            StopFlickering();
        }
    }

    private void Update()
    {
        if (!isFlickering) return;

        if (!isInFlickerSequence)
        {
            // Ждем следующего мигания
            if (Time.time >= nextFlickerTime)
            {
                StartFlickerSequence();
            }
        }
        else
        {
            // В процессе мигания - проверяем, когда включить свет обратно
            if (Time.time >= flickerEndTime)
            {
                EndFlickerSequence();
            }
        }
    }

    private void StartFlickerSequence()
    {
        isInFlickerSequence = true;

        // Выключаем свет
        SetLightState(false);

        // Устанавливаем время, когда свет снова включится
        float flickerDuration = Random.Range(minDelay, maxDelay);
        flickerEndTime = Time.time + flickerDuration;
    }

    private void EndFlickerSequence()
    {
        isInFlickerSequence = false;

        // Включаем свет обратно
        SetLightState(true);

        // Устанавливаем время следующего мигания
        ScheduleNextFlicker();
    }

    private void ScheduleNextFlicker()
    {
        float delay = Random.Range(minFrequency, maxFrequency);
        nextFlickerTime = Time.time + delay;
    }

    private void SetLightState(bool state)
    {
        if (light2D != null)
        {
            light2D.enabled = state;
            isLightOn = state;
        }
    }

    public void StartFlickering()
    {
        if (isFlickering) return;

        isFlickering = true;
        isInFlickerSequence = false;
        SetLightState(true);
        ScheduleNextFlicker();

        Debug.Log("Light flickering started");
    }

    public void StopFlickering()
    {
        if (!isFlickering) return;

        isFlickering = false;
        isInFlickerSequence = false;
        SetLightState(true); // Всегда включаем свет при остановке

        Debug.Log("Light flickering stopped");
    }

    public void ToggleFlickering()
    {
        if (isFlickering)
            StopFlickering();
        else
            StartFlickering();
    }

    // Методы для работы с триггерами
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTriggerActivation) return;

        if (triggerLayer == -1 || (triggerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            StartFlickering();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!useTriggerActivation) return;

        if (triggerLayer == -1 || (triggerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            StopFlickering();
        }
    }

    // Методы для изменения настроек во время выполнения
    public void SetFrequency(float newMin, float newMax)
    {
        minFrequency = Mathf.Max(0.1f, newMin);
        maxFrequency = Mathf.Max(minFrequency, newMax);
    }

    public void SetDelay(float newMin, float newMax)
    {
        minDelay = Mathf.Max(0.01f, newMin);
        maxDelay = Mathf.Max(minDelay, newMax);
    }

    // Геттеры для отладки
    public float GetTimeUntilNextFlicker() => isFlickering ? Mathf.Max(0, nextFlickerTime - Time.time) : 0;
    public bool GetIsLightOn() => isLightOn;
    public bool GetIsInFlickerSequence() => isInFlickerSequence;
}
