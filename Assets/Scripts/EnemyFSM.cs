/*
 * EnemyFSM.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Controls turret detection, aiming, firing cooldowns, and the stronger mode caused by puzzle failure.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyFSM : MonoBehaviour
{
    public enum EnemyState { Idle, AttackPlayer}
    [Header("Turret State")]
    public EnemyState currentState;
    public float lastShootTime;
    [Header("Target Detection")]
    public Sight sightSensor;
    [Header("Projectile and Effects")]
    [FormerlySerializedAs("prefab")]
    public GameObject projectilePrefab;
    public GameObject shootPoint;
    public ParticleSystem muzzleFlash;
    [Header("Firing Timing")]
    [Tooltip("Seconds between shots. Lower values fire faster.")]
    [FormerlySerializedAs("fireRate")]
    public float shotIntervalSeconds = 2f;

    [Header("Super Mode")]
    [Tooltip("How many times faster this turret fires after puzzle failure.")]
    [SerializeField, Min(1f)] private float superFireRateMultiplier = 5f;
    [SerializeField, Min(1f)] private float superProjectileSpeedMultiplier = 3f;
    [Tooltip("Multiplier for the turret's detection radius during super mode.")]
    [SerializeField, Min(1f)] private float superSightRangeMultiplier = 2f;
    [Tooltip("Multiplier for the turret's viewing half-angle, capped at 180 degrees.")]
    [SerializeField, Min(1f)] private float superSightAngleMultiplier = 2f;

    private bool activated;

    public bool IsSuperMode { get; private set; }

    public void ActivateSuperMode()
    {
        // Repeated puzzle failures do not stack the multipliers.
        if (IsSuperMode)
        {
            return;
        }

        IsSuperMode = true;
        if (sightSensor != null)
        {
            sightSensor.distance *= Mathf.Max(1f, superSightRangeMultiplier);
            sightSensor.angle = Mathf.Min(180f, sightSensor.angle * Mathf.Max(1f, superSightAngleMultiplier));
        }
    }

    public void Deactivate() {
        activated = false;
    }

    void Start()
    {
        activated = true;
        // Wait for live detection and start a fresh cooldown after a scene reload.
        currentState = EnemyState.Idle;
        lastShootTime = Time.time;
    }

    // Detection is owned by Sight; this state machine decides when to aim and fire.
    void Update() {
        // If turret is not active, then keep in Idle state
        if (!activated) {
            currentState = EnemyState.Idle;
            return;
        }

        if (currentState == EnemyState.Idle)
        {
            if (sightSensor.detectedObject != null)
            {
                currentState = EnemyState.AttackPlayer;
            }
        } else if (currentState == EnemyState.AttackPlayer){
            AttackPlayer();
        }
    }

    void AttackPlayer()
    {
        if (sightSensor.detectedObject == null)
        {
            currentState = EnemyState.Idle;
            return;
        }
        LookTo(sightSensor.detectedObject.transform.position);
        Shoot();
    }

    void Shoot()
    {
        var timeSinceLastShoot = Time.time - lastShootTime;
        float shotInterval = IsSuperMode ? shotIntervalSeconds / Mathf.Max(1f, superFireRateMultiplier) : shotIntervalSeconds;
        if (timeSinceLastShoot > shotInterval)
        {
            lastShootTime = Time.time;
            muzzleFlash.Play();
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.transform.position, shootPoint.transform.rotation);
            if (IsSuperMode && projectile.TryGetComponent<ProjectileMovement>(out var movement))
            {
                movement.speed *= Mathf.Max(1f, superProjectileSpeedMultiplier);
            }
        }
    }

    void LookTo(Vector3 targetPosition)
    {
        // Rotate only the parent turret's yaw; ignore the target's height.
        Vector3 directionToPosition = Vector3.Normalize(targetPosition - transform.parent.position);
        directionToPosition.y = 0;
        transform.parent.forward = directionToPosition;
      
    }


}
