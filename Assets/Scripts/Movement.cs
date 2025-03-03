using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;

    public float moveSpeed = 1f;
    public float verticalSpeedMultiplier = 1.1f; // Faster movement for vertical
    public float diagonalSpeedMultiplier = 1.1f; // Faster movement for vertical
    public float sprintMultiplier = 1.5f;
    public float collisionCheckDistance = 0.1f; // Adjust for fine-tuned collision
    public LayerMask collisionLayers;

    private Vector3 moveDirection;
    private Quaternion idleRotation;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider; // For collision size reference


    public DialogueUI DialogueUI => dialogueUI;
    public IInteractable Interactable { get; set; }


     
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        idleRotation = transform.rotation; // Store default rotation
    }
 

    private void Update()
    {
        if (dialogueUI.isOpen) return;

        if (Input.GetKeyDown(KeyCode.Return)) {
            if (Interactable != null)
            {
                Interactable.Interact(this);
            }
        }
    }

    void FixedUpdate()
    {
        if (dialogueUI.isOpen) return;

        HandleMovement();
    }

    void HandleMovement()
    {
        // Get raw input (WASD / Arrow keys)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Normalize movement direction
        moveDirection = new Vector3(horizontal, 0, vertical);

        if (moveDirection != Vector3.zero)
        {
            // Snap movement to 8 directions
            moveDirection = SnapToEightDirections(moveDirection);

            // Adjust speed (faster for vertical & balanced for diagonal)
            float adjustedSpeed = moveSpeed;
            bool movingDiagonally = Mathf.Abs(horizontal) > 0 && Mathf.Abs(vertical) > 0;

            if (Mathf.Abs(moveDirection.z) > Mathf.Abs(moveDirection.x))
            {
                adjustedSpeed *= verticalSpeedMultiplier; // Increase speed for vertical movement
            }

            if (movingDiagonally)
            {
                adjustedSpeed *= diagonalSpeedMultiplier;
            }

            // Apply sprint multiplier if Shift is held
            adjustedSpeed *= Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f;

            // Check for collisions before moving
            if (!CheckCollision(moveDirection))
            {
                rb.MovePosition(transform.position + moveDirection * adjustedSpeed);
            }

            // Fix rotation (swap X and Z to match RPG-style facing direction)
            float angle = Mathf.Atan2(-moveDirection.z, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        else
        {
            transform.rotation = idleRotation; // Reset rotation when idle
        }
    }

    Vector3 SnapToEightDirections(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        angle = Mathf.Round(angle / 45) * 45; // Snap to 45-degree increments

        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
    }

    bool CheckCollision(Vector3 direction)
    {
        // Use CapsuleCast for accurate mesh collider detection
        float capsuleRadius = capsuleCollider.radius * 0.9f; // Slightly reduce to prevent edge sticking
        float capsuleHeight = capsuleCollider.height * 0.5f; // Center of capsule

        Vector3 startPos = transform.position + Vector3.up * capsuleHeight; // Cast from capsule center
        return Physics.CapsuleCast(startPos, startPos, capsuleRadius, direction, collisionCheckDistance, collisionLayers);
    }
}
