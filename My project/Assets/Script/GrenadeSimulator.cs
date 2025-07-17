using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine.Events;
using TMPro;
[RequireComponent(typeof(Rigidbody), typeof(TrailRenderer))]
public class GrenadeSimulator : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Hook up your UIManager here")]
    [SerializeField] public UIManager uiManager;
    [Header("Throw Parameters (from camera)")]
    [Tooltip("Launch speed in meters/second")]
    [SerializeField] private float velocity = 15f;
    [Tooltip("Normalized direction vector (X=right, Y=up, Z=forward)")]
    [SerializeField] private Vector3 direction = new Vector3(0f, 0.7f, 0.7f);

    [Header("Aerodynamics Parameters")]
    [Tooltip("Air density ρ (kg/m³)")]
    [SerializeField] private float airDensity = 1.225f;
    [Tooltip("Drag coefficient C_d (dimensionless)")]
    [SerializeField] private float dragCoefficient = 0.47f;
    [Tooltip("Cross-sectional area A (m²)")]
    [SerializeField] private float crossSectionalArea = 0.03f;
    [Tooltip("Mass of the grenade m (kg)")]
    [SerializeField] private float mass = 0.4f;

    [Header("Wind & Spin")]
    [Tooltip("Constant wind velocity vector (m/s)")]
    [SerializeField] private Vector3 windVelocity = Vector3.zero;
    [Tooltip("Spin axis (unit vector)")]
    [SerializeField] private Vector3 spinAxis = new Vector3(0, 0, 1);
    [Tooltip("Lift coefficient for Magnus effect C_l (dimensionless)")]
    [SerializeField] private float liftCoefficient = 0.2f;

    [Header("Spin Settings")]
