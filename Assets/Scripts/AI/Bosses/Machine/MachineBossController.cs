using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Interactables;
using ObjectPool;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace AI.Bosses.Machine
{
    public class MachineBossController : MonoBehaviour, IHittable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private MachineBossHealthUI healthUI;

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
        [FormerlySerializedAs("light2D")] [SerializeField] private Light2D[] lights2D;
        [SerializeField] private Color attackColor;
        [SerializeField] private Color cooldownColor;
        [SerializeField] private GameObject cooldownEffect;

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
        private readonly List<InteractableCharacter> availableInteractionCharacters = new List<InteractableCharacter>();
        private readonly List<InteractableCharacter> activeCooldownCharacters = new List<InteractableCharacter>();
        private readonly Dictionary<InteractableCharacter, UnityAction> interactionCharacterListeners = new Dictionary<InteractableCharacter, UnityAction>();

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

            ResetInteractionCharacterPool();
            RefreshHealthUi();
        }

        private void OnEnable()
        {
            SubscribeInteractionCharacters();
        }

        public void Init()
        {
            if (isDead || !isActiveAndEnabled || phaseRoutine != null)
            {
                return;
            }

            healthUI?.gameObject.SetActive(true);

            phaseRoutine = StartCoroutine(PhaseLoop());
        }

        public void SetHealthUi(MachineBossHealthUI bossHealthUi)
        {
            healthUI = bossHealthUi;
            RefreshHealthUi();
        }

        public void SetPlayer(PlayerController playerController)
        {
            player = playerController;
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
            DisableActiveCooldownCharacters();
        }

        public void Hit()
        {
            if (isDead) return;

            health = Mathf.Max(health - 1, 0);
            RefreshHealthUi();

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
            DisableActiveCooldownCharacters();
            SwitchColorLights(attackColor);
            cooldownEffect.SetActive(false);

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
            EnableRandomCooldownCharacters();
            SwitchColorLights(cooldownColor);
            cooldownEffect.SetActive(true);


            float timer = 0f;
            while (!isDead && !cooldownInterrupted && timer < cooldownDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            DisableActiveCooldownCharacters();
        }

        private void SwitchColorLights(Color color)
        {
            if (lights2D.Length > 0)
            {
                foreach (var light2D in lights2D)
                {
                    light2D.color = color;
                }
            }
        }

        private void EnableRandomCooldownCharacters()
        {
            DisableActiveCooldownCharacters();

            if (availableInteractionCharacters.Count == 0)
            {
                return;
            }

            int minCount = Mathf.Min(2, availableInteractionCharacters.Count);
            int maxCount = Mathf.Min(3, availableInteractionCharacters.Count);
            int targetCount = Random.Range(minCount, maxCount + 1);

            List<InteractableCharacter> selectionPool = new List<InteractableCharacter>(availableInteractionCharacters);
            for (int i = 0; i < targetCount && selectionPool.Count > 0; i++)
            {
                int index = Random.Range(0, selectionPool.Count);
                InteractableCharacter character = selectionPool[index];
                selectionPool.RemoveAt(index);

                if (character == null)
                {
                    continue;
                }

                activeCooldownCharacters.Add(character);
                character.SetCondition(true);
                character.SetInteractionEnabled(true);
            }
        }

        private void DisableActiveCooldownCharacters()
        {
            for (int i = 0; i < activeCooldownCharacters.Count; i++)
            {
                InteractableCharacter character = activeCooldownCharacters[i];
                if (character == null)
                {
                    continue;
                }

                character.SetCondition(false);
                character.SetInteractionEnabled(false);
            }

            activeCooldownCharacters.Clear();
        }

        private void OnInteractionCharacterInteracted(InteractableCharacter character)
        {
            if (character == null)
            {
                return;
            }

            if (activeCooldownCharacters.Remove(character))
            {
                availableInteractionCharacters.Remove(character);
                character.SetCondition(false);
                character.SetInteractionEnabled(false);
            }

            EndCooldown();
        }

        private void ResetInteractionCharacterPool()
        {
            availableInteractionCharacters.Clear();
            activeCooldownCharacters.Clear();
            interactionCharacterListeners.Clear();

            if (interactionCharacters == null)
            {
                return;
            }

            for (int i = 0; i < interactionCharacters.Length; i++)
            {
                InteractableCharacter character = interactionCharacters[i];
                if (character == null)
                {
                    continue;
                }

                availableInteractionCharacters.Add(character);
                character.SetCondition(false);
                character.SetInteractionEnabled(false);
            }
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
            ResetInteractionCharacterPool();

            if (interactionCharacters == null)
            {
                return;
            }

            for (int i = 0; i < interactionCharacters.Length; i++)
            {
                InteractableCharacter character = interactionCharacters[i];
                if (character == null)
                {
                    continue;
                }

                UnityAction listener = () => OnInteractionCharacterInteracted(character);
                interactionCharacterListeners[character] = listener;
                character.OnInteract.AddListener(listener);
            }
        }

        private void UnsubscribeInteractionCharacters()
        {
            if (interactionCharacters == null)
            {
                interactionCharacterListeners.Clear();
                return;
            }

            for (int i = 0; i < interactionCharacters.Length; i++)
            {
                InteractableCharacter character = interactionCharacters[i];
                if (character == null)
                {
                    continue;
                }

                if (interactionCharacterListeners.TryGetValue(character, out UnityAction listener))
                {
                    character.OnInteract.RemoveListener(listener);
                }
            }

            interactionCharacterListeners.Clear();
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
            RefreshHealthUi();
            StopLasers();
            DisableActiveCooldownCharacters();
            animationController?.TriggerDeath();
            cooldownEffect.SetActive(false);

            if (phaseRoutine != null)
            {
                StopCoroutine(phaseRoutine);
                phaseRoutine = null;
            }
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            health = Mathf.Clamp(health, 0, maxHealth);
            attackPhaseDuration = Mathf.Max(0f, attackPhaseDuration);
            cooldownDuration = Mathf.Max(0f, cooldownDuration);
            minAttackDelay = Mathf.Max(0.01f, minAttackDelay);
            maxAttackDelay = Mathf.Max(minAttackDelay, maxAttackDelay);

            laserVerticalMoveDistanceRange.x = Mathf.Max(0f, laserVerticalMoveDistanceRange.x);
            laserVerticalMoveDistanceRange.y = Mathf.Max(laserVerticalMoveDistanceRange.x, laserVerticalMoveDistanceRange.y);
            laserMoveSpeedRange.x = Mathf.Max(0.01f, laserMoveSpeedRange.x);
            laserMoveSpeedRange.y = Mathf.Max(laserMoveSpeedRange.x, laserMoveSpeedRange.y);
            rocketPoolPreloadCount = Mathf.Max(0, rocketPoolPreloadCount);
            RefreshHealthUi();
        }

        private void RefreshHealthUi()
        {
            healthUI?.SetHealth(health, maxHealth);
        }
    }
}
