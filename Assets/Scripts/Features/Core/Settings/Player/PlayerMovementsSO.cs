using System;
using System.Collections.Generic;
using Features.Player.Domain;
using UnityEngine;

namespace Features.Core.Settings.Player
{
    [CreateAssetMenu(fileName = "PlayerMovements", menuName = "Game Settings/PlayerMovements")]

    public class PlayerMovementsSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public Shape shape;
            public float speed;
            public float jumpForce;
            public float gravityScale;
        }

        [SerializeField] private List<Entry> _entries;

        private Dictionary<Shape, (float, float, float)> _map;

        public float GetSpeed(Shape shape)
        {
            _map ??= BuildMap();
            return _map[shape].Item1;
        }

        public float GetJumpForce(Shape shape)
        {
            _map ??= BuildMap();
            return _map[shape].Item2;
        }

        public float GetGravityScale(Shape shape)
        {
            _map ??= BuildMap();
            return _map[shape].Item3;
        }

        private Dictionary<Shape, (float,float, float)> BuildMap()
        {
            var dict = new Dictionary<Shape, (float, float, float)>();
            foreach (var e in _entries)
                dict[e.shape] = (e.speed, e.jumpForce, e.gravityScale);
            return dict;
        }
    }
}