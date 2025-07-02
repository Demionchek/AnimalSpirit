using System;
using AI;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public class Collectable : Opener
    {
        public InteractableCharacter character;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player") ||
                other.gameObject.layer == LayerMask.NameToLayer("Rat") ||
                other.gameObject.layer == LayerMask.NameToLayer("Bird"))
            {
                isActive = true;
                character.doorCondition = true;
                if (spriteRenderer != null) spriteRenderer.enabled = false;
            }
        }

    }
}