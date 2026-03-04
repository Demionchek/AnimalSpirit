using Features.Trigger.Application;
using Features.Trigger.Domain;
using UnityEngine;

namespace Features.Core.Settings.Triggers
{
    [CreateAssetMenu(menuName = "Triggers/Cutscene")]
    public sealed class CutsceneTriggerConfig : TriggerConfigSO
    {
        [SerializeField] private int cutsceneIndex;

        public override void Execute(WorldTriggerContext context)
        {
            context.Cutscene.PlaySingle(cutsceneIndex);
        }
    }
}