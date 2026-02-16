using Cysharp.Threading.Tasks;
using Features.Player.Domain;
using MessagePipe;

namespace Features.Player.Application
{
    public sealed class PlayerLifeService
    {
        private readonly PlayerModel _model;
        private readonly IPublisher<PlayerDied> _deathPub;
        private readonly IPublisher<PlayerRevived> _revivePub;

        public PlayerLifeService(
            PlayerModel model,
            IPublisher<PlayerDied> deathPub,
            IPublisher<PlayerRevived> revivePub)
        {
            _model = model;
            _deathPub = deathPub;
            _revivePub = revivePub;
        }

        public void Kill()
        {
            if (_model.IsDead) return;

            _model.SetDead(true);
            _deathPub.Publish(new PlayerDied());

            _ = ReviveAsync();
        }

        private async UniTask ReviveAsync()
        {
            await UniTask.Delay(3000);
            _model.SetDead(false);
            _revivePub.Publish(new PlayerRevived());
        }
    }

}