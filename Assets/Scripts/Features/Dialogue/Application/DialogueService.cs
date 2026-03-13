using System.Collections.Generic;
using Features.Core.Settings;
using Features.Core.Settings.Scene;
using Features.Dialogue.Domain;

namespace Features.Dialogue.Application
{
    public class DialogueService
    {
        private readonly DialogueModel _model;

        public DialogueService(DialogueModel model)
        {
            _model = model;
        }

        public void StartDialogue(DialogueEntry entry)
        {
            _model.Start(entry);
        }

        public void Next()
        {
            _model.Next();
        }

        public void Stop()
        {
            _model.Stop();
        }

        public bool IsRunning => _model.IsRunning;

        public string CurrentLine => _model.CurrentLine;

        public bool HasNext => _model.HasNext;
    }
}