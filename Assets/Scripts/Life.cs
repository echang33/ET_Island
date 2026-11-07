/*
Team members: Ethan Chang, Ryan Wu, Steven tan
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Life : MonoBehaviour {
    public float amount;
    public UnityEvent onDeath;
    [Tooltip("Optional particle-effect prefab spawned independently at this object's position on death.")]
    public GameObject deathEffectPrefab;

    private bool isDead;

    void Update() {
        if (!isDead && amount <= 0) {
            isDead = true;
            if (deathEffectPrefab != null) {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            if (onDeath != null) {
                onDeath.Invoke();
            }
            Destroy(gameObject);
        }
    }
}
