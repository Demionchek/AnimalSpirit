using Features.Interactables.Infrastructure;
using UnityEngine;

namespace Features.Interactables.Application
{
    public sealed class PlaySoundAction : IInteractionAction
    {
        private readonly AudioClip clip;
        private readonly AudioSource source;

        public PlaySoundAction(AudioClip clip, AudioSource source)
        {
            this.clip = clip;
            this.source = source;
        }

        public void Execute()
        {
            source.PlayOneShot(clip);
        }
    }
}