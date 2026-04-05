using Features.Interactables.Presentation;
using Features.Player.Domain;
using UnityEngine;

namespace Features.Core.Settings.Scene
{
    [CreateAssetMenu(menuName = "Interaction/Interaction Config")]
    public sealed class InteractionConfig : ScriptableObject
    {
        [Header("Dialogue")]
        public bool startDialogue;
        public int[] dialogueId;

        [Header("Timeline")]
        public bool startTimeline;
        public int timelineId;

        [Header("Unlock Shape")]
        public bool unlockShape;
        public Shape shape;

        [Header("Checkpoint")]
        public bool setCheckpoint;
        public int checkpointIndex;

        [Header("Attack")]
        public bool performAttack;
        public string triggerName;

        [Header("Physics")]
        public float circleRadius;
        public float xDistance;
        public float yDistance;

        public int GetDialogueId(int index)
        {
            if (dialogueId == null || dialogueId.Length == 0)
                return -1;

            if (index < 0 || index >= dialogueId.Length)
                return -1;

            return dialogueId[index];
        }
    }
}
