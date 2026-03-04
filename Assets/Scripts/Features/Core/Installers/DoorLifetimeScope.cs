using DefaultNamespace.Features.Interactables.Domain;
using Features.Interactables.Application;
using Features.Interactables.Presentation;
using VContainer;
using VContainer.Unity;

namespace Features.Core.Installers
{
    public sealed class DoorLifetimeScope : LifetimeScope
    {
        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<DoorModel>(Lifetime.Scoped);
            builder.Register<DoorService>(Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<DoorView>()
                   .AsSelf();
        }
    }
}