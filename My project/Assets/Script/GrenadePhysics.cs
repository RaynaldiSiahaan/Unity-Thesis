using UnityEngine;
using System.Collections;

public class GrenadeThrow : MonoBehaviour
{
    // Serialize Fields
    [SerializeField] private float x1Width, y1Height, z1Depth;
    [SerializeField] private float x2Width, y2Height, z2Depth;
    [SerializeField] private float timeInterval;
    [SerializeField] private float dragCoefficient = 0.1f;
    [SerializeField] private Camera mainCamera;
    
    [Header("Explosion Settings")]
    public GameObject explosionPrefab;    // assign your explosion VFX prefab here
    [SerializeField] private float explosionDelay = 4f;     // delay in seconds

    // Variables
    private Rigidbody rb;
    private Vector3 velocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        ThrowGrenade();
    }

    void ThrowGrenade()
    {
        // convert screen → world
        Vector3 worldPos1 = mainCamera.ScreenToWorldPoint(new Vector3(x1Width, y1Height, z1Depth));
        Vector3 worldPos2 = mainCamera.ScreenToWorldPoint(new Vector3(x2Width, y2Height, z2Depth));
        velocity = (worldPos2 - worldPos1) / timeInterval;
        rb.linearVelocity = velocity;

        // schedule explosion
        Invoke(nameof(Explode), explosionDelay);

        // debug
        Debug.Log($"Velocity: {velocity}");
    }

    void FixedUpdate()
    {
        // wind drag
        Vector3 windForce = -dragCoefficient * rb.linearVelocity.sqrMagnitude * rb.linearVelocity.normalized;
        rb.AddForce(windForce, ForceMode.Acceleration);
    }

    private void Explode()
    {
        // spawn explosion VFX
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // TODO: apply damage, area effects, sound, etc. here

        // clean up
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Terrain"))
        {
            DestroyLineRenderer();
        }
    }

    private void DestroyLineRenderer()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr != null) Destroy(lr, 2f);
    }
}
