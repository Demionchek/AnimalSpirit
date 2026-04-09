using Features.Openers.Infrastructure;
using Features.Openers.Domain;
using MessagePipe;

namespace Features.Openers.Application
{
    public sealed class ButtonService
    {
        private readonly OpenerModel _model;
        private readonly IPublisher<OpenerStateChanged> _publisher;

        public ButtonService(
            OpenerModel model,
            IPublisher<OpenerStateChanged> publisher)
        {
            _model = model;
            _publisher = publisher;
        }

        public void SetPressed(bool pressed)
        {
            if (_model.IsActive == pressed)
                return;

            _model.SetState(pressed);

            _publisher.Publish(
                new OpenerStateChanged(_model, pressed));
        }
    }
}