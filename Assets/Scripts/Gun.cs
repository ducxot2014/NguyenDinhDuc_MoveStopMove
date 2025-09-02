using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Fallback Bullet Prefab (gán nếu random thất bại)")]
    [SerializeField] private GameObject fallbackBulletPrefab = null;
    [SerializeField] private float fallbackBulletSpeed = 5f;

    [Header("Cài đặt súng")]
    [SerializeField] public Transform firePoint;
    [SerializeField] public float shootDelay = 0.5f;
    [SerializeField] public float shootCooldown = 1f;

    public bool isReady = true;
    private Character character;
    private Character currentTarget;
    public GameObject bulletPrefab;
    public float bulletSpeed;
    private PlayerWeaponManager playerWeaponManager;
    private SoundManager soundManager;

    private void Awake()
    {
        // Tìm PlayerWeaponManager trước
        playerWeaponManager = GetComponentInParent<PlayerWeaponManager>();
        if (playerWeaponManager == null)
        {
            playerWeaponManager = FindObjectOfType<PlayerWeaponManager>();
        }

        // Tìm Character
        if (playerWeaponManager != null)
        {
            character = playerWeaponManager.GetComponentInParent<Character>();
            if (character == null)
            {
                character = FindObjectOfType<Character>(true);
            }
        }
        else
        {
            character = GetComponentInParent<Character>();
            if (character == null)
            {
                BotController botController = GetComponentInParent<BotController>();
                if (botController != null)
                    character = botController.GetComponent<Character>();
            }
        }
       soundManager = FindObjectOfType<SoundManager>();

        if (character == null)
        {
            Debug.LogError($"[Gun] {name}: Không tìm thấy Character! Hierarchy: {GetHierarchyPath(gameObject)}");
        }
        else
        {
            Debug.Log($"[Gun] {name}: Tìm thấy Character: {character.name}");
        }

        // Tìm firePoint
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint") ?? transform;
            Debug.LogWarning($"[Gun] {name}: FirePoint chưa được gán, dùng {firePoint.name}.");
        }

        // Khởi tạo bulletPrefab ban đầu nếu là bot
        if (playerWeaponManager == null)
        {
            InitializeBotBulletPrefab();
        }
    }

    private void InitializeBotBulletPrefab()
    {
        BotController botController = GetComponentInParent<BotController>();
        if (botController != null && botController.skin != null && botController.skin.currentBulletPrefab != null)
        {
            SetBulletPrefab(botController.skin.currentBulletPrefab, botController.skin.currentBulletSpeed);
            Debug.Log($"[Gun] {name}: Khởi tạo bulletPrefab cho bot từ BotRandomSkin: {bulletPrefab?.name}, speed={bulletSpeed}");
        }
        else if (fallbackBulletPrefab != null)
        {
            SetBulletPrefab(fallbackBulletPrefab, fallbackBulletSpeed);
            Debug.Log($"[Gun] {name}: Khởi tạo bulletPrefab cho bot từ fallback: {bulletPrefab?.name}, speed={bulletSpeed}");
        }
        else
        {
            Debug.LogWarning($"[Gun] {name}: Không tìm thấy BotRandomSkin hoặc fallbackBulletPrefab để khởi tạo cho bot!");
        }
    }

    public void SetBulletPrefab(GameObject prefab, float speed)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[Gun] {name}: Prefab NULL, thử dùng fallback.");
            if (fallbackBulletPrefab != null)
            {
                bulletPrefab = fallbackBulletPrefab;
                bulletSpeed = fallbackBulletSpeed;
                Debug.Log($"[Gun] {name}: Dùng fallback {bulletPrefab.name}, ID: {bulletPrefab.GetInstanceID()}, speed={bulletSpeed}");
            }
            else
            {
                Debug.LogError($"[Gun] {name}: Prefab NULL và không có fallback! Vui lòng gán fallbackBulletPrefab trong Inspector.");
                bulletPrefab = null;
                bulletSpeed = 0f;
            }
        }
        else
        {
            if (!prefab.GetComponent<Bullet>())
            {
                Debug.LogError($"[Gun] {name}: bulletPrefab {prefab.name} thiếu component Bullet!");
                bulletPrefab = null;
                bulletSpeed = 0f;
            }
            else
            {
                bulletPrefab = prefab;
                bulletSpeed = speed;
                isReady = true; // Đảm bảo sẵn sàng bắn
                Debug.Log($"[Gun] {name}: Nhận bulletPrefab={prefab.name}, ID: {prefab.GetInstanceID()}, speed={speed}, isReady={isReady}");
            }
        }
    }

    public bool CanShoot()
    {
        bool can = isReady && bulletPrefab != null;
        if (playerWeaponManager != null)
            can = can && playerWeaponManager.shootPoint != null;
        else
            can = can && firePoint != null;
        if (!can)
        {
            Debug.LogWarning($"[Gun] {name}: CanShoot=false (isReady={isReady}, bulletPrefab={bulletPrefab != null}, shootPoint={(playerWeaponManager != null ? playerWeaponManager.shootPoint != null : "N/A")}, firePoint={firePoint != null})");
        }
        return can;
    }

    public void Shoot(Character target = null)
    {
        if (!CanShoot() || target == null || target == character || target.IsDead)
        {
            Debug.LogWarning($"[Gun] {name}: Không thể bắn, CanShoot={CanShoot()}, target={(target != null ? target.name : "null")}, character={(character != null ? character.name : "null")}, target.IsDead={(target != null ? target.IsDead : false)}");
            return;
        }

        soundManager.PlayThrow();
        currentTarget = target;
        isReady = false;

        if (character != null)
            character.SetState(Character.PlayerState.IsAttack);
        StartCoroutine(ShootRoutine());
        Debug.Log($"[Gun] {name}: Bắt đầu bắn {target.name}, bulletPrefab={bulletPrefab?.name}, ID: {bulletPrefab?.GetInstanceID()}");
    }

    private IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(shootDelay);
        FireBullet();
        yield return new WaitForSeconds(shootCooldown);

        if (character != null && character.IsInState(Character.PlayerState.IsAttack))
            character.SetState(Character.PlayerState.IsIdle);

        isReady = true;
        Debug.Log($"[Gun] {name}: Hoàn thành bắn, isReady={isReady}");
    }

    private void FireBullet()
    {
        if (currentTarget == null || bulletPrefab == null) return;

        Transform spawnPoint = playerWeaponManager != null ? playerWeaponManager.shootPoint : firePoint;
        if (spawnPoint == null)
        {
            Debug.LogError($"[Gun] {name}: Không thể bắn! spawnPoint null.");
            return;
        }

        Vector3 dir = (currentTarget.transform.position - spawnPoint.position);
        dir.y = 0f;
        dir.Normalize();

        if (playerWeaponManager != null)
        {
            FirePlayerBullet(dir, spawnPoint);
        }
        else
        {
            FireBotBullet(dir, spawnPoint);
        }
    }

    private void FirePlayerBullet(Vector3 dir, Transform spawnPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            dir = (hit.point - spawnPoint.position).normalized;
            dir.y = 0f;
        }

        spawnPoint.rotation = Quaternion.LookRotation(dir);
        Vector3 spawnPos = spawnPoint.position + spawnPoint.forward * 2f;

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, spawnPoint.rotation);
        if (bulletObj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetDirection(dir, bulletSpeed); // Chỉ truyền dir và bulletSpeed
            Debug.Log($"[Gun] {name}: Player bắn {bulletPrefab.name} vào {currentTarget.name}, speed={bulletSpeed}, clone: {bulletObj.name}");
        }
        else
        {
            Debug.LogError($"[Gun] {name}: Prefab {bulletObj.name} không có component Bullet! Kiểm tra bulletPrefab.");
        }

        Debug.DrawRay(spawnPoint.position, dir * 5f, Color.red, 1f);
    }

    private void FireBotBullet(Vector3 dir, Transform spawnPoint)
    {
        spawnPoint.rotation = Quaternion.LookRotation(dir);
        Vector3 spawnPos = spawnPoint.position + spawnPoint.forward * 0.5f;

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, spawnPoint.rotation);
        if (bulletObj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetDirection(dir, bulletSpeed); // Chỉ truyền dir và bulletSpeed
            Debug.Log($"[Gun] {name}: Bot bắn {bulletPrefab.name} vào {currentTarget.name}, speed={bulletSpeed}, clone: {bulletObj.name}");
        }
        else
        {
            Debug.LogError($"[Gun] {name}: Prefab {bulletObj.name} không có component Bullet! Kiểm tra bulletPrefab.");
        }

        Debug.DrawRay(spawnPoint.position, dir * 5f, Color.red, 1f);
    }

    public void ResetGun()
    {
        isReady = true;
        currentTarget = null;
        bulletPrefab = null;
        bulletSpeed = 0f;

        if (playerWeaponManager != null)
        {
            if (playerWeaponManager.currentBulletPrefab != null)
            {
                SetBulletPrefab(playerWeaponManager.currentBulletPrefab, playerWeaponManager.currentBulletSpeed);
                Debug.Log($"[Gun] {name}: Reset và đồng bộ từ PlayerWeaponManager: bulletPrefab={bulletPrefab?.name}, speed={bulletSpeed}");
            }
            else
            {
                Debug.LogWarning($"[Gun] {name}: PlayerWeaponManager.currentBulletPrefab null, không đồng bộ được.");
            }
        }
        else
        {
            BotController botController = GetComponentInParent<BotController>();
            if (botController != null && botController.skin != null && botController.skin.currentBulletPrefab != null)
            {
                SetBulletPrefab(botController.skin.currentBulletPrefab, botController.skin.currentBulletSpeed);
                Debug.Log($"[Gun] {name}: Reset và đồng bộ từ BotRandomSkin: bulletPrefab={bulletPrefab?.name}, speed={bulletSpeed}");
            }
            else if (fallbackBulletPrefab != null)
            {
                SetBulletPrefab(fallbackBulletPrefab, fallbackBulletSpeed);
                Debug.Log($"[Gun] {name}: Reset và dùng fallback: bulletPrefab={bulletPrefab?.name}, speed={bulletSpeed}");
            }
            else
            {
                Debug.LogWarning($"[Gun] {name}: Không tìm thấy BotController, BotRandomSkin hoặc fallbackBulletPrefab để đồng bộ!");
            }
        }

        if (firePoint != null)
            firePoint.rotation = transform.rotation;
    }

    private string GetHierarchyPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }
}