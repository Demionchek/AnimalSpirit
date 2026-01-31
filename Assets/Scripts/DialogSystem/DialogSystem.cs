using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private float delayAfterLines = 1.5f;

        [SerializeField] private bool isTriggerCutscene = false;
        [SerializeField] private DialogType CutsceneTrigger = DialogType.Dialog_3;
        [SerializeField] private UnityEventDictionary dialogEventDictionary;
        [SerializeField] private int timelineIndex = 3;

        public bool isDialogRunning = false;

        private List<string> lines = new List<string>();
        private int currentLine = 0;

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
            if (writer == null)  writer = dialogueText.gameObject.AddComponent<TMPWriter>();
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

            yield return new WaitWhile(() => _inputHandler.JumpPressed);

            writer.StartWriter();

            yield return new WaitUntil(() => writer.IsWriting == false || _inputHandler.JumpPressed);

            if (writer.IsWriting && _inputHandler.JumpPressed)
            {
                writer.SkipWriter();
                yield return new WaitUntil(() => writer.IsWriting == false);
            }

            currentLine++;

            yield return new WaitWhile(() => _inputHandler.JumpPressed);

            if(currentLine < lines.Count)
                yield return new WaitUntil(() => _inputHandler.JumpPressed);

            yield return null;

            yield return new WaitWhile(() => _inputHandler.JumpPressed);

            if (currentLine < lines.Count)
            {
                StartCoroutine(TypeLine());
            } else
            {
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

            if (dialogEventDictionary.Contains(currentType))
                dialogEventDictionary[currentType]?.Invoke();
        }
    }
}