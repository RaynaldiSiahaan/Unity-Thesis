using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("References to your TMP UI elements")]
    [SerializeField] private TextMeshProUGUI statusKameraText;
    [SerializeField] private TextMeshProUGUI jarakTargetText;

    [Header("Teleport & Target Setup")]
    [Tooltip("Drag in your PlayerMovement component here")]
    [SerializeField] private PlayerMovement playerMovement;

    [Tooltip("List of tags for your targets, in the same order as your teleport points")]
    [SerializeField] private string[] targetTags;

    void Start()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();

        statusKameraText.text = "Status Kamera: Menunggu Input";
        jarakTargetText.text  = "Jarak ke Target: –";

        // start the 2-second polling loop
        StartCoroutine(UpdateDistanceRoutine());
    }

    /// <summary>Call when Unity presses N and /start returns success.</summary>
    public void OnCameraReady()
    {
        statusKameraText.text = "Status Kamera: Siap digunakan";
    }

    /// <summary>Call when the grenade actually launches.</summary>
    public void OnThrowComplete()
    {
        statusKameraText.text = "Status Kamera: Menunggu Input";
    }

    private IEnumerator UpdateDistanceRoutine()
    {
        while (true)
        {
            // determine which target tag to use
            int idx = 0;
            if (playerMovement != null)
                idx = playerMovement.CurrentPoint();
            // clamp
            if (idx < 0 || idx >= targetTags.Length)
                idx = 0;
            
            string tag = targetTags[idx];
            GameObject tgt = GameObject.FindWithTag(tag);

            if (tgt != null)
            {
                float dist = Vector3.Distance(Camera.main.transform.position, tgt.transform.position);
                jarakTargetText.text = $"Jarak ke Target: {dist:F2} m";
            }
            else
            {
                jarakTargetText.text = $"Jarak ke Target: –";
            }

            // wait 2s before updating again
            yield return new WaitForSeconds(1f);
        }
    }
}
