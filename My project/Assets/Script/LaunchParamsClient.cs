using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(GrenadeSimulator))]
public class LaunchParamsClient : MonoBehaviour
{
    [Header("Server Settings")]
    [Tooltip("Base URL of your Python server (including port)")]
    public string serverIp   = "http://127.0.0.1:5000";
    public string startPath  = "/start";
    public string launchPath = "/launch";

    [Header("Camera & Simulation")]
    [Tooltip("The Camera that captured the launch vector")]
    public Camera sourceCamera;
    public UIManager uiManager;
    private GrenadeSimulator simulator;
    private bool isPolling;

    void Awake()
    {
        simulator = GetComponent<GrenadeSimulator>();
        if (sourceCamera == null) sourceCamera = Camera.main;
        Debug.Log($"[LaunchParamsClient] Using camera: {sourceCamera.name}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && !isPolling)
        {
            Debug.Log("[LaunchParamsClient] N pressed, starting detection…");
            uiManager.OnStartPressed();
            StartCoroutine(StartAndWait());
        }
    }

    IEnumerator StartAndWait()
    {
        isPolling = true;

        // 1) Tell Python to start detecting
        var startUrl = serverIp + startPath;
        Debug.Log($"[LaunchParamsClient] POST {startUrl}");
        using (var req = UnityWebRequest.Post(startUrl, new WWWForm()))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[LaunchParamsClient] /start failed: {req.error}");
                isPolling = false;
                yield break;
            }
            Debug.Log("[LaunchParamsClient] /start acknowledged");
        }

        // 2) Poll /launch until we get a real velocity
        var launchUrl = serverIp + launchPath;
        Debug.Log($"[LaunchParamsClient] polling {launchUrl} …");
        while (true)
        {
            using (var req = UnityWebRequest.Get(launchUrl))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var data = JsonUtility.FromJson<LaunchParams>(req.downloadHandler.text);
                    Debug.Log($"[LaunchParamsClient] got → v={data.velocity:F2}, dir={data.direction}");
                    if (data.velocity > 0f)
                    {
                        // --- convert from camera-space to world-space ---
                        Vector3 camDir   = data.direction.normalized;
                        Vector3 worldDir = sourceCamera.transform.TransformDirection(camDir).normalized;

                        // if camera is exactly to the side and still backwards, flip Z:
                        if (worldDir.z < 0f) worldDir.z *= -1f;

                        simulator.SetLaunchParams(data.velocity, worldDir);
                        simulator.LaunchFromParams();
                        uiManager.OnThrowComplete();
                        Debug.Log($"[LaunchParamsClient] Launched grenade at {data.velocity:F2} m/s → {worldDir}");
                        break;
                    }
                    else
                    {
                        Debug.Log("[LaunchParamsClient] waiting for non-zero velocity…");
                    }
                }
                else
                {
                    Debug.LogWarning($"[LaunchParamsClient] poll error: {req.error}");
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        isPolling = false;
        Debug.Log("[LaunchParamsClient] cycle complete");
    }

    [System.Serializable]
    private class LaunchParams
    {
        public float   velocity;
        public Vector3 direction;
    }
}
