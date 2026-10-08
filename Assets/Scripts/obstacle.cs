/*
Team members: Ethan Chang, Ryan Wu, Steven tan
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Required to use UnityEvents

public class Obstacle : MonoBehaviour {
    // Exposes a customizable event list in the Unity Inspector
    public UnityEvent onPlayerTouched;

    private void OnCollisionEnter(Collision collision) {
        // Verify the object hitting the obstacle is actually the player
        if (collision.gameObject.CompareTag("Player")) {
            // Trigger whatever actions you configure in the Inspector
            onPlayerTouched.Invoke();
        }
    }
}
