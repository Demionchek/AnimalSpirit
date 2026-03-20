using Features.AI.Domain;
using Features.AI.Infrastructure;
using UnityEngine;

namespace Features.AI.Application.Attacks
{
    public sealed class FlameAttack : IEnemyAttack
    {
        private readonly GameObject _fire;

        public FlameAttack(GameObject fire)
        {
            _fire = fire;
        }

        public void Execute(EnemyModel model)
        {
            _fire.SetActive(true);
        }
    }
}