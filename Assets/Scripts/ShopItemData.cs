using UnityEngine;

[System.Serializable]
public class ShopItemData
{
    public string itemId;
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    public GameObject visualPrefab; // GameObject rỗng có script Gun (visual trên tay)
    public GameObject bulletPrefab; // Vũ khí ném (ví dụ: Axe_0, có script Bullet)
    public int price;
    public float bulletSpeed; // Tốc độ ném
}

public enum ItemType
{
    Hat,
    Weapon
}