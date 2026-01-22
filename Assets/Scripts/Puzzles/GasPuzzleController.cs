using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    public class GasPuzzleController : MonoBehaviour
{
    [Header("Puzzle Elements")]
    [SerializeField] private List<GasVent> allVents = new List<GasVent>();
    [SerializeField] private List<GasSource> allGasSources = new List<GasSource>();

    [Header("Solution")]
    [SerializeField] private List<GasVent> solutionVents = new List<GasVent>();

    [Header("Events")]
    public UnityEvent onPuzzleSolved;

    [Header("Feedback")]
    [SerializeField] private AudioClip solvedSound;
    [SerializeField] private GameObject barrier;

    private bool isSolved = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Подписываемся на все вентили
        foreach (var vent in allVents)
        {
            vent.OnToggle += CheckPuzzleState;
        }
    }

    private void CheckPuzzleState()
    {
        if (isSolved) return;

        bool allGasOff = true;
        foreach (var gasSource in allGasSources)
        {
            if (gasSource != null && gasSource.IsActive)
            {
                allGasOff = false;
                break;
            }
        }

        if (allGasOff)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        isSolved = true;
        Debug.Log("Gas puzzle SOLVED!");

        // Воспроизводим звук
        if (solvedSound != null && audioSource != null)
            audioSource.PlayOneShot(solvedSound);

        // Убираем барьер
        if (barrier != null)
            barrier.SetActive(false);

        // Вызываем событие
        onPuzzleSolved?.Invoke();
    }

    public void ResetPuzzle()
    {
        isSolved = false;

        foreach (var vent in allVents)
        {
            // Нужно добавить метод Reset в GasVent
        }

        // Включаем все источники газа
        foreach (var gasSource in allGasSources)
        {
            if (gasSource != null)
                gasSource.SetActive(true);
        }

        if (barrier != null)
            barrier.SetActive(true);
    }
}
}