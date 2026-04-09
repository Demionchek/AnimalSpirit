using Features.Openers.Domain;

namespace Features.Openers.Infrastructure
{
    public readonly struct OpenerStateChanged
    {
        public readonly OpenerModel Opener;
        public readonly bool IsActive;

        public OpenerStateChanged(OpenerModel opener, bool isActive)
        {
            Opener = opener;
            IsActive = isActive;
        }
    }
}