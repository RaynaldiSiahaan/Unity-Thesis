using UnityEngine;

public class ObjectStaticGravity : MonoBehaviour
{
    public void OnCollisionEnter(Collision collision) { 
        if (collision.gameObject.tag == "Terrain") {
            GetComponent<Rigidbody>().isKinematic = true;
            Debug.Log("Collided");
        }
    }
}