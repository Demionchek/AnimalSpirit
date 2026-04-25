using System;
using System.Collections;
using AI;
using Animations;
using UnityEngine;

namespace Interactables
{
    public class CapsuleCage : MonoBehaviour
    {
        [SerializeField] private Transform _prisoner;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _moveDuration = 1f;
        [SerializeField] private float _yieldDuration = 1f;
        [SerializeField] private float _moveYDelta = 1f;
        private Animator  _animator;
        private Collider2D _collider2D;
        private Coroutine _moveCoroutine;
        private AudioSource _audioSource;
        private bool isOpen = false;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _collider2D = GetComponent<Collider2D>();
            _audioSource = GetComponent<AudioSource>();
        }

        public void Open()
        {
            if (isOpen) return;

            isOpen = true;

            _animator.SetTrigger(AnimationController.IS_OPEN_S);
            _audioSource.Play();

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MovePrisonerByCurve());
        }

        private IEnumerator MovePrisonerByCurve()
        {
            yield return new WaitForSeconds(_yieldDuration);

            _prisoner.TryGetComponent(out FriendlyNpcEnemy friendlyNpc);

            friendlyNpc?.SetSortingOrder(7);

            if (_prisoner == null || _moveDuration <= 0f)
            {
                yield break;
            }

            Vector3 startLocalPosition = _prisoner.localPosition;
            float elapsed = 0f;
            AnimationCurve curve = _curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);

            while (elapsed < _moveDuration)
            {
                float normalizedTime = elapsed / _moveDuration;
                float curveValue = curve.Evaluate(normalizedTime);

                Vector3 nextPosition = startLocalPosition;
                nextPosition.y = startLocalPosition.y + curveValue * _moveYDelta;
                _prisoner.localPosition = nextPosition;

                elapsed += Time.deltaTime;
                yield return null;
            }

            _collider2D.enabled = false;
            Vector3 finalPosition = startLocalPosition;
            finalPosition.y = startLocalPosition.y + curve.Evaluate(1f) * _moveYDelta;
            _prisoner.localPosition = finalPosition;
            _moveCoroutine = null;

            friendlyNpc?.Activate();
        }

    }
}
