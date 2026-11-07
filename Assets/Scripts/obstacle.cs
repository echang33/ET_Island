/*
 * obstacle.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Invokes Inspector-configured actions when a player physically collides with an obstacle.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Required to use UnityEvents

public class Obstacle : MonoBehaviour {
    [Header("Player Collision Events")]
    [Tooltip("Invoked on a physical collision with a Player-tagged object; triggers use ContactDamager instead.")]
    public UnityEvent onPlayerTouched;

    private void OnCollisionEnter(Collision collision) {
        // Verify the object hitting the obstacle is actually the player
        if (collision.gameObject.CompareTag("Player")) {
            // Trigger whatever actions you configure in the Inspector
            onPlayerTouched.Invoke();
        }
    }
}
