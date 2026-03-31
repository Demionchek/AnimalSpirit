using Cysharp.Threading.Tasks;
using Features.Player.Domain;
using MessagePipe;

namespace Features.Player.Application
{
    public sealed class PlayerLifeService
    {
        private readonly PlayerModel _model;

        public PlayerLifeService(PlayerModel model)
        {
            _model = model;
        }

        public bool TryKill()
        {
            if (_model.IsDead)
                return false;

            _model.SetDead(true);
            return true;
        }

        public void Revive()
        {
            _model.SetDead(false);

        }
    }

}