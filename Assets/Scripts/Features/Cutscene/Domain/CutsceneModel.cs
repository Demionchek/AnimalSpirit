namespace Features.Cutscene.Domain
{
    public class CutsceneModel
    {
        public bool IsPlaying { get; private set; }
        public int CurrentIndex { get; private set; }
        public bool IsSequence { get; private set; }

        public void StartSequence()
        {
            IsSequence = true;
            CurrentIndex = 0;
            IsPlaying = true;
        }

        public void StartSingle(int index)
        {
            IsSequence = false;
            CurrentIndex = index;
            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
        }

        public void Next()
        {
            CurrentIndex++;
        }
    }
}