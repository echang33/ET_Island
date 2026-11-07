/*
 * ProjectileMovement.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Moves projectiles along their local forward axis and removes them after a limited lifetime.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    [Header("Projectile Travel")]
    [Tooltip("Travel speed in units per second along the projectile's local forward axis.")]
    public float speed = 15f;
    
    [Header("Cleanup")]
    [Tooltip("Seconds before automatic destruction, including projectiles that miss everything.")]
    public float lifetime = 5f; 

    void Start()
    {
        // Automatically destroy this object after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Translate uses local space, so the shoot point's rotation determines travel direction.
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
