using Animations;
using Camera;
using Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DefaultNamespace.DependencyInjection
{
    public class IntroSceneInstaller : LifetimeScope
    {
        public override void Configure( IContainerBuilder builder )
        {
            builder.RegisterComponentInHierarchy<TimelineManager>();
            builder.RegisterComponentInHierarchy<PlayerController>();
            builder.RegisterComponentInHierarchy<CameraController>();
            builder.RegisterComponentInHierarchy<InputHandler>();
            builder.RegisterComponentInHierarchy<DialogueSystem>();
            builder.RegisterComponentInHierarchy<CheckPoints>();
        }
    }
}