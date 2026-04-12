using Features.Player.Domain;
using System.Collections.Generic;
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

        public void SetInitialState(IEnumerable<Shape> unlockedShapes, Shape selectedShape)
        {
            var unlocked = unlockedShapes != null
                ? new HashSet<Shape>(unlockedShapes)
                : new HashSet<Shape>();

            foreach (var icon in icons)
            {
                bool isUnlocked = unlocked.Contains(icon.Shape);
                icon.SetUnlocked(isUnlocked);
                icon.SetSelected(isUnlocked && icon.Shape == selectedShape);
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
