using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCustomizerUI : MonoBehaviour
{
    [Header("References")]
    public CharacterCustomizer characterCustomizer;

    [Header("Options")]
    public GameObject[] hatPrefabs;
    public GameObject[] weaponPrefabs;
    public Material[] skinMaterials;

    [Header("UI Dropdowns")]
    public TMP_Dropdown hatDropdown;
    public TMP_Dropdown weaponDropdown;
    public TMP_Dropdown skinDropdown;

    void Start()
    {
        // Gán sự kiện dropdown
        hatDropdown.onValueChanged.AddListener(OnHatChanged);
        weaponDropdown.onValueChanged.AddListener(OnWeaponChanged);
        skinDropdown.onValueChanged.AddListener(OnSkinChanged);

        // Khởi tạo chọn mặc định
        OnHatChanged(hatDropdown.value);
        OnWeaponChanged(weaponDropdown.value);
        OnSkinChanged(skinDropdown.value);
    }

    public void OnHatChanged(int index)
    {
        if (index >= 0 && index < hatPrefabs.Length)
        {
            characterCustomizer.SetHat(hatPrefabs[index]);
        }
    }

    public void OnWeaponChanged(int index)
    {
        if (index >= 0 && index < weaponPrefabs.Length)
        {
            characterCustomizer.SetWeapon(weaponPrefabs[index]);
        }
    }

    public void OnSkinChanged(int index)
    {
        if (index >= 0 && index < skinMaterials.Length)
        {
            characterCustomizer.SetBodySkin(skinMaterials[index]);
        }
    }
}
