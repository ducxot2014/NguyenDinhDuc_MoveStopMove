using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private Transform poolParent;
    [SerializeField] private int initialPoolSize;

    private Queue<GameObject> objectPool = new Queue<GameObject>();
    public static ObjectPool Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (objectPrefab == null)
        {
            Debug.LogError("[ObjectPool] objectPrefab chưa được gán!");
            return;
        }

        if (!ValidatePrefab(objectPrefab))
        {
            Debug.LogError("[ObjectPool] objectPrefab thiếu component cần thiết!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject enemy = Instantiate(objectPrefab, poolParent);
            ResetObject(enemy);
            enemy.SetActive(false);
            objectPool.Enqueue(enemy);
            Debug.Log($"[ObjectPool] Khởi tạo object {i + 1}/{initialPoolSize}: {enemy.name}");
        }

        Debug.Log($"[ObjectPool] Pool khởi tạo với {objectPool.Count} objects.");
    }

    public GameObject GetObject()
    {
        GameObject enemy = null;

        while (objectPool.Count > 0)
        {
            enemy = objectPool.Dequeue();
            if (ValidateObject(enemy))
            {
                ResetObject(enemy);
                enemy.SetActive(true);
                Debug.Log($"[ObjectPool] GetObject: lấy {enemy.name}, còn lại: {objectPool.Count}");
                return enemy;
            }
            else
            {
                Debug.LogWarning($"[ObjectPool] Object {enemy.name} thiếu component, hủy!");
                Destroy(enemy);
            }
        }

        if (objectPrefab != null)
        {
            enemy = Instantiate(objectPrefab, poolParent);
            if (ValidateObject(enemy))
            {
                ResetObject(enemy);
                enemy.SetActive(true);
                Debug.Log($"[ObjectPool] GetObject: tạo mới {enemy.name}.");
                return enemy;
            }
            else
            {
                Debug.LogError($"[ObjectPool] Object mới {enemy.name} thiếu component!");
                Destroy(enemy);
            }
        }
        else
        {
            Debug.LogError("[ObjectPool] objectPrefab null!");
        }

        Debug.LogWarning("[ObjectPool] GetObject: không có object hợp lệ!");
        return null;
    }

  



    public void ReturnObject(GameObject enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("[ObjectPool] ReturnObject: enemy null!");
            return;
        }

        ResetObject(enemy);
        enemy.SetActive(false);
        objectPool.Enqueue(enemy);
        Debug.Log($"[ObjectPool] ReturnObject: trả {enemy.name}, tổng pool: {objectPool.Count}, Active: {enemy.activeInHierarchy}");
    }

    private void ResetObject(GameObject enemy)
    {
        // Reset vị trí và xoay
        enemy.transform.position = Vector3.zero;
        enemy.transform.rotation = Quaternion.identity;

        // Reset các component
        Character character = enemy.GetComponent<Character>();
        if (character != null)
        {
            character.SetState(Character.PlayerState.IsIdle);
        }

        BotController botController = enemy.GetComponent<BotController>();
        if (botController != null)
        {
            botController.StopAllCoroutines();
            botController.Reset();
            botController.SetCanMove(false);
            botController.SpawnPoint = null;
        }

        BotRandomSkin randomSkin = enemy.GetComponent<BotRandomSkin>();
        if (randomSkin != null)
        {
            randomSkin.RandomizeAppearance();
        }

        BotGun gun = enemy.GetComponentInChildren<BotGun>();
        if (gun != null)
        {
            gun.ResetGun();
        }

        AttackRangeVisualBot attackRange = enemy.GetComponentInChildren<AttackRangeVisualBot>();
        if (attackRange != null && gun != null)
        {
            attackRange.gun = gun;
        }

        Cache.ClearCacheForObject(enemy);
        Debug.Log($"[ObjectPool] ResetObject: Đã reset {enemy.name} tại {enemy.transform.position}");
    }

    private bool ValidatePrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[ObjectPool] ValidatePrefab: prefab null!");
            return false;
        }

        bool isValid = true;
        if (prefab.GetComponent<Character>() == null) { Debug.LogError($"[ObjectPool] Prefab {prefab.name} thiếu Character!"); isValid = false; }
        if (prefab.GetComponent<BotController>() == null) { Debug.LogError($"[ObjectPool] Prefab {prefab.name} thiếu BotController!"); isValid = false; }
        if (prefab.GetComponent<BotRandomSkin>() == null) { Debug.LogError($"[ObjectPool] Prefab {prefab.name} thiếu BotRandomSkin!"); isValid = false; }
        if (prefab.GetComponentInChildren<BotGun>() == null) { Debug.LogError($"[ObjectPool] Prefab {prefab.name} thiếu BotGun!"); isValid = false; }
        if (prefab.GetComponentInChildren<AttackRangeVisualBot>() == null) { Debug.LogError($"[ObjectPool] Prefab {prefab.name} thiếu AttackRangeVisualBot!"); isValid = false; }
        if (prefab.GetComponent<NavMeshAgent>() == null) { Debug.LogError($"[ObjectPool] Prefab {prefab.name} thiếu NavMeshAgent!"); isValid = false; }

        return isValid;
    }

    private bool ValidateObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[ObjectPool] ValidateObject: object null!");
            return false;
        }

        bool isValid = true;
        if (obj.GetComponent<Character>() == null) { Debug.LogWarning($"[ObjectPool] Object {obj.name} thiếu Character!"); isValid = false; }
        if (obj.GetComponent<BotController>() == null) { Debug.LogWarning($"[ObjectPool] Object {obj.name} thiếu BotController!"); isValid = false; }
        if (obj.GetComponent<BotRandomSkin>() == null) { Debug.LogWarning($"[ObjectPool] Object {obj.name} thiếu BotRandomSkin!"); isValid = false; }
        if (obj.GetComponentInChildren<BotGun>() == null) { Debug.LogWarning($"[ObjectPool] Object {obj.name} thiếu BotGun!"); isValid = false; }
        if (obj.GetComponentInChildren<AttackRangeVisualBot>() == null) { Debug.LogWarning($"[ObjectPool] Object {obj.name} thiếu AttackRangeVisualBot!"); isValid = false; }
        if (obj.GetComponent<NavMeshAgent>() == null) { Debug.LogWarning($"[ObjectPool] Object {obj.name} thiếu NavMeshAgent!"); isValid = false; }

        return isValid;
    }
}