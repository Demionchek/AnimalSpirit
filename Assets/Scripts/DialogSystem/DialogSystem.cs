using System.Collections;
using System.Collections.Generic;
using Player;
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
        Dialog_3
    }

    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel; // Панель с текстом
        [SerializeField] private TextMeshProUGUI dialogueText; // Текст для вывода
        [SerializeField] private float textSpeed = 0.05f; // Скорость появления текста
        [SerializeField] private float delayAfterLines = 1.5f; // Задержка после последней строки

        public bool isDialogRunning = false;

        private List<string> lines = new List<string>(); // Список строк диалога
        private int currentLine = 0; // Текущая строка

        private LinesContainer linesContainer;

        [Inject]
        private InputHandler _inputHandler;

        void Start()
        {
            linesContainer = GetComponent<LinesContainer>();
            InputLines(linesContainer.introLines);
        }

        public void InitDialogue( DialogType type )
        {
            ClearLines();
            switch (type)
            {
                case DialogType.Intro:
                    InputLines(linesContainer.introLines);
                    break;
                case DialogType.Dialog_1:
                    InputLines(linesContainer.interactionLine1);
                    break;
                case DialogType.Dialog_2:
                    InputLines(linesContainer.interactionLine2);
                    break;
                case DialogType.Dialog_3:
                    InputLines(linesContainer.interactionLine3);
                    break;
            }

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
            // Очищаем текст перед началом новой строки
            dialogueText.text = "";

            // Постепенно выводим каждый символ текущей строки
            foreach (char c in lines[currentLine].ToCharArray())
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }

            yield return new WaitForSeconds(delayAfterLines);

            // Переходим к следующей строке или закрываем диалог
            currentLine++;
            if (currentLine < lines.Count)
            {
                StartCoroutine(TypeLine());
            }
            else
            {
                // Задержка перед закрытием панели
                yield return new WaitForSeconds(delayAfterLines);
                EndDialogue();
            }
        }

        void EndDialogue()
        {
            dialoguePanel.SetActive(false);
            isDialogRunning = false;
        }
    }
}