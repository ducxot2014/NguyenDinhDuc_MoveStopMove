using UnityEngine;

public class BotRandomSkin : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private WeaponDatabase database;

    [Header("Slots")]
    public Transform headSlot;
    public Transform handSlot;

    [HideInInspector] public WeaponData currentWeapon;
    [HideInInspector] public HelmetData currentHelmet;

    [HideInInspector] public GameObject currentBulletPrefab;
    [HideInInspector] public float currentBulletSpeed;

    private WeaponManager weaponManager;

    private void Awake()
    {
        weaponManager = GetComponentInParent<WeaponManager>();
        if (headSlot == null) Debug.LogWarning($"[BotRandomSkin] {name}: headSlot null!");
        if (handSlot == null) Debug.LogWarning($"[BotRandomSkin] {name}: handSlot null!");
        RandomizeAppearance(); // Gọi ngay trong Awake
    }

    private void Start()
    {
        Debug.Log($"[BotRandomSkin] {name}: Start called, không random lại.");
    }

    public void RandomizeAppearance()
    {
        if (weaponManager != null && weaponManager.currentInstance != null)
        {
            Debug.Log($"[BotRandomSkin] {name}: Weapon đã được equip bởi WeaponManager, đồng bộ.");
            SyncWithWeaponManager();
            return;
        }

        if (headSlot != null)
            foreach (Transform c in headSlot) Destroy(c.gameObject);
        if (handSlot != null)
            foreach (Transform c in handSlot) Destroy(c.gameObject);

        currentBulletPrefab = null;
        currentBulletSpeed = 0f;
        currentWeapon = null;
        currentHelmet = null;

        if (database == null)
        {
            Debug.LogError($"[BotRandomSkin] {name}: WeaponDatabase không được gán! Vui lòng kiểm tra Inspector.");
            return;
        }

        if (database.helmets != null && database.helmets.Length > 0)
        {
            int h = Random.Range(0, database.helmets.Length);
            currentHelmet = database.helmets[h];
            if (headSlot != null && currentHelmet.helmetPrefab != null)
                Instantiate(currentHelmet.helmetPrefab, headSlot);
        }

        if (database.weapons != null && database.weapons.Length > 0)
        {
            int w = Random.Range(0, database.weapons.Length);
            currentWeapon = database.weapons[w];

            if (currentWeapon != null && currentWeapon.weaponPrefab != null && handSlot != null)
            {
                var newWeapon = Instantiate(currentWeapon.weaponPrefab, handSlot);
                if (newWeapon != null)
                {
                    Destroy(newWeapon.GetComponent<Collider>());
                    Destroy(newWeapon.GetComponent<Rigidbody>());
                }
            }

            currentBulletPrefab = currentWeapon.bulletPrefab;
            currentBulletSpeed = currentWeapon.bulletSpeed > 0 ? currentWeapon.bulletSpeed : 5f;

            if (currentBulletPrefab == null)
                Debug.LogError($"[BotRandomSkin] {name}: bulletPrefab null từ WeaponData {currentWeapon?.weaponName}! Kiểm tra WeaponDatabase.");
            else
                Debug.Log($"[BotRandomSkin] {name}: Đã gán bulletPrefab={currentBulletPrefab.name} từ {currentWeapon?.weaponName}");
        }
        else
        {
            Debug.LogError($"[BotRandomSkin] {name}: Database weapons null hoặc rỗng! Vui lòng kiểm tra WeaponDatabase.");
        }

        Debug.Log($"[BotRandomSkin] {name}: currentBulletPrefab={(currentBulletPrefab != null ? currentBulletPrefab.name : "null")}, currentBulletSpeed={currentBulletSpeed}.");
    }

    private void SyncWithWeaponManager()
    {
        if (weaponManager != null && weaponManager.currentInstance != null)
        {
            WeaponData weaponData = weaponManager.currentInstance.GetComponent<WeaponData>();
            if (weaponData != null)
                currentWeapon = weaponData;

            currentBulletPrefab = currentWeapon.bulletPrefab;
            currentBulletSpeed = currentWeapon.bulletSpeed > 0 ? currentWeapon.bulletSpeed : 5f;

            if (currentBulletPrefab == null)
                Debug.LogError($"[BotRandomSkin] {name}: bulletPrefab null từ WeaponManager! Kiểm tra WeaponData.");

            if (handSlot != null && currentWeapon.weaponPrefab != null)
            {
                var newWeapon = Instantiate(currentWeapon.weaponPrefab, handSlot);
                if (newWeapon != null)
                {
                    Destroy(newWeapon.GetComponent<Collider>());
                    Destroy(newWeapon.GetComponent<Rigidbody>());
                }
            }
        }
    }
}