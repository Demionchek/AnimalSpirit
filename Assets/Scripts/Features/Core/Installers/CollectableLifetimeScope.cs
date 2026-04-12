using Features.Openers.Application;
using Features.Openers.Domain;
using Features.Openers.Infrastructure;
using Features.Openers.Presentation;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace Features.Core.Installers
{
    public sealed class CollectableLifetimeScope : LifetimeScope
    {
        public override void Configure(IContainerBuilder builder)
        {
            // Standalone collectable scopes need a local broker; collectables under DoorLifetimeScope use the parent's broker.
            if (Parent is not DoorLifetimeScope)
            {
                var options = builder.RegisterMessagePipe();
                builder.RegisterMessageBroker<OpenerStateChanged>(options);
            }

            builder.Register<OpenerModel>(Lifetime.Scoped);
            builder.Register<CollectableService>(Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<CollectableView>();
        }
    }
}
