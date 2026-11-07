/*
 * LaserOscillator.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Moves laser obstacles and their hitboxes along a smooth cycle with optional randomized starting phases.
 */

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LaserOscillator : MonoBehaviour
{
    [Header("Movement Path")]
    [Tooltip("Movement direction in world space: (0,1,0) for up/down, (1,0,0) for left/right.")]
    public Vector3 direction = Vector3.up;
    [Tooltip("Maximum distance on either side of the starting position, in world units.")]
    [Min(0f)] public float distance = 2f;
    [Header("Cycle Timing")]
    [Tooltip("Seconds for a complete back-and-forth cycle; changing this does not change the travel range.")]
    [Min(0.01f)] public float cycleDuration = 4f;
    [Tooltip("Timing offset in seconds. Use different offsets to stagger multiple lasers.")]
    public float timeOffset;
    [Tooltip("Start each laser at a random point in its cycle, in addition to Time Offset.")]
    public bool randomizeStart = true;

    private Rigidbody body;
    private Vector3 startPosition;
    private float elapsedSeconds;
    private float randomPhaseRadians;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        // A kinematic body moves the trigger hitbox through physics without falling under gravity.
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        startPosition = body.position;
    }

    private void Start()
    {
        randomPhaseRadians = randomizeStart ? Random.Range(0f, 2f * Mathf.PI) : 0f;
        // Place it at its initial phase immediately, before physics movement begins.
        body.position = GetOscillationPosition();
    }

    private void FixedUpdate()
    {
        elapsedSeconds += Time.fixedDeltaTime;
        body.MovePosition(GetOscillationPosition());
    }

    private Vector3 GetOscillationPosition()
    {
        // One full cycle is 2*pi radians. Normalizing keeps direction length from changing distance.
        float phase = randomPhaseRadians + (elapsedSeconds + timeOffset) * (2f * Mathf.PI / Mathf.Max(0.01f, cycleDuration));
        Vector3 offset = direction.normalized * (Mathf.Sin(phase) * distance);
        return startPosition + offset;
    }
}
