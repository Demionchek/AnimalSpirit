using Features.Trigger.Application;
using UnityEngine;

namespace Features.Trigger.Domain
{
    public abstract class TriggerConfigSO : ScriptableObject
    {
        public abstract void Execute(WorldTriggerContext context);
    }
}