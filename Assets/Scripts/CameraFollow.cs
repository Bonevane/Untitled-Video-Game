using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector2 worldZBounds = new Vector2(100f, 150f); // Absolute world coordinates
    [SerializeField] private float deadZoneRadius = 2f; // Distance from camera center before following starts
    [SerializeField] private bool followEnabled = true;

    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference is not assigned to CameraFollow script!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) // Toggle camera following
        {
            followEnabled = !followEnabled;
            Debug.Log($"Camera follow: {(followEnabled ? "Enabled" : "Disabled")}");
        }
    }

    void LateUpdate()
    {
        if (!followEnabled || player == null) return;

        Vector3 targetPosition = transform.position;

        // Always follow the player's X position
        targetPosition.x = player.position.x;

        // Calculate where camera wants to be (player + 25 on Z-axis)
        float idealCameraZ = player.position.z - 15f;

        // Clamp this ideal position to stay within world bounds
        float clampedIdealZ = Mathf.Clamp(idealCameraZ, worldZBounds.x, worldZBounds.y);

        // Calculate distance from current camera position to clamped ideal position
        float distanceFromIdeal = clampedIdealZ - transform.position.z;

        // Only move camera if the ideal position is beyond the dead zone
        if (Mathf.Abs(distanceFromIdeal) > deadZoneRadius)
        {
            // Move camera toward the clamped ideal position, maintaining dead zone
            float targetZ;
            if (distanceFromIdeal > 0)
            {
                targetZ = clampedIdealZ - deadZoneRadius;
            }
            else
            {
                targetZ = clampedIdealZ + deadZoneRadius;
            }

            // Make sure we don't go outside bounds
            targetZ = Mathf.Clamp(targetZ, worldZBounds.x, worldZBounds.y);

            // Smoothly move to the target position
            targetPosition.z = Mathf.Lerp(transform.position.z, targetZ, Time.deltaTime * smoothSpeed);
        }

        transform.position = targetPosition;
    }

    // Helper method to update bounds at runtime
    public void SetWorldBounds(Vector2 newBounds)
    {
        worldZBounds = newBounds;
    }

    // Visualize bounds and dead zone in Scene view
    void OnDrawGizmos()
    {
        // Draw world bounds
        Gizmos.color = Color.yellow;
        Vector3 minBound = new Vector3(transform.position.x, transform.position.y, worldZBounds.x);
        Vector3 maxBound = new Vector3(transform.position.x, transform.position.y, worldZBounds.y);

        Gizmos.DrawLine(minBound + Vector3.up * 2f, minBound + Vector3.down * 2f);
        Gizmos.DrawLine(maxBound + Vector3.up * 2f, maxBound + Vector3.down * 2f);

        // Draw ideal camera position (player.z + 25 line)
        if (player != null)
        {
            Gizmos.color = Color.red;
            Vector3 idealCameraLine = new Vector3(transform.position.x, transform.position.y, player.position.z + 25f);
            Gizmos.DrawLine(idealCameraLine + Vector3.up * 1.5f, idealCameraLine + Vector3.down * 1.5f);
        }

        // Draw dead zone around current camera position
        Gizmos.color = Color.green;
        Vector3 cameraCenter = transform.position;
        Vector3 deadZoneMin = cameraCenter + Vector3.back * deadZoneRadius;
        Vector3 deadZoneMax = cameraCenter + Vector3.forward * deadZoneRadius;

        Gizmos.DrawLine(deadZoneMin + Vector3.up * 0.5f, deadZoneMin + Vector3.down * 0.5f);
        Gizmos.DrawLine(deadZoneMax + Vector3.up * 0.5f, deadZoneMax + Vector3.down * 0.5f);

        // Draw dead zone area
        Gizmos.color = Color.green * 0.3f;
        Gizmos.DrawCube(cameraCenter, new Vector3(0.1f, 1f, deadZoneRadius * 2f));
    }
}