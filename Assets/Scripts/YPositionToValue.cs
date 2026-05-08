using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class YPositionToValue : MonoBehaviour
{
    public float minY;
    public float maxY; 
    public float minValue = -0.1f;
    public bool enableMusic = false;

    public Light2D globalLight;
    public Light2D playerLight;

    public AudioSource audioSource;

    private bool isPlayerInside = false;
    private Transform playerTransform;

    private PlayerController playerController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            isPlayerInside = true;
            playerTransform = other.transform;
            playerController = player;
            //playerController.OnRevive += SetToDefault;
        }
    }

    private IEnumerator SoundEnableSmoothly()
    {
        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.volume < 0.5f)
        {
            yield return new WaitForSeconds(0.2f);
            audioSource.volume += 0.05f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            isPlayerInside = false;
            
            if (enableMusic)  audioSource.Play();
        }
    }

    private void Update()
    {
        if (isPlayerInside && playerTransform != null)
        {
            // Нормализуем позицию Y между 0 и 1, затем инвертируем (1 - ...)
            float normalizedValue = 1 - Mathf.InverseLerp(minY, maxY, playerTransform.position.y);

            // Ограничиваем значение между 0 и 1
            normalizedValue = Mathf.Clamp01(normalizedValue);

            Debug.Log("Current value: " + normalizedValue);
            
            if (globalLight != null && globalLight.intensity > minValue) globalLight.intensity = normalizedValue;

            if (playerLight != null) playerLight.intensity = 1 - normalizedValue;
        }
    }

    private void OnDisable()
    {
        if (playerController != null)
            playerController.OnRevive -= SetToDefault;
    }

    private void SetToDefault()
    {
        if (globalLight != null) globalLight.intensity = 1;

        if (playerLight != null) playerLight.intensity = minValue;
    }
}
