using Features.Cutscene.Infrastructure;
using Features.Interactables.Infrastructure;

namespace Features.Interactables.Application
{
    public class PlayTimelineAction : IInteractionAction
    {
        private readonly int _index;
        private readonly ICutscenePort _port;
        private bool isPlayed;

        public PlayTimelineAction(
            ICutscenePort port,
            int  index)
        {
            _index = index;
            _port = port;
        }

        public void Execute()
        {
            if (!isPlayed)
            {
                _port.Play(_index);
                isPlayed = true;
            }
        }
    }
}