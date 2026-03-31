using System;
using Features.Checkpoints.Domain;
using Features.Checkpoints.Infrastructure;
using Features.Player.Domain;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Checkpoints.Application
{
    public class CheckpointFacade : IInitializable, IDisposable
    {
        private readonly CheckpointService _service;
        private readonly ICheckpointsContainer _checkpointContainer;
        private readonly IPublisher<CheckpointCallback> _checkpointCallback;

        private IDisposable _requestSub;

        public CheckpointFacade(
            CheckpointService service,
            ICheckpointsContainer checkpointContainer,
            IPublisher<CheckpointCallback> checkpointCallback)
        {
            _service = service;
            _checkpointCallback = checkpointCallback;
            _checkpointContainer = checkpointContainer;
        }

        [Inject]
        private void Construct(
            ISubscriber<CheckpointRequest> requestHandler)
        {
            _requestSub = requestHandler.Subscribe(_ => RequestCallback());
        }

        public void Initialize()
        {
            _service.SetCheckpoint(0 ,_checkpointContainer.GetPositionByIndex(0));
        }

        private void RequestCallback()
        {
            Vector3 position = _service.GetCheckpointPosition();
            _checkpointCallback.Publish(new CheckpointCallback(position));
        }

        public void Dispose()
        {
            _requestSub?.Dispose();
        }


    }
}