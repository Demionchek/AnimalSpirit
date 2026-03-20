using Features.Core.ObjectPool.Infrastructure;
using Interfaces;
using UnityEngine;

namespace Features.Core.ObjectPool.Presentation
{

    public sealed class BulletView : MonoBehaviour
    {
        private IObjectPool<BulletView> _pool;
        private Rigidbody2D _rb;

        [SerializeField] private float speed = 5f;
        [SerializeField] private float lifetime = 3f;

        private float _timer;

        public void Init(IObjectPool<BulletView> pool)
        {
            _pool = pool;
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Fire(Vector2 direction)
        {
            _timer = lifetime;
            _rb.linearVelocity = direction * speed;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                Release();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IHittable>(out var h))
            {
                h.Hit();
            }

            Release();
        }

        private void Release()
        {
            _rb.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
            _pool.Release(this);
        }
    }
}