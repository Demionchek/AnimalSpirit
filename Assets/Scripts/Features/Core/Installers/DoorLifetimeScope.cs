using System.Collections.Generic;
using Features.Openers.Application;
using Features.Openers.Domain;
using Features.Openers.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Core.Installers
{
    public sealed class DoorLifetimeScope : LifetimeScope
    {
        [SerializeField] private List<OpenerView> _openers;

        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<DoorModel>(Lifetime.Scoped);
            builder.Register<DoorService>(Lifetime.Scoped);

            builder.RegisterInstance(_openers);

            builder.Register<DoorFacade>(Lifetime.Scoped)
                   .As<IInitializable>()
                   .AsSelf();

            builder.RegisterComponentInHierarchy<DoorView>();
        }
    }
}