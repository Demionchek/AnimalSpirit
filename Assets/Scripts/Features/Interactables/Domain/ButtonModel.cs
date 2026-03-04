namespace DefaultNamespace.Features.Interactables.Domain
{
    public sealed class ButtonModel
    {
        public bool IsPressed { get; private set; }

        public void SetPressed(bool value)
        {
            IsPressed = value;
        }
    }
}