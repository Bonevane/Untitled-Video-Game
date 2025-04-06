using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector2 verticalBounds = new Vector2(-5f, 5f);
    [SerializeField] private bool followEnabled = true;

    private float initialZOffset;

    void Start()
    {
        if (player)
            initialZOffset = transform.position.z - player.position.z;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) // Toggle camera following
        {
            followEnabled = !followEnabled;
        }
    }

    void LateUpdate()
    {
        if (!followEnabled || player == null) return;

        Vector3 targetPosition = transform.position;

        // Always follow the player's X position
        targetPosition.x = player.position.x;

        // Calculate desired camera Z position
        float desiredZ = player.position.z + initialZOffset;

        // If player is within bounds, allow camera to move
        if (player.position.z > verticalBounds.x && player.position.z < verticalBounds.y)
        {
            targetPosition.z = Mathf.Lerp(transform.position.z, desiredZ, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // If player exceeds bounds, camera stops at limits
            targetPosition.z = Mathf.Lerp(transform.position.z, Mathf.Clamp(transform.position.z, verticalBounds.x + initialZOffset, verticalBounds.y + initialZOffset), Time.deltaTime * smoothSpeed);
        }

        transform.position = targetPosition;
    }
}
