using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public enum DialogType
    {
        Intro,
        Dialog_1,
        Dialog_2,
        Dialog_3,
        Dialog_4,
        Dialog_5
    }

    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel; // Панель с текстом
        [SerializeField] private TextMeshProUGUI dialogueText; // Текст для вывода
        [SerializeField] private float textSpeed = 0.05f; // Скорость появления текста
        [SerializeField] private float delayAfterLines = 1.5f; // Задержка после последней строки

        [SerializeField] private bool isTriggerCutscene = false;
        [SerializeField] private DialogType CutsceneTrigger = DialogType.Dialog_3;
        [SerializeField] private int timelineIndex = 3;

        public bool isDialogRunning = false;

        private List<string> lines = new List<string>(); // Список строк диалога
        private int currentLine = 0; // Текущая строка

        private LinesContainer linesContainer;
        private DialogType currentType;
        private TMPWriter writer;

        [Inject]
        private InputHandler _inputHandler;

        [Inject]
        private TimelineManager _timelineManager;

        void Start()
        {
            linesContainer = GetComponent<LinesContainer>();
            InputLines(linesContainer.dialogLines[(int)DialogType.Intro].lines);
            writer = dialogueText.gameObject.GetComponent<TMPWriter>();
        }

        public void InitDialogue(int index)
        {
            if (isDialogRunning) return;
            currentType = (DialogType)index;

            ClearLines();
            InputLines(linesContainer.dialogLines[index].lines);

            StartDialogue();
        }

        private void ClearLines()
        {
            lines.Clear();
        }

        private void InputLines(List<string> newLines)
        {
            this.lines.AddRange(newLines);
        }

        public void StartDialogue()
        {
            dialoguePanel.SetActive(true);
            currentLine = 0;
            StartCoroutine(TypeLine());
        }

        IEnumerator TypeLine()
        {
            isDialogRunning = true;

            bool isPlayingCutscene = _timelineManager.IsCutscenePlaying();

            if (isPlayingCutscene)
                if (_timelineManager.GetCutscene() != null)
                    _timelineManager.GetCutscene().Pause();

            dialogueText.text = lines[currentLine];

            // Сбрасываем состояние нажатия в начале каждой строки
            // Ждем пока кнопка будет отпущена перед началом новой строки
            yield return new WaitWhile(() => _inputHandler.JumpPressed);

            // Постепенно выводим каждый символ текущей строки
            writer.StartWriter();

            // Ждем пока текст напечатается ИЛИ пока не нажмут прыжок для пропуска
            yield return new WaitUntil(() => writer.IsWriting == false || _inputHandler.JumpPressed);

            // Если текст еще печатался и была нажата кнопка - пропускаем анимацию
            if (writer.IsWriting && _inputHandler.JumpPressed)
            {
                writer.SkipWriter();
                // Ждем пока текст полностью не пропустится
                yield return new WaitUntil(() => writer.IsWriting == false);
            }

            currentLine++;

            // Теперь ждем НОВОГО нажатия для продолжения
            // Сначала убедимся что кнопка отпущена
            yield return new WaitWhile(() => _inputHandler.JumpPressed);

            // Затем ждем нового нажатия
            if(currentLine < lines.Count)
                yield return new WaitUntil(() => _inputHandler.JumpPressed);

            // Короткая задержка чтобы убедиться что нажатие зарегистрировано
            yield return null;

            // Ждем отпускания кнопки
            yield return new WaitWhile(() => _inputHandler.JumpPressed);

            if (currentLine < lines.Count)
            {
                StartCoroutine(TypeLine());
            } else
            {
                // Для последней строки тоже ждем нового нажатия
                yield return new WaitWhile(() => _inputHandler.JumpPressed);
                yield return new WaitUntil(() => _inputHandler.JumpPressed);
                yield return null;
                yield return new WaitWhile(() => _inputHandler.JumpPressed);

                if (_timelineManager.IsCutscenePaused())
                    _timelineManager.GetCutscene().Resume();

                EndDialogue();
            }
        }

        void EndDialogue()
        {
            dialoguePanel.SetActive(false);
            isDialogRunning = false;

            if (isTriggerCutscene && currentType == CutsceneTrigger)
            {
                _timelineManager.PlayCutscene(timelineIndex);
            }
        }
    }
}