[Tooltip("Impulse torque strength")]
[SerializeField] private float spinTorque = 5f;

    [Header("Explosion Settings")]
    public GameObject explosionPrefab;    // assign your explosion VFX prefab here
    public AudioSource explosionSound; // assign your explosion sound here
    [SerializeField] private float explosionDelay = 4f;     // delay in seconds
    [SerializeField] private float _explosionRadius = 3;   //radius ledakan
    [SerializeField] private float _explosionForce = 500;  //kekuatan ledakan
    [SerializeField] private float respawnDelay = 2f; // delay before respawning grenade
    public GameObject barrel;

    private int barrelCount = 0;
    // cached starting transform granat
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Vector3 _initialScale;

    private Rigidbody rb;
    private TrailRenderer trail;
    // cached starting transform barrel
    private List<Transform> _barrels = new List<Transform>();
    private Dictionary<Transform, Vector3> _initialBarrelPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> _initialBarrelRotations = new Dictionary<Transform, Quaternion>();
    private Dictionary<Transform, Vector3> _initialBarrelScales = new Dictionary<Transform, Vector3>();

    void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();

        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialScale = transform.localScale;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = mass;

        // cache all barrels tagged "Barrel"
        foreach (var go in GameObject.FindGameObjectsWithTag("Target 1"))
            _barrels.Add(go.transform);

        Debug.Log($"Found {_barrels.Count} barrel(s) tagged 'Target 1'");

        // normalize inputs
        direction = direction.normalized;
        spinAxis = spinAxis.normalized;

        // Trail setup
        trail = GetComponent<TrailRenderer>();
        trail.time = 2f;
        trail.startWidth = 0.05f;
        trail.endWidth = 0.01f;
        trail.emitting = false; // disable until thrown
        if (trail.material == null)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = Color.white;
            trail.material = mat;
        }
    }

    void Start()
    {
        rb.isKinematic = true;
        Debug.Log($"[GrenadeSimulator] Speed={velocity:F2}, Dir={direction}, Mass={mass}kg");
    }

    void ThrowGrenade()
    {
        trail.emitting = true; // enable trail rendering
        Vector3 initialVel = direction * velocity;
        rb.linearVelocity = initialVel;

            rb.AddTorque(spinAxis.normalized * spinTorque, ForceMode.Impulse);

        // schedule explosion
        Invoke(nameof(Explode), explosionDelay);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Terrain"))
            Debug.Log("Touched");
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // Prevent any further physics from moving it
        rb.isKinematic = true;
        trail.Clear();
    }

    void FixedUpdate()
    {
        // 1) Relative velocity to wind
        Vector3 vRel = rb.linearVelocity - windVelocity;
        float speedRel = vRel.magnitude;
        if (speedRel < 1e-4f) return;

        // 2) Drag force: F_d = -½·ρ·C_d·A·|v_rel|²·v̂_rel
        Vector3 dragForce = -0.5f * airDensity * dragCoefficient * crossSectionalArea
                            * speedRel * speedRel * (vRel / speedRel);

        // 3) Magnus lift: F_m = ½·ρ·C_l·A·|v_rel|²·(ω̂ × v̂_rel)
        //    spinAxis defines the rotation axis; lift perpendicular to both spin and velocity.
        Vector3 magnusForce = 0.5f * airDensity * liftCoefficient * crossSectionalArea
                               * speedRel * speedRel * Vector3.Cross(spinAxis, vRel.normalized);

        // 4) Apply total aerodynamic forces
        rb.AddForce(dragForce + magnusForce, ForceMode.Force);
    }

    public void Explode()

    {
        Vector3 origin = transform.position;
        Vector3 downAxis = transform.up;
        float halfAngle = 45f;
        // spawn explosion VFX
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        foreach (var rend in GetComponentsInChildren<MeshRenderer>())
            rend.enabled = false;
        // 0) log distances to all tracked barrels
        foreach (var barrelTransform in _barrels)
        {
            float dist = Vector3.Distance(transform.position, barrelTransform.position);
            Debug.Log($"[Explode] Distance to {barrelTransform.name}: {dist:F2} m");
        }
        //Code untuk Sphere
        Collider[] hits = Physics.OverlapSphere(origin, _explosionRadius);
         foreach (var hitCol in hits)
    {
        // Skip the grenade itself
        if (hitCol.transform == transform)
            continue;

        // Direction and distance to target
        Vector3 toTarget = hitCol.bounds.center - origin;
        float   dist     = toTarget.magnitude;

        // 4) Line‑of‑sight check (optional)
        if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit info, dist)
            && info.collider != hitCol)
            continue;
        Rigidbody targetRb = hitCol.GetComponent<Rigidbody>();
        // 5) Apply explosion force
        if (targetRb != null)
        {
            targetRb.AddExplosionForce(_explosionForce, origin, _explosionRadius);

            // Count barrels hit
            if (_barrels.Contains(hitCol.transform))
                barrelCount++;
        }
    }
        //Code untuk Cone Explosion
        // foreach (var hitCol in hits)
        // {
        //     // skip self
        //     if (hitCol.transform == transform)
        //         continue;

        //     Vector3 toTarget = hitCol.bounds.center - origin;

        //     float angle = Vector3.Angle(downAxis, toTarget);
        //     float dist = toTarget.magnitude;

        //     if (angle <= halfAngle)
        //     {
        //         Debug.DrawRay(origin, toTarget.normalized * _explosionRadius, Color.red, 2f);

        //         if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit info, dist) && info.collider != hitCol)
        //             continue;


        //     // now safe to apply force
        //     Rigidbody targetRb = hitCol.GetComponent<Rigidbody>();
        //     if (targetRb != null)
        //     {
        //         targetRb.AddExplosionForce(_explosionForce, origin, _explosionRadius);

        //         // count barrels only
        //         if (_barrels.Contains(hitCol.transform))
        //             barrelCount++;


        //     }
        //     }

        //     // line-of-sight check

        // }
        if (barrelCount > 0)
        {
            uiManager.SetTargetStatus("Kena");
        }
        else if (barrelCount == 0)
        {
            uiManager.SetTargetStatus("Tidak Kena");
        }
        // 2) play audio
        if (explosionSound != null)
        {
            explosionSound.Play();
            // wait for clip length before destroying
        }
        // clean up
        Invoke(nameof(Respawn), respawnDelay);


    }

    private void Respawn()
    {
        Debug.Log("[GrenadeSimulator] Respawning grenade");

        // restore transform
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
        transform.localScale = _initialScale;

        // re-enable physics
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        trail.Clear();
        trail.emitting = false; // disable until thrown
        // re-enable every MeshRenderer
        foreach (var rend in GetComponentsInChildren<MeshRenderer>())
            rend.enabled = true;
    }



    /// <summary>Set from external JSON data</summary>
    public void SetLaunchParams(float v, Vector3 dir)
    {
        velocity = v;
        direction = dir.normalized;
    }

    /// Trigger the rowrow using the just-set params.
    public void LaunchFromParams()
    {
        rb.isKinematic = false; // allow physics to take over
        ThrowGrenade();
    }

void OnDrawGizmosSelected()
{
    if (!Application.isPlaying) return;

    Gizmos.color = Color.yellow;
    Vector3 origin   = transform.position;
    Vector3 axis     = transform.up;   // or -transform.up for downward cone
    float   angleDeg = 45f;
    float   length   = _explosionRadius;

    // draw a fan of rays around the axis
    for (int i = 0; i < 360; i += 30)
    {
        // first boundary
        Vector3 dirA = Quaternion.AngleAxis(angleDeg, Quaternion.AngleAxis(i, axis) * transform.forward)
                      * axis;
        Gizmos.DrawRay(origin, dirA.normalized * length);

        // second boundary
        Vector3 dirB = Quaternion.AngleAxis(-angleDeg, Quaternion.AngleAxis(i, axis) * transform.forward)
                      * axis;
        Gizmos.DrawRay(origin, dirB.normalized * length);
    }

    // and a wire‑sphere to show your full radius
    Gizmos.color = new Color(1,1,0,0.2f);
    Gizmos.DrawWireSphere(origin, length);
}
void OnDrawGizmos()
{
    if (rb == null) return;
    Gizmos.color = Color.cyan;
    Gizmos.DrawLine(
      transform.position,
      transform.position + rb.angularVelocity.normalized * 0.5f
    );
}


}

