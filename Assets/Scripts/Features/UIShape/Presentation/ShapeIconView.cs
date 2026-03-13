using Features.Player.Domain;
using UnityEngine;

namespace Features.UIShape.Presentation
{
    public sealed class ShapeIconView : MonoBehaviour
    {
        [SerializeField] private Shape shape;
        [SerializeField] private Transform icon;

        private Vector3 _selectedScale;
        private Vector3 _unselectedScale;

        public Shape Shape => shape;

        public void Initialize(Vector3 selected, Vector3 unselected)
        {
            _selectedScale = selected;
            _unselectedScale = unselected;
        }

        public void SetSelected(bool selected)
        {
            icon.localScale = selected ? _selectedScale : _unselectedScale;
        }

        public void SetUnlocked(bool unlocked)
        {
            icon.gameObject.SetActive(unlocked);
        }
    }
}