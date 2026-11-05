/*
Team members: Ethan Chang, Ryan Wu, Steven tan
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {
    public float speed;
    [Tooltip("Movement speed multiplier while holding Sprint.")]
    [Min(1f)] public float sprintMultiplier = 1.5f;
    [Header("Sprint Stamina")]
    [Min(0.01f)] public float sprintDuration = 5f;
    [Min(0f)] public float staminaRegenDelay = 1f;
    [Tooltip("Seconds to refill an empty stamina bar after the delay.")]
    [Min(0.01f)] public float staminaRefillTime = 5f;
    [Tooltip("Optional custom UI Slider. A bar is created automatically if empty.")]
    public Slider sprintBar;

    [Header("Sprint Effects")]
    [Tooltip("Optional looping particle effect that emits only while sprinting.")]
    public ParticleSystem sprintParticles;

    private float stamina;
    private float regenDelayRemaining;
    private bool exhausted;
    public float rotationSpeed;
    public float jumpForce;

    [Header("Air Movement")]
    [Tooltip("How quickly movement keys change horizontal velocity in the air. Zero disables air control.")]
    [Min(0f)] public float airAcceleration = 6f;
    
    // An empty GameObject placed at player head, tracked by Cinemachine camera
    public GameObject cameraTarget;

    private CapsuleCollider capsule;
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector2 movementValue;

    private float pitchInput;
    private float yawInput;

    private float xRotation = 0f; 
    [SerializeField] private float gravityMultiplier = 2f;
    private readonly RaycastHit[] groundHits = new RaycastHit[16];

    [SerializeField] private float minPitch = 0f;
    [SerializeField] private float maxPitch = 60f;
    
    [SerializeField] private float maxSlopeAngle = 85f;

    private bool jumping = false;
    private bool sprinting = false;

    Animator animator;
    private void Awake() {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        capsule = GetComponent<CapsuleCollider>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
        stamina = sprintDuration;
        InitializeSprintBar();
        if (sprintParticles != null) {
            var main = sprintParticles.main;
            main.playOnAwake = false;
            main.loop = true;
            sprintParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    
    public void OnMove(InputValue value) {
        movementValue = Vector2.ClampMagnitude(value.Get<Vector2>(), 1f);
        bool isMoving = movementValue.sqrMagnitude > 0.01f;
        animator.SetBool("isWalking", isMoving);
        Debug.Log(isMoving);
    }

    public void OnSprint(InputValue value) {
        // Held state is read in FixedUpdate, alongside stamina consumption.
    }

    private void OnDisable() {
        sprinting = false;
        if (sprintParticles != null) {
            sprintParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        movementValue = Vector2.zero;
        jumping = false;
        pitchInput = 0f;
        yawInput = 0f;
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

        // Min y component of unit normal to be considered grounded (allows jumping on slopes)
        float minGroundNormal = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        for (int i = 0; i < hitCount; i++) {
            RaycastHit hit = groundHits[i];
            if (hit.collider.attachedRigidbody != rb && hit.normal.y >= minGroundNormal) {
                return true;
            }
        }

        return false;
    }

    public void OnJump(InputValue value) {
        jumping = value.isPressed;
        animator.SetBool("isJumping", true);
        Debug.Log("Jumping is true");
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
        // Ignore the ground probe while rising so takeoff cannot count as grounded.
        bool grounded = rb.velocity.y <= 0.1f && IsGrounded();
        // Read the held state directly so a missed release callback cannot leave sprint on.
        InputAction sprintAction = playerInput != null && playerInput.isActiveAndEnabled
            ? playerInput.actions.FindAction("Player/Sprint", false) : null;
        bool sprintHeld = Application.isFocused && sprintAction != null
            && sprintAction.enabled && sprintAction.IsPressed();
        // Keep the full regeneration delay pending until Shift is released,
        // including while stationary or exhausted.
        if (sprintHeld) {
            regenDelayRemaining = staminaRegenDelay;
        }
        if (!sprintHeld) {
            exhausted = false;
        }

        sprinting = sprintHeld && !exhausted && stamina > 0f
            && movementValue.sqrMagnitude > 0.01f;
        if (sprinting) {
            stamina = Mathf.Max(0f, stamina - Time.fixedDeltaTime);
            if (stamina <= 0f) {
                // Require a release after exhaustion to avoid rapid sprint/walk pulses.
                exhausted = true;
            }
        } else if (!sprintHeld) {
            float refillDelta = Mathf.Max(0f, Time.fixedDeltaTime - regenDelayRemaining);
            regenDelayRemaining = Mathf.Max(0f, regenDelayRemaining - Time.fixedDeltaTime);
            stamina = Mathf.Min(sprintDuration, stamina
                + refillDelta * sprintDuration / Mathf.Max(0.01f, staminaRefillTime));
        }
        if (sprintBar != null) {
            sprintBar.value = stamina / Mathf.Max(0.01f, sprintDuration);
        }
        UpdateSprintParticles();

        Vector3 movementDirection = (transform.right * movementValue.x) + (transform.forward * movementValue.y);
        float movementSpeed = speed * (sprinting ? sprintMultiplier : 1f);
        if (grounded) {
            Vector3 movement = movementDirection * movementSpeed;
            rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        } else if (movementValue.sqrMagnitude > 0.01f) {
            // Keep momentum without input; steer toward input gradually instead of
            // instantly replacing velocity. Preserve faster sprint takeoff speed.
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            float airSpeed = Mathf.Max(movementSpeed, horizontalVelocity.magnitude);
            Vector3 targetVelocity = movementDirection * airSpeed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity,
                airAcceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }
        
        // Apply horizontal rotation to the Rigidbody (this rotates the camera as well with respect to pivot).
        float yaw = yawInput * rotationSpeed;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, yaw, 0f));

        // Consume mouse input after applying it.
        yawInput = 0f;

        // Extra gravity (allows player to fall faster)
        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);

        // Automatically jump whenever grounded and jump key is pressed
        if (jumping && grounded) {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        else if (animator.GetBool("isJumping") && rb.velocity.y <= 0f && grounded)
        {
            animator.SetBool("isJumping", false);
            Debug.Log("Jumping: False");
        }
    }

    private void UpdateSprintParticles() {
        if (sprintParticles == null) {
            return;
        }

        if (sprinting && stamina > 0f) {
            // isEmitting allows a quick restart while older particles fade out.
            if (!sprintParticles.isEmitting) {
                sprintParticles.Play(true);
            }
        } else if (sprintParticles.isEmitting) {
            sprintParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void InitializeSprintBar() {
        if (sprintBar == null) {
            GameObject canvasObject = new GameObject("Sprint HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject barObject = new GameObject("Sprint Stamina", typeof(RectTransform), typeof(Image), typeof(Slider));
            barObject.transform.SetParent(canvasObject.transform, false);
            RectTransform barRect = barObject.GetComponent<RectTransform>();
            barRect.anchorMin = barRect.anchorMax = new Vector2(0.5f, 0f);
            barRect.anchoredPosition = new Vector2(0f, 60f);
            barRect.sizeDelta = new Vector2(260f, 18f);
            Image background = barObject.GetComponent<Image>();
            background.color = new Color(0.08f, 0.1f, 0.12f, 0.85f);
            background.raycastTarget = false;

            GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(barRect, false);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            Image fill = fillObject.GetComponent<Image>();
            fill.color = new Color(0.2f, 0.85f, 0.65f, 1f);
            fill.raycastTarget = false;
            sprintBar = barObject.GetComponent<Slider>();
            sprintBar.fillRect = fillRect;
        }

        sprintBar.interactable = false;
        sprintBar.navigation = new Navigation { mode = Navigation.Mode.None };
        sprintBar.direction = Slider.Direction.LeftToRight;
        sprintBar.minValue = 0f;
        sprintBar.maxValue = 1f;
        sprintBar.wholeNumbers = false;
        sprintBar.value = 1f;
    }
}
