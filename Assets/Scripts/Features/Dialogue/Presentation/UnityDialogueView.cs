using System;
using Features.Dialogue.Application;
using Features.Dialogue.Domain;
using Features.Dialogue.Infrastructure;
using Features.Player.Domain;
using MessagePipe;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using VContainer;

namespace Features.Dialogue.Presentation
{
    public sealed class UnityDialogueView :
        MonoBehaviour,
        IDialogueViewPort
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _text;

        private TMPWriter _writer;
        private DialogueFacade _facade;

        public bool IsTyping => _writer != null && _writer.IsWriting;

        private readonly IPublisher<DialogueRequested> _dialogueTriggerPub;

        [Inject]
        public void Construct(DialogueFacade facade,
                              ISubscriber<PlayerJumpPressed> skipSub)
        {
            _facade = facade;
            _facade.BindView(this);
        }

        private void Awake()
        {
            _writer = _text.gameObject.GetComponent<TMPWriter>();
            if (_writer == null)
                _writer = gameObject.AddComponent<TMPWriter>();
        }

        public void Show()
        {
            _panel.SetActive(true);
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }

        public void DisplayLine(string line)
        {
            _text.text = line;
        }

        public void PlayTyping()
        {
            _writer.StartWriter();
        }

        public void SkipTyping()
        {
            _writer.SkipWriter();
        }
    }
}