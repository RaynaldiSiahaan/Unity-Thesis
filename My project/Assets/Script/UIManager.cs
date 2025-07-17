// UIManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Text Elements")]
    [SerializeField] private TextMeshProUGUI statusKameraText;
    [SerializeField] private TextMeshProUGUI jarakTargetText;

    [SerializeField] private TextMeshProUGUI statusTargetText;

    [Header("Teleport & Target Setup")]
    [Tooltip("Drag in your PlayerMovement component here")]
    [SerializeField] private PlayerMovement playerMovement;
    [Tooltip("Tags for each target, in the same order as your teleport points")]
    [SerializeField] private string[] targetTags;

    private enum CamState { Off, Standby, Mencari }
    private CamState camState = CamState.Off;
    private const string COORDS_URL = "http://127.0.0.1:5000/coords";

    private GrenadeSimulator simulator;


    void Start()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();

        simulator = FindObjectOfType<GrenadeSimulator>();


        // Kick off the camera‐connection check
        StartCoroutine(CheckCameraLoop());

        // Start updating distance every 2 seconds
        StartCoroutine(UpdateDistanceRoutine());
    }

    /// <summary>
    /// Polls the /coords endpoint once per second to detect camera readiness.
    /// </summary>
    private IEnumerator CheckCameraLoop()
    {
        while (true)
        {
            using (var req = UnityWebRequest.Get(COORDS_URL))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                    SetCamState(CamState.Standby);
                else
                    SetCamState(CamState.Off);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Call this right when you press N (i.e. POST /start goes out).
    /// </summary>
    public void OnStartPressed()
    {
        SetCamState(CamState.Mencari);
    }

    /// <summary>
    /// Call this when the grenade actually launches (after valid /launch).
    /// </summary>
    public void OnThrowComplete()
    {
        SetCamState(CamState.Standby);
    }

    /// <summary>
    /// Updates the distance to current target every 2 seconds.
    /// </summary>
    private IEnumerator UpdateDistanceRoutine()
    {
        while (true)
        {
            int idx = 0;
            if (playerMovement != null)
                idx = playerMovement.CurrentPoint();

            if (idx < 0 || idx >= targetTags.Length)
                idx = 0;

            var tag = targetTags[idx];
            var tgt = GameObject.FindWithTag(tag);
            if (tgt != null)
            {
                float dist = Vector3.Distance(Camera.main.transform.position, tgt.transform.position);
                jarakTargetText.text = $"Jarak ke Target: {dist:F2} m";
            }
            else
            {
                jarakTargetText.text = "Jarak ke Target: –";
            }

            yield return new WaitForSeconds(2f);
        }
    }

    /// <summary>
    /// Internal helper to switch the status text.
    /// </summary>
    private void SetCamState(CamState newState)
    {
        if (camState == newState) return;
        camState = newState;

        switch (camState)
        {
            case CamState.Off:
                statusKameraText.text = "Status Kamera: Off";
                break;
            case CamState.Standby:
                statusKameraText.text = "Status Kamera: Standby";
                break;
            case CamState.Mencari:
                statusKameraText.text = "Status Kamera: Mencari granat";
                break;
        }
    }
    
    public void SetTargetStatus(string status)
    {
        statusTargetText.text = $"Status Target: {status}";
    }
}
