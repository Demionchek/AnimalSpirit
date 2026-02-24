namespace Features.Dialogue.Infrastructure
{
    public interface IDialogueViewPort
    {
        void Show();
        void Hide();
        void DisplayLine(string line);
        void PlayTyping();
        void SkipTyping();
        bool IsTyping { get; }
    }
}