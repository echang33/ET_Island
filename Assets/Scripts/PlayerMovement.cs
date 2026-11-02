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
    
    // An empty GameObject placed at player head, tracked by Cinemachine camera
    public GameObject cameraTarget;

    private CapsuleCollider capsule;
    private Rigidbody rb;
    private Vector2 movementValue;

    private float pitchInput;
    private float yawInput;

    private float xRotation = 0f; 
    [SerializeField] private float gravityMultiplier = 2f;
    private readonly RaycastHit[] groundHits = new RaycastHit[16];

    [SerializeField] private float minPitch = 0f;
    [SerializeField] private float maxPitch = 60f;
    private bool jumping = false;

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
        Vector2 input = value.Get<Vector2>();

        yawInput += input.x;
        pitchInput += input.y;
    }

    private bool IsGrounded() {
        Bounds bounds = capsule.bounds;
        float radius = Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.9f;
        const float groundTolerance = 0.1f;
        Vector3 origin = bounds.center;
        float distance = bounds.extents.y - radius + groundTolerance;
        int hitCount = Physics.SphereCastNonAlloc(origin, radius, Vector3.down,
            groundHits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++) {
            RaycastHit hit = groundHits[i];
            if (hit.collider.attachedRigidbody != rb && hit.normal.y >= 0.5f) {
                return true;
            }
        }

        return false;
    }

    public void OnJump(InputValue value) {
        jumping = value.isPressed;
    }

    void Update() {
        // Pitch is applied directly to the camera pivot.
        xRotation -= pitchInput * rotationSpeed;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        cameraTarget.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // Consume mouse input after applying it.
        pitchInput = 0f;
    }

    void FixedUpdate() {
        Vector3 movement = (transform.right * movementValue.x) + (transform.forward * movementValue.y);
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        
        // Apply horizontal rotation to the Rigidbody (this rotates the camera as well with respect to pivot).
        float yaw = yawInput * rotationSpeed;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, yaw, 0f));

        // Consume mouse input after applying it.
        yawInput = 0f;

        // Extra gravity (allows player to fall faster)
        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);

        // Automatically jump whenever grounded and jump key is pressed
        if (jumping && rb.velocity.y <= 0.1f && IsGrounded()) {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
