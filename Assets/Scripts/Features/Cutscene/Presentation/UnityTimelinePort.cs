using System;
using Features.Cutscene.Infrastructure;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

namespace Features.Cutscene.Presentation
{
    public sealed class UnityTimelinePort :
        MonoBehaviour,
        ICutscenePort
    {
        [SerializeField] private bool _allowOnStart = true;

        [SerializeField]
        private PlayableDirector[] _cutscenes;

        private PlayableDirector _current;

        public int CutsceneCount => _cutscenes.Length;

        public bool IsPlaying =>
            _current != null &&
            _current.state == PlayState.Playing;

        private void Start()
        {
            foreach (var cutscene in _cutscenes)
            {
                if (_allowOnStart)
                {
                    if (cutscene.playOnAwake)
                    {
                        _current = cutscene;
                        break;
                    }
                } else
                {
                    cutscene.playOnAwake = false;
                }
            }
        }

        public void Play(int index)
        {
            if (index < 0 || index >= _cutscenes.Length)
                return;

            Stop();

            _current = _cutscenes[index];
            _current.Play();
        }

        public void Stop()
        {
            if (_current != null)
            {
                _current.Stop();
                _current = null;
            }
        }

        public void Pause()
        {
            if (_current != null)
                _current.Pause();
        }

        public void Resume()
        {
            if (_current != null)
                _current.Resume();
        }
    }
}