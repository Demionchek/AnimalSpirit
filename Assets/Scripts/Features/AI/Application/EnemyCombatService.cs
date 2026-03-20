using Features.AI.Domain;
using Features.AI.Infrastructure;
using Features.Core.Settings.AI;
using UnityEngine;

namespace Features.AI.Application
{
    public sealed class EnemyCombatService
    {
        private readonly EnemyModel _model;
        private readonly EnemyTypeConfig _config;
        private readonly IEnemyAttack _attack;

        public EnemyCombatService(
            EnemyModel model,
            EnemyTypeConfig config,
            IEnemyAttack attack)
        {
            _model = model;
            _config = config;
            _attack = attack;
        }

        public void TryAttack()
        {
            if (_model.Target == null)
                return;

            float time = Time.time;

            if (time < _model.LastAttackTime + _config.attackDelay)
                return;

            _model.LastAttackTime = time;

            _attack.Execute(_model);
        }
    }
}