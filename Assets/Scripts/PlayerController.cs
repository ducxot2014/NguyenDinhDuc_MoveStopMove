using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float rotationSpeed = 5f;

    private AttackRangeVisual attackRangeVisual;
    private Character character;
    private PlayerMoving playerMoving;
    private PlayerWeaponManager weaponManager;
    private Gun gun;
    private bool isDead = false;
    private float lastShootTime = 0f;

    private void Awake()
    {
        playerMoving = Cache.GetComponent<PlayerMoving>(gameObject);
        attackRangeVisual = Cache.GetComponent<AttackRangeVisual>(gameObject);
        character = Cache.GetCharacter(gameObject);
        weaponManager = GetComponentInChildren<PlayerWeaponManager>();

        if (playerMoving == null)
            Debug.LogWarning($"[PlayerController] {name}: PlayerMoving not found!");
        if (attackRangeVisual == null)
            Debug.LogWarning($"[PlayerController] {name}: AttackRangeVisual not found!");
        if (character == null)
            Debug.LogWarning($"[PlayerController] {name}: Character not found!");
        if (weaponManager == null)
            Debug.LogWarning($"[PlayerController] {name}: PlayerWeaponManager not found!");
    }

    private void OnEnable()
    {
        if (playerMoving != null)
            playerMoving.OnMovementStateChanged += HandleMovementStateChanged;
    }

    private void OnDisable()
    {
        if (playerMoving != null)
            playerMoving.OnMovementStateChanged -= HandleMovementStateChanged;
    }

    private void Start()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return new WaitForEndOfFrame(); // Chờ PlayerWeaponManager load

        if (weaponManager != null && weaponManager.currentInstance != null)
        {
            gun = weaponManager.currentInstance.GetComponent<Gun>();
            if (gun != null)
                Debug.Log($"[PlayerController] {name}: Gán Gun từ PlayerWeaponManager.currentInstance: {weaponManager.currentInstance.name}");
        }
    }

    private void Update()
    {
        if (isDead || character == null) return;

        HandleRotationAndShooting();
    }

    private void HandleMovementStateChanged(bool isMoving)
    {
        if (isDead || character == null) return;

        if (isMoving)
        {
            if (!character.IsInState(Character.PlayerState.IsAttack))
                character.SetState(Character.PlayerState.IsRun);
        }
        else
        {
            if (!character.IsInState(Character.PlayerState.IsAttack))
                character.SetState(Character.PlayerState.IsIdle);
        }
    }

    private void HandleRotationAndShooting()
    {
        if (attackRangeVisual == null || gun == null || isDead) return;

        Character target = attackRangeVisual.GetCurrentTarget();

        // Kiểm tra di chuyển
        if (playerMoving != null && !playerMoving.IsStandingStill())
        {
            if (!character.IsInState(Character.PlayerState.IsAttack))
                character.SetState(Character.PlayerState.IsRun);
            return;
        }

        if (target == null)
        {
            if (!character.IsInState(Character.PlayerState.IsAttack))
                character.SetState(Character.PlayerState.IsIdle);
            return;
        }

        // Quay về target
        Vector3 dir = (target.transform.position - transform.position).normalized;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        // Bắn
        bool readyToShoot = Time.time >= lastShootTime + attackCooldown && gun.CanShoot();
        bool lookingAtTarget = Vector3.Dot(transform.forward.normalized, dir) > 0.9f;
        if (readyToShoot && lookingAtTarget)
        {
            gun.Shoot(target);
            lastShootTime = Time.time;
            character.SetState(Character.PlayerState.IsAttack);
        }
    }


    public void Die()
    {
        if (isDead) return;
        isDead = true;
        character.SetState(Character.PlayerState.IsDead);
        Debug.Log($"[PlayerController] {name}: Died!");

    }
}