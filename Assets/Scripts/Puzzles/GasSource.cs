using System;
using Interfaces;
using Player;
using UnityEngine;

namespace Puzzles
{
    public class GasSource : MonoBehaviour
    {
        [SerializeField] private bool isActive = true;
        [SerializeField] private SpriteRenderer gasSprite;
        [SerializeField] private Collider2D gasCollider;

        public bool IsActive => isActive;

        private void Start()
        {
            UpdateVisuals();
        }

        public void Toggle()
        {
            isActive = !isActive;
            UpdateVisuals();
            Debug.Log($"Gas source {name} is now {(isActive ? "ON" : "OFF")}");
        }

        public void SetActive(bool active)
        {
            isActive = active;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (gasSprite != null)
                gasSprite.enabled = isActive;

            if (gasCollider != null)
                gasCollider.enabled = isActive;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")
                || other.gameObject.layer == LayerMask.NameToLayer("Rat")
                || other.gameObject.layer == LayerMask.NameToLayer("Bird"))
            {
                IHittable hittable = other.GetComponent<IHittable>();
                hittable?.Hit();
            }
        }
    }
}
