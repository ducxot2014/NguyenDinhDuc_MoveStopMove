using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRangeVisual : MonoBehaviour, IAttackRange
{
    [Header("General Settings")]
    [SerializeField] private GameObject attackRangeObject;
    [SerializeField] private float defaultScale = 1f;

    [Header("Rotation Settings")]
    [SerializeField] private Transform rotatingObject;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minLookDot = 0.9f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1f;
    private Gun gun;
    private float lastShootTime = 0f;
    private int killCount = 0; // Đếm số địch đã giết
    private readonly int killsRequiredForScaleIncrease = 1; // Số địch cần giết để tăng scale (có thể điều chỉnh)

    private Character character;
    private IMoving playerMoving;
    private PlayerWeaponManager weaponManager;
    private HashSet<Character> enemiesInRange = new HashSet<Character>();

    private void Awake()
    {
        character = Cache.GetCharacter(transform.root.gameObject);
        playerMoving = transform.root.GetComponent<PlayerMoving>() as IMoving;
        weaponManager = transform.root.GetComponentInChildren<PlayerWeaponManager>();

        if (attackRangeObject == null)
            Debug.LogError($"[AttackRangeVisual] {name}: attackRangeObject không được gán!");
        if (character == null)
            Debug.LogWarning($"[AttackRangeVisual] {name}: Không tìm thấy Character!");
        if (playerMoving == null)
            Debug.LogWarning($"[AttackRangeVisual] {name}: Không tìm thấy IMoving!");
        if (weaponManager == null && transform.root.CompareTag("Player"))
            Debug.LogWarning($"[AttackRangeVisual] {name}: Không tìm thấy PlayerWeaponManager!");

        // Khởi tạo scale ban đầu
        SetDefaultScale(defaultScale);
    }

    private void Start()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return new WaitForEndOfFrame();
        UpdateGunReference();
    }

    // Hàm cập nhật tham chiếu gun
    public void OnWeaponChanged()
    {
        UpdateGunReference();
    }

    private void UpdateGunReference()
    {
        if (weaponManager != null && weaponManager.currentInstance != null)
        {
            gun = weaponManager.currentInstance.GetComponent<Gun>();
            if (gun == null)
                Debug.LogError($"[AttackRangeVisual] {name}: Không tìm thấy Gun component trong PlayerWeaponManager.currentInstance ({weaponManager.currentInstance.name})!");
            else
                Debug.Log($"[AttackRangeVisual] {name}: Gán Gun: {gun.name}");
        }
        else if (transform.root.CompareTag("Enemy"))
        {
            gun = character.GetComponentInChildren<Gun>();
            if (gun == null)
                Debug.LogWarning($"[AttackRangeVisual] {name}: Không tìm thấy Gun (Enemy)!");
        }
        else
        {
            Debug.LogWarning($"[AttackRangeVisual] {name}: PlayerWeaponManager hoặc currentInstance là null!");
        }
    }

    private void Update()
    {
        // Không bắn ở đây
        // Chỉ update hướng nếu cần hiển thị visual
        if (enemiesInRange.Count == 0 || rotatingObject == null) return;

        Character target = GetCurrentTarget();
        if (target == null) return;

        Vector3 dir = (target.transform.position - rotatingObject.position).normalized;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            rotatingObject.rotation = Quaternion.Slerp(rotatingObject.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }


    public Character GetCurrentTarget()
    {
        if (enemiesInRange.Count == 0) return null;

        Character closest = null;
        float minDist = float.MaxValue;
        foreach (var enemy in enemiesInRange)
        {
            if (enemy == null || enemy.IsDead) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    public void EnlargeBy(float size)
    {
        defaultScale += size;
        SetDefaultScale(defaultScale);
        Debug.Log($"[AttackRangeVisual] {name}: Enlarged scale to {defaultScale}");
    }

    public void SetDefaultScale(float scale)
    {
        defaultScale = scale;
        if (attackRangeObject != null)
        {
            attackRangeObject.transform.localScale = Vector3.one * defaultScale;
            Debug.Log($"[AttackRangeVisual] {name}: Set scale to {defaultScale}");
        }
        else
        {
            Debug.LogError($"[AttackRangeVisual] {name}: attackRangeObject is null, cannot set scale!");
        }
    }

    public void ResetAttack()
    {
        lastShootTime = 0f;
        foreach (var enemy in enemiesInRange)
        {
            if (enemy != null)
                enemy.onCharacterDead -= RemoveEnemyOnDead;
        }
        enemiesInRange.Clear();
        Debug.Log($"[AttackRangeVisual] {name}: ResetAttack called, kept scale at {defaultScale}");
    }

    public void ShootAtTarget(Character target)
    {
        if (gun != null && gun.CanShoot() && target != null)
        {
            gun.Shoot(target);
            character?.SetState(Character.PlayerState.IsAttack);
            lastShootTime = Time.time;
            Debug.Log($"[AttackRangeVisual] {name}: Bắn vào {target.name}");
        }
        else
        {
            Debug.LogWarning($"[AttackRangeVisual] {name}: Không thể bắn, gun={(gun != null ? gun.name : "null")}, CanShoot={(gun != null ? gun.CanShoot() : false)}, target={(target != null ? target.name : "null")}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Bullet>() != null)
        {
            Debug.Log($"[AttackRangeVisual] {name}: Bỏ qua va chạm với đạn {other.name}");
            return;
        }

        if (!other.CompareTag("Enemy")) return;

        Character targetCharacter = Cache.GetCharacter(other.gameObject);
        if (targetCharacter == null || targetCharacter == character || targetCharacter.IsDead || enemiesInRange.Contains(targetCharacter))
            return;

        enemiesInRange.Add(targetCharacter);
        character.AddTarget(targetCharacter);
        targetCharacter.onCharacterDead += RemoveEnemyOnDead;

        Debug.Log($"[AttackRangeVisual] {name}: Added enemy {targetCharacter.name} to range.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Character target = Cache.GetCharacter(other.gameObject);
        if (target != null)
            RemoveEnemy(target);
    }

    private void RemoveEnemy(Character target)
    {
        if (enemiesInRange.Remove(target))
        {
            target.onCharacterDead -= RemoveEnemyOnDead;
            character.RemoveTarget(target);
            Debug.Log($"[AttackRangeVisual] {name}: Removed enemy {target.name} from range.");
        }
    }

    private void RemoveEnemyOnDead(Character target)
    {
        RemoveEnemy(target);
        // Tăng số địch đã giết
        killCount++;
        Debug.Log($"[AttackRangeVisual] {name}: Enemy {target.name} died, killCount={killCount}");

        // Kiểm tra nếu đủ số địch để tăng scale
        if (killCount >= killsRequiredForScaleIncrease)
        {
            defaultScale += 2f; // Tăng scale thêm 2
            SetDefaultScale(defaultScale);
            killCount = 0; // Reset killCount sau khi tăng scale
            Debug.Log($"[AttackRangeVisual] {name}: Increased scale to {defaultScale} after killing {killsRequiredForScaleIncrease} enemies");
        }
    }

    private void OnDestroy()
    {
        ResetAttack();
    }
}