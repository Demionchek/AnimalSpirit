using System;
using System.Collections.Generic;
using System.Linq;
using Features.Core.Settings;
using Features.Core.Settings.Scene;
using Features.Dialogue.Domain;
using Features.Dialogue.Infrastructure;
using Features.Player.Domain;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace Features.Dialogue.Application
{
    public sealed class DialogueFacade :
        IInitializable,
        IDisposable
    {
        private readonly DialogueService _service;
        private readonly DialogueDatabase _database;
        private IDialogueViewPort _view;

        private readonly IPublisher<DialogueFinished> _finishedPub;
        private readonly IPublisher<PlayerControlStateChanged> _controlPub;

        private IDisposable _requestSub;
        private IDisposable _advanceSub;


        public DialogueFacade(
            DialogueService service,
            DialogueDatabase database,
            IPublisher<DialogueFinished> finishedPub,
            IPublisher<PlayerControlStateChanged> controlPub)
        {
            _service = service;
            _database = database;
            _finishedPub = finishedPub;
            _controlPub = controlPub;
        }

        [Inject]
        private void Construct(
            ISubscriber<PlayerJumpPressed> advanceSub,
            ISubscriber<DialogueRequested> requestSub)
        {
            _advanceSub = advanceSub.Subscribe(_ => Advance());
            _requestSub = requestSub.Subscribe(e => StartDialogue(e.DialogueId));
        }

        public void BindView(IDialogueViewPort view)
        {
            _view = view;
        }

        public void Initialize() { }

        public void StartDialogue(int id)
        {
            DialogueEntry entry = _database.Dialogues
                                 .FirstOrDefault(d => d.Id == id);

            if (entry == null)
                return;

            StartDialogue(entry);
            _controlPub.Publish(new PlayerControlStateChanged(false));
        }

        private void StartDialogue(DialogueEntry entry)
        {
            _service.StartDialogue(entry);

            if (!_service.IsRunning)
                return;

            _view.Show();
            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            _view.DisplayLine(_service.CurrentLine);
            _view.PlayTyping();
        }

        public void Advance()
        {
            if (!_service.IsRunning)
                return;

            if (_view.IsTyping)
            {
                _view.SkipTyping();
                return;
            }

            if (_service.HasNext)
            {
                _service.Next();
                ShowCurrentLine();
            }
            else
            {
                _service.Stop();
                _view.Hide();
                _finishedPub.Publish(new DialogueFinished());
                _controlPub.Publish(new PlayerControlStateChanged(true));
            }
        }

        public void Dispose()
        {
            _advanceSub?.Dispose();
        }
    }
}