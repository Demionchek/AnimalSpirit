using System;
using Animations;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public class ButtonInteractor : Opener
    {
        [SerializeField] private LayerMask[] interactLayers;

        private int collidingObjectsCount = 0; // Счетчик объектов в триггере
        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsLayerInInteractLayers(other.gameObject.layer))
            {
                collidingObjectsCount++;
                UpdateButtonState();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsLayerInInteractLayers(other.gameObject.layer))
            {
                collidingObjectsCount--;
                if (collidingObjectsCount < 0) collidingObjectsCount = 0; // Защита от отрицательных значений
                UpdateButtonState();
            }
        }

        private void UpdateButtonState()
        {
            bool newState = collidingObjectsCount > 0;
            if (newState != isActive)
            {
                isActive = newState;
                animator.SetBool(AnimationController.IS_ACTIVE_S, isActive);
            }
        }

        private bool IsLayerInInteractLayers(int layer)
        {
            int layerMask = 1 << layer;
            foreach (var interactLayer in interactLayers)
            {
                if (layerMask == interactLayer)
                {
                    return true;
                }
            }
            return false;
        }

    }
}