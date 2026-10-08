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

    void Update() {
        if (amount <= 0) {
            if (onDeath != null) {
                onDeath.Invoke();
            }
            Destroy(gameObject);
        }
    }
}
