using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, 7);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private AttackRangeVisual attackRangeVisual;
    [SerializeField] private Vector3 onPlayoffset = new Vector3(0, 5, 7); // Offset khi bắt đầu chơi

    private Vector3 defaultOffset; // Lưu offset gốc

    void Start()
    {
        // Always store the initial offset as defaultOffset at Start
        defaultOffset = offset;
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
            Debug.Log("CameraFollow initialized with target: " + target.name);
        }
    }


    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }

    public void ZoomIn(float zoomAmount = 2f)
    {
        offset.z += zoomAmount;    // Tiến gần
        offset.y -= zoomAmount * 0.5f;
    }

    public void ZoomOut(float zoomAmount = 2f)
    {
        offset.z -= zoomAmount;    // Lùi xa
        offset.y += zoomAmount * 0.5f;
    }

    public void ResetCamera()
    {
        // Restore offset to the original value
        offset = defaultOffset; // Trả camera về vị trí ban đầu
        Debug.Log("Camera position reset to default.");
    }

    public void OnPlayed()
    {
   
        // Khi bắt đầu chơi, thay đổi offset để camera nhìn từ trên cao xuống
        offset = onPlayoffset;
        Debug.Log("Camera offset changed for gameplay.");
        
        // Nếu có AttackRangeVisual, cập nhật vị trí của nó
        if (attackRangeVisual != null)
        {
           
            Debug.Log("AttackRangeVisual position updated for gameplay.");
        }

    }
}
