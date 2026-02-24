namespace Features.Cutscene.Infrastructure
{
    public interface ICutscenePort
    {
        void Play(int index);
        void Stop();
        void Pause();
        void Resume();

        bool IsPlaying { get; }

        int CutsceneCount { get; }
    }
}