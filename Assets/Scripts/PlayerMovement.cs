/*
Team members: Ethan Chang, Ryan Wu, Steven tan
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
    public float speed;
    public float rotationSpeed;
    public float jumpForce;
    
    // The invisible point the camera will orbit around
    public Transform cameraPivot;

    private CapsuleCollider capsule;
    private Rigidbody rb;
    private Vector2 movementValue;
    private Vector2 lookValue; 
    private float xRotation = 0f; 
    [SerializeField] private float gravityMultiplier = 2f;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void OnMove(InputValue value) {
        movementValue = value.Get<Vector2>() * speed;
    }

    public void OnLook(InputValue value) {
        lookValue = value.Get<Vector2>() * rotationSpeed;
    }

    private bool IsGrounded() {
        // Casts a ray (half the height of the capsule from the center) downwards
        // to check if the player is grounded.
        float halfHeight = capsule.height / 2 + 0.5f;
        return Physics.Raycast(transform.position, Vector3.down, halfHeight);
    }

    public void OnJump(InputValue value) {
        // TODO: Only jump if player is grounded. Tried using the IsGrounded() check above
        // but was kind of jank, so reverting for now.
        if (value.isPressed) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Update() {
        // Pitch the pivot up and down. Because the camera is set back, it will orbit.
        xRotation -= lookValue.y * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Yaw the entire player body left and right
        transform.Rotate(Vector3.up * lookValue.x * Time.deltaTime);
    }

    void FixedUpdate() {
        Vector3 movement = (transform.right * movementValue.x) + (transform.forward * movementValue.y);
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        
        // Extra gravity (allows player to fall faster)
        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
    }
}