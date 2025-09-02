using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomizer : MonoBehaviour
{
    [Header("Character Customization")]
    [SerializeField] private Transform Weapon;
    [SerializeField] private Transform Hat;

    [Header("Character Skin")]
    [SerializeField] private SkinnedMeshRenderer bodyRendered;

    private GameObject currentWeapon;
    private GameObject currentHat;

    public void SetWeapon(GameObject weaponPrefab)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }
        if (weaponPrefab != null)
        {
            currentWeapon = Instantiate(weaponPrefab, Weapon);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;

        }
    }
    public void SetHat(GameObject hatPrefab)
    {
        if (currentHat != null)
        {
            Destroy(currentHat);
        }
        if (hatPrefab != null)
        {
            currentHat = Instantiate(hatPrefab, Hat);
            currentHat.transform.localPosition = Vector3.zero;
            currentHat.transform.localRotation = Quaternion.identity;
        }
    }
   public void SetBodySkin(Material newMaterial)
    {
if (bodyRendered != null && newMaterial != null)
        {
            Material[] materials = bodyRendered.materials;
            materials[0] = newMaterial; // Assuming the first material is the one to change
            bodyRendered.materials = materials;
        }
    }

}
