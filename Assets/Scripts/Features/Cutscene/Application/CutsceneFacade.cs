using System;
using Features.Core.Settings.Triggers;
using Features.Dialogue.Domain;
using Features.Trigger.Domain;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace Features.Cutscene.Application
{
    public sealed class CutsceneFacade :
        IInitializable
    {
        private readonly CutsceneService _service;

        private IDisposable _dialogueFinishedSub;
        private IDisposable _dialogueRequestedSub;

        [Inject]
        private void Construct(
            ISubscriber<DialogueFinished> dialogueFinishedSub,
            ISubscriber<DialogueRequested> dialogueRequestedSub)
        {
            _dialogueFinishedSub =
                dialogueFinishedSub.Subscribe(OnDialogueFinished);
            _dialogueRequestedSub = dialogueRequestedSub.Subscribe(OnDialogueRequested);
        }

        public CutsceneFacade(
            CutsceneService service)
        {
            _service = service;
        }

        public void Initialize() { }

        public void Play(int index)
        {
            _service.PlaySingle(index);
        }

        private void OnDialogueFinished(DialogueFinished e)
        {
            _service.Resume();
        }

        public void PlaySequence()
        {
            _service.PlaySequence();
        }

        public void Skip()
        {
            _service.Skip();
        }

        public void NotifyFinished()
        {
            _service.OnCutsceneFinished();
        }

        private void OnDialogueRequested(DialogueRequested e)
        {
            _service.Pause();
        }
    }
}