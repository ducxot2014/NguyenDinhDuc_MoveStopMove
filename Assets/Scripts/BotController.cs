using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    public Character currentTarget; // SỬA: Đổi từ GameObject sang Character
    public float detectionRadius = 5f;
    public bool isDead = false;
    public EnemySpawner enemySpawner;
    public BotGun gun;
    public Transform SpawnPoint { get; set; }
    public bool canMove = false;
    [SerializeField] public BotRandomSkin skin; // Có thể gán trong Inspector, nhưng không bắt buộc

    private Animator animator;
    public AttackRangeVisualBot attackRangeVisualBot;
    private IMoving botMovement;
    private Character character;

    private void Awake()
    {
        animator = Cache.GetComponent<Animator>(gameObject);
        if (animator == null)
            Debug.LogWarning($"[BotController] {name}: Animator không được gán!");

        attackRangeVisualBot = Cache.GetAttackRangeVisualBot(gameObject);
        if (attackRangeVisualBot == null)
            Debug.LogWarning($"[BotController] {name}: AttackRangeVisualBot không được gán!");

        botMovement = Cache.GetBotMovement(gameObject);
        if (botMovement == null)
            Debug.LogWarning($"[BotController] {name}: BotMovement không được gán!");

        character = Cache.GetCharacter(gameObject);
        if (character == null)
            Debug.LogWarning($"[BotController] {name}: Character không được gán!");

        gun = Cache.GetComponent<BotGun>(gameObject);
        if (gun == null)
            Debug.LogWarning($"[BotController] {name}: BotGun không được gán!");

        // Tìm BotRandomSkin trong hierarchy nếu không được gán trong Inspector
        if (skin == null)
        {
            skin = GetComponentInChildren<BotRandomSkin>();
            if (skin == null)
            {
                skin = GetComponentInParent<BotRandomSkin>();
                if (skin == null)
                    Debug.LogError($"[BotController] {name}: Không tìm thấy BotRandomSkin trong hierarchy! Kiểm tra prefab.");
                else
                    Debug.Log($"[BotController] {name}: Tìm thấy BotRandomSkin trong parent: {skin.name}");
            }
            else
            {
                Debug.Log($"[BotController] {name}: Tìm thấy BotRandomSkin trong children: {skin.name}");
            }
        }
        else
        {
            Debug.Log($"[BotController] {name}: BotRandomSkin được gán trong Inspector: {skin.name}");
        }
    }

    private void Start()
    {
        enemySpawner = Cache.GetEnemySpawner(FindObjectOfType<EnemySpawner>()?.gameObject);
        if (enemySpawner == null)
            Debug.LogWarning($"[BotController] {name}: Không tìm thấy EnemySpawner!");
        else
            Debug.Log($"[BotController] {name}: EnemySpawner được gán thành công.");

        if (gun != null && skin != null)
        {
            gun.ResetGun(); // Đồng bộ Gun với Skin khi khởi tạo
            Debug.Log($"[BotController] {name}: Đồng bộ BotGun với BotRandomSkin.");
        }
        else if (gun == null)
        {
            Debug.LogError($"[BotController] {name}: BotGun null, không thể đồng bộ!");
        }
        else if (skin == null)
        {
            Debug.LogError($"[BotController] {name}: BotRandomSkin null, không thể đồng bộ!");
        }
    }

    public void OnEnable()
    {
        BotEvent.OnBotDied += OnTargetDied;
    }

    void OnDisable()
    {
        BotEvent.OnBotDied -= OnTargetDied;
    }

    public void NotifyDeath()
    {
        if (enemySpawner != null)
        {
            enemySpawner.OnBotDied(this.gameObject);
            Debug.Log($"[BotController] {name}: Đã thông báo cho EnemySpawner về cái chết.");
        }
    }

    void Update()
    {
        if (isDead) return;
        if (character == null || attackRangeVisualBot == null) return;

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            bool isValidTarget = currentTarget != null && !currentTarget.IsDead &&
                                 (currentTarget.CompareTag("Enemy") || currentTarget.CompareTag("Player")) &&
                                 dist <= detectionRadius;

            if (!isValidTarget)
            {
                Debug.Log($"[BotController] {name}: Target {currentTarget.name} không hợp lệ hoặc ra khỏi tầm, reset target");
                currentTarget = null;
                attackRangeVisualBot?.ResetAttack();
                canMove = true;
                botMovement?.ResumeMoving();
            }
            else
            {
                canMove = false;
                botMovement?.StopMoving();

                Vector3 dir = (currentTarget.transform.position - transform.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(dir.normalized);
                    transform.rotation = lookRotation;
                    //Debug.Log($"[BotController] {name}: Xoay ngay lập tức về hướng {currentTarget.name}");
                }

                if (attackRangeVisualBot.isAttacking && gun != null)
                {
                    gun.Shoot();
                    Debug.Log($"[BotController] {name}: Bắn vào {currentTarget.name} trong tầm tấn công.");
                    character.SetState(Character.PlayerState.IsAttack);

                }
            }
        }
        else
        {
            canMove = true;
            botMovement?.ResumeMoving();
        }

        if (canMove)
        {
            if (character != null)
                character.SetState(Character.PlayerState.IsRun);
        }
        else
        {
            if (attackRangeVisualBot != null && attackRangeVisualBot.isAttacking)
            {
                if (character != null)
                    character.SetState(Character.PlayerState.IsAttack);
            }
            else
            {
                if (character != null)
                    character.SetState(Character.PlayerState.IsIdle);
            }
        }

        if (currentTarget == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
            List<Collider> validHits = new List<Collider>();
            foreach (var hit in hits)
            {
                if ((hit.CompareTag("Enemy") || hit.CompareTag("Player")) && hit.gameObject != this.gameObject)
                {
                    Character targetChar = hit.GetComponent<Character>();
                    if (targetChar != null && !targetChar.IsDead)
                    {
                        validHits.Add(hit);
                    }
                }
            }

            if (validHits.Count > 0)
            {
                validHits.Sort((a, b) => Vector3.Distance(transform.position, a.transform.position).CompareTo(Vector3.Distance(transform.position, b.transform.position)));
                currentTarget = validHits[0].GetComponent<Character>();
                Debug.Log($"[BotController] {name}: Thấy mục tiêu gần nhất: {currentTarget.name}");
            }
        }

        if (currentTarget != null)
        {
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red);
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        Debug.Log($"[BotController] {name}: canMove = {canMove}");
        if (botMovement != null)
        {
            if (value)
                botMovement.ResumeMoving();
            else
                botMovement.StopMoving();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"[BotController] {name}: Died!");

        // Chơi animation chết
        if (character != null)
        {
            character.SetState(Character.PlayerState.IsDead); // Giả sử bạn có trạng thái IsDead
        }

        // Dừng bot
        SetCanMove(false);

        // Gọi sự kiện bot chết
        BotEvent.RaiseBotDied(gameObject);

        // Start Coroutine để chờ animation
        StartCoroutine(HandleDeathCoroutine());
    }

    private IEnumerator HandleDeathCoroutine()
    {
        // Chờ 1 giây (hoặc thời gian animation)
        yield return new WaitForSeconds(1f);

        // Thông báo EnemySpawner
        if (enemySpawner == null)
        {
            enemySpawner = Cache.GetEnemySpawner(FindObjectOfType<EnemySpawner>()?.gameObject);
            if (enemySpawner == null)
            {
                Debug.LogWarning($"[BotController] {name}: Không tìm thấy EnemySpawner!");
            }
        }

        if (enemySpawner != null)
        {
            enemySpawner.OnBotDied(gameObject);
        }

        // Trả bot về pool
        ObjectPool.Instance.ReturnObject(gameObject);

        Debug.Log($"[BotController] {name}: Bot đã trả về pool sau animation chết.");
    }


    void OnTargetDied(GameObject dead)
    {
        if (currentTarget != null && currentTarget.gameObject == dead)
        {
            Debug.Log($"[BotController] {name}: Target {dead.name} đã chết, reset target");
            currentTarget = null;
            attackRangeVisualBot?.ResetAttack();
            canMove = true;
            botMovement?.ResumeMoving();
        }
    }

    public void Reset()
    {
        isDead = false;
        currentTarget = null;
        SpawnPoint = null;
        canMove = false;

        if (gun != null)
            gun.ResetGun();

        if (attackRangeVisualBot != null)
        {
            attackRangeVisualBot.ResetAttack();
            attackRangeVisualBot.SetDefaultScale(5f);
        }

        if (enemySpawner == null)
        {
            enemySpawner = Cache.GetEnemySpawner(FindObjectOfType<EnemySpawner>()?.gameObject);
            if (enemySpawner == null)
            {
                Debug.LogWarning($"[BotController] {name}: Không tìm thấy EnemySpawner khi reset!");
            }
        }

        if (character != null)
            character.ResetCharacter();
        else
            Debug.LogWarning($"[BotController] {name}: Character is null in Reset!");

        if (botMovement != null)
            botMovement.StopMoving();
        else
            Debug.LogWarning($"[BotController] {name}: BotMovement is null in Reset!");

        Debug.Log($"[BotController] {name}: Đã được reset.");
    }
}