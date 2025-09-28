using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private GameObject playerModel;

    [SerializeField] private Animator animator;
    private int lastDirection = 0;

    public float moveSpeed = 1f;
    public float verticalSpeedMultiplier = 1.1f;
    public float diagonalSpeedMultiplier = 1.1f;
    public float sprintMultiplier = 1.5f;
    public float collisionCheckDistance = 0.1f;
    public bool rotation = true;
    public LayerMask collisionLayers;



    private Vector3 moveDirection;
    private Quaternion idleRotation;
    private Rigidbody rb;
    private BoxCollider boxCollider; // Change to BoxCollider

    private bool enterReleased = true;
    public DialogueUI DialogueUI => dialogueUI;
    public IInteractable Interactable { get; set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>(); // Get BoxCollider component
        dialogueUI = FindAnyObjectByType<DialogueUI>();
        idleRotation = playerModel.transform.rotation;
    }

    private void Update()
    {
        if (dialogueUI.isOpen) return;

        if (Input.GetKeyUp(KeyCode.Return)) enterReleased = true;

        if (Input.GetKeyDown(KeyCode.Return) && enterReleased)
        {
            enterReleased = false;
            if (Interactable != null)
            {
                Interactable.Interact(this);
            }
        }
    }

    void FixedUpdate()
    {
        if (dialogueUI.isOpen) {
            animator.speed = 0.0f;
            return;
        };

        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

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

            adjustedSpeed *= isSprinting ? sprintMultiplier : 1f;

            // Check for collisions using BoxCast
            if (!CheckCollision(moveDirection))
            {
                rb.MovePosition(transform.position + moveDirection * adjustedSpeed);
            }

            float angle = Mathf.Atan2(moveDirection.z, -moveDirection.x) * Mathf.Rad2Deg;
            
            if (rotation)
                playerModel.transform.rotation = Quaternion.Euler(0, angle, 0);

            
        }
        else
        {
            playerModel.transform.rotation = idleRotation;
        }

        // Animation Related
        int directionID = GetDirectionID(moveDirection);
        if (directionID != lastDirection)
        {
            animator.SetInteger("Direction", directionID);
            animator.SetTrigger("Changed");    // Trigger transitions from Any State
            lastDirection = directionID;
        }
        animator.speed = (isSprinting && moveDirection != Vector3.zero) ? 1.5f : 1.0f;
    }

    Vector3 SnapToEightDirections(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        angle = Mathf.Round(angle / 45) * 45;

        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
    }

    int GetDirectionID(Vector3 dir)
    {
        if (dir == Vector3.zero) return 0; // idle
        if (Mathf.Abs(dir.z) > Mathf.Abs(dir.x))
            return dir.z > 0 ? 2 : 1; // Up or Down
        else
            return dir.x > 0 ? 3 : 4; // Right or Left
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
