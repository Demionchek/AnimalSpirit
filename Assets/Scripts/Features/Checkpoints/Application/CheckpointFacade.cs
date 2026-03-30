using Features.Checkpoints.Domain;

namespace Features.Checkpoints.Application
{
    public class CheckpointFacade
    {
        private readonly CheckpointService _service;

        public CheckpointFacade(
            CheckpointService service)
        {
            _service = service;
        }

    }
}