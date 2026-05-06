using System;
using Interfaces;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Layouts;
using Zenject;

namespace DefaultNamespace
{
    public class TriggerEvent : Opener
    {
        [SerializeField] private bool isActiveOnStart;
        [SerializeField] private bool triggerOnDeath;
        [SerializeField] private float maxHeightToTriggerOnDeath = -1;
        [SerializeField] private UnityEvent onTriggerEnter2D;
        [SerializeField] private UnityEvent onEventTrigger;
        [SerializeField] private bool _oneShot = false;
        private bool _isTriggered = false; 
        private PlayerController _playerController;

        private void Awake()
        {
            _playerController = FindAnyObjectByType<PlayerController>();

            if (triggerOnDeath) _playerController.OnRevive += OnPlayerRevive;

            isActive = isActiveOnStart;
        }

        private void OnPlayerRevive()
        {
            if (_playerController.transform.position.y < maxHeightToTriggerOnDeath)
                onEventTrigger?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isTriggered && _oneShot) return;   
            onTriggerEnter2D?.Invoke();
            _isTriggered = true;
        }

        private void OnDestroy()
        {
            _playerController.OnRevive -= OnPlayerRevive;
        }
    }
}