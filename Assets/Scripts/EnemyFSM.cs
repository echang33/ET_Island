using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    public enum EnemyState { Idle, AttackPlayer}
    public EnemyState currentState;
    public float lastShootTime;
    public Sight sightSensor;
    public GameObject prefab;
    public GameObject shootPoint;

    public float fireRate = 2f;

    [Header("Super Mode")]
    [Tooltip("How many times faster this turret fires after puzzle failure.")]
    [SerializeField, Min(1f)] private float superFireRateMultiplier = 5f;
    [SerializeField, Min(1f)] private float superProjectileSpeedMultiplier = 3f;
    [Tooltip("Multiplier for the turret's detection radius during super mode.")]
    [SerializeField, Min(1f)] private float superSightRangeMultiplier = 2f;
    [Tooltip("Multiplier for the turret's viewing half-angle, capped at 180 degrees.")]
    [SerializeField, Min(1f)] private float superSightAngleMultiplier = 2f;
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

    void Start()
    {
        // Wait for live detection and start a fresh cooldown after a scene reload.
        currentState = EnemyState.Idle;
        lastShootTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
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
        float shotInterval = IsSuperMode ? fireRate / Mathf.Max(1f, superFireRateMultiplier) : fireRate;
        if (timeSinceLastShoot > shotInterval)
        {
            lastShootTime = Time.time;
            GameObject projectile = Instantiate(prefab, shootPoint.transform.position, shootPoint.transform.rotation);
            if (IsSuperMode && projectile.TryGetComponent<ProjectileMovement>(out var movement))
            {
                movement.speed *= Mathf.Max(1f, superProjectileSpeedMultiplier);
            }
        }
    }

    void LookTo(Vector3 targetPosition)
    {
        Vector3 directionToPosition = Vector3.Normalize(targetPosition - transform.parent.position);
        directionToPosition.y = 0;
        transform.parent.forward = directionToPosition;
      
    }


}
