using System;
using Features.Player.Application;
using Features.Core.Settings;
using Features.Interactables.Infrastructure;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using Interfaces;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Features.Player.Presentation
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerView : MonoBehaviour, IPlayerViewPort, IHittable
    {
        public Vector2 Position => transform.position;
        public Vector2 CurrentVelocity => currentVelocity;

        private PlayerFacade _facade;
        private GameSettings _settings;

        private Rigidbody2D _rb;
        private CapsuleCollider2D _capsule;
        private BoxCollider2D _boxTrigger;
        private PlayerAnimationView _animView;

        private CollisionTypes[] _layerMap = new CollisionTypes[32];

        private IDisposable _deathSub;
        private IDisposable _reviveSub;
        private IDisposable _groundedSub;
        private Vector2 currentVelocity;

        [Inject]
        public void Construct(
            PlayerFacade facade,
            GameSettings gameSettings,
            ISubscriber<PlayerDied> deathSub,
            ISubscriber<PlayerRevived> reviveSub,
            ISubscriber<PlayerGroundedChanged> groundedSub)
        {
            _facade = facade;
            _facade.BindView(this);
            _settings = gameSettings;
            _deathSub = deathSub.Subscribe(OnDeath);
            _reviveSub = reviveSub.Subscribe(OnRevive);
            _groundedSub = groundedSub.Subscribe(e => OnGroundedChanged(e.IsGrounded));
        }

        private void Awake()
        {
            if (_rb == null || _capsule == null || _boxTrigger == null)
                Init();
        }

        private void Init()
        {
            BuildLayerMap();
            _rb = GetComponent<Rigidbody2D>();
            _capsule = GetComponent<CapsuleCollider2D>();
            _boxTrigger = GetComponent<BoxCollider2D>();
        }


        private void FixedUpdate()
        {
            currentVelocity = _rb.linearVelocity;
        }

        public void ApplyHorizontalVelocity(float x)
        {
            var v = _rb.linearVelocity;
            v.x = x;
            _rb.linearVelocity = v;
        }

        public void ApplyVelocity(Vector2 v) => _rb.linearVelocity = v;

        public void ApplyForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
        {
            _rb.AddForce(force, mode);
        }

        public void ApplyCollider(Vector2 size, Vector2 offset, CapsuleDirection2D direction)
        {
            if (_rb == null || _capsule == null || _boxTrigger == null)
                Init();

            _capsule.size = size;
            _capsule.offset = offset;
            _capsule.direction = direction;

            _boxTrigger.offset = offset;
            _boxTrigger.size = size;
        }

        public void ApplyGravity(float gravity)
        {
            _rb.gravityScale = gravity;
        }

        public void ApplyLayer(int layer)
        {
            gameObject.layer = layer;
        }

        public void ApplyPosition(Vector2 position) => transform.position = position;

        private void OnGroundedChanged(bool grounded) { }

        private void OnDeath(PlayerDied _) { }

        public void Hit()
        {
            _facade?.Kill();
        }

        private void OnRevive(PlayerRevived _) { }

        private void OnCollisionStay2D(Collision2D collision)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    _facade.NotifyGroundedState(true);
                    return;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            CollisionTypes type = MapLayerToCollisionType(other.gameObject.layer);

            float? effectorSpeed = null;

            if (other.gameObject.TryGetComponent<SurfaceEffector2D>(out var effector))
                effectorSpeed = effector.speed;

            _facade.NotifyCollisionEnter(type, effectorSpeed);
        }


        private void OnCollisionExit2D(Collision2D collision)
        {
            _facade.NotifyGroundedState(false);

            if (collision.gameObject.GetComponent<SurfaceEffector2D>() != null)
                _facade.NotifyEffectorExit();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            CollisionTypes type = MapLayerToCollisionType(other.gameObject.layer);
            _facade.NotifyTriggerEnter(type);
        }

        private CollisionTypes MapLayerToCollisionType(int layer)
        {
            return _layerMap[layer];
        }

        private void BuildLayerMap()
        {
            for (int i = 0; i < 32; i++)
                _layerMap[i] = CollisionTypes.None;

            FillMask(_settings.Layers.EnemyMask, CollisionTypes.Enemy);
            FillMask(_settings.Layers.BulletMask, CollisionTypes.Bullet);
            FillMask(_settings.Layers.GroundMask, CollisionTypes.Ground);
        }

        private void FillMask(LayerMask mask, CollisionTypes type)
        {
            int value = mask.value;

            for (int i = 0; i < 32; i++)
            {
                if ((value & (1 << i)) != 0)
                    _layerMap[i] = type;
            }
        }

        private void OnDestroy()
        {
            _deathSub?.Dispose();
            _reviveSub?.Dispose();
            _groundedSub?.Dispose();
        }
    }
}
