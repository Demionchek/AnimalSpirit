using Features.Interactables.Presentation;
using Features.Player.Domain;
using UnityEngine;

namespace Features.Core.Settings
{
    [CreateAssetMenu(menuName = "Interaction/Interaction Config")]
    public sealed class InteractionConfig : ScriptableObject
    {
        [Header("Dialogue")]
        public bool startDialogue;
        public int dialogueId;

        [Header("Unlock Shape")]
        public bool unlockShape;
        public Shape shape;

        [Header("Checkpoint")]
        public bool setCheckpoint;
        public int checkpointIndex;
    }
}