using System.Collections;
using Interfaces;
using Interactables;
using ObjectPool;
using Player;
using UnityEngine;

namespace AI.Bosses.Machine
{
    public class MachineBossController : MonoBehaviour, IHittable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 10;

        [Header("Phases")]
        [SerializeField] private float attackPhaseDuration = 12f;
        [SerializeField] private float cooldownDuration = 5f;
        [SerializeField] private float maxAttackDelay = 2.5f;
        [SerializeField] private float minAttackDelay = 0.7f;
        [SerializeField] private bool startInAttackPhase = true;

        [Header("References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private MachineAnimationController animationController;
        [SerializeField] private InteractableCharacter[] interactionCharacters;

        [Header("Rocket Attack")]
        [SerializeField] private MachineHomingRocket rocketPrefab;
        [SerializeField] private int rocketPoolPreloadCount = 8;
        [SerializeField] private Transform[] rocketSpawnPoints;
        [SerializeField] private Transform[] rocketRisePoints;

        [Header("Laser Attack")]
        [SerializeField] private MachineMovingLaser[] verticalWallLasers;
        [SerializeField] private MachineMovingLaser[] horizontalWallLasers;
        [SerializeField] private Vector2 laserVerticalMoveDistanceRange = new Vector2(1f, 3f);
        [SerializeField] private Vector2 laserHorizontalMoveDistanceRange = new Vector2(1f, 3f);
        [SerializeField] private Vector2 laserMoveSpeedRange = new Vector2(1f, 3f);

        private MachineMovingLaser activeVerticalLaser;
        private MachineMovingLaser activeHorizontalLaser;
        private Coroutine phaseRoutine;
        private GameObjectPool rocketPool;
        private int health;
        private bool isDead;
        private bool cooldownInterrupted;

        private void Awake()
        {
            health = maxHealth;

            if (animationController == null)
            {
                animationController = GetComponent<MachineAnimationController>();
            }

            if (rocketPrefab != null)
            {
                rocketPool = new GameObjectPool(rocketPrefab.gameObject, rocketPoolPreloadCount);
            }

            SetInteractionCharactersEnabled(false);
        }

        private void OnEnable()
        {
            SubscribeInteractionCharacters();
            phaseRoutine = StartCoroutine(PhaseLoop());
        }

        private void OnDisable()
        {
            UnsubscribeInteractionCharacters();

            if (phaseRoutine != null)
            {
                StopCoroutine(phaseRoutine);
                phaseRoutine = null;
            }

            StopLasers();
        }

        public void Hit()
        {
            if (isDead) return;

            health = Mathf.Max(health - 1, 0);

            if (health <= 0)
            {
                Die();
                return;
            }

            animationController?.TriggerHurt();
        }

        public void EndCooldown()
        {
            cooldownInterrupted = true;
        }

        private IEnumerator PhaseLoop()
        {
            if (!startInAttackPhase)
            {
                yield return CooldownPhase();
            }

            while (!isDead)
            {
                yield return AttackPhase();
                yield return CooldownPhase();
            }
        }

        private IEnumerator AttackPhase()
        {
            SetInteractionCharactersEnabled(false);

            float phaseTime = 0f;
            float attackTimer = 0f;

            while (!isDead && phaseTime < attackPhaseDuration)
            {
                phaseTime += Time.deltaTime;
                attackTimer += Time.deltaTime;

                float currentDelay = GetCurrentAttackDelay();
                if (attackTimer >= currentDelay)
                {
                    attackTimer = 0f;
                    DoRandomAttack();
                }

                yield return null;
            }
        }

        private IEnumerator CooldownPhase()
        {
            StopLasers();
            cooldownInterrupted = false;
            SetInteractionCharactersEnabled(true);

            float timer = 0f;
            while (!isDead && !cooldownInterrupted && timer < cooldownDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            SetInteractionCharactersEnabled(false);
        }

        private void DoRandomAttack()
        {
            bool canRocket = rocketPrefab != null && rocketSpawnPoints != null && rocketSpawnPoints.Length > 0;
            bool canLaser = HasLaserAttack();

            if (!canRocket && !canLaser) return;

            if (canRocket && (!canLaser || Random.value < 0.5f))
            {
                FireRocket();
            }
            else
            {
                MoveLasers();
            }
        }

        private void FireRocket()
        {
            if (rocketPool == null) return;

            Transform spawnPoint = rocketSpawnPoints[Random.Range(0, rocketSpawnPoints.Length)];
            Transform risePoint = GetRocketRisePoint(spawnPoint);

            GameObject rocketObject = rocketPool.Get();
            rocketObject.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            MachineHomingRocket rocket = rocketObject.GetComponent<MachineHomingRocket>();
            if (rocket == null)
            {
                Debug.LogWarning("MachineBossController: spawned rocket has no MachineHomingRocket component.");
                rocketPool.Return(rocketObject);
                return;
            }

            rocket.SetPool(rocketPool);
            rocket.Launch(player, risePoint != null ? risePoint.position : spawnPoint.position + Vector3.up);

            animationController?.TriggerRocket();
        }

        private Transform GetRocketRisePoint(Transform spawnPoint)
        {
            if (rocketRisePoints == null || rocketRisePoints.Length == 0)
            {
                return null;
            }

            int spawnIndex = System.Array.IndexOf(rocketSpawnPoints, spawnPoint);
            if (spawnIndex >= 0 && spawnIndex < rocketRisePoints.Length && rocketRisePoints[spawnIndex] != null)
            {
                return rocketRisePoints[spawnIndex];
            }

            return rocketRisePoints[Random.Range(0, rocketRisePoints.Length)];
        }

        private void MoveLasers()
        {
            StopLasers();

            activeVerticalLaser = ActivateRandomLaser(verticalWallLasers);
            activeHorizontalLaser = ActivateRandomLaser(horizontalWallLasers);

            animationController?.TriggerLaser();
        }

        private MachineMovingLaser ActivateRandomLaser(MachineMovingLaser[] lasers)
        {
            if (lasers == null || lasers.Length == 0)
            {
                return null;
            }

            MachineMovingLaser laser = lasers[Random.Range(0, lasers.Length)];
            if (laser == null)
            {
                return null;
            }

            float minDistance = laser.WallOrientationValue == MachineMovingLaser.WallOrientation.Vertical ?
                laserVerticalMoveDistanceRange.x : laserHorizontalMoveDistanceRange.x;
            float maxDistance = laser.WallOrientationValue == MachineMovingLaser.WallOrientation.Vertical ?
                laserVerticalMoveDistanceRange.y : laserHorizontalMoveDistanceRange.y;
            float distance = Random.Range(minDistance, maxDistance);
            float speed = Random.Range(laserMoveSpeedRange.x, laserMoveSpeedRange.y);
            laser.Activate(distance, speed);
            return laser;
        }

        private bool HasLaserAttack()
        {
            return HasAnyLaser(verticalWallLasers) || HasAnyLaser(horizontalWallLasers);
        }

        private bool HasAnyLaser(MachineMovingLaser[] lasers)
        {
            return lasers != null && lasers.Length > 0;
        }

        private float GetCurrentAttackDelay()
        {
            if (maxHealth <= 0) return minAttackDelay;

            float damageProgress = 1f - (float)health / maxHealth;
            return Mathf.Lerp(maxAttackDelay, minAttackDelay, damageProgress);
        }

        private void SetInteractionCharactersEnabled(bool isEnabled)
        {
            if (interactionCharacters == null) return;

            for (int i = 0; i < interactionCharacters.Length; i++)
            {
                if (interactionCharacters[i] != null)
                {
                    interactionCharacters[i].SetInteractionEnabled(isEnabled);
                }
            }
        }

        private void SubscribeInteractionCharacters()
        {
            if (interactionCharacters == null) return;

            for (int i = 0; i < interactionCharacters.Length; i++)
            {
                if (interactionCharacters[i] != null)
                {
                    interactionCharacters[i].OnInteract.AddListener(EndCooldown);
                }
            }
        }

        private void UnsubscribeInteractionCharacters()
        {
            if (interactionCharacters == null) return;

            for (int i = 0; i < interactionCharacters.Length; i++)
            {
                if (interactionCharacters[i] != null)
                {
                    interactionCharacters[i].OnInteract.RemoveListener(EndCooldown);
                }
            }
        }

        private void StopLasers()
        {
            activeVerticalLaser?.Deactivate();
            activeHorizontalLaser?.Deactivate();
            activeVerticalLaser = null;
            activeHorizontalLaser = null;
        }

        private void Die()
        {
            isDead = true;
            StopLasers();
            SetInteractionCharactersEnabled(false);
            animationController?.TriggerDeath();

            if (phaseRoutine != null)
            {
                StopCoroutine(phaseRoutine);
                phaseRoutine = null;
            }
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            attackPhaseDuration = Mathf.Max(0f, attackPhaseDuration);
            cooldownDuration = Mathf.Max(0f, cooldownDuration);
            minAttackDelay = Mathf.Max(0.01f, minAttackDelay);
            maxAttackDelay = Mathf.Max(minAttackDelay, maxAttackDelay);

            laserVerticalMoveDistanceRange.x = Mathf.Max(0f, laserVerticalMoveDistanceRange.x);
            laserVerticalMoveDistanceRange.y = Mathf.Max(laserVerticalMoveDistanceRange.x, laserVerticalMoveDistanceRange.y);
            laserMoveSpeedRange.x = Mathf.Max(0.01f, laserMoveSpeedRange.x);
            laserMoveSpeedRange.y = Mathf.Max(laserMoveSpeedRange.x, laserMoveSpeedRange.y);
            rocketPoolPreloadCount = Mathf.Max(0, rocketPoolPreloadCount);
        }
    }
}
