using System.Collections;
using UnityEngine;

public class BotGun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootDelay = 0.5f;
    [SerializeField] private float shootCooldown = 1f;

    private bool isReady = true;
    private Character character;
    private BotController botController;
    private GameObject bulletPrefab;
    private float bulletSpeed;

    private void Awake()
    {
        character = GetComponentInParent<Character>();
        if (character == null)
        {
            botController = GetComponentInParent<BotController>();
            if (botController != null)
                character = botController.GetComponent<Character>();
        }

        if (character == null)
            Debug.LogError($"[BotGun] {name}: Không tìm thấy Character! Hierarchy: {GetHierarchyPath(gameObject)}");
        else
            Debug.Log($"[BotGun] {name}: Tìm thấy Character: {character.name}");

        if (botController == null)
        {
            botController = GetComponentInParent<BotController>();
            if (botController == null)
                Debug.LogError($"[BotGun] {name}: Không tìm thấy BotController! Kiểm tra hierarchy.");
        }
        else
        {
            Debug.Log($"[BotGun] {name}: Tìm thấy BotController: {botController.name}");
        }

        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint") ?? transform;
            Debug.LogWarning($"[BotGun] {name}: FirePoint chưa được gán, dùng {firePoint.name}.");
        }

        InitializeBulletPrefab();
    }

    private void InitializeBulletPrefab()
    {
        Debug.Log($"[BotGun] {name}: Bắt đầu khởi tạo bulletPrefab.");

        if (botController == null)
        {
            Debug.LogError($"[BotGun] {name}: BotController null! Kiểm tra hierarchy.");
            return;
        }

        if (botController.skin == null)
        {
            Debug.LogError($"[BotGun] {name}: BotController.skin null! BotRandomSkin không được tìm thấy trong hierarchy.");
            return;
        }

        if (botController.skin.currentBulletPrefab != null)
        {
            SetBulletPrefab(botController.skin.currentBulletPrefab, botController.skin.currentBulletSpeed);
            Debug.Log($"[BotGun] {name}: Khởi tạo bulletPrefab từ BotRandomSkin: {bulletPrefab.name}, speed={bulletSpeed}");
        }
    }

    public void SetBulletPrefab(GameObject prefab, float speed)
    {
        if (prefab == null)
        {
            Debug.LogError($"[BotGun] {name}: Prefab NULL! Kiểm tra WeaponDatabase.");
            bulletPrefab = null;
            bulletSpeed = 0f;
        }
        else
        {
            if (!prefab.GetComponent<Bullet>())
            {
                Debug.LogError($"[BotGun] {name}: bulletPrefab {prefab.name} thiếu component Bullet!");
                bulletPrefab = null;
                bulletSpeed = 0f;
            }
            else
            {
                bulletPrefab = prefab;
                bulletSpeed = speed;
                Debug.Log($"[BotGun] {name}: Nhận bulletPrefab={prefab.name}, ID: {prefab.GetInstanceID()}, speed={speed}");
            }
        }
    }

    public bool CanShoot()
    {
        bool can = isReady && bulletPrefab != null && firePoint != null;
        if (!can)
            Debug.LogWarning($"[BotGun] {name}: CanShoot=false (isReady={isReady}, bulletPrefab={bulletPrefab != null}, firePoint={firePoint != null})");
        return can;
    }

    public void Shoot()
    {
        if (!CanShoot() || botController == null || botController.currentTarget == null || botController.currentTarget.IsDead)
        {
            Debug.LogWarning($"[BotGun] {name}: Không thể bắn, CanShoot={CanShoot()}, botController={botController != null}, target={(botController?.currentTarget != null ? botController.currentTarget.name : "null")}, target.IsDead={(botController?.currentTarget != null ? botController.currentTarget.IsDead : false)}");
            return;
        }

        isReady = false;

        character.SetState(Character.PlayerState.IsAttack);
        StartCoroutine(ShootRoutine());
        Debug.Log($"[BotGun] {name}: Bắt đầu bắn {botController.currentTarget.name}, bulletPrefab={bulletPrefab.name}, ID: {bulletPrefab.GetInstanceID()}");
    }

    private IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(shootDelay);
        FireBullet();
        yield return new WaitForSeconds(shootCooldown);

        if (character != null && character.IsInState(Character.PlayerState.IsAttack))
            character.SetState(Character.PlayerState.IsIdle);

        isReady = true;
        Debug.Log($"[BotGun] {name}: Hoàn thành bắn, isReady={isReady}");
    }

    private void FireBullet()
    {
        if (botController == null || botController.currentTarget == null || bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning($"[BotGun] {name}: Không thể bắn: botController={botController != null}, target={(botController?.currentTarget != null ? botController.currentTarget.name : "null")}, bulletPrefab={bulletPrefab != null}, firePoint={firePoint != null}");
            return;
        }

        Vector3 dir = (botController.currentTarget.transform.position - firePoint.position).normalized;
        dir.y = 0f;

        firePoint.rotation = Quaternion.LookRotation(dir);
        Vector3 spawnPos = firePoint.position + firePoint.forward * 0.5f;

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, firePoint.rotation);
        if (bulletObj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetDirection(dir, bulletSpeed); // Chỉ truyền dir và bulletSpeed
            Debug.Log($"[BotGun] {name}: Bot bắn/ném {bulletPrefab.name} vào {botController.currentTarget.name}, speed={bulletSpeed}, clone: {bulletObj.name}");
        }
        else
        {
            Debug.LogError($"[BotGun] {name}: Prefab {bulletObj.name} không có component Bullet! Kiểm tra bulletPrefab.");
        }

        Debug.DrawRay(firePoint.position, dir * 5f, Color.red, 1f);
    }

    public void ResetGun()
    {
        isReady = true;
        bulletPrefab = null;
        bulletSpeed = 0f;

        if (botController != null && botController.skin != null && botController.skin.currentBulletPrefab != null)
        {
            SetBulletPrefab(botController.skin.currentBulletPrefab, botController.skin.currentBulletSpeed);
            Debug.Log($"[BotGun] {name}: Reset và đồng bộ từ BotRandomSkin: bulletPrefab={bulletPrefab.name}, speed={bulletSpeed}");
        }
        else
        {
            Debug.LogError($"[BotGun] {name}: Không tìm thấy BotRandomSkin hoặc currentBulletPrefab null! Kiểm tra WeaponDatabase.");
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