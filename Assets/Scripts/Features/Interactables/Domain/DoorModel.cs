namespace DefaultNamespace.Features.Interactables.Domain
{
    public sealed class DoorModel
    {
        public bool IsOpen { get; private set; }

        public void SetState(bool isOpen)
        {
            IsOpen = isOpen;
        }
    }
}