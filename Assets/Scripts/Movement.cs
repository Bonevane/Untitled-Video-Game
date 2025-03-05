using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;

    public float moveSpeed = 1f;
    public float verticalSpeedMultiplier = 1.1f;
    public float diagonalSpeedMultiplier = 1.1f;
    public float sprintMultiplier = 1.5f;
    public float collisionCheckDistance = 0.1f;
    public LayerMask collisionLayers;

    private Vector3 moveDirection;
    private Quaternion idleRotation;
    private Rigidbody rb;
    private BoxCollider boxCollider; // Change to BoxCollider


    public DialogueUI DialogueUI => dialogueUI;
    public IInteractable Interactable { get; set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>(); // Get BoxCollider component
        dialogueUI = FindAnyObjectByType<DialogueUI>();
        idleRotation = transform.rotation;
    }

    private void Update()
    {
        if (dialogueUI.isOpen) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
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
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(horizontal, 0, vertical);

        if (moveDirection != Vector3.zero)
        {
            moveDirection = SnapToEightDirections(moveDirection);

            float adjustedSpeed = moveSpeed;
            bool movingDiagonally = Mathf.Abs(horizontal) > 0 && Mathf.Abs(vertical) > 0;

            if (Mathf.Abs(moveDirection.z) > Mathf.Abs(moveDirection.x))
            {
                adjustedSpeed *= verticalSpeedMultiplier;
            }

            if (movingDiagonally)
            {
                adjustedSpeed *= diagonalSpeedMultiplier;
            }

            adjustedSpeed *= Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f;

            // Check for collisions using BoxCast
            if (!CheckCollision(moveDirection))
            {
                rb.MovePosition(transform.position + moveDirection * adjustedSpeed);
            }

            float angle = Mathf.Atan2(-moveDirection.x, -moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        else
        {
            transform.rotation = idleRotation;
        }
    }

    Vector3 SnapToEightDirections(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        angle = Mathf.Round(angle / 45) * 45;

        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
    }

    bool CheckCollision(Vector3 direction)
    {
        Vector3 boxSize = boxCollider.bounds.extents * 0.9f; // Slightly reduce to prevent edge sticking
        Vector3 center = boxCollider.bounds.center; // Use world-space center
        float skinWidth = 0.02f; // Prevent floating-point errors

        Debug.DrawRay(center, direction * (collisionCheckDistance - skinWidth), Color.red, 0.1f);
        return Physics.BoxCast(center, boxSize, direction, transform.rotation, collisionCheckDistance - skinWidth, collisionLayers);
    }

}
