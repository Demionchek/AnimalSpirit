using System;
using Features.Player.Domain;
using Features.UIShape.Presentation;
using MessagePipe;
using VContainer.Unity;

namespace Features.UIShape.Application
{
    public sealed class UIShapeFacade :
        IInitializable,
        IDisposable
    {
        private readonly UIShapeView _view;

        private readonly ISubscriber<PlayerShapeChanged> _shapeChanged;
        private readonly ISubscriber<PlayerShapeUnlocked> _shapeUnlocked;

        private IDisposable _subShapeChanged;
        private IDisposable _subShapeUnlocked;

        public UIShapeFacade(
            UIShapeView view,
            ISubscriber<PlayerShapeChanged> shapeChanged,
            ISubscriber<PlayerShapeUnlocked> shapeUnlocked)
        {
            _view = view;
            _shapeChanged = shapeChanged;
            _shapeUnlocked = shapeUnlocked;
        }

        public void Initialize()
        {
            _subShapeChanged = _shapeChanged.Subscribe(e =>
            {
                _view.SetShape(e.Shape);
            });

            _subShapeUnlocked = _shapeUnlocked.Subscribe(e =>
            {
                _view.UnlockShape(e.Shape);
            });
        }

        public void Dispose()
        {
            _subShapeChanged?.Dispose();
            _subShapeUnlocked?.Dispose();
        }
    }
}