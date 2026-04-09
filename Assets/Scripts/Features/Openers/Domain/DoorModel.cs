namespace Features.Openers.Domain
{
    public sealed class DoorModel
    {
        public bool IsOpen { get; private set; }

        public void SetState(bool state)
        {
            IsOpen = state;
        }
    }
}