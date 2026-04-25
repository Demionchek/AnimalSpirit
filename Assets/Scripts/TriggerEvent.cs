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
        [SerializeField] private UnityEvent onTriggerEnter2D;
        [SerializeField] private UnityEvent onEventTrigger;

        private PlayerController _playerController;

        private void Awake()
        {
            _playerController = FindAnyObjectByType<PlayerController>();

            if (triggerOnDeath) _playerController.OnRevive += OnPlayerRevive;

            isActive = isActiveOnStart;
        }

        private void OnPlayerRevive()
        {
            onEventTrigger?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            onTriggerEnter2D?.Invoke();
        }

        private void OnDestroy()
        {
            _playerController.OnRevive -= OnPlayerRevive;
        }
    }
}