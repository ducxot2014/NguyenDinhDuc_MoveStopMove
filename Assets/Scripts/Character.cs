using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Action<Character> onCharacterDead;
    public static event Action OnPlayerDied;

    private IAttackRange attackRangeVisual;
    private List<Character> targets = new();
    private bool isDead = false;
    private Component gun; // Đổi thành Component để hỗ trợ cả Gun và BotGun
    private PlayerWeaponManager weaponManager;
    private SoundManager soundManager;

    public Component Gun => gun; // Trả về Gun hoặc BotGun

    public enum PlayerState
    {
        IsIdle,
        IsRun,
        IsDead,
        IsAttack,
        IsDance,
        IsWin,
        IsUlti,
    }

    public PlayerState CurrentState { get; private set; } = PlayerState.IsIdle;
    public Animator animator;

    [SerializeField] private float deathDelay = 1f;

    public bool IsDead
    {
        get => isDead;
        set
        {
            if (isDead == value) return;
            isDead = value;
            if (isDead)
            {
                SetState(PlayerState.IsDead);
                StartCoroutine(HandleDeath());
                onCharacterDead?.Invoke(this);
            }
        }
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(deathDelay);
        if (gameObject.CompareTag("Enemy"))
        {
            ObjectPool.Instance.ReturnObject(gameObject);
            Debug.Log($"[Character] {name} (Enemy) đã chết và trả về pool!");
        }
        else if (gameObject.CompareTag("Player"))
        {
         
            OnPlayerDied?.Invoke();
            gameObject.SetActive(false);
            UIManager.Instance.ShowEndStagePanel(true, false);
            Time.timeScale = 0f;
            Debug.Log("Player Died - Game Over");
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning($"[Character] {name}: Không tìm thấy Animator!");

        attackRangeVisual = GetComponentInChildren<IAttackRange>();
        if (attackRangeVisual != null)
        {
            attackRangeVisual.ResetAttack();
            Debug.Log($"[Character] {name}: Đã reset AttackRangeVisual.");
        }

        weaponManager = GetComponentInChildren<PlayerWeaponManager>();
        if (weaponManager == null && !gameObject.CompareTag("Enemy"))
            Debug.LogWarning($"[Character] {name}: Không tìm thấy PlayerWeaponManager!");

        soundManager = FindObjectOfType<SoundManager>();
    }

    private void Start()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return new WaitForEndOfFrame(); // Chờ ShopManager load
        if (gameObject.CompareTag("Player") && weaponManager != null && weaponManager.currentInstance != null)
        {
            gun = weaponManager.currentInstance.GetComponent<Gun>();
            if (gun == null)
                Debug.LogWarning($"[Character] {name}: Không tìm thấy Gun trong PlayerWeaponManager.currentInstance!");
            else
                Debug.Log($"[Character] {name}: Gán Gun: {gun.name}");
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            gun = GetComponentInChildren<BotGun>(); // SỬA: Lấy BotGun cho Enemy
            if (gun == null)
                Debug.LogWarning($"[Character] {name}: Không tìm thấy BotGun (Enemy)!");
            else
                Debug.Log($"[Character] {name}: Gán BotGun: {gun.name}");
        }
    }

    public void AddTarget(Character target)
    {
        if (target == null || targets.Contains(target) || target == this)
        {
            Debug.LogWarning($"[Character] {name}: Không thể thêm target {target?.name}: null, đã tồn tại hoặc là bản thân!");
            return;
        }
        targets.Add(target);
        target.onCharacterDead += RemoveTarget;
        Debug.Log($"[Character] {name}: Đã thêm mục tiêu {target.name}");
    }

    public void RemoveTarget(Character target)
    {
        if (targets.Contains(target))
        {
            targets.Remove(target);
            target.onCharacterDead -= RemoveTarget;
            Debug.Log($"[Character] {name}: Đã xóa mục tiêu {target.name}");
        }
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        Debug.Log($"[Character] {name}: Gọi Die(), trigger animation chết.");
        GetComponent<BotController>()?.NotifyDeath();
       soundManager.PlayKillEnemy();

    }

    public void ResetCharacter()
    {
        isDead = false;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

            foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
            {
                animator.ResetTrigger(state.ToString());
            }
        }

        SetState(PlayerState.IsIdle);

        foreach (var target in targets.ToList())
        {
            RemoveTarget(target);
        }
        Debug.Log($"[Character] {name}: Đã xóa hết targets. Tổng targets: {targets.Count}");

        if (attackRangeVisual != null)
        {
            attackRangeVisual.ResetAttack();
            Debug.Log($"[Character] {name}: Đã reset AttackRangeVisual.");
        }

        gameObject.SetActive(true);
        Debug.Log($"[Character] {name}: Đã reset và kích hoạt lại.");
    }

    public void SetState(PlayerState newState)
    {
        if (IsDead && newState != PlayerState.IsDead) return;

        if (animator == null)
        {
            Debug.LogWarning($"[Character] {name}: Animator null, không thể set state {newState}.");
            return;
        }

        foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
        {
            animator.ResetTrigger(state.ToString());
        }

        animator.SetTrigger(newState.ToString());
        CurrentState = newState;
        //Debug.Log($"[Character] {name}: Đặt trạng thái thành {newState}");

        if (newState == PlayerState.IsAttack)
        {
            StopAllCoroutines();
            StartCoroutine(BackToIdleAfterAttack(0.7f));
        }
        else
        {
            StopAllCoroutines();
        }
    }

    private IEnumerator BackToIdleAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (CurrentState == PlayerState.IsAttack && !IsDead)
        {
            SetState(PlayerState.IsIdle);
        }
    }

    public bool IsInState(PlayerState state)
    {
        return CurrentState == state;
    }

    public void OnAttackAnimationComplete()
    {
        if (CurrentState == PlayerState.IsAttack)
        {
            SetState(PlayerState.IsIdle);
        }
    }
}