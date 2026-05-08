using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Interactables;
using ObjectPool;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Zenject;

namespace AI.Bosses.Machine
{
    public class MachineBossController : MonoBehaviour, IHittable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private MachineBossHealthUI healthUI;
        [Header("OnDeath")]
        [SerializeField] private UnityEvent onDeathTrigger;

        [Header("Phases")]
        [SerializeField] private float attackPhaseDuration = 12f;
        [SerializeField] private float cooldownDuration = 5f;
        [SerializeField] private float maxAttackDelay = 2.5f;
        [SerializeField] private float minAttackDelay = 0.7f;
        [SerializeField] private bool startInAttackPhase = true;
        
        [Header("Dynamic Difficulty")]
        [SerializeField] private bool useDynamicDifficulty = true;
        [SerializeField] private AnimationCurve difficultyByHealth = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField] private float attackPhaseDurationAtLowHealth = 16f;
        [SerializeField] private float cooldownDurationAtLowHealth = 3f;
        [SerializeField] private bool allowAttackPhaseShorterThanBase = false;
        [SerializeField] private Vector2 laserVerticalMoveDistanceRangeAtLowHealth = new Vector2(2f, 4f);
        [SerializeField] private Vector2 laserHorizontalMoveDistanceRangeAtLowHealth = new Vector2(2f, 4f);
        [SerializeField] private Vector2 laserMoveSpeedRangeAtLowHealth = new Vector2(2f, 5f);
        [SerializeField, Range(0f, 1f)] private float rocketAttackChanceAtFullHealth = 0.5f;
        [SerializeField, Range(0f, 1f)] private float rocketAttackChanceAtLowHealth = 0.35f;
        [SerializeField] private Vector2Int cooldownCharactersCountAtFullHealth = new Vector2Int(2, 3);
        [SerializeField] private Vector2Int cooldownCharactersCountAtLowHealth = new Vector2Int(1, 2);
        [SerializeField] private int cooldownInteractionsRequiredAtFullHealth = 1;
        [SerializeField] private int cooldownInteractionsRequiredAtLowHealth = 1;

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
        [SerializeField, Range(0f, 1f)] private float dualLaserHealthThreshold = 0.75f;
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
        private int cooldownInteractionsRemaining;
        private int nextVerticalLaserIndex;
        private int nextHorizontalLaserIndex;
        private bool useVerticalLaserForSingleAttack = true;
        private readonly List<InteractableCharacter> availableInteractionCharacters = new List<InteractableCharacter>();
        private readonly List<InteractableCharacter> activeCooldownCharacters = new List<InteractableCharacter>();
        private readonly Dictionary<InteractableCharacter, UnityAction> interactionCharacterListeners = new Dictionary<InteractableCharacter, UnityAction>();
        
        [Inject] private TimelineManager timelineManager;

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

        public void SetOnDeathTimeline(PlayableDirector playableDirector) =>
            onDeathTrigger.AddListener(playableDirector.Play);

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
            float currentAttackPhaseDuration = GetCurrentAttackPhaseDuration();

            while (!isDead && phaseTime < currentAttackPhaseDuration)
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
            cooldownInteractionsRemaining = GetCurrentCooldownInteractionsRequired();

            float timer = 0f;
            float currentCooldownDuration = GetCurrentCooldownDuration();
            while (!isDead && !cooldownInterrupted && timer < currentCooldownDuration)
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

            Vector2Int countRange = GetCurrentCooldownCharacterCountRange();
            int minCount = Mathf.Clamp(countRange.x, 0, availableInteractionCharacters.Count);
            int maxCount = Mathf.Clamp(countRange.y, minCount, availableInteractionCharacters.Count);
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

            cooldownInteractionsRemaining = Mathf.Max(0, cooldownInteractionsRemaining - 1);
            if (cooldownInteractionsRemaining <= 0)
            {
                EndCooldown();
            }
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

            float rocketChance = GetCurrentRocketAttackChance();
            if (canRocket && (!canLaser || Random.value < rocketChance))
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

            bool canUseVertical = HasAnyLaser(verticalWallLasers);
            bool canUseHorizontal = HasAnyLaser(horizontalWallLasers);
            if (!canUseVertical && !canUseHorizontal)
            {
                return;
            }

            bool useDualLaserMode = GetHealthNormalized() <= dualLaserHealthThreshold;
            if (useDualLaserMode)
            {
                activeVerticalLaser = ActivateNextLaser(verticalWallLasers, ref nextVerticalLaserIndex);
                activeHorizontalLaser = ActivateNextLaser(horizontalWallLasers, ref nextHorizontalLaserIndex);
            }
            else
            {
                bool useVertical = canUseVertical;
                if (canUseVertical && canUseHorizontal)
                {
                    useVertical = useVerticalLaserForSingleAttack;
                    useVerticalLaserForSingleAttack = !useVerticalLaserForSingleAttack;
                }

                if (useVertical)
                {
                    activeVerticalLaser = ActivateNextLaser(verticalWallLasers, ref nextVerticalLaserIndex);
                }
                else
                {
                    activeHorizontalLaser = ActivateNextLaser(horizontalWallLasers, ref nextHorizontalLaserIndex);
                }
            }

            animationController?.TriggerLaser();
        }

        private MachineMovingLaser ActivateNextLaser(MachineMovingLaser[] lasers, ref int nextIndex)
        {
            if (lasers == null || lasers.Length == 0)
            {
                return null;
            }

            MachineMovingLaser laser = null;
            int checkedCount = 0;
            while (checkedCount < lasers.Length)
            {
                int candidateIndex = nextIndex % lasers.Length;
                nextIndex = (nextIndex + 1) % lasers.Length;
                checkedCount++;

                if (lasers[candidateIndex] != null)
                {
                    laser = lasers[candidateIndex];
                    break;
                }
            }

            if (laser == null)
            {
                return null;
            }

            float minDistance = laser.WallOrientationValue == MachineMovingLaser.WallOrientation.Vertical ?
                GetCurrentLaserVerticalMoveDistanceRange().x : GetCurrentLaserHorizontalMoveDistanceRange().x;
            float maxDistance = laser.WallOrientationValue == MachineMovingLaser.WallOrientation.Vertical ?
                GetCurrentLaserVerticalMoveDistanceRange().y : GetCurrentLaserHorizontalMoveDistanceRange().y;
            float distance = Random.Range(minDistance, maxDistance);
            Vector2 speedRange = GetCurrentLaserMoveSpeedRange();
            float speed = Random.Range(speedRange.x, speedRange.y);
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

            return Mathf.Lerp(maxAttackDelay, minAttackDelay, GetDifficultyProgressByHealth());
        }

        private float GetCurrentAttackPhaseDuration()
        {
            float duration = Mathf.Lerp(attackPhaseDuration, attackPhaseDurationAtLowHealth, GetDifficultyProgressByHealth());
            if (!allowAttackPhaseShorterThanBase)
            {
                duration = Mathf.Max(attackPhaseDuration, duration);
            }

            return duration;
        }

        private float GetCurrentCooldownDuration()
        {
            return Mathf.Lerp(cooldownDuration, cooldownDurationAtLowHealth, GetDifficultyProgressByHealth());
        }

        private float GetCurrentRocketAttackChance()
        {
            return Mathf.Lerp(rocketAttackChanceAtFullHealth, rocketAttackChanceAtLowHealth, GetDifficultyProgressByHealth());
        }

        private int GetCurrentCooldownInteractionsRequired()
        {
            float value = Mathf.Lerp(cooldownInteractionsRequiredAtFullHealth, cooldownInteractionsRequiredAtLowHealth, GetDifficultyProgressByHealth());
            return Mathf.Max(1, Mathf.RoundToInt(value));
        }

        private Vector2Int GetCurrentCooldownCharacterCountRange()
        {
            float progress = GetDifficultyProgressByHealth();
            int min = Mathf.RoundToInt(Mathf.Lerp(cooldownCharactersCountAtFullHealth.x, cooldownCharactersCountAtLowHealth.x, progress));
            int max = Mathf.RoundToInt(Mathf.Lerp(cooldownCharactersCountAtFullHealth.y, cooldownCharactersCountAtLowHealth.y, progress));
            return new Vector2Int(min, Mathf.Max(min, max));
        }

        private Vector2 GetCurrentLaserVerticalMoveDistanceRange()
        {
            return Vector2.Lerp(laserVerticalMoveDistanceRange, laserVerticalMoveDistanceRangeAtLowHealth, GetDifficultyProgressByHealth());
        }

        private Vector2 GetCurrentLaserHorizontalMoveDistanceRange()
        {
            return Vector2.Lerp(laserHorizontalMoveDistanceRange, laserHorizontalMoveDistanceRangeAtLowHealth, GetDifficultyProgressByHealth());
        }

        private Vector2 GetCurrentLaserMoveSpeedRange()
        {
            return Vector2.Lerp(laserMoveSpeedRange, laserMoveSpeedRangeAtLowHealth, GetDifficultyProgressByHealth());
        }

        private float GetDifficultyProgressByHealth()
        {
            if (maxHealth <= 0)
            {
                return 1f;
            }

            float health01 = Mathf.Clamp01((float)health / maxHealth);
            float damageProgress = 1f - health01;
            if (!useDynamicDifficulty || difficultyByHealth == null || difficultyByHealth.length == 0)
            {
                return damageProgress;
            }

            return Mathf.Clamp01(difficultyByHealth.Evaluate(health01));
        }

        private float GetHealthNormalized()
        {
            if (maxHealth <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01((float)health / maxHealth);
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
            onDeathTrigger?.Invoke();
            //animationController?.TriggerDeath(); controlled with timeline
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
            attackPhaseDurationAtLowHealth = Mathf.Max(0f, attackPhaseDurationAtLowHealth);
            cooldownDurationAtLowHealth = Mathf.Max(0f, cooldownDurationAtLowHealth);
            minAttackDelay = Mathf.Max(0.01f, minAttackDelay);
            maxAttackDelay = Mathf.Max(minAttackDelay, maxAttackDelay);
            dualLaserHealthThreshold = Mathf.Clamp01(dualLaserHealthThreshold);

            laserVerticalMoveDistanceRange.x = Mathf.Max(0f, laserVerticalMoveDistanceRange.x);
            laserVerticalMoveDistanceRange.y = Mathf.Max(laserVerticalMoveDistanceRange.x, laserVerticalMoveDistanceRange.y);
            laserVerticalMoveDistanceRangeAtLowHealth.x = Mathf.Max(0f, laserVerticalMoveDistanceRangeAtLowHealth.x);
            laserVerticalMoveDistanceRangeAtLowHealth.y = Mathf.Max(laserVerticalMoveDistanceRangeAtLowHealth.x, laserVerticalMoveDistanceRangeAtLowHealth.y);
            laserHorizontalMoveDistanceRange.x = Mathf.Max(0f, laserHorizontalMoveDistanceRange.x);
            laserHorizontalMoveDistanceRange.y = Mathf.Max(laserHorizontalMoveDistanceRange.x, laserHorizontalMoveDistanceRange.y);
            laserHorizontalMoveDistanceRangeAtLowHealth.x = Mathf.Max(0f, laserHorizontalMoveDistanceRangeAtLowHealth.x);
            laserHorizontalMoveDistanceRangeAtLowHealth.y = Mathf.Max(laserHorizontalMoveDistanceRangeAtLowHealth.x, laserHorizontalMoveDistanceRangeAtLowHealth.y);
            laserMoveSpeedRange.x = Mathf.Max(0.01f, laserMoveSpeedRange.x);
            laserMoveSpeedRange.y = Mathf.Max(laserMoveSpeedRange.x, laserMoveSpeedRange.y);
            laserMoveSpeedRangeAtLowHealth.x = Mathf.Max(0.01f, laserMoveSpeedRangeAtLowHealth.x);
            laserMoveSpeedRangeAtLowHealth.y = Mathf.Max(laserMoveSpeedRangeAtLowHealth.x, laserMoveSpeedRangeAtLowHealth.y);

            cooldownCharactersCountAtFullHealth.x = Mathf.Max(0, cooldownCharactersCountAtFullHealth.x);
            cooldownCharactersCountAtFullHealth.y = Mathf.Max(cooldownCharactersCountAtFullHealth.x, cooldownCharactersCountAtFullHealth.y);
            cooldownCharactersCountAtLowHealth.x = Mathf.Max(0, cooldownCharactersCountAtLowHealth.x);
            cooldownCharactersCountAtLowHealth.y = Mathf.Max(cooldownCharactersCountAtLowHealth.x, cooldownCharactersCountAtLowHealth.y);
            cooldownInteractionsRequiredAtFullHealth = Mathf.Max(1, cooldownInteractionsRequiredAtFullHealth);
            cooldownInteractionsRequiredAtLowHealth = Mathf.Max(1, cooldownInteractionsRequiredAtLowHealth);
            rocketPoolPreloadCount = Mathf.Max(0, rocketPoolPreloadCount);
            RefreshHealthUi();
        }

        private void RefreshHealthUi()
        {
            healthUI?.SetHealth(health, maxHealth);
        }
    }
}
