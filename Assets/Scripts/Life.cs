/*
 * Life.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Tracks health and runs death effects and events once before destroying the object.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Life : MonoBehaviour {
    [Header("Health")]
    [Tooltip("Remaining health. Reaching zero or less triggers death on the next Update.")]
    public float amount;
    [Header("Death Events and Effects")]
    public UnityEvent onDeath;
    [Tooltip("Optional particle-effect prefab spawned independently at this object's position on death.")]
    public GameObject deathEffectPrefab;

    private bool isDead;

    void Update() {
        if (!isDead && amount <= 0) {
            // Set the guard before invoking callbacks so death cannot run twice.
            isDead = true;
            if (deathEffectPrefab != null) {
                // No parent: the effect survives this object's destruction.
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            if (onDeath != null) {
                onDeath.Invoke();
            }
            Destroy(gameObject);
        }
    }
}
