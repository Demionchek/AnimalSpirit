using System;
using System.Collections.Generic;
using System.Linq;
using Features.Openers.Infrastructure;
using Features.Openers.Domain;
using Features.Openers.Presentation;
using MessagePipe;
using VContainer.Unity;

namespace Features.Openers.Application
{
    public sealed class DoorFacade : IInitializable, ITickable, IDisposable
    {
        private readonly DoorService _service;
        private readonly DoorModel _model;
        private readonly ISubscriber<OpenerStateChanged> _sub;
        private readonly List<OpenerView> _openers;

        private IDisposable _subscription;

        public DoorFacade(
            DoorService service,
            DoorModel model,
            ISubscriber<OpenerStateChanged> sub,
            List<OpenerView> openers)
        {
            _service = service;
            _model = model;
            _sub = sub;
            _openers = openers;
        }

        public void Initialize()
        {
            _service.SetOpeners(
                _openers.Select(o => o.Model).ToList());

            _subscription = _sub.Subscribe(_ =>
            {
                _service.Evaluate();
            });

            _service.Evaluate();
        }

        public void SetOpen(bool open)
        {
            _service.SetManual(open);
            _service.Evaluate();
        }

        public void Tick()
        {
            _service.Evaluate();
        }

        public bool IsOpen => _model.IsOpen;

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
