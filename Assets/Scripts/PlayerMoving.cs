using UnityEngine;
using System;

public class PlayerMoving : MonoBehaviour, IMoving
{
    [SerializeField] private Joystick joystick;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Rigidbody rb;

    private Vector3 moveDirection;
    private bool isMoving = false;

    // Sự kiện thông báo trạng thái di chuyển thay đổi
    public event Action<bool> OnMovementStateChanged;

    private void Awake()
    {
        if (joystick == null)
            Debug.LogWarning($"[PlayerMoving] {name}: Joystick không được gán!");
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }



    private void HandleRotation()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void HandleMovement()
    {
        if (joystick == null || rb == null) return;

        moveDirection = new Vector3(-joystick.Horizontal, 0, -joystick.Vertical).normalized;
        rb.MovePosition(transform.position + moveDirection * speed * Time.fixedDeltaTime);

        bool currentlyMoving = moveDirection != Vector3.zero;

        if (currentlyMoving != isMoving)
        {
            isMoving = currentlyMoving;
            OnMovementStateChanged?.Invoke(isMoving); // Chỉ thông báo, không set anim ở đây
        }
    }

    public bool IsStandingStill() => !isMoving;

    public void StopMoving()
    {
        // Không cần làm gì vì di chuyển dựa trên joystick
    }

    public void ResumeMoving()
    {
        Character character = GetComponent<Character>();
        if (character != null)
        {
            character.SetState(Character.PlayerState.IsRun);
        }
        else
        {
            Debug.LogWarning($"[PlayerMoving] {name}: Character không được gán!");
        }
    }
}
