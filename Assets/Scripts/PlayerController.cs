using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]public bool lockedCursor = true;

    [SerializeField] Transform playerCamera; //First-Person camera parented to the player GameObject.
    [SerializeField] float horizontalMouseSensitivity = 2.5f; //Multiplyer of the base sensetivity.
    [SerializeField] float verticalMouseSensitivity = 2.5f; //Multiplyer of the base sensetivity.
    [SerializeField] float walkSpeed = 5.0f; //The default speed that the player will move at.
    [SerializeField] [Range(0.0f, 0.5f)] float movementSmoothTime = 0.3f; //The time taken to smooth between the current movement vector, and the next one.
    [SerializeField] [Range(0.0f, 0.1f)] float mouseSmoothTime = 0.03f;
    [SerializeField] float gravity = -13.0f;
    [SerializeField] float jumpForce = 5.0f;

    float cameraPitch = 0.0f;
    CharacterController controller = null; //CharacterController component on the player GameObject.

    Vector2 currentDir = Vector2.zero;
    Vector2 currentDirVelocity = Vector2.zero;

    Vector2 currentMouseDelta = Vector2.zero;
    Vector2 currentMouseDeltaVelocity = Vector2.zero;

    float velocityY = 0.0f; //How quickly the player is moving on the Y axis.

    bool canJump = false;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        UpdateCursorLock();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E/*scape*/))
        {
            lockedCursor = !lockedCursor;
            UpdateCursorLock();
        }

        if (lockedCursor) //Game is currently playing.
        {
            UpdateCamera();
            UpdateMovement();

            if (Input.GetKeyDown(KeyCode.Space) && canJump)
            {
                controller.Move(Vector3.up * jumpForce * Time.deltaTime);
                canJump = false;
            }
        }

        
    }

    void UpdateCamera() //An update function to change the rotation of the First-Person camera.
    {
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); //A vector of how much the mouse has moved by since the last call.
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        transform.Rotate(Vector3.up * currentMouseDelta.x * horizontalMouseSensitivity); //Change camera Yaw by rotating player. This means the player forward vector is always aligned with the camera.

        cameraPitch -= currentMouseDelta.y * verticalMouseSensitivity; //Calculates the amount to move the camera pitch by.
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f); //Clamps the angle to prevent flipping.
        playerCamera.localEulerAngles = Vector3.right * cameraPitch; //Change camera pitch by rotating the camera itself.
    }

    void UpdateCursorLock() //An update function to handle the lock state and visibility of the cursor when entering and exiting gameplay.
    {
        if (lockedCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void UpdateMovement() //An update function to change the position of the player character.
    {
        Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); //maps out movement directions onto a 2d vector.
        targetDir.Normalize(); //Normalizes the vector, so all directions have the length of 1. This means that diagonals are not longer.

        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, movementSmoothTime); //Smooths the vector.

        if (controller.isGrounded)
        {
            velocityY = 0.0f;
            canJump = true;
        }
        velocityY += gravity * Time.deltaTime;

        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * walkSpeed + Vector3.up * velocityY; //Calculates the movement vector.
        controller.Move(velocity * Time.deltaTime); //Passes movement to the CharacterController component.
    }
}
