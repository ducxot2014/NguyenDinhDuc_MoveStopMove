using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/Weapon Data", order = 1)]
public class WeaponData : ScriptableObject 
{
    [Header("Weapon Info")]
    public string weaponId;               // ID vũ khí (dùng cho lưu trữ hoặc nhận diện)
    public string weaponName;             // Tên hiển thị

    [Header("Prefabs")]
    public GameObject weaponPrefab;       // Prefab của vũ khí (dùng để gắn lên nhân vật)
    public GameObject bulletPrefab;       // Prefab đạn khi bắn

    public float bulletSpeed = 15f; // Tốc độ đạn


}
