using UnityEngine;

public class GrenadeThrow : MonoBehaviour
{
    //Serialize Field
    [SerializeField] private float x1Width, y1Height, z1Depth; // Initial screen position + depth in meters
    [SerializeField] private float x2Width, y2Height, z2Depth; // Final screen position + depth in meters
    [SerializeField] private float timeInterval; // Time difference (seconds)
    [SerializeField] private float dragCoefficient = 0.1f; // Wind resistance factor
    [SerializeField] private Camera mainCamera; // Assign in Inspector

    //Variable
    private float x1, y1, z1, x2, y2, z2;
    private Rigidbody rb;
    private Vector3 velocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        x1 = x1Width;
        y1 = y1Height;
        z1 = z1Depth;
        x2 = x2Width;
        y2 = y2Height;
        z2 = z2Depth;
        ThrowGrenade();
    }

    void ThrowGrenade()
    {
        // Convert screen pixels to world space using depth (z)
        Vector3 worldPos1 = mainCamera.ScreenToWorldPoint(new Vector3(x1, y1, z1));
        Vector3 worldPos2 = mainCamera.ScreenToWorldPoint(new Vector3(x2, y2, z2));

        // Calculate velocity (meters per second)
        velocity = (worldPos2 - worldPos1) / timeInterval;

        // Apply velocity to Rigidbody
        rb.linearVelocity = velocity;

        // Debugging
        Debug.Log($"World Position Before: {worldPos1}");
        Debug.Log($"World Position After: {worldPos2}");
        Debug.Log($"Velocity: {velocity}");
        Debug.Log($"Throw Angle: {Mathf.Atan2(velocity.y, Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z)) * Mathf.Rad2Deg} degrees");
    }

    void FixedUpdate()
    {
        // Apply wind resistance (drag)
        Vector3 windForce = -dragCoefficient * rb.linearVelocity.sqrMagnitude * rb.linearVelocity.normalized;
        rb.AddForce(windForce, ForceMode.Acceleration);
    }
}

/// RUMUS
/// 