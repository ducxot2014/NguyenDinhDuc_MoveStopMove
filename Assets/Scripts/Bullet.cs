using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 3f;

    private Vector3 direction;
    private float speed = 10f; // Tăng tốc độ để dễ quan sát
    private float spawnTime;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.freezeRotation = true; // Ngăn đạn bị xoay do va chạm
            Debug.Log($"[Bullet] {name}: Rigidbody được khởi tạo với collisionDetectionMode=ContinuousDynamic");
        }
        else
        {
            Debug.LogError($"[Bullet] {name}: Không tìm thấy Rigidbody!");
        }

        Collider bulletCollider = GetComponent<Collider>();
        if (bulletCollider != null)
        {
            bulletCollider.isTrigger = true; // Bật Trigger để dùng OnTriggerEnter
            Debug.Log($"[Bullet] {name}: Collider được khởi tạo với isTrigger=true, Layer={LayerMask.LayerToName(gameObject.layer)}");
        }
        else
        {
            Debug.LogError($"[Bullet] {name}: Không tìm thấy Collider!");
        }

        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
        Debug.Log($"[Bullet] {name}: Đạn được bắn tại vị trí {transform.position}, thời gian sống: {lifetime}");
    }

    public void SetDirection(Vector3 dir, float bulletSpeed)
    {
        direction = dir.normalized;
        speed = bulletSpeed > 0 ? bulletSpeed : speed; // Đảm bảo tốc độ không âm
        Debug.Log($"[Bullet] {name}: Đặt hướng {direction}, tốc độ: {speed}");
    }

    private void FixedUpdate()
    {
        if (rb != null && direction != Vector3.zero)
        {
            Vector3 newPos = rb.position + direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
            Debug.DrawLine(rb.position, newPos, Color.green, 0.1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Log chi tiết về đối tượng va chạm
        string collisionInfo = $"[Bullet] {name}: Va chạm với đối tượng {other.name} (Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}, Hierarchy: {GetHierarchyPath(other.gameObject)}) tại vị trí {transform.position}";
        Debug.Log(collisionInfo);

        // Kiểm tra tag Player hoặc Enemy
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            Debug.Log($"[Bullet] {name}: Tag hợp lệ ({other.tag}), kiểm tra Character component...");

            // Tìm Character trên đối tượng va chạm hoặc trong parent/child
            Character targetCharacter = Cache.GetCharacter(other.gameObject);
            if (targetCharacter == null)
            {
                Debug.Log($"[Bullet] {name}: Cache.GetCharacter trả về null, thử GetComponentInParent...");
                targetCharacter = other.GetComponentInParent<Character>(); // Tìm trong parent
            }
            if (targetCharacter == null)
            {
                Debug.Log($"[Bullet] {name}: GetComponentInParent trả về null, thử GetComponentInChildren...");
                targetCharacter = other.GetComponentInChildren<Character>(); // Tìm trong child
            }

            if (targetCharacter != null && !targetCharacter.IsDead)
            {
                Debug.Log($"[Bullet] {name}: Va chạm với {other.tag} {other.name} (Character: {targetCharacter.name}) tại vị trí {transform.position}");

                // Gọi chết
                targetCharacter.Die();
                Debug.Log($"[Bullet] {name}: Đã tiêu diệt mục tiêu {targetCharacter.name}!");

                // Nếu có AttackRangeVisualBot thì phóng to
                AttackRangeVisualBot visual = targetCharacter.GetComponentInChildren<AttackRangeVisualBot>();
                if (visual != null)
                {
                    visual.EnlargeBy(1f);
                    Debug.Log($"[Bullet] {name}: Đã phóng to AttackRangeVisualBot của {targetCharacter.name}");
                }

                // Hủy đạn sau khi trúng
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[Bullet] {name}: Va chạm với {other.name} nhưng không có Character component hoặc đã chết! (IsDead: {targetCharacter?.IsDead}, Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
            }
        }
        else
        {
            Debug.Log($"[Bullet] {name}: Đạn xuyên qua đối tượng {other.name} do tag không hợp lệ (Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"[Bullet] {name}: Đạn đã bị hủy tại vị trí {transform.position}");
    }

    // Helper method to get hierarchy path for debugging
    private string GetHierarchyPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }
}