using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int maxActiveEnemies = 5;
    [SerializeField] public int maxTotalEnemies = 50;
    [SerializeField] private float minSpawnDistance = 1f;
    [SerializeField] private float respawnDelay = 2f;

    private int enemyCount = 0;
    private List<BotController> activeBots = new List<BotController>();
    private HashSet<Transform> usedSpawnPoints = new HashSet<Transform>();
    private Coroutine spawnLoopCoroutine;
    private float lastBotDeathTime = -Mathf.Infinity;

    public static event Action OnAllBotsDead;

    private void Start()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("[EnemySpawner]: spawnPoints chưa được gán hoặc rỗng!");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner]: enemyPrefab chưa được gán!");
            return;
        }

        ValidatePrefab();

        SpawnAllEnemiesAtStart();
        spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }

    private void ValidatePrefab()
    {
        if (enemyPrefab != null)
        {
            bool isValid = true;
            if (enemyPrefab.GetComponent<BotController>() == null) { Debug.LogError("[EnemySpawner]: enemyPrefab thiếu BotController!"); isValid = false; }
            if (enemyPrefab.GetComponent<BotRandomSkin>() == null) { Debug.LogError("[EnemySpawner]: enemyPrefab thiếu BotRandomSkin!"); isValid = false; }
            if (enemyPrefab.GetComponentInChildren<BotGun>() == null) { Debug.LogError("[EnemySpawner]: enemyPrefab thiếu BotGun!"); isValid = false; }
            if (enemyPrefab.GetComponent<NavMeshAgent>() == null) { Debug.LogError("[EnemySpawner]: enemyPrefab thiếu NavMeshAgent!"); isValid = false; }
            if (enemyPrefab.GetComponentInChildren<AttackRangeVisualBot>() == null) { Debug.LogError("[EnemySpawner]: enemyPrefab thiếu AttackRangeVisualBot!"); isValid = false; }
            if (!isValid) Debug.LogError("[EnemySpawner]: Prefab không hợp lệ, kiểm tra các component!");
        }
    }

    private void SpawnAllEnemiesAtStart()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (activeBots.Count >= maxActiveEnemies || enemyCount >= maxTotalEnemies)
                break;

            SpawnEnemy(spawnPoints[i]);
            enemyCount++;
        }

        UpdateEnemyCounter();
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (activeBots.Count < maxActiveEnemies && enemyCount < maxTotalEnemies)
            {
                if (Time.time >= lastBotDeathTime + respawnDelay)
                {
                    Transform spawnPoint = GetSafeSpawnPoint();
                    if (spawnPoint != null)
                    {
                        SpawnEnemy(spawnPoint);
                        enemyCount++;
                        UpdateEnemyCounter();
                    }
                    else
                    {
                        Debug.LogWarning("[EnemySpawner]: Không tìm thấy điểm spawn an toàn!");
                    }
                }
                else
                {
                    Debug.Log($"[EnemySpawner]: Đợi {respawnDelay - (Time.time - lastBotDeathTime):F2}s trước khi spawn.");
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void SpawnEnemy(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("[EnemySpawner]: spawnPoint is null!");
            return;
        }

        GameObject enemy = ObjectPool.Instance.GetObject();
        if (enemy == null)
        {
            Debug.LogError("[EnemySpawner]: ObjectPool trả về enemy null!");
            return;
        }

        // Đặt vị trí spawn và đảm bảo bot trên NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPoint.position, out hit, 10f, NavMesh.AllAreas))
        {
            enemy.transform.position = hit.position;
            NavMeshAgent navMeshAgent = enemy.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = true;
                navMeshAgent.Warp(hit.position);
            }
        }
        else
        {
            Debug.LogWarning($"[EnemySpawner] Không tìm thấy vị trí NavMesh hợp lệ tại {spawnPoint.position}!");
            enemy.transform.position = spawnPoint.position; // Fallback
        }
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetActive(true);

        Character character = Cache.GetCharacter(enemy);
        if (character == null)
        {
            Debug.LogError($"[EnemySpawner]: {enemy.name} thiếu Character!");
            ObjectPool.Instance.ReturnObject(enemy);
            return;
        }
        character.SetState(Character.PlayerState.IsRun);

        BotController botController = Cache.GetBotController(enemy);
        if (botController == null)
        {
            Debug.LogError($"[EnemySpawner]: {enemy.name} thiếu BotController!");
            ObjectPool.Instance.ReturnObject(enemy);
            return;
        }

        BotRandomSkin randomSkin = enemy.GetComponent<BotRandomSkin>();
        if (randomSkin == null)
        {
            Debug.LogError($"[EnemySpawner]: {enemy.name} thiếu BotRandomSkin!");
            ObjectPool.Instance.ReturnObject(enemy);
            return;
        }
        randomSkin.RandomizeAppearance();
        if (randomSkin.currentBulletPrefab == null)
        {
            Debug.LogWarning($"[EnemySpawner]: {enemy.name} currentBulletPrefab null!");
        }

        botController.Reset();
        botController.SpawnPoint = spawnPoint;
        botController.SetCanMove(true);
        usedSpawnPoints.Add(spawnPoint);

        BotGun gun = Cache.GetBotGun(enemy);
        if (gun != null)
        {
            gun.ResetGun();
        }
        else
        {
            Debug.LogError($"[EnemySpawner]: {enemy.name} thiếu BotGun!");
        }

        AttackRangeVisualBot attackRange = Cache.GetAttackRangeVisualBot(enemy);
        if (attackRange != null && gun != null)
        {
            attackRange.gun = gun;
        }
        else
        {
            Debug.LogError($"[EnemySpawner]: {enemy.name} thiếu AttackRangeVisualBot hoặc BotGun!");
        }

        activeBots.Add(botController);
        Debug.Log($"[EnemySpawner] Spawn bot #{enemyCount} tại {enemy.transform.position} | Active: {activeBots.Count}");
    }

    private Transform GetSafeSpawnPoint()
    {
        List<Transform> availablePoints = new List<Transform>();
        foreach (Transform point in spawnPoints)
        {
            if (!usedSpawnPoints.Contains(point) && IsPointSafe(point))
                availablePoints.Add(point);
        }

        if (availablePoints.Count > 0)
        {
            Transform chosen = availablePoints[Random.Range(0, availablePoints.Count)];
            return chosen;
        }

        foreach (Transform point in spawnPoints)
        {
            if (IsPointSafe(point))
            {
                usedSpawnPoints.Remove(point);
                return point;
            }
        }

        return null;
    }

    private bool IsPointSafe(Transform spawnPoint)
    {
        foreach (var bot in activeBots)
        {
            if (bot != null && Vector3.Distance(spawnPoint.position, bot.transform.position) < minSpawnDistance)
                return false;
        }

        foreach (var used in usedSpawnPoints)
        {
            if (Vector3.Distance(spawnPoint.position, used.position) < minSpawnDistance)
                return false;
        }

        return true;
    }

    public void OnBotDied(GameObject deadBot)
    {
        if (deadBot == null) return;

        BotController botController = Cache.GetBotController(deadBot);
        if (botController != null)
        {
            activeBots.Remove(botController);
            if (botController.SpawnPoint != null)
            {
                usedSpawnPoints.Remove(botController.SpawnPoint);
            }
            Debug.Log($"[EnemySpawner] Bot {deadBot.name} đã chết.");
        }
        else
        {
            Debug.LogWarning($"[EnemySpawner] Bot {deadBot.name} không có BotController!");
        }

        ObjectPool.Instance.ReturnObject(deadBot);
        Cache.ClearCacheForObject(deadBot);
        lastBotDeathTime = Time.time;

        if (activeBots.Count == 0 && enemyCount >= maxTotalEnemies)
        {
            OnAllBotsDead?.Invoke();
            Debug.Log("[EnemySpawner]: Tất cả bot đã chết, kích hoạt OnAllBotsDead!");
        }

        Debug.Log($"[EnemySpawner] Bot chết. Active: {activeBots.Count}");
    }

    private void UpdateEnemyCounter()
    {
        Debug.Log($"[EnemySpawner] Tổng bot đã spawn: {enemyCount} | Active: {activeBots.Count}");
    }

    public List<BotController> GetActiveBots()
    {
        return activeBots;
    }

    public int GetTopScore()
    {
        // Số bot đã chết = tổng bot spawn - số bot active
        int deadBots = enemyCount - activeBots.Count;

        // Top = tổng bot tối đa - số bot đã chết + 1 (player)
        int top = maxTotalEnemies - deadBots + 1;

        // Giảm xuống tối thiểu là 1
        return Mathf.Max(top, 1);
    }

    public void ResetAllBots()
    {
        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
            spawnLoopCoroutine = null;
        }

        // Tìm tất cả bot trong scene
        BotController[] allBotsInScene = FindObjectsOfType<BotController>();
        Debug.Log($"[EnemySpawner] Số bot trong scene trước reset: {allBotsInScene.Length}");

        // Kiểm tra và trả bot về pool
        foreach (var bot in allBotsInScene)
        {
            if (bot != null)
            {
                GameObject botGameObject = bot.gameObject;
                Debug.Log($"[EnemySpawner] Xử lý bot {botGameObject.name}, Active: {botGameObject.activeInHierarchy}");

                // Đảm bảo bot được vô hiệu hóa
                bot.SetCanMove(false);
                ObjectPool.Instance.ReturnObject(botGameObject);

                // Kiểm tra trạng thái sau khi trả về pool
                if (botGameObject.activeInHierarchy)
                {
                    Debug.LogError($"[EnemySpawner] Lỗi: Bot {botGameObject.name} vẫn active sau khi trả về pool!");
                }
                else
                {
                    Debug.Log($"[EnemySpawner] Trả bot {botGameObject.name} về pool thành công.");
                }
            }
        }

        // Kiểm tra lại scene để tìm bot còn sót
        allBotsInScene = FindObjectsOfType<BotController>();
        if (allBotsInScene.Length > 0)
        {
            Debug.LogError($"[EnemySpawner] Lỗi: Vẫn còn {allBotsInScene.Length} bot trong scene sau reset!");
            foreach (var bot in allBotsInScene)
            {
                Debug.LogError($"[EnemySpawner] Bot sót lại: {bot.gameObject.name}, Active: {bot.gameObject.activeInHierarchy}, Position: {bot.transform.position}");
            }
        }

        // Reset trạng thái spawner
        activeBots.Clear();
        usedSpawnPoints.Clear();
        enemyCount = 0;
        lastBotDeathTime = -Mathf.Infinity;

        SpawnAllEnemiesAtStart();
        spawnLoopCoroutine = StartCoroutine(SpawnLoop());

        Debug.Log("[EnemySpawner]: Reset tất cả bot và bắt đầu spawn mới!");
    }
}