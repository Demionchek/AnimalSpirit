using System;
using Features.Trigger.Domain;
using MessagePipe;
using VContainer.Unity;

namespace Features.Trigger.Application
{
    public sealed class WorldTriggerFacade :
        IInitializable,
        IDisposable
    {
        private readonly ISubscriber<WorldTriggerRequested> _subscriber;
        private readonly WorldTriggerService _service;

        private IDisposable _subscription;

        public WorldTriggerFacade(
            ISubscriber<WorldTriggerRequested> subscriber,
            WorldTriggerService service)
        {
            _subscriber = subscriber;
            _service = service;
        }

        public void Initialize()
        {
            _subscription = _subscriber.Subscribe(e =>
            {
                _service.Execute(e.Config);
            });
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}