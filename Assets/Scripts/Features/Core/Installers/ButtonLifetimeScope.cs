using Features.Openers.Application;
using Features.Openers.Domain;
using Features.Openers.Presentation;
using VContainer;
using VContainer.Unity;

namespace Features.Core.Installers
{
    public sealed class ButtonLifetimeScope : LifetimeScope
    {
        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<OpenerModel>(Lifetime.Scoped);
            builder.Register<ButtonService>(Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<ButtonView>()
                   .AsSelf();
        }
    }
}