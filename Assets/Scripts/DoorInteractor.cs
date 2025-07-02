using System;
using Animations;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public class DoorInteractor : MonoBehaviour
    {
        [SerializeField] public Opener[] openers;

        private BoxCollider2D boxCollider2D;

        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
            boxCollider2D = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            if (openers == null || openers.Length == 0) return;

            int count = openers.Length;
            int activeCount = 0;
            foreach (var opener in openers)
            {
                if (opener.isActive) activeCount++;
            }

            if (count == activeCount)
            {
                animator.SetBool(AnimationController.IS_OPEN_S, true);
                boxCollider2D.enabled = false;
            } else
            {
                animator.SetBool(AnimationController.IS_OPEN_S, false);
                boxCollider2D.enabled = true;
            }
        }

        public void OpenManual()
        {
            animator.SetBool(AnimationController.IS_OPEN_S, true);
            boxCollider2D.enabled = false;
        }
    }
}