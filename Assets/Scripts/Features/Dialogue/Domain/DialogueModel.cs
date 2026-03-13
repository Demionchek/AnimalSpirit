using System.Collections.Generic;
using Features.Core.Settings.Scene;

namespace Features.Dialogue.Domain
{
    public class DialogueModel
    {
        private readonly List<string> _lines = new();

        public int Id {get; private set;}
        public bool IsRunning { get; private set; }
        public int CurrentIndex { get; private set; }

        public IReadOnlyList<string> Lines => _lines;

        public void Start(DialogueEntry entry)
        {
            _lines.Clear();
            _lines.AddRange(entry.Lines);

            Id = entry.Id;
            CurrentIndex = 0;
            IsRunning = _lines.Count > 0;
        }

        public void Stop()
        {
            IsRunning = false;
            _lines.Clear();
            CurrentIndex = 0;
        }

        public bool HasNext => CurrentIndex < _lines.Count - 1;

        public void Next()
        {
            if (!HasNext)
            {
                Stop();
                return;
            }

            CurrentIndex++;
        }

        public string CurrentLine =>
            _lines.Count == 0 ? string.Empty : _lines[CurrentIndex];
    }
}