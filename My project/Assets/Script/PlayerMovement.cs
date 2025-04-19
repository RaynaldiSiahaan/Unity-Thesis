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
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    public float crouchTransitionSpeed = 15f; // Speed of crouch height transition
    public int teleportPointIndex = 0; // Index of teleport point to move to

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;
    private bool isCurrentlyRunning = false; // Tracks running status
    private float targetHeight; // Target height for crouching/standing

    public Transform[] teleportPoints; // Array to store teleport points
    private int currentPointIndex = 0; // Tracks current teleport index

    public Rigidbody rb;

    void Start()
    {
        gameObject.transform.position = teleportPoints[currentPointIndex].position;
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        // Set initial height
        targetHeight = defaultHeight;
    }

    public void ContinueCursor(){
        canMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Crouch logic (press and hold)
        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            targetHeight = crouchHeight;
        }
        else
        {
            targetHeight = defaultHeight;
        }
        
        //Teleport
        if (Input.GetKeyDown("y")) // Checks if y button is pressed
        {
            TeleportToNextPoint();
        }

        //Show APC UI
        if (Input.GetKeyDown("h") || Input.GetKeyDown("p")) // Checks if u button is pressed
        {
            if (canMove)
            {
                canMove = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                canMove = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

        }



        // Move.
        MovementControl();
    }



    // Handle Teleportation
    public void TeleportToNextPoint()
    {
        if (teleportPoints.Length == 0) return;

        currentPointIndex = (currentPointIndex + 1) % teleportPoints.Length;
        Debug.Log($"Teleporting to point index: {currentPointIndex}");
        if (rb != null)
        {
            StartCoroutine(Teleport()); // Use transform position
        }
        else
        {
            Debug.Log("Rigidbody not found!");
        }
    }

    public int CurrentPoint(){
        return currentPointIndex;
    }

    IEnumerator Teleport()
    {
        characterController.enabled = false;
        gameObject.transform.position = teleportPoints[currentPointIndex].position;
        Debug.Log($"Teleported to: {teleportPoints[currentPointIndex].position}");
        characterController.enabled = true;

        yield return new WaitForSeconds(0.5f);
        
    }

    // Handle player movement
    void MovementControl()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Determine if the player is grounded
        bool isGrounded = characterController.isGrounded;

        characterController.height = Mathf.Lerp(characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        // Adjust speed while crouching
        bool isCrouching = Mathf.Abs(characterController.height - crouchHeight) < 0.1f;
        if (isCrouching)
        {
            isCurrentlyRunning = false; // Cannot run while crouching
        }

        // Determine movement speed
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded && !isCrouching)
        {
            isCurrentlyRunning = true; // Start running if grounded and shift is held
        }
        else if (isGrounded)
        {
            isCurrentlyRunning = false; // Stop running if grounded and shift is not held
        }

        float currentSpeed = isCrouching ? crouchSpeed : (isCurrentlyRunning ? runSpeed : walkSpeed);
        float curSpeedX = canMove ? currentSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? currentSpeed * Input.GetAxis("Horizontal") : 0;

        // Preserve Y-axis movement
        float movementDirectionY = moveDirection.y;

        if (isGrounded)
        {
            // Update movement direction when grounded
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        }
        else
        {
            // Maintain momentum while airborne
            Vector3 horizontalVelocity = new Vector3(moveDirection.x, 0, moveDirection.z);
            Vector3 inputVelocity = (forward * curSpeedX) + (right * curSpeedY);

            // Add input to current horizontal velocity
            if (inputVelocity != Vector3.zero)
            {
                horizontalVelocity = inputVelocity;
            }

            moveDirection = horizontalVelocity;
        }

        // Apply Y-axis movement
        moveDirection.y = movementDirectionY;

        // Jump logic
        if (Input.GetButton("Jump") && canMove && isGrounded)
        {
            moveDirection.y = jumpPower;
        }

        // Apply gravity if not grounded
        if (!isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the character
        characterController.Move(moveDirection * Time.deltaTime);

        // Handle camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}
