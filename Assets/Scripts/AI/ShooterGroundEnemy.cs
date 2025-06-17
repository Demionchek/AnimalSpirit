using System;
using AI.States;

namespace AI
{
    public class ShooterGroundEnemy : BaseEnemy
    {
        private void Start()
        {
            Init();
            ChangeState<PatrolStateAI>();
            StartCoroutine(DetectionRoutine());
        }

        private void Update()
        {

        }
    }
}