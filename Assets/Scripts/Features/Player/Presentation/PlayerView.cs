using System;
using DefaultNamespace.Features.Player.Application;
using Features.Player.Domain;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace DefaultNamespace.Features.Player.Presentation
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(BoxCollider2D))]
public class PlayerView : MonoBehaviour
{
    [Inject] private PlayerService _service;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsule;
    private BoxCollider2D boxTrigger;
    private PlayerAnimationView animView;

    private IPublisher<ActualVelocityChanged> _actualVelPub;

    private IDisposable moveSub, jumpSub, barkSub, shapeSub, deathSub, reviveSub, groundedSub, velocitySub, wallSub;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        boxTrigger = GetComponent<BoxCollider2D>();
        animView = GetComponent<PlayerAnimationView>();
    }

    [Inject]
    private void Construct(
        ISubscriber<PlayerMoveInput> moveSub,
        ISubscriber<PlayerJumpPressed> jumpSub,
        ISubscriber<PlayerBarkPressed> barkSub,
        ISubscriber<PlayerShapeRequest> shapeSub,
        ISubscriber<PlayerDied> deathSub,
        ISubscriber<PlayerRevived> reviveSub,
        ISubscriber<PlayerGroundedChanged> groundedSub,
        ISubscriber<PlayerVelocityChanged> velocitySub,
        ISubscriber<PlayerWallDetected> wallSub,
        IPublisher<ActualVelocityChanged> actualVelSub)
    {
        this.moveSub = moveSub.Subscribe(OnMove);
        this.jumpSub = jumpSub.Subscribe(OnJump);
        this.barkSub = barkSub.Subscribe(OnBark);
        this.shapeSub = shapeSub.Subscribe(OnShapeRequest);
        this.deathSub = deathSub.Subscribe(OnDeath);
        this.reviveSub = reviveSub.Subscribe(OnRevive);
        this.groundedSub = groundedSub.Subscribe(e => OnGroundedChanged(e.IsGrounded));
        this.velocitySub = velocitySub.Subscribe(e => OnVelocityChanged(e.Velocity));
        this.wallSub = wallSub.Subscribe(e => OnWallDetected(e.IsWall));
        this._actualVelPub = actualVelSub;
    }



    private void OnMove(PlayerMoveInput e) => _service.Move(e.Value) ;
    private void OnJump(PlayerJumpPressed _) => _service.TryJump();
    private void OnBark(PlayerBarkPressed _) => _service.PerformBark(transform.position);
    private void OnShapeRequest(PlayerShapeRequest e) => _service.RequestShapeChange(e.Target, transform.position);

    private void OnDeath(PlayerDied _) { /* анимация смерти */ }
    private void OnRevive(PlayerRevived _) { /* респаун */ }

    private void FixedUpdate()
    {
        _actualVelPub.Publish(new ActualVelocityChanged(rb.linearVelocity));
    }

    private void OnGroundedChanged(bool isGrounded)
    {
    }

    private void OnVelocityChanged(Vector2 velocity)
    {
    }

    private void OnWallDetected(bool isWall)
    {
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        _service.OnCollisionEnter(other);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        _service.OnCollisionStay(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _service.OnCollisionExit(collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _service.OnTriggerEnter(other);
    }

    private void OnDestroy()
    {
        moveSub?.Dispose(); jumpSub?.Dispose(); barkSub?.Dispose();
        shapeSub?.Dispose(); deathSub?.Dispose(); reviveSub?.Dispose();
        groundedSub?.Dispose(); velocitySub?.Dispose(); wallSub?.Dispose();
    }
}
}