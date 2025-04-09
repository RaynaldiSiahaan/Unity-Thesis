using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class APCController : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints; // Array of waypoints
    [SerializeField] private float speedKPH = 10f; // Speed in km/h
    [SerializeField] private float fixedY = 1f;
    [SerializeField] private float stopDuration = 5f;
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

    public PlayerMovement playerMovement; // Reference to PlayerMovement script
    public InputController inputController; // Reference to InputController script
    private float newSpeed; // New speed from InputController

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
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
        if (waypoints.Length != 1 || isStopped) return;

        // Convert speed from km/h to m/s
        speedMPS = speedKPH * 0.27778f;

        // Get current waypoint position but keep Y fixed
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        targetPosition.y = fixedY; // Set Y to fixed value

        // Move towards the next waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speedMPS * Time.deltaTime);

        float distanceTraveled = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position; // Update last position
        Debug.Log("The APC is moving!" + " Current speed: " + speedKPH + " km/h");

        // Rotate the wheels
        RotateWheels(distanceTraveled);

        // Check if the car reached the waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            Debug.Log("You've arrived at the waypoint!");

            if (currentWaypointIndex == waypoints.Length - 1) // Last waypoint
            {
                StartCoroutine(StopAndReset());
            }
            else
            {
                currentWaypointIndex++;
            }
        }
    }

    private IEnumerator StopAndReset()
    {
        isStopped = true;
        yield return new WaitForSeconds(stopDuration); // Stop for 5 seconds
        currentWaypointIndex = 0; // Go back to the first waypoint
        isStopped = false;
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
