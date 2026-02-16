using System.Collections.Generic;
using Features.Core.Settings;
using Features.Player.Domain;
using Interfaces;
using UnityEngine;

namespace Features.Player.Infrastructure
{
    public class UnityPlayerPhysicsPort : IPlayerPhysicsPort
    {

        private readonly Transform _transform;
        private readonly PlayerModel _model;
        private readonly GameSettings _gameSettings;

        public UnityPlayerPhysicsPort(Transform transform, PlayerModel model, GameSettings gameSettings)
        {
            _transform = transform;
            _model = model;
            _gameSettings = gameSettings;
        }

        public bool HasWall(float direction, float distance)
        {
            var hit = Physics2D.Raycast(_transform.position, Vector2.right * direction, distance);
            return hit.collider != null;
        }

        public bool HasSpaceAbove()
        {
            Vector2 currentOffset = _gameSettings.ShapesColliderSettings.GetOffset(_model.CurrentShape) + (Vector2)_transform.position;
            float distance = (_gameSettings.ShapesColliderSettings.GetOffset(_model.CurrentShape).y
                              + _transform.position.y + _gameSettings.ShapesColliderSettings.GetSize(Shape.Dog).y / 2) - currentOffset.y;
            int layerMask = 1 << LayerMask.NameToLayer("Ground");

            return !Physics2D.Raycast(_transform.position, Vector2.up, distance, layerMask);
        }

        public IReadOnlyList<IInteractable> OverlapInteractables( float radius)
        {
            Vector2 origin = _gameSettings.ShapesColliderSettings.GetOffset(_model.CurrentShape) + (Vector2)_transform.position;
            Collider2D[] results = new Collider2D[10];
            Physics2D.OverlapCircleNonAlloc(origin, radius, results);
            var list = new List<IInteractable>();

            foreach (var h in results)
                if (h.TryGetComponent<IInteractable>(out var i))
                    list.Add(i);

            return list;
        }
    }
}