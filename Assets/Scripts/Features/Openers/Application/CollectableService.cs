using Features.Openers.Infrastructure;
using Features.Openers.Domain;
using MessagePipe;

namespace Features.Openers.Application
{
    public sealed class CollectableService
    {
        private readonly OpenerModel _model;
        private readonly IPublisher<OpenerStateChanged> _publisher;

        public CollectableService(
            OpenerModel model,
            IPublisher<OpenerStateChanged> publisher)
        {
            _model = model;
            _publisher = publisher;
        }

        public void Collect()
        {
            if (_model.IsActive)
                return;

            _model.SetState(true);

            _publisher.Publish(
                new OpenerStateChanged(_model, true));
        }
    }
}