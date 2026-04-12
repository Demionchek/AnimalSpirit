using System.Collections.Generic;
using Features.Core.Settings;
using Features.Core.Settings.Scene;
using Features.Cutscene.Infrastructure;
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
        [SerializeField] private InteractionRequirementGate requirementGate;
        private SceneInteractionReferences _sceneRefs;
        private InteractableCharacterPhysicsPort _physicsPort;
        private InteractionActionFactory _factory;
        private ICutscenePort _cutscenePort;

        private readonly List<IInteractionAction> _actions =
            new List<IInteractionAction>();

        private Animator _animator;
        private float _lastInteractTime;
        private int _dialogueActionIndex = -1;
        private int _currentDialogueIndex;

        public InteractionConfig Config => config;

        [Inject]
        public void Construct(
            InteractionActionFactory factory,
            ICutscenePort cutscenePort)
        {
            _factory = factory;
            _cutscenePort = cutscenePort;

            if (config == null)
            {
                Debug.LogError("InteractionConfig missing", this);
                return;
            }

            _animator = GetComponent<Animator>();
            _sceneRefs = GetComponent<SceneInteractionReferences>();
            _physicsPort = GetComponent<InteractableCharacterPhysicsPort>();

            if (config.startTimeline)
                _actions.Add(factory.CreateTimeline(cutscenePort,
                    config.timelineId));

            if (config.startDialogue)
            {
                _currentDialogueIndex = 0;
                ReplaceDialogueAction(_currentDialogueIndex);
            }

            if (config.unlockShape)
                _actions.Add(factory.CreateUnlock(config.shape));

            if (config.setCheckpoint)
                _actions.Add(factory.CreateCheckpoint(
                    config.checkpointIndex));

            if (config.performAttack)
                _actions.Add(factory.CreateAttackAction(
                    _animator,
                    config.triggerName,
                    _physicsPort));

            if (_sceneRefs.objectToActivate != null)
                _actions.Add(factory.CreateActivate(
                    _sceneRefs.objectToActivate));

            if (_sceneRefs.doorToOpen != null)
                _actions.Add(factory.CreateDoor(
                    _sceneRefs.doorToOpen,
                    _sceneRefs.manualDoorOpen));

            if (_sceneRefs.audioSource != null && _sceneRefs.audioClip != null)
                _actions.Add(factory.CreateAudioSource(_sceneRefs.audioSource, _sceneRefs.audioClip));
        }

        public void Interact()
        {
            if (Time.time - _lastInteractTime < interactDelay)
                return;

            _lastInteractTime = Time.time;

            if (requirementGate != null && !requirementGate.IsSatisfied())
            {
                if (requirementGate.MissingDialogueId >= 0)
                    _factory.CreateDialogue(requirementGate.MissingDialogueId)
                            .Execute();

                return;
            }

            interactSign?.SetActive(false);

            foreach (var action in _actions)
                action.Execute();
        }

        public void SetDialogueIndex(int dialogueIndex)
        {
            if (config == null || _factory == null)
                return;

            _currentDialogueIndex = dialogueIndex;
            ReplaceDialogueAction(_currentDialogueIndex);
        }

        private void ReplaceDialogueAction(int dialogueIndex)
        {
            var dialogueId = config.GetDialogueId(dialogueIndex);
            if (dialogueId < 0)
                return;

            var dialogueAction = _factory.CreateDialogue(dialogueId);
            if (_dialogueActionIndex >= 0 && _dialogueActionIndex < _actions.Count)
            {
                _actions[_dialogueActionIndex] = dialogueAction;
                return;
            }

            _dialogueActionIndex = _actions.Count;
            _actions.Add(dialogueAction);
        }

        public void AddTimelineAction(int index)
        {
            _actions.Insert(0,_factory.CreateTimeline(_cutscenePort ,index));
        }
    }
}
