using Features.Interactables.Application;
using Features.Interactables.Presentation;
using VContainer;
using VContainer.Unity;

namespace Features.Core.Installers
{
    public sealed class InteractableLifetimeScope : LifetimeScope
    {
        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<InteractionActionFactory>(
                Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<
                InteractableCharacter>();
        }
    }
}