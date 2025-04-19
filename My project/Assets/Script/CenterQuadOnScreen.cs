using UnityEngine;

public class CenterQuadOnScreen : MonoBehaviour
{
public Transform cameraTransform; // Drag your main camera here
    public float distance = 2f;
    public float rightOffset = 1.5f; // Offset to the right of the camera



    void LateUpdate()
    {
        // Position the quad directly in front of the camera
        transform.position = cameraTransform.position + cameraTransform.forward * distance + cameraTransform.right*rightOffset;

        // Rotate to face the camera
        // transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
        Vector3 lookDirection = transform.position - cameraTransform.position;
        lookDirection.y = 0; // Prevents tilting up/down

    }

}
