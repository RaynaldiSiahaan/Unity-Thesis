using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class APCController : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints; // Array of waypoints
    [SerializeField] private float speedKPH = 10f; // Speed in km/h
    [SerializeField] private float stopDuration = 1f;
    [SerializeField] private float wheelRadius = 0.5f; // Adjust based on actual wheel size
    [SerializeField] private Transform[] wheels;

    private int currentWaypointIndex = 0;
    private float speedMPS; // Speed in meters per second
    private bool isStopped = false;

    public GameObject inputPanel; // Reference to the input panel
    private string input;
    private int index;
    private Rigidbody rb;
    private Vector3 lastPosition;
    private Quaternion lastRotation; // Store the last rotation
    public PlayerMovement playerMovement; // Reference to PlayerMovement script
    public InputController inputController; // Reference to InputController script
    private float newSpeed; // New speed from InputController

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    public void ReadStringInput(string s)
    {
        input = s;
        speedKPH = float.Parse(input); // Convert string to float
        Debug.Log("Speed set to: " + speedKPH + " km/h");

        if (inputPanel != null)
        {
            inputPanel.SetActive(false); // Hide the input panel after reading the input
            Debug.Log("Input panel deactivated.");
        }

        isStopped = false;
    }

    private void Update()
    {
        index = playerMovement.CurrentPoint();
        bool isActive = inputController.inputPanelAPC.activeSelf;

        if (index == 1 && !isActive)
        {
            isStopped = false;
            MoveAPC();
        }
        else
        {
            isStopped = true;
        }
    }

    private void MoveAPC()
    {
        if (isStopped) return;

        // Convert speed from km/h to m/s
        speedMPS = speedKPH * 0.27778f;

        // Get the target waypoint position but keep Y fixed
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;

        // Move towards the target waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speedMPS * Time.deltaTime);

        float distanceTraveled = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position; // Update last position

        // Rotate the wheels
        RotateWheels(distanceTraveled);

        // Check if the APC reached the target waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            Debug.Log("You've arrived at the waypoint!");

            if (currentWaypointIndex == waypoints.Length - 1) // Last waypoint (Finish)
            {
                rb.isKinematic = true;
                StartCoroutine(StopAndReset());
            }
            else
            {
                currentWaypointIndex++; // Move to the next waypoint
            }
        }
    }

    private IEnumerator StopAndReset()
    {
        Debug.Log("Stopping at the finish waypoint!");

        isStopped = true;
        yield return new WaitForSeconds(stopDuration); // Stop for the specified duration

        // Reset to the first waypoint's position
        Vector3 Start = waypoints[0].position;
        transform.position = Start;
        transform.rotation = lastRotation;
        currentWaypointIndex = 0; // Reset to the first waypoint
        isStopped = false;
        yield return new WaitForSeconds(stopDuration);
        rb.isKinematic = false; // Reactivate the Rigidbody
    }

    void RotateWheels(float distance)
    {
        float rotationAngle = (distance / (2 * Mathf.PI * wheelRadius)) * 360f;

        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(0.34f, 1f, 0f, Space.Self); // Rotate around local X-axis
        }
    }
}
