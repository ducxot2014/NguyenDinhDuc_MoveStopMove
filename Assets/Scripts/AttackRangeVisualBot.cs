using UnityEngine;

public class AttackRangeVisualBot : MonoBehaviour, IAttackRange
{
    public bool isAttacking { get; private set; }

    private Character character;
    [SerializeField] private GameObject attackRangeObject;
    [SerializeField] public float defaultScale = 5f;
    [HideInInspector] public BotGun gun;
    [HideInInspector] public float originalScale = 5f;

    private void Awake()
    {
        character = GetComponentInParent<Character>();
        if (character == null)
            Debug.LogError($"[AttackRangeVisualBot] {name}: Không tìm thấy Character trong parent!");

        if (attackRangeObject != null)
        {
            attackRangeObject.transform.localScale = Vector3.one * defaultScale;
            Collider rangeCollider = attackRangeObject.GetComponent<Collider>();
            if (rangeCollider == null || !rangeCollider.isTrigger)
                Debug.LogError($"[AttackRangeVisualBot] {name}: attackRangeObject thiếu Collider hoặc không phải Trigger!");
        }
        else
        {
            Debug.LogError($"[AttackRangeVisualBot] {name}: attackRangeObject không được gán!");
        }

        originalScale = defaultScale;

        gun = Cache.GetBotGun(gameObject);
        if (gun == null)
            Debug.LogWarning($"[AttackRangeVisualBot] {name}: Không tìm thấy BotGun trong parent!");
    }

    private void OnTriggerStay(Collider other)
    {
        if (character == null) return;

        if (other.GetComponent<Bullet>() != null || (!other.CompareTag("Player") && !other.CompareTag("Enemy")))
        {
            //Debug.Log($"[AttackRangeVisualBot] {name}: Bỏ qua {other.name} (tag={other.tag}, isBullet={other.GetComponent<Bullet>() != null})");
            return;
        }

        Character target = Cache.GetCharacter(other.gameObject);
        if (target != null && target != character && !target.IsDead)
        {
            isAttacking = true;

            BotController botController = character.GetComponent<BotController>();
            if (botController != null)
            {
                botController.currentTarget = target;
                Debug.Log($"[AttackRangeVisualBot] {name}: Phát hiện mục tiêu {target.name}, đặt currentTarget.");
                ShootAtTarget(target);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (character == null) return;

        // Bỏ qua nếu other là đạn (Bullet component)
        if (other.GetComponent<Bullet>() != null)
        {
            Debug.Log($"[AttackRangeVisualBot] {name}: Bỏ qua OnTriggerExit cho đạn {other.name}.");
            return;
        }

        Character target = Cache.GetCharacter(other.gameObject);
        if (target != null)
        {
            isAttacking = false;

            BotController botController = character.GetComponent<BotController>();
            if (botController != null && botController.currentTarget == target)
            {
                botController.currentTarget = null;
                Debug.Log($"[AttackRangeVisualBot] {name}: Mục tiêu {target.name} ra khỏi tầm, reset currentTarget.");
            }
        }
    }

    public void EnlargeBy(float amount)
    {
        defaultScale += amount;
        if (attackRangeObject != null)
            attackRangeObject.transform.localScale = Vector3.one * defaultScale;
        Debug.Log($"[AttackRangeVisualBot] {name}: Phóng to phạm vi tấn công lên {defaultScale}.");
    }

    public void SetDefaultScale(float scale)
    {
        defaultScale = scale;
        if (attackRangeObject != null)
            attackRangeObject.transform.localScale = Vector3.one * defaultScale;
        Debug.Log($"[AttackRangeVisualBot] {name}: Đặt phạm vi tấn công thành {defaultScale}.");
    }

    public void ResetScale()
    {
        defaultScale = originalScale;
        if (attackRangeObject != null)
            attackRangeObject.transform.localScale = Vector3.one * defaultScale;
        Debug.Log($"[AttackRangeVisualBot] {name}: Reset phạm vi tấn công về {defaultScale}.");
    }

    public void ResetAttack()
    {
        isAttacking = false;
        Debug.Log($"[AttackRangeVisualBot] {name}: Reset trạng thái tấn công.");
    }

    public void ShootAtTarget(Character target)
    {
        if (gun == null || target == null ) return;

        BotController botController = character.GetComponent<BotController>();
        if (botController != null && botController.currentTarget == target)
        {
            isAttacking = true;
            character.SetState(Character.PlayerState.IsAttack);
            gun.Shoot();
            Debug.Log($"[AttackRangeVisualBot] {name}: Đặt trạng thái tấn công và bắn vào mục tiêu {target.name}.");
        }
    }
}