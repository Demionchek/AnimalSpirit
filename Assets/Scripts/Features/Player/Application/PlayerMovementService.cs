using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using UnityEngine;

namespace Features.Player.Application
{
    public sealed class PlayerMovementService
    {
        private readonly PlayerModel _model;
    private readonly GameSettings _settings;
    private readonly IPlayerPhysicsPort _physics;
    private readonly IPlayerViewPort _view;

    private Vector2 _currentInput;
    private Vector2 _effectorVelocity;

    public PlayerMovementService(
        PlayerModel model,
        GameSettings settings,
        IPlayerPhysicsPort physics,
        IPlayerViewPort view)
    {
        _model = model;
        _settings = settings;
        _physics = physics;
        _view = view;
    }

    public void SetInput(Vector2 input)
    {
        _currentInput = input;
    }

    public void FixedTick()
    {
        if (_model.IsDead) return;

        var velocity = CalculateVelocity();
        _view.ApplyVelocity(velocity);
    }

    private Vector2 CalculateVelocity()
    {
        float speed = _settings.PlayerMovements.GetSpeed(_model.CurrentShape);
        Vector2 currentVelocity = _view.CurrentVelocity;

        if (_model.CurrentShape == Shape.Bird)
            return _currentInput * speed;

        bool wall = false;

        if (Mathf.Abs(_currentInput.x) > 0.1f)
        {
            float dir = Mathf.Sign(_currentInput.x);
            float dist = _settings.ShapesColliderSettings
                .GetSize(_model.CurrentShape).x * 0.5f;

            wall = _physics.HasWall(dir, dist);
        }

        float horizontal = wall ? 0f : _currentInput.x * speed;
        return new Vector2(horizontal + _effectorVelocity.x, currentVelocity.y);
    }

    public void Jump()
    {
        if (!_model.IsGrounded || _model.IsDead ||
            _model.CurrentShape == Shape.Bird)
            return;

        float force = _settings.PlayerMovements
            .GetJumpForce(_model.CurrentShape);

        _view.ApplyForce(Vector2.up * force);
    }

    public void SetGrounded(bool grounded)
    {
        _model.SetGrounded(grounded);
    }

    public void SetEffectorVelocity(float speed)
    {
        _effectorVelocity = new Vector2(speed, 0);
    }

    public void ClearEffector()
    {
        _effectorVelocity = Vector2.zero;
    }
    }
}