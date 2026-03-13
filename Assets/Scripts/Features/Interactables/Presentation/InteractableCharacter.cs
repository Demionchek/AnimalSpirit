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

            if (config.startDialogue)
                _actions.Add(factory.CreateDialogue(config.dialogueId));

            if (config.unlockShape)
                _actions.Add(factory.CreateUnlock(config.shape));

            // if (config.setCheckpoint)
            //     _actions.Add(factory.CreateCheckpoint(config.checkpointIndex));

            if (sceneRefs.objectToActivate != null)
                _actions.Add(factory.CreateActivate(
                    sceneRefs.objectToActivate));

            if (sceneRefs.doorToOpen != null)
                _actions.Add(factory.CreateDoor(
                    sceneRefs.doorToOpen,
                    sceneRefs.manualDoorOpen));
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