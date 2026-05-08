using Player;
using UnityEngine;
using UnityEngine.Playables;
using Zenject;

namespace AI.Bosses.Machine
{
    public class SpawnMachineBossOnPlayerDeath : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform bossTransform;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool spawnOnEveryDeath = true;
        [SerializeField] private bool autoInitBoss = false;

        [Header("Dependencies")]
        [SerializeField] private MachineBossHealthUI bossHealthUI;
        [SerializeField] private PlayableDirector deathTimeline;

        [Inject] private PlayerController playerController;

        private bool hasSpawned;
        private GameObject currentBoss;

        private void OnEnable()
        {
            if (playerController != null)
            {
                playerController.OnRevive += OnPlayerRevive;
            }

            currentBoss = bossTransform != null ? bossTransform.gameObject: gameObject;
        }

        private void OnDisable()
        {
            if (playerController != null)
            {
                playerController.OnRevive -= OnPlayerRevive;
            }
        }

        private void OnPlayerRevive()
        {
            Destroy(currentBoss);
            bossHealthUI.gameObject.SetActive(false);

            if (bossPrefab == null)
            {
                return;
            }

            if (!spawnOnEveryDeath && hasSpawned)
            {
                return;
            }

            Transform targetPoint = spawnPoint != null ? spawnPoint : transform;

            GameObject spawnedBossObj = Instantiate(bossPrefab, targetPoint.position, targetPoint.rotation);
            currentBoss = spawnedBossObj;
            MachineBossController boss = spawnedBossObj.GetComponentInChildren<MachineBossController>();
            boss.SetPlayer(playerController);
            boss.SetHealthUi(bossHealthUI);
            boss.SetOnDeathTimeline(deathTimeline);
            // if (autoInitBoss)
            // {
            //     boss.Init();
            // }

            hasSpawned = true;
        }
    }
}
