using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class RealSenseFetcher : MonoBehaviour
{
    public float x;
    public string serverURL = "http://127.0.0.1:5000/coords";  // <-- replace with your Flask server IP

    void Start()
    {
        StartCoroutine(FetchCoordinates());
    }

    IEnumerator FetchCoordinates()
    {
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(serverURL);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[Error] {www.error}");
            }
            else
            {
                string jsonResult = www.downloadHandler.text;
                Coordinates coords = JsonUtility.FromJson<Coordinates>(jsonResult);

                if (coords != null)
                {
                    x = coords.x;
                    Debug.Log($"[Grenade] X: {coords.x:F2}, Y: {coords.y:F2}, Z: {coords.z:F2}, Distance: {coords.distance:F2} meters");
                }
            }

            yield return new WaitForSeconds(0.1f);  // fetch every 0.1s
        }
    }

    [System.Serializable]
    public class Coordinates
    {
        public float x;
        public float y;
        public float z;
        public float distance;
    }
}
