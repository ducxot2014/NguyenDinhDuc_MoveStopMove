using System.Collections;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponAttachPoint;
    [SerializeField] public Transform shootPoint;
    [SerializeField] private WeaponDatabase weaponDatabase;

    public WeaponData[] weaponData;
    public int currentWeaponIndex;
    public GameObject currentInstance; // Visual của vũ khí (có Gun)
    private GameObject lastEquippedVisualPrefab;
    private bool isPreviewing = false;
    [HideInInspector] public GameObject currentBulletPrefab; // Vũ khí ném (có Bullet)
    [HideInInspector] public float currentBulletSpeed;

    private void Awake()
    {
        if (!gameObject.CompareTag("Player"))
        {
            Debug.LogWarning($"[PlayerWeaponManager] {name}: This script should only be attached to a GameObject with 'Player' tag!");
        }
        if (weaponAttachPoint == null)
        {
            Debug.LogError($"[PlayerWeaponManager] {name}: weaponAttachPoint is not assigned in Inspector!");
        }
        if (shootPoint == null)
        {
            Debug.LogError($"[PlayerWeaponManager] {name}: shootPoint is not assigned in Inspector!");
        }
        Debug.Log($"[PlayerWeaponManager] {name}: Awake called.");
    }

    private void OnEnable()
    {
        // Không gọi DelayedSync nữa, để ShopManager xử lý đồng bộ
    }

    public void Shoot(Player player)
    {
        if (player == null)
        {
            Debug.LogWarning($"[PlayerWeaponManager] {name}: Không thể ném, Player là null!");
            return;
        }

        if (currentBulletPrefab == null || shootPoint == null)
        {
            Debug.LogWarning($"[PlayerWeaponManager] {name}: Không thể ném: bulletPrefab={(currentBulletPrefab == null ? "null" : currentBulletPrefab.name)}, shootPoint={(shootPoint == null ? "null" : shootPoint.name)}!");
            return;
        }

        Vector3 spawnPos = shootPoint.position + shootPoint.forward * 2f;
        GameObject bulletObj = Instantiate(currentBulletPrefab, spawnPos, shootPoint.rotation);
        if (bulletObj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetDirection(shootPoint.forward, currentBulletSpeed);
            Debug.Log($"[PlayerWeaponManager] {name}: Đã ném vũ khí {currentBulletPrefab.name} từ {shootPoint.name} với tốc độ {currentBulletSpeed}");
        }
        else
        {
            Debug.LogError($"[PlayerWeaponManager] {name}: Vũ khí ném {bulletObj.name} không có component Bullet!");
        }
    }

    public void EquipWeapon(GameObject visualPrefab, GameObject bulletPrefab, float bulletSpeed)
    {
        if (visualPrefab == null)
        {
            Debug.LogWarning($"[PlayerWeaponManager] {name}: EquipWeapon: visualPrefab is null, nothing to equip.");
            return;
        }

        DestroyCurrentInstance();
        lastEquippedVisualPrefab = visualPrefab;
        currentInstance = Instantiate(visualPrefab, weaponAttachPoint);
        currentBulletPrefab = bulletPrefab;
        currentBulletSpeed = bulletSpeed;

        Gun gun = currentInstance.GetComponent<Gun>();
        if (gun != null)
        {
            if (currentBulletPrefab != null)
            {
                gun.SetBulletPrefab(currentBulletPrefab, currentBulletSpeed);
                gun.isReady = true; // Đảm bảo Gun sẵn sàng bắn
                Debug.Log($"[PlayerWeaponManager] {name}: Truyền bulletPrefab={currentBulletPrefab?.name} (speed={currentBulletSpeed}) cho Gun trên {visualPrefab.name}, isReady={gun.isReady}");
            }
            else
            {
                Debug.LogWarning($"[PlayerWeaponManager] {name}: currentBulletPrefab is null for weapon {visualPrefab.name}, cannot shoot!");
            }
        }
        else
        {
            Debug.LogError($"[PlayerWeaponManager] {name}: Không tìm thấy component Gun trên {visualPrefab.name}! Kiểm tra prefab vũ khí.");
        }

        Collider weaponCollider = currentInstance.GetComponent<Collider>();
        if (weaponCollider != null)
        {
            Destroy(weaponCollider);
            Debug.Log($"[PlayerWeaponManager] {name}: Đã xóa Collider từ visual {currentInstance.name}");
        }
        Rigidbody weaponRigidbody = currentInstance.GetComponent<Rigidbody>();
        if (weaponRigidbody != null)
        {
            Destroy(weaponRigidbody);
            Debug.Log($"[PlayerWeaponManager] {name}: Đã xóa Rigidbody từ visual {currentInstance.name}");
        }
        isPreviewing = false;
        Debug.Log($"[PlayerWeaponManager] {name}: Equipped weapon visual: {visualPrefab.name}");

        // Thông báo cho AttackRangeVisual cập nhật gun
        var attackRangeVisual = GetComponentInParent<Character>()?.GetComponentInChildren<AttackRangeVisual>();
        if (attackRangeVisual != null)
        {
            attackRangeVisual.OnWeaponChanged();
            Debug.Log($"[PlayerWeaponManager] {name}: Notified AttackRangeVisual to update gun.");
        }
    }

    public void PreviewWeapon(GameObject visualPrefab)
    {
        DestroyCurrentInstance();
        if (visualPrefab == null)
        {
            Debug.LogWarning($"[PlayerWeaponManager] {name}: PreviewWeapon: visualPrefab is null, nothing to preview.");
            return;
        }

        currentInstance = Instantiate(visualPrefab, weaponAttachPoint);
        isPreviewing = true;
        Debug.Log($"[PlayerWeaponManager] {name}: Previewing weapon visual: {visualPrefab.name}");
    }

    public void ClearPreview()
    {
        if (!isPreviewing) return;

        DestroyCurrentInstance();
        if (lastEquippedVisualPrefab != null)
        {
            currentInstance = Instantiate(lastEquippedVisualPrefab, weaponAttachPoint);
            isPreviewing = false;
            Debug.Log($"[PlayerWeaponManager] {name}: Cleared preview, restored weapon visual: {lastEquippedVisualPrefab.name}");

            // Thông báo cho AttackRangeVisual cập nhật gun
            var attackRangeVisual = GetComponentInParent<Character>()?.GetComponentInChildren<AttackRangeVisual>();
            if (attackRangeVisual != null)
            {
                attackRangeVisual.OnWeaponChanged();
                Debug.Log($"[PlayerWeaponManager] {name}: Notified AttackRangeVisual to update gun after clearing preview.");
            }
        }
        else
        {
            isPreviewing = false;
            Debug.Log($"[PlayerWeaponManager] {name}: Cleared preview, no equipped weapon to restore.");
        }
    }

    public void ClearAll()
    {
        DestroyCurrentInstance();
        lastEquippedVisualPrefab = null;
        currentBulletPrefab = null;
        currentBulletSpeed = 0f;
        isPreviewing = false;
        Debug.Log($"[PlayerWeaponManager] {name}: Cleared all weapons.");

        // Thông báo cho AttackRangeVisual cập nhật gun
        var attackRangeVisual = GetComponentInParent<Character>()?.GetComponentInChildren<AttackRangeVisual>();
        if (attackRangeVisual != null)
        {
            attackRangeVisual.OnWeaponChanged();
            Debug.Log($"[PlayerWeaponManager] {name}: Notified AttackRangeVisual to update gun after clearing all.");
        }
    }

    public void ResetToLastEquipped()
    {
        DestroyCurrentInstance();
        if (lastEquippedVisualPrefab != null)
        {
            currentInstance = Instantiate(lastEquippedVisualPrefab, weaponAttachPoint);
            isPreviewing = false;
            Debug.Log($"[PlayerWeaponManager] {name}: Restored last equipped weapon visual: {lastEquippedVisualPrefab.name}");

            // Thông báo cho AttackRangeVisual cập nhật gun
            var attackRangeVisual = GetComponentInParent<Character>()?.GetComponentInChildren<AttackRangeVisual>();
            if (attackRangeVisual != null)
            {
                attackRangeVisual.OnWeaponChanged();
                Debug.Log($"[PlayerWeaponManager] {name}: Notified AttackRangeVisual to update gun after resetting to last equipped.");
            }
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