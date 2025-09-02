using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("Prefabs & Panels")]
    public GameObject itemPrefab;
    public Transform hatContentPanel;
    public Transform weaponContentPanel;

    [Header("All items (both types)")]
    public List<ShopItemData> items;

    [Header("Buy & Select UI")]
    public Button buyButton;
    public Button selectButton;
    public TextMeshProUGUI buyButtonText;
    public TextMeshProUGUI selectButtonText;

    [Header("Managers")]
    public HatManager hatManager;
    public PlayerWeaponManager weaponManager;

    private ShopItemData selectedItemData;
    private string lastSelectedHatId;
    private string lastSelectedWeaponId;
    private bool hasLoadedItems = false;

    void Start()
    {
        // Validate managers
        if (hatManager == null)
        {
            Debug.LogWarning($"[ShopManager] {name}: HatManager chưa được gán trong Inspector!");
        }
        if (weaponManager == null)
        {
            Debug.LogWarning($"[ShopManager] {name}: PlayerWeaponManager chưa được gán trong Inspector!");
        }
        else if (weaponManager.GetComponent<WeaponManager>() != null)
        {
            Debug.LogError($"[ShopManager] {name}: PlayerWeaponManager được gán sai thành WeaponManager trên {weaponManager.gameObject.name}! Vui lòng gán đúng PlayerWeaponManager.");
        }

        // Kiểm tra items
        if (items == null || items.Count == 0)
        {
            Debug.LogError($"[ShopManager] {name}: Danh sách items rỗng hoặc null!");
        }
        else
        {
            foreach (var item in items)
            {
                if (item.itemType == ItemType.Weapon && (item.visualPrefab == null || item.bulletPrefab == null || item.bulletSpeed <= 0))
                {
                    Debug.LogError($"[ShopManager] {name}: Weapon item {item.itemName} (ID: {item.itemId}) có visualPrefab={item.visualPrefab?.name}, bulletPrefab={item.bulletPrefab?.name}, bulletSpeed={item.bulletSpeed}. Kiểm tra Inspector!");
                }
                else if (item.itemType == ItemType.Weapon && !item.visualPrefab.GetComponent<Gun>())
                {
                    Debug.LogError($"[ShopManager] {name}: Weapon item {item.itemName} (ID: {item.itemId}) visualPrefab thiếu component Gun!");
                }
            }
        }

        lastSelectedHatId = PlayerPrefs.GetString("SelectedHat", "");
        lastSelectedWeaponId = PlayerPrefs.GetString("SelectedWeapon", "");

        PopulateShop();

        if (buyButton != null) buyButton.onClick.AddListener(OnBuyButtonClicked);
        else Debug.LogWarning($"[ShopManager] {name}: BuyButton chưa được gán trong Inspector!");
        if (selectButton != null) selectButton.onClick.AddListener(OnSelectButtonClicked);
        else Debug.LogWarning($"[ShopManager] {name}: SelectButton chưa được gán trong Inspector!");

        if (!hasLoadedItems)
        {
            LoadSelectedItems();
            hasLoadedItems = true;
        }

        Debug.Log($"[ShopManager] {name}: Initialized. Items count: {items.Count}, SelectedHat: {lastSelectedHatId}, SelectedWeapon: {lastSelectedWeaponId}");
    }

    void PopulateShop()
    {
        ClearPanel(hatContentPanel);
        ClearPanel(weaponContentPanel);

        if (items == null || items.Count == 0)
        {
            Debug.LogWarning($"[ShopManager] {name}: Danh sách items rỗng hoặc null!");
            return;
        }

        foreach (var it in items)
        {
            var item = it;
            Transform parent = (item.itemType == ItemType.Hat) ? hatContentPanel : weaponContentPanel;
            if (parent == null)
            {
                Debug.LogWarning($"[ShopManager] {name}: Parent panel is null for item type {item.itemType}, item: {item.itemName}");
                continue;
            }

            GameObject obj = Instantiate(itemPrefab, parent);
            var txt = obj.transform.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
            var img = obj.transform.Find("Image")?.GetComponent<Image>();

            if (txt != null) txt.text = item.itemName;
            else Debug.LogWarning($"[ShopManager] {name}: Text (TMP) không tìm thấy trên item {item.itemName}");
            if (img != null) img.sprite = item.icon;
            else Debug.LogWarning($"[ShopManager] {name}: Image không tìm thấy trên item {item.itemName}");

            Button btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnItemClicked(item));
            }
            else
            {
                Debug.LogWarning($"[ShopManager] {name}: Button không tìm thấy trên item {item.itemName}");
            }
        }
    }

    void ClearPanel(Transform panel)
    {
        if (panel == null)
        {
            Debug.LogWarning($"[ShopManager] {name}: Panel is null!");
            return;
        }
        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            Destroy(panel.GetChild(i).gameObject);
        }
    }

    void OnItemClicked(ShopItemData item)
    {
        selectedItemData = item;
        PreviewItem(item);
        UpdateBuySelectUI(item);
        Debug.Log($"[ShopManager] {name}: Item clicked: {item.itemName} (ID: {item.itemId}, Type: {item.itemType})");
    }

   public void PreviewItem(ShopItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning($"[ShopManager] {name}: PreviewItem: Item is null!");
            return;
        }

        if (item.itemType == ItemType.Hat)
        {
            if (hatManager != null)
            {
                hatManager.PreviewHat(item.visualPrefab);
                Debug.Log($"[ShopManager] {name}: Previewing hat: {item.itemName} (visualPrefab: {item.visualPrefab?.name})");
            }
            else
            {
                Debug.LogWarning($"[ShopManager] {name}: HatManager is null, cannot preview hat {item.itemName}");
            }
        }
        else if (item.itemType == ItemType.Weapon)
        {
            if (weaponManager != null)
            {
                weaponManager.PreviewWeapon(item.visualPrefab);
                Debug.Log($"[ShopManager] {name}: Previewing weapon: {item.itemName} (visualPrefab: {item.visualPrefab?.name})");
            }
            else
            {
                Debug.LogWarning($"[ShopManager] {name}: PlayerWeaponManager is null, cannot preview weapon {item.itemName}");
            }
        }
    }

    void UpdateBuySelectUI(ShopItemData item)
    {
        if (item == null)
        {
            buyButtonText.text = "";
            selectButtonText.text = "";
            if (buyButton != null) buyButton.interactable = false;
            if (selectButton != null) selectButton.interactable = false;
            Debug.LogWarning($"[ShopManager] {name}: UpdateBuySelectUI: Item is null!");
            return;
        }

        bool isBought = PlayerPrefs.GetInt("Bought_" + item.itemId, 0) == 1;
        bool isSelected = (item.itemType == ItemType.Hat)
            ? PlayerPrefs.GetString("SelectedHat", "") == item.itemId
            : PlayerPrefs.GetString("SelectedWeapon", "") == item.itemId;

        if (buyButtonText != null)
        {
            if (isBought)
            {
                buyButtonText.text = "Owned";
                if (buyButton != null) buyButton.interactable = false;
            }
            else
            {
                buyButtonText.text = item.price.ToString();
                if (buyButton != null)
                    buyButton.interactable = GameManager.Instance != null && GameManager.Instance.HasEnoughCoin(item.price);
            }
        }
        else
        {
            Debug.LogWarning($"[ShopManager] {name}: BuyButtonText is null!");
        }

        if (selectButtonText != null)
        {
            if (isBought)
            {
                if (isSelected)
                {
                    selectButtonText.text = "Selected";
                    if (selectButton != null) selectButton.interactable = false;
                }
                else
                {
                    selectButtonText.text = "Select";
                    if (selectButton != null) selectButton.interactable = true;
                }
            }
            else
            {
                selectButtonText.text = "Not Owned";
                if (selectButton != null) selectButton.interactable = false;
            }
        }
        else
        {
            Debug.LogWarning($"[ShopManager] {name}: SelectButtonText is null!");
        }

        Debug.Log($"[ShopManager] {name}: Updated UI for item {item.itemName}: isBought={isBought}, isSelected={isSelected}");
    }

    void OnBuyButtonClicked()
    {
        if (selectedItemData == null)
        {
            Debug.LogWarning($"[ShopManager] {name}: OnBuyButtonClicked: No item selected!");
            return;
        }

        bool isAlreadyBought = PlayerPrefs.GetInt("Bought_" + selectedItemData.itemId, 0) == 1;
        if (isAlreadyBought)
        {
            Debug.Log($"[ShopManager] {name}: Item {selectedItemData.itemName} already bought!");
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.SpendCoin(selectedItemData.price))
        {
            PlayerPrefs.SetInt("Bought_" + selectedItemData.itemId, 1);
            PlayerPrefs.Save();
            UpdateBuySelectUI(selectedItemData);
            PreviewItem(selectedItemData);
            Debug.Log($"[ShopManager] {name}: Bought item: {selectedItemData.itemId} for {selectedItemData.price} coins");
        }
        else
        {
            Debug.LogWarning($"[ShopManager] {name}: Not enough coins to buy {selectedItemData.itemName} (Price: {selectedItemData.price})");
        }
    }

    void OnSelectButtonClicked()
    {
        if (selectedItemData == null)
        {
            Debug.LogWarning($"[ShopManager] {name}: OnSelectButtonClicked: No item selected!");
            return;
        }

        bool isBought = PlayerPrefs.GetInt("Bought_" + selectedItemData.itemId, 0) == 1;
        if (!isBought)
        {
            Debug.LogWarning($"[ShopManager] {name}: Cannot select item {selectedItemData.itemName}, not bought!");
            return;
        }

        if (selectedItemData.itemType == ItemType.Hat)
        {
            if (hatManager != null)
            {
                hatManager.EquipHat(selectedItemData.visualPrefab);
                PlayerPrefs.SetString("SelectedHat", selectedItemData.itemId);
                lastSelectedHatId = selectedItemData.itemId;
                Debug.Log($"[ShopManager] {name}: Selected hat: {selectedItemData.itemName} (ID: {selectedItemData.itemId})");
            }
            else
            {
                Debug.LogWarning($"[ShopManager] {name}: HatManager is null, cannot select hat {selectedItemData.itemName}");
            }
        }
        else if (selectedItemData.itemType == ItemType.Weapon)
        {
            if (weaponManager != null)
            {
                weaponManager.EquipWeapon(selectedItemData.visualPrefab, selectedItemData.bulletPrefab, selectedItemData.bulletSpeed);
                PlayerPrefs.SetString("SelectedWeapon", selectedItemData.itemId);
                lastSelectedWeaponId = selectedItemData.itemId;
                Debug.Log($"[ShopManager] {name}: Selected weapon: {selectedItemData.itemName} (ID: {selectedItemData.itemId}, visualPrefab: {selectedItemData.visualPrefab?.name}, bulletPrefab: {selectedItemData.bulletPrefab?.name}, bulletSpeed: {selectedItemData.bulletSpeed})");
            }
            else
            {
                Debug.LogWarning($"[ShopManager] {name}: PlayerWeaponManager is null, cannot select weapon {selectedItemData.itemName}");
            }
        }

        PlayerPrefs.Save();
        UpdateBuySelectUI(selectedItemData);
    }

    public void LoadSelectedItems()
    {
        if (hasLoadedItems)
        {
            Debug.Log($"[ShopManager] {name}: LoadSelectedItems đã được gọi trước đó, bỏ qua.");
            return;
        }

        string selectedHat = PlayerPrefs.GetString("SelectedHat", "");
        if (!string.IsNullOrEmpty(selectedHat))
        {
            var hatData = items.Find(x => x.itemId == selectedHat && x.itemType == ItemType.Hat);
            if (hatData != null)
            {
                hatManager?.EquipHat(hatData.visualPrefab);
                Debug.Log($"[ShopManager] {name}: Loaded selected hat: {selectedHat} (visualPrefab: {hatData.visualPrefab?.name})");
            }
            else
            {
                Debug.LogWarning($"[ShopManager] {name}: Không tìm thấy hat với ID: {selectedHat}");
            }
        }

        string selectedWeapon = PlayerPrefs.GetString("SelectedWeapon", "");
        if (!string.IsNullOrEmpty(selectedWeapon))
        {
            var weaponData = items.Find(x => x.itemId == selectedWeapon && x.itemType == ItemType.Weapon);
            if (weaponData != null)
            {
                if (weaponManager != null)
                {
                    weaponManager.EquipWeapon(weaponData.visualPrefab, weaponData.bulletPrefab, weaponData.bulletSpeed);
                    Debug.Log($"[ShopManager] {name}: Loaded selected weapon: {selectedWeapon} (visualPrefab: {weaponData.visualPrefab?.name}, bulletPrefab: {weaponData.bulletPrefab?.name}, bulletSpeed: {weaponData.bulletSpeed})");
                }
                else
                {
                    Debug.LogWarning($"[ShopManager] {name}: PlayerWeaponManager is null, cannot load weapon {selectedWeapon}");
                }
            }
            else
            {
                Debug.LogWarning($"[ShopManager] {name}: Không tìm thấy weapon với ID: {selectedWeapon}");
            }
        }

        hasLoadedItems = true;
    }

    public void ResetToLastSelected()
    {
        if (!string.IsNullOrEmpty(lastSelectedHatId))
        {
            var hatData = items.Find(x => x.itemId == lastSelectedHatId && x.itemType == ItemType.Hat);
            if (hatData != null)
            {
                hatManager?.EquipHat(hatData.visualPrefab);
                Debug.Log($"[ShopManager] {name}: Reset to last selected hat: {lastSelectedHatId} (visualPrefab: {hatData.visualPrefab?.name})");
            }
            else
            {
                hatManager?.ClearAll();
                Debug.Log($"[ShopManager] {name}: No hat data found for ID {lastSelectedHatId}, cleared hat");
            }
        }
        else
        {
            hatManager?.ClearAll();
            Debug.Log($"[ShopManager] {name}: No last selected hat, cleared hat");
        }

        if (!string.IsNullOrEmpty(lastSelectedWeaponId))
        {
            var weaponData = items.Find(x => x.itemId == lastSelectedWeaponId && x.itemType == ItemType.Weapon);
            if (weaponData != null)
            {
                if (weaponManager != null)
                {
                    weaponManager.EquipWeapon(weaponData.visualPrefab, weaponData.bulletPrefab, weaponData.bulletSpeed);
                    Debug.Log($"[ShopManager] {name}: Reset to last selected weapon: {lastSelectedWeaponId} (visualPrefab: {weaponData.visualPrefab?.name}, bulletPrefab: {weaponData.bulletPrefab?.name}, bulletSpeed: {weaponData.bulletSpeed})");
                }
                else
                {
                    Debug.LogWarning($"[ShopManager] {name}: PlayerWeaponManager is null, cannot reset weapon {lastSelectedWeaponId}");
                }
            }
            else
            {
                weaponManager?.ClearAll();
                Debug.Log($"[ShopManager] {name}: No weapon data found for ID {lastSelectedWeaponId}, cleared weapon");
            }
        }
        else
        {
            weaponManager?.ClearAll();
            Debug.Log($"[ShopManager] {name}: No last selected weapon, cleared weapon");
        }
    }
    public void PreviewItemById(string itemId)
    {
        var item = items.Find(x => x.itemId == itemId);
        if (item != null)
        {
            PreviewItem(item);
            UpdateBuySelectUI(item);
            selectedItemData = item;
            Debug.Log($"[ShopManager] Auto preview item: {item.itemName} (ID: {item.itemId})");
        }
        else
        {
            Debug.LogWarning($"[ShopManager] Không tìm thấy item với ID: {itemId}");
        }
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        hatManager?.ClearAll();
        weaponManager?.ClearAll();
        hasLoadedItems = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log($"[ShopManager] {name}: Game reset: all PlayerPrefs deleted and scene reloaded.");
    }
}