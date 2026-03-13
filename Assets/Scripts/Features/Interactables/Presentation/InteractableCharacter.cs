using System.Collections.Generic;
using Features.Core.Settings;
using Features.Core.Settings.Scene;
using Features.Interactables.Application;
using Features.Interactables.Infrastructure;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace Features.Interactables.Presentation
{
    [RequireComponent(typeof(SceneInteractionReferences))]
    public sealed class InteractableCharacter :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private InteractionConfig config;
        [SerializeField] private float interactDelay = 0.5f;
        [SerializeField] private GameObject interactSign;
        [SerializeField] private SceneInteractionReferences sceneRefs;

        private readonly List<IInteractionAction> _actions =
            new List<IInteractionAction>();

        private Animator animator;

        private float _lastInteractTime;

        [Inject]
        public void Construct(
            InteractionActionFactory factory)
        {
            if (config == null)
            {
                Debug.LogError("InteractionConfig missing", this);
                return;
            }

            animator = GetComponent<Animator>();

            if (config.startDialogue)
                _actions.Add(factory.CreateDialogue(config.dialogueId));

            if (config.unlockShape)
                _actions.Add(factory.CreateUnlock(config.shape));

            // if (config.setCheckpoint)
            //     _actions.Add(factory.CreateCheckpoint(config.checkpointIndex));

            if (config.performAttack)
                _actions.Add(factory.CreateAttackAction(animator, config.triggerName));


            if (sceneRefs.objectToActivate != null)
                _actions.Add(factory.CreateActivate(
                    sceneRefs.objectToActivate));

            if (sceneRefs.doorToOpen != null)
                _actions.Add(factory.CreateDoor(
                    sceneRefs.doorToOpen,
                    sceneRefs.manualDoorOpen));

            if (sceneRefs.audioSource != null && sceneRefs.audioClip != null)
                _actions.Add(factory.CreateAudioSource(sceneRefs.audioSource, sceneRefs.audioClip));


        }

        public void Interact()
        {
            if (Time.time - _lastInteractTime < interactDelay)
                return;

            _lastInteractTime = Time.time;

            interactSign?.SetActive(false);

            foreach (var action in _actions)
                action.Execute();
        }
    }
}