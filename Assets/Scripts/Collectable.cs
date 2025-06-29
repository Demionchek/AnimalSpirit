using System;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public class Collectable : Opener
    {
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

                if (spriteRenderer != null) spriteRenderer.enabled = false;
            }
        }

    }
}