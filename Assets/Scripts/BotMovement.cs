using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BotMovement : MonoBehaviour, IMoving
{
    [Header("Cài đặt di chuyển")]
    public float wanderRadius = 20f;
    public float wanderDelay = 2f;
    public float attackRange = 3f;

    [Header("Trạng thái game")]
    public static bool isGameStarted = false;

    private NavMeshAgent agent;
    private float timer;
    private BotController botController;
    private Character character;
    private Animator anim;
    private bool isMoving = false;
    private bool canMove = true;

    void Awake()
    {
        character = Cache.GetCharacter(gameObject);
        if (character == null)
        {
            Debug.LogError($"[BotMovement] {name}: Character không được tìm thấy qua Cache! Kiểm tra prefab có component Character.");
            enabled = false; // Tắt script nếu không có Character
            return;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[BotMovement] {name}: NavMeshAgent không được gán! Tắt script.");
            enabled = false;
            return;
        }

        botController = GetComponent<BotController>();
        if (botController == null)
            Debug.LogWarning($"[BotMovement] {name}: BotController không được gán!");

        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.LogWarning($"[BotMovement] {name}: Animator không được gán!");

        timer = wanderDelay;

        if (tag != "Enemy")
        {
            tag = "Enemy";
            Debug.Log($"[BotMovement] {name}: Tag được đặt thành 'Enemy'.");
        }
    }

    void Update()
    {
        if (!isGameStarted || !canMove)
        {
            StopMoving();
            return;
        }

        HandleBotLogic();
    }

    void HandleBotLogic()
    {
        bool hasValidTarget = false;

        if (botController != null && botController.currentTarget != null)
        {
            Character targetChar = botController.currentTarget; // SỬA: Dùng trực tiếp currentTarget (kiểu Character)
            float distance = Vector3.Distance(transform.position, targetChar.transform.position);

            if (!targetChar.IsDead && (targetChar.CompareTag("Enemy") || targetChar.CompareTag("Player")))
            {
                agent.isStopped = true;
                isMoving = false;
                hasValidTarget = true;
            }
            else
            {
                agent.isStopped = false;
            }
        }
        else
        {
            agent.isStopped = false;
        }

        timer += Time.deltaTime;
        if (timer >= wanderDelay && !agent.isStopped)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            timer = 0;
        }

        isMoving = agent.velocity.magnitude > 0.1f;

        if (!botController.isDead && (botController.currentTarget == null || !botController.attackRangeVisualBot.isAttacking))
        {
            if (isMoving)
                character.SetState(Character.PlayerState.IsRun);
            else
                character.SetState(Character.PlayerState.IsIdle);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist + origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, NavMesh.AllAreas);
        return navHit.position;
    }

    public bool IsStandingStill()
    {
        return !isMoving;
    }

    public void StopMoving()
    {
        canMove = false;
        if (agent != null)
            agent.isStopped = true;
        isMoving = false;
        if (character != null)
            character.SetState(Character.PlayerState.IsIdle);
        else
            Debug.LogWarning($"[BotMovement] {name}: Character is null in StopMoving!");
    }

    public void ResumeMoving()
    {
        if (agent != null)
            canMove = true;
        else
            Debug.LogWarning($"[BotMovement] {name}: Cannot resume moving, agent is null!");
    }
}