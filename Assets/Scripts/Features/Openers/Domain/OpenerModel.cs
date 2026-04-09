namespace Features.Openers.Domain
{
    public sealed class OpenerModel
    {
        public bool IsActive { get; private set; }

        public void SetState(bool isActive)
        {
            IsActive = isActive;
        }
    }
}