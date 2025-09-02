using System.Collections;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponAttachPoint;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private WeaponDatabase weaponDatabase;

    public WeaponData[] weaponData;
    public int currentWeaponIndex;
    public GameObject currentInstance;
    private GameObject lastEquippedWeaponPrefab;
    private bool isPreviewing = false;

    [SerializeField] private BotRandomSkin botRandomSkin;

    private void OnEnable()
    {
        if (botRandomSkin == null)
            botRandomSkin = GetComponentInParent<BotRandomSkin>();

        StartCoroutine(DelayedSync());
    }

    private IEnumerator DelayedSync()
    {
        yield return null; // Chờ 1 frame

        if (weaponDatabase != null && weaponData == null)
        {
            weaponData = weaponDatabase.weapons;
            Debug.Log($"[WeaponManager] {name}: Đồng bộ weaponData từ WeaponDatabase.");
        }

        if (botRandomSkin != null && weaponData != null && weaponData.Length > 0)
        {
            if (botRandomSkin.currentBulletPrefab != null)
            {
                weaponData[0].bulletPrefab = botRandomSkin.currentBulletPrefab;
                weaponData[0].bulletSpeed = botRandomSkin.currentBulletSpeed;
                Debug.Log($"[WeaponManager] {name}: Đồng bộ thành công từ BotRandomSkin: {weaponData[0].bulletPrefab.name}");

                if (lastEquippedWeaponPrefab != null)
                {
                    EquipWeapon(lastEquippedWeaponPrefab);
                }
            }
            else
            {
                Debug.LogWarning($"[WeaponManager] {name}: BotRandomSkin currentBulletPrefab null, không đồng bộ.");
            }
        }
        else
        {
            Debug.LogWarning($"[WeaponManager] {name}: Không tìm thấy BotRandomSkin hoặc weaponData để đồng bộ!");
        }
    }

    private void Start()
    {
        Debug.Log($"[WeaponManager] {name}: Start called, botRandomSkin={botRandomSkin != null}, weaponDatabase={weaponDatabase != null}");
        if (botRandomSkin == null)
            botRandomSkin = GetComponentInParent<BotRandomSkin>();
    }

    public void Shoot(Player player)
    {
        if (player == null)
        {
            Debug.LogWarning($"[WeaponManager] {name}: Không thể bắn, Player là null!");
            return;
        }

        if (weaponData == null || weaponData.Length == 0 || currentWeaponIndex < 0 || currentWeaponIndex >= weaponData.Length)
        {
            Debug.LogWarning($"[WeaponManager] {name}: Không thể bắn: weaponData null hoặc currentWeaponIndex không hợp lệ (index: {currentWeaponIndex})!");
            return;
        }

        WeaponData weapon = weaponData[currentWeaponIndex];
        if (weapon == null || weapon.bulletPrefab == null || shootPoint == null)
        {
            Debug.LogWarning($"[WeaponManager] {name}: Không thể bắn: WeaponData={weapon}, bulletPrefab={(weapon == null ? "null" : weapon.bulletPrefab?.name)}, shootPoint={(shootPoint == null ? "null" : shootPoint.name)}!");
            return;
        }

        Vector3 spawnPos = shootPoint.position + shootPoint.forward * 2f;
        GameObject bulletObj = Instantiate(weapon.bulletPrefab, spawnPos, shootPoint.rotation);
        if (bulletObj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetDirection(shootPoint.forward, weapon.bulletSpeed); // Chỉ truyền dir và bulletSpeed
            Debug.Log($"[WeaponManager] {name}: Đã bắn đạn {weapon.bulletPrefab.name} từ {shootPoint.name} với tốc độ {weapon.bulletSpeed}");
        }
        else
        {
            Debug.LogError($"[WeaponManager] {name}: Đạn {bulletObj.name} không có component Bullet!");
        }
    }

    public void EquipWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning($"[WeaponManager] {name}: EquipWeapon: weaponPrefab is null, nothing to equip.");
            return;
        }

        DestroyCurrentInstance();
        lastEquippedWeaponPrefab = weaponPrefab;
        currentInstance = Instantiate(weaponPrefab, weaponAttachPoint);

        Gun gun = currentInstance.GetComponent<Gun>();
        if (gun != null && weaponData != null && weaponData.Length > currentWeaponIndex && weaponData[currentWeaponIndex] != null)
        {
            gun.SetBulletPrefab(weaponData[currentWeaponIndex].bulletPrefab, weaponData[currentWeaponIndex].bulletSpeed);
            Debug.Log($"[WeaponManager] {name}: Truyền bulletPrefab={weaponData[currentWeaponIndex].bulletPrefab?.name} cho Gun");
        }

        Collider weaponCollider = currentInstance.GetComponent<Collider>();
        if (weaponCollider != null)
        {
            Destroy(weaponCollider);
            Debug.Log($"[WeaponManager] {name}: Đã xóa Collider từ vũ khí {currentInstance.name}");
        }
        Rigidbody weaponRigidbody = currentInstance.GetComponent<Rigidbody>();
        if (weaponRigidbody != null)
        {
            Destroy(weaponRigidbody);
            Debug.Log($"[WeaponManager] {name}: Đã xóa Rigidbody từ vũ khí {currentInstance.name}");
        }
        isPreviewing = false;
        Debug.Log($"[WeaponManager] {name}: Equipped weapon: {weaponPrefab.name}");
    }

    public void PreviewWeapon(GameObject weaponPrefab)
    {
        DestroyCurrentInstance();
        if (weaponPrefab == null)
        {
            Debug.LogWarning($"[WeaponManager] {name}: PreviewWeapon: weaponPrefab is null, nothing to preview.");
            return;
        }

        currentInstance = Instantiate(weaponPrefab, weaponAttachPoint);
        isPreviewing = true;
        Debug.Log($"[WeaponManager] {name}: Previewing weapon: {weaponPrefab.name}");
    }

    public void ClearPreview()
    {
        if (!isPreviewing) return;

        DestroyCurrentInstance();
        if (lastEquippedWeaponPrefab != null)
        {
            currentInstance = Instantiate(lastEquippedWeaponPrefab, weaponAttachPoint);
            isPreviewing = false;
            Debug.Log($"[WeaponManager] {name}: Cleared preview, restored weapon: {lastEquippedWeaponPrefab.name}");
        }
        else
        {
            isPreviewing = false;
            Debug.Log($"[WeaponManager] {name}: Cleared preview, no equipped weapon to restore.");
        }
    }

    public void ClearAll()
    {
        DestroyCurrentInstance();
        lastEquippedWeaponPrefab = null;
        isPreviewing = false;
        Debug.Log($"[WeaponManager] {name}: Cleared all weapons.");
    }

    public void ResetToLastEquipped()
    {
        DestroyCurrentInstance();
        if (lastEquippedWeaponPrefab != null)
        {
            currentInstance = Instantiate(lastEquippedWeaponPrefab, weaponAttachPoint);
            isPreviewing = false;
            Debug.Log($"[WeaponManager] {name}: Restored last equipped weapon: {lastEquippedWeaponPrefab.name}");
        }
    }

    private void DestroyCurrentInstance()
    {
        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }
    }
}