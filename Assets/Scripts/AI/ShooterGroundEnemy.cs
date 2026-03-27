using System;
using System.Collections;
using AI.States;
using Features.Interactables.Infrastructure;
using Interfaces;
using ObjectPool;
using UnityEngine;

namespace AI
{
    public class ShooterGroundEnemy : BaseEnemy, IHittable
    {

        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform shootPosLeft;
        [SerializeField] private Transform shootPosRight;
        private GameObjectPool pool;

        private void Start()
        {
            Init();
            pool = new GameObjectPool(bulletPrefab, 10);
            StartCoroutine(DetectionRoutine());
            ChangeState<PatrolStateAI>();
            audioSource = GetComponent<AudioSource>();

            StartCoroutine(StateUpdateCoroutine());

        }

        private void Update()
        {
            currState?.StateUpdate();
            currentAttackTime = Time.time;
        }

        private IEnumerator StateUpdateCoroutine()
        {
            while (!isDead)
            {
                yield return new WaitForSeconds(0.5f);

                bool isAttackState = currState is AttackStateAI;

                if (canSeeTarget && !isAttackState )
                {
                    ChangeState<AttackStateAI>();
                }

                if (!canSeeTarget )
                {
                    ChangeState<PatrolStateAI>();
                }
            }
        }

        private void FixedUpdate()
        {
            currState?.StateFixedUpdate();
        }

        public void OnFire()
        {
            if (target == null) return;

            GameObject bullet = pool.Get();
            bullet.transform.position = AnimationController.GetSpriteRenderer().flipX ? shootPosLeft.position : shootPosRight.position;
            Vector2 targetPosition = new Vector2(target.transform.position.x, target.transform.position.y + 0.15f);
            // Направление к цели
            Vector2 direction = targetPosition - (Vector2)bullet.transform.position;
            // Вычисляем угол в радианах и конвертируем в градусы
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Поворачиваем объект (для 2D обычно используется ось Z)
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            audioSource.Play();
        }

        public void Hit()
        {
            isDead = true;
            ChangeState<DeathState>();
        }
    }
}