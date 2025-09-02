using System.Collections.Generic;
using UnityEngine;

public static class Cache
{
    private static Dictionary<GameObject, Character> characterCache = new();
    private static Dictionary<GameObject, Gun> gunCache = new();
    private static Dictionary<GameObject, BotGun> botGunCache = new();
    private static Dictionary<GameObject, PlayerMoving> playerMovingCache = new();
    private static Dictionary<GameObject, Player> playerCache = new();
    private static Dictionary<GameObject, UIManager> uiManagerCache = new();
    private static Dictionary<GameObject, CameraFollow> cameraFollowCache = new();
    private static Dictionary<GameObject, ShopManager> shopManagerCache = new();
    private static Dictionary<GameObject, BotController> botControllerCache = new();
    private static Dictionary<GameObject, EnemySpawner> enemySpawnerCache = new();
    private static Dictionary<GameObject, AttackRangeVisual> attackRangeVisualCache = new();
    private static Dictionary<GameObject, AttackRangeVisualBot> attackRangeVisualBotCache = new();
    private static Dictionary<GameObject, HatManager> hatManagerCache = new();
    private static Dictionary<GameObject, GameManager> gameManagerCache = new();
    private static Dictionary<GameObject, BotMovement> botMovementCache = new();
    private static Dictionary<(GameObject, System.Type), Component> genericComponentCache = new();

    public static T GetComponent<T>(GameObject obj) where T : Component
    {
        if (obj == null)
        {
            Debug.LogWarning("[Cache] GetComponent: GameObject is null!");
            return null;
        }

        var key = (obj, typeof(T));
        if (!genericComponentCache.TryGetValue(key, out var component))
        {
            component = obj.GetComponent<T>()
                        ?? obj.GetComponentInChildren<T>() ?? obj.GetComponentInParent<T>();

            genericComponentCache[key] = component;
            if (component == null)
            {
                Debug.LogWarning($"[Cache] GetComponent: Không tìm thấy {typeof(T).Name} trên GameObject {obj.name} hoặc children!");
            }
        }
        return component as T;
    }

    public static Character GetCharacter(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[Cache] GetCharacter: GameObject is null!");
            return null;
        }

