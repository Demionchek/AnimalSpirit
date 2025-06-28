using System;
using Animations;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public class DoorInteractor : MonoBehaviour
    {
        [SerializeField] private ButtonInteractor buttonInteractor;

        private BoxCollider2D boxCollider2D;

        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
            boxCollider2D = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            if (buttonInteractor == null) return;

            if (buttonInteractor.IsPressed)
            {
                animator.SetBool(AnimationController.IS_OPEN_S, true);
                boxCollider2D.enabled = false;
            } else
            {
                animator.SetBool(AnimationController.IS_OPEN_S, false);
                boxCollider2D.enabled = true;
            }
        }
    }
}