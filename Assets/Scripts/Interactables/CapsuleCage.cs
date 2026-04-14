using System;
using System.Collections;
using Animations;
using UnityEngine;

namespace Interactables
{
    public class CapsuleCage : MonoBehaviour
    {
        [SerializeField] private Transform _prisonier;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _moveDuration = 1f;
        [SerializeField] private float _yieldDuration = 1f;
        [SerializeField] private float _moveYDelta = 1f;
        private Animator  _animator;
        private Coroutine _moveCoroutine;
        private bool isOpen = false;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Open()
        {
            if (isOpen) return;

            isOpen = true;

            _animator.SetTrigger(AnimationController.IS_OPEN_S);

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MovePrisonierByCurve());
        }

        private IEnumerator MovePrisonierByCurve()
        {
            yield return new WaitForSeconds(_yieldDuration);

            if (_prisonier == null || _moveDuration <= 0f)
            {
                yield break;
            }

            Vector3 startLocalPosition = _prisonier.localPosition;
            float elapsed = 0f;
            AnimationCurve curve = _curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
            float curveStartValue = curve.Evaluate(0f);
            float curveEndValue = curve.Evaluate(1f);
            float curveRange = curveEndValue - curveStartValue;

            while (elapsed < _moveDuration)
            {
                float normalizedTime = elapsed / _moveDuration;
                float curveValue = curve.Evaluate(normalizedTime);
                float progress = Mathf.Abs(curveRange) > Mathf.Epsilon
                    ? (curveValue - curveStartValue) / curveRange
                    : normalizedTime;

                Vector3 nextPosition = startLocalPosition;
                nextPosition.y = startLocalPosition.y + progress * _moveYDelta;
                _prisonier.localPosition = nextPosition;

                elapsed += Time.deltaTime;
                yield return null;
            }

            Vector3 finalPosition = startLocalPosition;
            finalPosition.y = startLocalPosition.y + _moveYDelta;
            _prisonier.localPosition = finalPosition;
            _moveCoroutine = null;
        }

    }
}
