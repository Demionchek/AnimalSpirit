using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.Settings.AI;
using UnityEngine;

namespace Features.AI.Application
{
    public sealed class EnemyCombatService : IEnemyCombatPort
    {
        private readonly EnemyModel _model;
        private readonly EnemyTypeConfig _config;

        private IEnemyAttack _attack;

        public EnemyCombatService(
            EnemyModel model,
            EnemyTypeConfig config)
        {
            _model = model;
            _config = config;
        }

        public void Initialize(IEnemyAttack attack)
        {
            _attack = attack;
        }

        public void TryAttack()
        {
            if (_model.Target == null)
                return;

            if (Time.time < _model.LastAttackTime + _config.attackDelay)
                return;

            _model.LastAttackTime = Time.time;

            _attack.Execute(_model);
        }
    }
}
