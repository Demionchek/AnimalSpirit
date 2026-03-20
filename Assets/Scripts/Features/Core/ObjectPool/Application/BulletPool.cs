using System.Collections.Generic;
using Features.Core.ObjectPool.Infrastructure;
using Features.Core.ObjectPool.Presentation;
using UnityEngine;

namespace Features.Core.ObjectPool.Application
{
    public sealed class BulletPool : IObjectPool<BulletView>
    {
        private readonly BulletView _prefab;
        private readonly Transform _parent;

        private readonly Stack<BulletView> _pool = new();

        public BulletPool(BulletView prefab, int initialSize, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                Create();
            }
        }

        private BulletView Create()
        {
            var bullet = Object.Instantiate(_prefab, _parent);
            bullet.Init(this);
            bullet.gameObject.SetActive(false);
            _pool.Push(bullet);
            return bullet;
        }

        public BulletView Get()
        {
            if (_pool.Count == 0)
                Create();

            var bullet = _pool.Pop();
            return bullet;
        }

        public void Release(BulletView obj)
        {
            _pool.Push(obj);
        }
    }
}