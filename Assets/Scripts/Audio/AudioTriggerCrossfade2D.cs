using System;
using System.Collections;
using Player;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Collider2D))]
    public class AudioTriggerCrossfade2D : MonoBehaviour
    {
        private enum Axis
        {
            X,
            Y
        }

        private enum EnterPlaybackAction
        {
            None,
            PlayBoth,
            StopBoth,
            ToggleBoth
        }

        [Header("Sources")]
        [SerializeField] private AudioSource toSource;
        [SerializeField] private AudioSource fromSource;

        [Header("Fade")]
        [SerializeField] private float fadeDuration = 1.5f;
        [SerializeField] private bool stopFromSourceAfterFade = true;
        [SerializeField] private bool twoWaySwitchByPosition = true;

        [Header("Position Condition")]
        [SerializeField] private bool useMinPositionCondition;
        [SerializeField] private Axis conditionAxis = Axis.X;
        [SerializeField] private float minAxisValue;

        [Header("Trigger")]
        [SerializeField] private bool oneShot = true;
        [SerializeField] private EnterPlaybackAction enterPlaybackAction = EnterPlaybackAction.None;

        private Coroutine _fadeRoutine;
        private bool _isTriggered;
        private PlayerController _currentPlayer;
        private bool _isPlayerInside;
        private bool _isToSourceActive;
        private Collider2D _collider;

        private void Awake()
        {
            _collider =  GetComponent<Collider2D>();
        }

        private void Reset()
        {
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
            {
                return;
            }
            
            if (_isTriggered && oneShot)
            {
                _collider.enabled = false;
                return;
            }

            _currentPlayer = player;
            _isPlayerInside = true;
            ApplyEnterPlaybackAction();

            if (twoWaySwitchByPosition && useMinPositionCondition)
            {
                UpdateSwitchByPlayerPosition(player.transform.position, true);
                _isTriggered = true;
                return;
            }

            if (useMinPositionCondition && !PassesMinPositionCondition(player.transform.position))
            {
                return;
            }

            StartFade(isToActive: true);
            _isTriggered = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!twoWaySwitchByPosition || !useMinPositionCondition)
            {
                return;
            }

            if (_isTriggered && oneShot && !twoWaySwitchByPosition)
            {
                return;
            }

            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
            {
                return;
            }

            UpdateSwitchByPlayerPosition(player.transform.position, false);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || player != _currentPlayer)
            {
                return;
            }

            if (_isTriggered && oneShot) return;
            
            _isPlayerInside = false;
            _currentPlayer = null;
        }

        private bool PassesMinPositionCondition(Vector3 position)
        {
            var axisValue = conditionAxis == Axis.X ? position.x : position.y;
            return axisValue >= minAxisValue;
        }

        private void ApplyEnterPlaybackAction()
        {
            // if (toSource == null || fromSource == null)
            // {
            //     return;
            // }

            switch (enterPlaybackAction)
            {
                case EnterPlaybackAction.None:
                    break;
                case EnterPlaybackAction.PlayBoth:
                    if (toSource != null && !toSource.isPlaying) toSource?.Play();
                    if (fromSource != null && !fromSource.isPlaying) fromSource?.Play();
                    break;
                case EnterPlaybackAction.StopBoth:
                    toSource?.Stop();
                    fromSource?.Stop();
                    break;
                case EnterPlaybackAction.ToggleBoth:
                    TogglePlayback(toSource);
                    TogglePlayback(fromSource);
                    break;
            }
        }

        private static void TogglePlayback(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            if (source.isPlaying) source.Stop();
            else source.Play();
        }

        private void UpdateSwitchByPlayerPosition(Vector3 playerPosition, bool forceSwitch)
        {
            var shouldActivateToSource = PassesMinPositionCondition(playerPosition);
            if (!forceSwitch && shouldActivateToSource == _isToSourceActive)
            {
                return;
            }

            StartFade(shouldActivateToSource);
        }

        private void StartFade(bool isToActive)
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(CrossfadeRoutine(isToActive));
            _isToSourceActive = isToActive;
        }

        private IEnumerator CrossfadeRoutine(bool isToActive)
        {
            // if (toSource == null || fromSource == null)
            // {
            //     yield break;
            // }

            var duration = Mathf.Max(0.01f, fadeDuration);
            var startFromVolume = 0f;
            var startToVolume = 0f;
            if (toSource != null) startFromVolume = toSource.volume;
            if (fromSource != null) startToVolume = fromSource.volume;
            var targetFromVolume = isToActive ? 0f : 1f;
            var targetToVolume = isToActive ? 1f : 0f;

            if (fromSource != null && !fromSource.isPlaying)
            {
                fromSource.Play();
            }

            if (toSource != null && !toSource.isPlaying)
            {
                toSource.Play();
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (twoWaySwitchByPosition && useMinPositionCondition && _isPlayerInside && _currentPlayer != null)
                {
                    var shouldActivateToSource = PassesMinPositionCondition(_currentPlayer.transform.position);
                    if (shouldActivateToSource != isToActive)
                    {
                        StartFade(shouldActivateToSource);
                        yield break;
                    }
                }

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                if (toSource != null) toSource.volume = Mathf.Lerp(startFromVolume, targetFromVolume, t);
                if (fromSource != null) fromSource.volume = Mathf.Lerp(startToVolume, targetToVolume, t);
                yield return null;
            }

            if (toSource != null) toSource.volume = targetFromVolume;
            if (fromSource != null)fromSource.volume = targetToVolume;

            if (stopFromSourceAfterFade && isToActive)
            {
                if (toSource != null) toSource?.Stop();
            }

            if (stopFromSourceAfterFade && !isToActive)
            {
                if (fromSource != null) fromSource?.Stop();
            }
        }
    }
}
