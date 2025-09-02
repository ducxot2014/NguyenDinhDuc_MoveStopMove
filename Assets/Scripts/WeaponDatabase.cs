using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Game/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    public WeaponData[] weapons;
    public HelmetData[] helmets;
}
