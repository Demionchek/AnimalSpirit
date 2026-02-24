using Features.Cutscene.Domain;
using Features.Cutscene.Infrastructure;

namespace Features.Cutscene.Application
{
    public sealed class CutsceneService
    {
        private readonly CutsceneModel _model;
        private readonly ICutscenePort _port;

        public CutsceneService(
            CutsceneModel model,
            ICutscenePort port)
        {
            _model = model;
            _port = port;
        }

        public void PlaySingle(int index)
        {
            _model.StartSingle(index);
            _port.Play(index);
        }

        public void PlaySequence()
        {
            if (_port.CutsceneCount == 0)
                return;

            _model.StartSequence();
            _port.Play(0);
        }

        public void OnCutsceneFinished()
        {
            if (!_model.IsSequence)
            {
                _model.Stop();
                return;
            }

            _model.Next();

            if (_model.CurrentIndex >= _port.CutsceneCount)
            {
                _model.Stop();
                return;
            }

            _port.Play(_model.CurrentIndex);
        }

        public void Skip()
        {
            if (!_model.IsPlaying)
                return;

            _port.Stop();
            OnCutsceneFinished();
        }

        public void Resume()
        {
            _port.Resume();
        }
    }
}