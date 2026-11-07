/*
 * TurretShoot.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Provides a simple repeating turret shooter without the EnemyFSM detection and state logic.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TurretShoot : MonoBehaviour {
    [Header("Projectile and Effects")]
    [FormerlySerializedAs("prefab")]
    public GameObject projectilePrefab;
    public GameObject shootPoint;
    public ParticleSystem muzzle;
    [Header("Firing Timing")]
    [Tooltip("Seconds between shots. Lower values fire faster.")]
    [FormerlySerializedAs("fireRate")]
    public float shotIntervalSeconds = 2f;
    
    // Tracks time passed since the last shot
    private float secondsSinceLastShot = 0f;

    void Update() {
        // Scaled time pauses this cooldown while a graph puzzle has Time.timeScale set to zero.
        secondsSinceLastShot += Time.deltaTime;

        // Fire once the cooldown has elapsed, then restart the timer.
        if (secondsSinceLastShot >= shotIntervalSeconds) {
            OnFire();
            secondsSinceLastShot = 0f;
        }
    }

    public void OnFire() {
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.transform.position, shootPoint.transform.rotation);
        // Muzzle particles are currently configured separately; firing only spawns the projectile.
    }
}
