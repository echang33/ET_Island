/*
 * PlayerMovement.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Handles player input, Rigidbody movement, jumping, optional air momentum, sprint stamina, and sprint effects.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {
    [Header("Ground Movement")]
    [Tooltip("Normal horizontal movement speed in units per second.")]
    public float speed;
    [Tooltip("Movement speed multiplier while holding Sprint.")]
    [Min(1f)] public float sprintMultiplier = 1.5f;
    [Header("Sprint Stamina")]
    [Tooltip("Stamina capacity measured in seconds of continuous sprinting.")]
    [Min(0.01f)] public float sprintDuration = 5f;
    [Tooltip("Seconds to wait after Sprint is released before stamina can refill.")]
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
    private bool isSprintExhausted;
    [Header("Look and Jump")]
    [Tooltip("Mouse-look sensitivity multiplier for yaw and pitch.")]
    public float rotationSpeed;
    [Tooltip("Upward impulse applied when jumping from the ground.")]
    public float jumpForce;

    [Header("Air Movement")]
    [Tooltip("Preserve jump momentum with gradual air control. Disable for instant movement and stopping in midair.")]
    public bool useJumpMomentum = true;
    [Tooltip("How quickly movement keys change horizontal velocity in the air. Zero disables air control.")]
    [Min(0f)] public float airAcceleration = 6f;
    
    [Header("Camera Target")]
    // An empty GameObject placed at player head, tracked by Cinemachine camera.
    public GameObject cameraTarget;

    private CapsuleCollider playerCollider;
    private Rigidbody playerBody;
    private PlayerInput playerInput;
    private Vector2 movementInput;

    private float pitchInput;
    private float yawInput;

    private float cameraPitch = 0f;
    [Header("Gravity")]
    [SerializeField] private float gravityMultiplier = 2f;
    private readonly RaycastHit[] groundProbeHits = new RaycastHit[16];

    [Header("Camera Pitch Limits")]
    [SerializeField] private float minPitch = 0f;
    [SerializeField] private float maxPitch = 60f;
    
    [Header("Ground Detection")]
    [SerializeField] private float maxSlopeAngle = 85f;

    private bool jumpHeld = false;
    private bool isSprinting = false;

    Animator animator;
    private void Awake() {
        playerBody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerCollider = GetComponent<CapsuleCollider>();
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
        movementInput = Vector2.ClampMagnitude(value.Get<Vector2>(), 1f);
        bool isMoving = movementInput.sqrMagnitude > 0.01f;
        animator.SetBool("isWalking", isMoving);
        Debug.Log(isMoving);
    }

    public void OnSprint(InputValue value) {
        // PlayerInput's Send Messages mode expects this receiver. Read the held
        // state in FixedUpdate instead, so a missed release cannot leave sprint stuck on.
    }

    private void OnDisable() {
        isSprinting = false;
        if (sprintParticles != null) {
            sprintParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        movementInput = Vector2.zero;
        jumpHeld = false;
        pitchInput = 0f;
        yawInput = 0f;
    }

    public void OnLook(InputValue value) {
        Vector2 input = value.Get<Vector2>();

        yawInput += input.x;
        pitchInput += input.y;
    }

    private bool IsGrounded() {
        // Probe slightly inside the capsule; the surface-normal check rejects steep walls.
        Bounds bounds = playerCollider.bounds;
        float radius = Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.9f;
        const float groundTolerance = 0.1f;
        Vector3 origin = bounds.center;
        float distance = bounds.extents.y - radius + groundTolerance;
        int hitCount = Physics.SphereCastNonAlloc(origin, radius, Vector3.down,
            groundProbeHits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        // Minimum Y component of a surface normal that qualifies as ground, including slopes.
        float minGroundNormal = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        for (int i = 0; i < hitCount; i++) {
            RaycastHit hit = groundProbeHits[i];
            if (hit.collider.attachedRigidbody != playerBody && hit.normal.y >= minGroundNormal) {
                return true;
            }
        }

        return false;
    }

    public void OnJump(InputValue value) {
        jumpHeld = value.isPressed;
        animator.SetBool("isJumping", true);
        Debug.Log("Jumping is true");
    }

    void Update() {
        // Pitch is applied directly to the camera pivot.
        cameraPitch -= pitchInput * rotationSpeed;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        cameraTarget.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        // Consume mouse input after applying it.
        pitchInput = 0f;
    }

    void FixedUpdate() {
        // Ignore the ground probe while rising so takeoff cannot count as grounded.
        bool grounded = playerBody.velocity.y <= 0.1f && IsGrounded();
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
            isSprintExhausted = false;
        }

        isSprinting = sprintHeld && !isSprintExhausted && stamina > 0f
            && movementInput.sqrMagnitude > 0.01f;
        if (isSprinting) {
            stamina = Mathf.Max(0f, stamina - Time.fixedDeltaTime);
            if (stamina <= 0f) {
                // Require a release after exhaustion to avoid rapid sprint/walk pulses.
                isSprintExhausted = true;
            }
        } else if (!sprintHeld) {
            // Only the part of this physics step after the delay expires may refill stamina.
            float refillDelta = Mathf.Max(0f, Time.fixedDeltaTime - regenDelayRemaining);
            regenDelayRemaining = Mathf.Max(0f, regenDelayRemaining - Time.fixedDeltaTime);
            stamina = Mathf.Min(sprintDuration, stamina
                + refillDelta * sprintDuration / Mathf.Max(0.01f, staminaRefillTime));
        }
        if (sprintBar != null) {
            sprintBar.value = stamina / Mathf.Max(0.01f, sprintDuration);
        }
        UpdateSprintParticles();

        Vector3 movementDirection = (transform.right * movementInput.x) + (transform.forward * movementInput.y);
        float movementSpeed = speed * (isSprinting ? sprintMultiplier : 1f);
        // With momentum disabled, the original instant steering/stopping also applies in midair.
        if (grounded || !useJumpMomentum) {
            Vector3 movement = movementDirection * movementSpeed;
            playerBody.velocity = new Vector3(movement.x, playerBody.velocity.y, movement.z);
        } else if (movementInput.sqrMagnitude > 0.01f) {
            // Keep momentum without input; steer toward input gradually instead of
            // instantly replacing velocity. Preserve faster sprint takeoff speed.
            Vector3 horizontalVelocity = new Vector3(playerBody.velocity.x, 0f, playerBody.velocity.z);
            float airSpeed = Mathf.Max(movementSpeed, horizontalVelocity.magnitude);
            Vector3 targetVelocity = movementDirection * airSpeed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity,
                airAcceleration * Time.fixedDeltaTime);
            playerBody.velocity = new Vector3(horizontalVelocity.x, playerBody.velocity.y, horizontalVelocity.z);
        }
        
        // Apply horizontal rotation to the Rigidbody (this rotates the camera as well with respect to pivot).
        float yaw = yawInput * rotationSpeed;
        playerBody.MoveRotation(playerBody.rotation * Quaternion.Euler(0f, yaw, 0f));

        // Consume mouse input after applying it.
        yawInput = 0f;

        // Extra gravity (allows player to fall faster)
        playerBody.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);

        // Automatically jump whenever grounded and jump key is pressed
        if (jumpHeld && grounded) {
            playerBody.velocity = new Vector3(playerBody.velocity.x, 0f, playerBody.velocity.z);
            playerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        else if (animator.GetBool("isJumping") && playerBody.velocity.y <= 0f && grounded)
        {
            animator.SetBool("isJumping", false);
            Debug.Log("Jumping: False");
        }
        // Recheck after draining stamina so the sprint animation ends on the exhaustion frame.
        isSprinting = sprintHeld && !isSprintExhausted && stamina > 0f && movementInput.sqrMagnitude > 0.01f;
        animator.SetBool("isSprinting", isSprinting);
    }

    private void UpdateSprintParticles() {
        if (sprintParticles == null) {
            return;
        }

        if (isSprinting && stamina > 0f) {
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
            // Parent the generated HUD to the player so it is cleaned up on death.
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
