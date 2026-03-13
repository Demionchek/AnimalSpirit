using System;
using System.Collections.Generic;
using Features.Player.Domain;
using UnityEngine;

namespace Features.Core.Settings.Player
{
    [CreateAssetMenu(fileName = "Shape Settings", menuName = "Game Settings/Create Shape Settings")]
    public class ShapesColliderSettingSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public Shape shape;
            public Vector2 colliderSize;
            public Vector2 colliderOffset;
            public CapsuleDirection2D colliderDir;
            public LayerMask colliderLayer;
        }

        [SerializeField] private List<Entry> _entries;

        private Dictionary<Shape, (Vector2, Vector2, CapsuleDirection2D, LayerMask)> _map;

        public Vector2 GetSize(Shape shape)
        {
            _map ??= BuildMap();
            return _map[shape].Item1;
        }

        public Vector2 GetOffset(Shape shape)
        {
            _map ??= BuildMap();
            return _map[shape].Item2;
        }

        public CapsuleDirection2D GetDirection(Shape shape)
        {
            _map ??= BuildMap();
            return _map[shape].Item3;
        }

        public LayerMask GetLayer(Shape shape)
        {
            _map ??= BuildMap();
            return _map[shape].Item4;
        }

        private Dictionary<Shape, (Vector2,Vector2, CapsuleDirection2D, LayerMask)> BuildMap()
        {
            var dict = new Dictionary<Shape, (Vector2, Vector2, CapsuleDirection2D, LayerMask)>();
            foreach (var e in _entries)
                dict[e.shape] = (e.colliderSize, e.colliderOffset, e.colliderDir, e.colliderLayer);
            return dict;
        }
    }
}