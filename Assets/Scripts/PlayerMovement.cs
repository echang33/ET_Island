using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float jumpForce;
    
    // The invisible point the camera will orbit around
    public Transform cameraPivot;

    private Rigidbody rb;
    private Vector2 movementValue;
    private Vector2 lookValue; 
    private float xRotation = 0f; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void OnMove(InputValue value)
    {
        movementValue = value.Get<Vector2>() * speed;
    }

    public void OnLook(InputValue value)
    {
        lookValue = value.Get<Vector2>() * rotationSpeed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Update()
    {
        // Pitch the pivot up and down. Because the camera is set back, it will orbit.
        xRotation -= lookValue.y * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Yaw the entire player body left and right
        transform.Rotate(Vector3.up * lookValue.x * Time.deltaTime);
    }

    void FixedUpdate()
    {
        Vector3 movement = (transform.right * movementValue.x) + (transform.forward * movementValue.y);
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
    }
}