using UnityEngine;

[CreateAssetMenu(fileName = "HelmetData", menuName = "Game/Helmet Data")]
public class HelmetData : ScriptableObject
{
    public int helmetId;
    public string helmetName;
    public GameObject helmetPrefab;
}
