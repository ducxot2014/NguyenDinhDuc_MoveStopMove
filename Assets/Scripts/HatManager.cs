using UnityEngine;

public class HatManager : MonoBehaviour
{
    [SerializeField] private Transform hatAttachPoint;

    private GameObject currentInstance;          // instance đang hiển thị (preview hoặc equipped)
    private GameObject lastEquippedHatPrefab;    // prefab đã equip thật sự
    private bool isPreviewing = false;

    // Equip (chọn thật sự)
    public void EquipHat(GameObject hatPrefab)
    {
        if (hatPrefab == null)
        {
            Debug.LogWarning("EquipHat: hatPrefab is null, nothing to equip.");
            return;
        }

        DestroyCurrentInstance();
        lastEquippedHatPrefab = hatPrefab;
        currentInstance = Instantiate(hatPrefab, hatAttachPoint);
        //currentInstance.transform.localPosition = Vector3.zero;
        //currentInstance.transform.localRotation = Quaternion.identity;
        isPreviewing = false;
        Debug.Log("HatManager: Equipped hat.");
    }

    // Preview (tạm, không lưu)
    public void PreviewHat(GameObject hatPrefab)
    {
        DestroyCurrentInstance();

        if (hatPrefab == null)
        {
            Debug.LogWarning("PreviewHat: hatPrefab is null, nothing to preview.");
            return;
        }

        currentInstance = Instantiate(hatPrefab, hatAttachPoint);
        //currentInstance.transform.localPosition = Vector3.zero;
        //currentInstance.transform.localRotation = Quaternion.identity;
        isPreviewing = true;
        Debug.Log("HatManager: Previewing hat.");
    }

    // Clear preview (hủy preview, trả về hat đã equip nếu có)
    public void ClearPreview()
    {
        if (!isPreviewing) return;

        DestroyCurrentInstance();

        // Nếu có hat đã equip thì trả lại
        if (lastEquippedHatPrefab != null)
        {
            currentInstance = Instantiate(lastEquippedHatPrefab, hatAttachPoint);
            //currentInstance.transform.localPosition = Vector3.zero;
            //currentInstance.transform.localRotation = Quaternion.identity;
            isPreviewing = false;
        }
        else
        {
            isPreviewing = false;
            // hiện không có instance nào
        }
        Debug.Log("HatManager: Cleared preview.");
    }

    // Clear tất cả (xóa cả equip lẫn preview)
    public void ClearAll()
    {
        DestroyCurrentInstance();
        lastEquippedHatPrefab = null;
        isPreviewing = false;
        Debug.Log("HatManager: Cleared all hats.");
    }

    // Nếu muốn restore last equipped (dùng khi game load hoặc thoát shop)
    public void ResetToLastEquipped()
    {
        DestroyCurrentInstance();
        if (lastEquippedHatPrefab != null)
        {
            currentInstance = Instantiate(lastEquippedHatPrefab, hatAttachPoint);
            //currentInstance.transform.localPosition = Vector3.zero;
            //currentInstance.transform.localRotation = Quaternion.identity;
            isPreviewing = false;
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
