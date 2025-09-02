using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float defaultScale = 1f; // Tỉ lệ mặc định

    // Phóng to player theo hệ số tỉ lệ
    public void EnlargePlayer(float scaleMultiplier)
    {
        transform.localScale *= scaleMultiplier;
        Debug.Log($"[Player] Enlarged {name}. New scale: {transform.localScale}");
    }

    // Phóng to lên 1 đơn vị mỗi lần
    public void EnlargeByOneUnit()
    {
        transform.localScale += Vector3.one;
        Debug.Log($"[Player] Increased scale of {name} by 1 unit. New scale: {transform.localScale}");
    }

    // Đặt lại về tỉ lệ mặc định
    public void ResetScale()
    {
        transform.localScale = Vector3.one * defaultScale;
        Debug.Log($"[Player] Reset scale of {name} to default: {defaultScale}");
    }
}