        if (!characterCache.TryGetValue(obj, out var character))
        {
            character = obj.GetComponent<Character>()
                        ?? obj.GetComponentInParent<Character>()
                        ?? obj.GetComponentInChildren<Character>();
            characterCache[obj] = character;
            if (character == null)
            {
                Debug.LogWarning($"[Cache] GetCharacter: Không tìm thấy Character trên GameObject {obj.name}, parent, hoặc children!");
            }
            else
            {
                Debug.Log($"[Cache] GetCharacter: Tìm thấy Character trên {obj.name}: {character.name}");
            }
        }
        return character;
    }

    public static Gun GetGun(GameObject obj)
    {
        if (!gunCache.TryGetValue(obj, out var gun))
        {
            gun = obj.GetComponent<Gun>();
            gunCache[obj] = gun;
        }
        return gun;
    }
    public static BotGun GetBotGun(GameObject obj)
    {
        if (!botGunCache.TryGetValue(obj, out var botGun))
        {
            botGun = obj.GetComponent<BotGun>() ?? obj.GetComponentInChildren<BotGun>() ?? obj.GetComponentInParent<BotGun>();
            botGunCache[obj] = botGun;
        }
        return botGun;
    }

    public static PlayerMoving GetPlayerMoving(GameObject obj)
    {
        if (!playerMovingCache.TryGetValue(obj, out var pm))
        {
            pm = obj.GetComponent<PlayerMoving>()
                 ?? obj.GetComponentInParent<PlayerMoving>()
                 ?? obj.GetComponentInChildren<PlayerMoving>();
            playerMovingCache[obj] = pm;
        }
        return pm;
    }

    public static Player GetPlayer(GameObject obj)
    {
        if (!playerCache.TryGetValue(obj, out var player))
        {
            player = obj.GetComponent<Player>();
            playerCache[obj] = player;
        }
        return player;
    }

    public static UIManager GetUIManager(GameObject obj)
    {
        if (!uiManagerCache.TryGetValue(obj, out var uiManager))
        {
            uiManager = obj.GetComponent<UIManager>();
            uiManagerCache[obj] = uiManager;
        }
        return uiManager;
    }

    public static CameraFollow GetCameraFollow(GameObject obj)
    {
        if (!cameraFollowCache.TryGetValue(obj, out var cameraFollow))
        {
            cameraFollow = obj.GetComponent<CameraFollow>();
            cameraFollowCache[obj] = cameraFollow;
        }
        return cameraFollow;
    }

    public static ShopManager GetShopManager(GameObject obj)
    {
        if (!shopManagerCache.TryGetValue(obj, out var shopManager))
        {
            shopManager = obj.GetComponent<ShopManager>();
            shopManagerCache[obj] = shopManager;
        }
        return shopManager;
    }

    public static BotController GetBotController(GameObject obj)
    {
        if (!botControllerCache.TryGetValue(obj, out var botController))
        {
            botController = obj.GetComponent<BotController>() ?? obj.GetComponentInChildren<BotController>() ?? obj.GetComponentInParent<BotController>();
            botControllerCache[obj] = botController;
        }
        return botController;
    }

    public static EnemySpawner GetEnemySpawner(GameObject obj)
    {
        if (!enemySpawnerCache.TryGetValue(obj, out var enemySpawner))
        {
            enemySpawner = obj.GetComponent<EnemySpawner>();
            enemySpawnerCache[obj] = enemySpawner;
        }
        return enemySpawner;
    }

    public static AttackRangeVisual GetAttackRangeVisual(GameObject obj)
    {
        if (!attackRangeVisualCache.TryGetValue(obj, out var attackRangeVisual))
        {
            attackRangeVisual = obj.GetComponentInChildren<AttackRangeVisual>()
                                ?? obj.GetComponent<AttackRangeVisual>()
                                ?? obj.GetComponentInParent<AttackRangeVisual>()
                                ?? obj.GetComponentInChildren<IAttackRange>() as AttackRangeVisual;
            attackRangeVisualCache[obj] = attackRangeVisual;
        }
        return attackRangeVisual;
    }

    public static AttackRangeVisualBot GetAttackRangeVisualBot(GameObject obj)
    {
        if (!attackRangeVisualBotCache.TryGetValue(obj, out var attackRangeVisualBot))
        {
            attackRangeVisualBot = obj.GetComponentInChildren<AttackRangeVisualBot>()
                                  ?? obj.GetComponent<AttackRangeVisualBot>()
                                  ?? obj.GetComponentInParent<AttackRangeVisualBot>();
            attackRangeVisualBotCache[obj] = attackRangeVisualBot;
            if (attackRangeVisualBot == null)
            {
                Debug.LogWarning($"[Cache] GetAttackRangeVisualBot: Không tìm thấy AttackRangeVisualBot trên GameObject {obj.name}, parent, hoặc children!");
            }
        }
        return attackRangeVisualBot;
    }

    public static HatManager GetHatManager(GameObject obj)
    {
        if (!hatManagerCache.TryGetValue(obj, out var hatManager))
        {
            hatManager = obj.GetComponent<HatManager>();
            hatManagerCache[obj] = hatManager;
        }
        return hatManager;
    }

    public static GameManager GetGameManager(GameObject obj)
    {
        if (!gameManagerCache.TryGetValue(obj, out var gameManager))
        {
            gameManager = obj.GetComponent<GameManager>();
            gameManagerCache[obj] = gameManager;
        }
        return gameManager;
    }

    public static BotMovement GetBotMovement(GameObject obj)
    {
        if (!botMovementCache.TryGetValue(obj, out var botMovement))
        {
            botMovement = obj.GetComponent<BotMovement>();
            botMovementCache[obj] = botMovement;
        }
        return botMovement;
    }

    public static void ClearCacheForObject(GameObject obj)
    {
        characterCache.Remove(obj);
        gunCache.Remove(obj);
        playerMovingCache.Remove(obj);
        playerCache.Remove(obj);
        uiManagerCache.Remove(obj);
        cameraFollowCache.Remove(obj);
        shopManagerCache.Remove(obj);
        botControllerCache.Remove(obj);
        enemySpawnerCache.Remove(obj);
        attackRangeVisualCache.Remove(obj);
        attackRangeVisualBotCache.Remove(obj);
        hatManagerCache.Remove(obj);
        gameManagerCache.Remove(obj);
        botMovementCache.Remove(obj);
        botGunCache.Remove(obj);

        // SỬA: Thay RemoveWhere bằng cách thu thập keys và xóa
        var keysToRemove = new List<(GameObject, System.Type)>();
        foreach (var kvp in genericComponentCache)
        {
            if (kvp.Key.Item1 == obj)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            genericComponentCache.Remove(key);
        }

        Debug.Log($"[Cache] Cleared cache for {obj.name}");
    }
}