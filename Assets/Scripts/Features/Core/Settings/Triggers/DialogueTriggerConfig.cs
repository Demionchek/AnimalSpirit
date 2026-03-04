using Features.Trigger.Application;
using Features.Trigger.Domain;
using UnityEngine;

namespace Features.Core.Settings.Triggers
{
    [CreateAssetMenu(menuName = "Triggers/Dialogue")]
    public sealed class DialogueTriggerConfig : TriggerConfigSO
    {
        [SerializeField] private int dialogueId;

        public override void Execute(WorldTriggerContext context)
        {
            context.Dialogue.StartDialogue(dialogueId);
        }
    }
}