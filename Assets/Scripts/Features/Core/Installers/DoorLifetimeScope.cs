using System;
using System.Collections.Generic;
using Features.Openers.Application;
using Features.Openers.Domain;
using Features.Openers.Infrastructure;
using Features.Openers.Presentation;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Core.Installers
{
    public sealed class DoorLifetimeScope : LifetimeScope
    {
        [SerializeField] private List<OpenerView> _openers;
        [SerializeField] private DoorView _doorView;

        public override void Configure(IContainerBuilder builder)
        {
            ResolveLocalDependencies();
            var options = builder.RegisterMessagePipe();

            builder.Register<DoorModel>(Lifetime.Scoped);
            builder.Register<DoorService>(Lifetime.Scoped);
            builder.RegisterMessageBroker<OpenerStateChanged>(options);

            builder.RegisterInstance(_openers);
            builder.RegisterInstance(_doorView);

            builder.Register<DoorFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .As<ITickable>()
                   .AsSelf();

            builder.RegisterBuildCallback(container =>
            {
                container.Inject(_doorView);
            });
        }

        private void ResolveLocalDependencies()
        {
            _doorView ??= GetComponentInChildren<DoorView>(true);

            if (_doorView == null)
                throw new InvalidOperationException($"{nameof(DoorLifetimeScope)} on {name} requires {nameof(DoorView)} in children.");
        }
    }
}
