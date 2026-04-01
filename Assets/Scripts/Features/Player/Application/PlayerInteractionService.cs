using Features.Core.Settings;
using Features.Player.Domain;
using Features.Player.Infrastructure;
using MessagePipe;
using UnityEngine;

namespace Features.Player.Application
{
public sealed class PlayerInteractionService
{
    private readonly PlayerModel _model;
    private readonly IPlayerPhysicsPort _physics;
    private readonly IPlayerAnimationPort _animation;
    private readonly GameSettings _settings;

    private float _cooldown;

    public PlayerInteractionService(
        PlayerModel model,
        IPlayerPhysicsPort physics,
        IPlayerAnimationPort animation,
        GameSettings settings)
    {
        _model = model;
        _physics = physics;
        _settings = settings;
        _animation = animation;
    }

    public void Tick(float deltaTime)
    {
        _cooldown -= deltaTime;
    }

    public bool TryInteract()
    {
        if (_model.IsDead || _cooldown > 0f)
            return false;

        _cooldown = 0.5f;

        var cast = _settings.PlayerSphereCastSettings;
        Vector2 offset = new(
            _animation.isFlipped ? -cast.offsetX : cast.offsetX,
            cast.offsetY
        );

        var interactables =
            _physics.OverlapInteractables( offset,
                _settings.PlayerSphereCastSettings.radius);

        foreach (var i in interactables)
            i.Interact();

        return true;
    }
}

}