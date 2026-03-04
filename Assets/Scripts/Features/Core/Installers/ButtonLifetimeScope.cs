using DefaultNamespace.Features.Interactables.Domain;
using Features.Interactables.Application;
using Features.Interactables.Infrastructure;
using Features.Interactables.Presentation;
using VContainer;
using VContainer.Unity;

namespace Features.Core.Installers
{
    public sealed class ButtonLifetimeScope : LifetimeScope
    {
        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<ButtonModel>(Lifetime.Scoped);
            builder.Register<ButtonService>(Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<ButtonView>()
                   .AsSelf()
                   .As<IOpener>();
        }
    }
}