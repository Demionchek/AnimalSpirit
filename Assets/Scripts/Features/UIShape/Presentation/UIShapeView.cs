using Features.Player.Domain;
using UnityEngine;

namespace Features.UIShape.Presentation
{
    public sealed class UIShapeView : MonoBehaviour
    {
        [SerializeField] private Vector2 selectedScale;
        [SerializeField] private Vector2 unselectedScale;
        [SerializeField]private ShapeIconView[] icons;


        private void Awake()
        {
            if (icons == null || icons.Length == 0)
            {
                Debug.LogError("UIShapeView: No icons were selected");
                return;
            }

            foreach (var icon in icons)
            {
                icon.Initialize(selectedScale, unselectedScale);
            }
        }

        public void SetShape(Shape shape)
        {
            foreach (var icon in icons)
            {
                icon.SetSelected(icon.Shape == shape);
            }
        }

        public void UnlockShape(Shape shape)
        {
            foreach (var icon in icons)
            {
                if (icon.Shape == shape)
                    icon.SetUnlocked(true);
            }
        }

        public void LockShape(Shape shape)
        {
            foreach (var icon in icons)
            {
                if (icon.Shape == shape)
                    icon.SetUnlocked(false);
            }
        }
    }
}