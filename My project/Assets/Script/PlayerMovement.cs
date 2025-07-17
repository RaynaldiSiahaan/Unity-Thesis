using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpPower = 7f;
    public float gravity = 20f;
    public float lookSpeed = 2f;
    public float lookXLimit = 90f;
    public float defaultHeight = 2f;
    public float proneHeight = 1f;
    public float proneSpeed = 3f;
    public float proneTransitionSpeed = 15f; // Speed of prone height transition
    public int teleportPointIndex = 0; // Index of teleport point to move to

    private bool isProneState = false;  // Tracks if player is in prone state

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;
    private bool isCurrentlyRunning = false; // Tracks running status
    private float targetHeight; // Target height for prone/standing

    public Transform[] teleportPoints; // Array to store teleport points
    private int currentPointIndex = 0; // Tracks current teleport index

    //Camera height and rotation
    private float camStandY;
    private float camProneY;
    private Quaternion camStandRot;
    private Quaternion camProneRot;
    [Header("Prone Camera Rotation")]
    [Tooltip("Local Euler angles for camera when prone")]
    public Vector3 proneCameraEuler = new Vector3(90f, 0f, 0f);
    

    public Rigidbody rb;

    void Start()
    {
        // Start at the first teleport point
        if (teleportPoints.Length > 0)
            transform.position = teleportPoints[currentPointIndex].position;

        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        targetHeight = defaultHeight;

        //cache camera positions and rotations
        camStandY   = playerCamera.transform.localPosition.y;
        camProneY   = camStandY - (defaultHeight - proneHeight);
        camStandRot = playerCamera.transform.localRotation;
        camProneRot = Quaternion.Euler(proneCameraEuler);
    }

    public void ContinueCursor()
    {
        canMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Prone logic (press and hold Left Control)
        if (canMove && Input.GetKeyDown(KeyCode.LeftControl))
        {
            isProneState = !isProneState;
            targetHeight = isProneState ? proneHeight : defaultHeight;

            //adjust camera height
            Vector3 camPos = playerCamera.transform.localPosition;
            camPos.y = isProneState ? camProneY : camStandY;
            playerCamera.transform.localPosition = camPos;

            //adjust camera rotation
            playerCamera.transform.localRotation = isProneState
                ? camProneRot
                : camStandRot;
        }

        // Teleport
        if (Input.GetKeyDown(KeyCode.Y))
            TeleportToNextPoint();

        // Toggle UI (APC)
        if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.P))
        {
            canMove = !canMove;
            Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible   = !canMove;
        }

        MovementControl();
    }

    // Handle Teleportation
    public void TeleportToNextPoint()
    {
        if (teleportPoints.Length == 0) return;

        currentPointIndex = (currentPointIndex + 1) % teleportPoints.Length;
        Debug.Log($"Teleporting to point index: {currentPointIndex}");

        if (rb != null)
            StartCoroutine(Teleport());
        else
            Debug.LogWarning("Rigidbody not found!");
    }

    public int CurrentPoint()
    {
        return currentPointIndex;
    }

    IEnumerator Teleport()
    {
        characterController.enabled = false;
        transform.position = teleportPoints[currentPointIndex].position;
        Debug.Log($"Teleported to: {teleportPoints[currentPointIndex].position}");
        characterController.enabled = true;
        yield return new WaitForSeconds(0.5f);
    }

    // Handle player movement
    void MovementControl()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right   = transform.TransformDirection(Vector3.right);
        bool isGrounded = characterController.isGrounded;

        // Smoothly transition character height
        characterController.height = Mathf.Lerp(
            characterController.height,
            targetHeight,
            proneTransitionSpeed * Time.deltaTime
        );

        //recenter
            //     characterController.center =
            // new Vector3(0f, characterController.height / 2f, 0f);

        // Check if currently prone
        bool isProne = Mathf.Abs(characterController.height - proneHeight) < 0.1f;
        if (isProne)
            isCurrentlyRunning = false; // Can't run while prone

        // Running input (Left Shift)
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded && !isProne)
            isCurrentlyRunning = true;
        else if (isGrounded)
            isCurrentlyRunning = false;

        // Determine speed
        float currentSpeed = isProne
            ? proneSpeed
            : (isCurrentlyRunning ? runSpeed : walkSpeed);

        float curSpeedX = canMove ? currentSpeed * Input.GetAxis("Vertical")   : 0;
        float curSpeedY = canMove ? currentSpeed * Input.GetAxis("Horizontal") : 0;

        // Preserve Y velocity for gravity/jump
        float movementDirectionY = moveDirection.y;

        // Grounded movement
        if (isGrounded)
            moveDirection = forward * curSpeedX + right * curSpeedY;
        else
        {
            // Airborne retains horizontal momentum plus any new input
            Vector3 horizontalVelocity = new Vector3(moveDirection.x, 0, moveDirection.z);
            Vector3 inputVelocity      = forward * curSpeedX + right * curSpeedY;
            if (inputVelocity != Vector3.zero)
                horizontalVelocity = inputVelocity;
            moveDirection = horizontalVelocity;
        }

        // Reapply Y
        moveDirection.y = movementDirectionY;

        // Jump
        if (Input.GetButton("Jump") && canMove && isGrounded)
            moveDirection.y = jumpPower;

        // Gravity
        if (!isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        // Move controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(
                0,
                Input.GetAxis("Mouse X") * lookSpeed,
                0
            );
        }
    }
}
